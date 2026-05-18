# C# Testing Blueprints: xUnit, Testcontainers, & Respawn

## 1. Unit Testing via TypedResults (No Network Pipeline Latency)

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

public static class TaskEndpoints
{
    public static Results<Ok<TaskItem>, NotFound> GetTaskById(int id, ITaskRepository repo)
    {
        var task = repo.GetById(id);
        return task is not null? TypedResults.Ok(task) : TypedResults.NotFound();
    }
}

public class TaskEndpointTests
{
    [Fact]
    public void GetTaskById_ReturnsOkWithData_WhenTaskExists()
    {
        // Arrange
        var mockRepository = new Mock<ITaskRepository>();
        var expectedTask = new TaskItem(42, "Standardize Testing Practices");
        mockRepository.Setup(repo => repo.GetById(42)).Returns(expectedTask);

        // Act
        var result = TaskEndpoints.GetTaskById(42, mockRepository.Object);

        // Assert
        var okResult = Assert.IsType<Ok<TaskItem>>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Equal("Standardize Testing Practices", okResult.Value?.Title);
    }
}

```

---

## 2. Comprehensive E2E Integration Testing Blueprint

Orchestrates PostgreSQL container instances dynamically, Waiting on database readiness, and integrating Respawn truncation to isolate test state without reconstruction overhead.

The Circuit Breaker failure metric ratio is calculated as follows:


$$F_{ratio} = \frac{\sum_{i=1}^{N} \mathbb{I}(\text{attempt}_i = \text{failure})}{N} \ge F_{threshold} \quad \text{for } N \ge T_{min}$$

```csharp
using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;

public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Define a PostgreSQL container with dynamic port mapping and a robust wait strategy
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
       .WithImage("postgres:16-alpine")
       .WithDatabase("IntegrationDb")
       .WithUsername("postgres")
       .WithPassword("SecurePassword99!")
       .WithPortBinding(5432, true)
       .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready -U postgres"))
       .Build();

    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;

    public HttpClient HttpClient { get; private set; } = default!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            // Inject the dynamic Testcontainer connection string
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        HttpClient = CreateClient();

        // Establish connection to initialize Respawn database isolation
        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();

        // Apply any pending database migrations before initializing Respawn
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();
        }

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    public new async Task DisposeAsync()
    {
        await _dbConnection.DisposeAsync();
        await _dbContainer.StopAsync();
    }
}

public class IntegrationCollection : ICollectionFixture<IntegrationTestFactory> { }

[Collection(nameof(IntegrationCollection))]
public class ProductIntegrationTests(IntegrationTestFactory factory) : IAsyncLifetime
{
    private readonly HttpClient _client = factory.HttpClient;

    public Task InitializeAsync() => Task.CompletedTask;

    // Reset database state before executing the next test case
    public async Task DisposeAsync() => await factory.ResetDatabaseAsync();

    [Fact]
    public async Task Post_CreatesProduct_InRealDatabase()
    {
        // Arrange
        var newProduct = new ProductDto("Architectural Testing Book", 49.99m);

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", newProduct);

        // Assert
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);
        Assert.Equal("Architectural Testing Book", product.Title);
    }
}

```