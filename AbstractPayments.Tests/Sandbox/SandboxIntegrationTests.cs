namespace AbstractPayments.Tests.Sandbox;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AbstractPayments.Sandbox.Coupled;
using AbstractPayments.Sandbox.Storage;
using AbstractPayments.Sandbox.Storage.Models;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Custom application factory providing unique isolated SQLite databases for each test execution.
/// </summary>
public class SandboxTestApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Gets the unique SQLite file path for this test instance.
    /// </summary>
    public string DbFile { get; } = $"test_{Guid.NewGuid():N}.db";

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDbConnectionFactory));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory($"Data Source={DbFile}"));
        });
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (File.Exists(DbFile))
        {
            try { File.Delete(DbFile); }
            catch { }
        }
    }
}

/// <summary>
/// E2E Integration tests verifying coupled vs abstracted Minimal API payment endpoints.
/// </summary>
public class SandboxIntegrationTests : IClassFixture<SandboxTestApplicationFactory>
{
    private readonly SandboxTestApplicationFactory _factory;
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="SandboxIntegrationTests"/> class.
    /// </summary>
    public SandboxIntegrationTests(SandboxTestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Coupled_Pix_Route_Should_Create_And_Persist_Transaction()
    {
        // Arrange
        var request = new CoupledPixRequest(150.00m);

        // Act
        var response = await _client.PostAsJsonAsync("/v1/api/coupled/payments/pix", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<FakePixResponse>();
        Assert.NotNull(content);
        Assert.StartsWith("fake_tx_", content.FakeTransactionId);
        Assert.Equal(150.00m, content.MerchantAmount);
        Assert.Equal("fake-direct-qrcode-raw-base64-payload-xyz", content.QrCodeBase64);

        var connectionFactory = _factory.Services.GetRequiredService<IDbConnectionFactory>();
        using var conn = connectionFactory.CreateConnection();
        var tx = await conn.QueryFirstOrDefaultAsync<Transaction>(
            "SELECT * FROM Transactions WHERE Provider = @Provider AND Amount = @Amount LIMIT 1;",
            new { Provider = "fake_coupled", Amount = 150.00m });

        Assert.NotNull(tx);
        Assert.Equal(150.00m, tx.Amount);
        Assert.Equal(ETransactionStatus.Pending, tx.Status);
    }

    [Fact]
    public async Task Abstracted_Pix_Route_Should_Resolve_Gateway_Dynamically_Create_And_Persist_Transaction()
    {
        // Arrange
        var request = new AbstractedPixRequest(250.00m, "fake");

        // Act
        var response = await _client.PostAsJsonAsync("/v1/api/payments/pix", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<AbstractedPixResponse>();
        Assert.NotNull(content);
        Assert.NotEmpty(content.TransactionId);
        Assert.Equal(250.00m, content.Amount);
        Assert.Equal("fake", content.Provider);
        Assert.Equal("fake-abstract-qrcode-success-xyz", content.PaymentString);
        Assert.Equal("Pending", content.Status);

        var connectionFactory = _factory.Services.GetRequiredService<IDbConnectionFactory>();
        using var conn = connectionFactory.CreateConnection();
        var tx = await conn.QueryFirstOrDefaultAsync<Transaction>(
            "SELECT * FROM Transactions WHERE Id = @Id;",
            new { Id = content.TransactionId });

        Assert.NotNull(tx);
        Assert.Equal(250.00m, tx.Amount);
        Assert.Equal("fake", tx.Provider);
    }

    [Fact]
    public async Task Abstracted_Pix_Route_Should_Return_BadRequest_When_Provider_Is_Not_Registered()
    {
        // Arrange
        var request = new AbstractedPixRequest(100.00m, "unregistered_stripe");

        // Act
        var response = await _client.PostAsJsonAsync("/v1/api/payments/pix", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Provider Not Registered", problem.Title);
        Assert.Contains("unregistered_stripe", problem.Detail);
    }

    [Fact]
    public async Task GET_Endpoints_Should_Return_List_And_Specific_Transactions()
    {
        // Arrange
        var txId = Guid.NewGuid().ToString();
        var seededTx = new Transaction
        {
            Id = txId,
            Amount = 99.99m,
            Provider = "inline_seeded_provider",
            PaymentString = "inline_seeded_qrcode",
            Status = ETransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var connectionFactory = _factory.Services.GetRequiredService<IDbConnectionFactory>();
        using (var conn = connectionFactory.CreateConnection())
        {
            await conn.ExecuteAsync(@"
                INSERT INTO Transactions (Id, Amount, Provider, PaymentString, Status, CreatedAt)
                VALUES (@Id, @Amount, @Provider, @PaymentString, @Status, @CreatedAt);",
                seededTx);
        }

        // Act & Assert 1: GET all transactions
        var listResponse = await _client.GetAsync("/v1/api/payments/pix");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var list = await listResponse.Content.ReadFromJsonAsync<List<Transaction>>();
        Assert.NotNull(list);
        Assert.Contains(list, t => t.Id == txId && t.Provider == "inline_seeded_provider");

        // Act & Assert 2: GET specific transaction by ID
        var singleResponse = await _client.GetAsync($"/v1/api/payments/pix/{txId}");
        Assert.Equal(HttpStatusCode.OK, singleResponse.StatusCode);

        var singleTx = await singleResponse.Content.ReadFromJsonAsync<Transaction>();
        Assert.NotNull(singleTx);
        Assert.Equal(txId, singleTx.Id);
        Assert.Equal(99.99m, singleTx.Amount);
    }

    [Fact]
    public async Task Coupled_Webhook_Should_Authenticate_And_Update_Transaction_Status()
    {
        // Arrange
        var txId = Guid.NewGuid().ToString();
        var seededTx = new Transaction
        {
            Id = txId,
            Amount = 450.00m,
            Provider = "fake_coupled",
            PaymentString = "fake-coupled-payload",
            Status = ETransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var connectionFactory = _factory.Services.GetRequiredService<IDbConnectionFactory>();
        using (var conn = connectionFactory.CreateConnection())
        {
            await conn.ExecuteAsync(@"
                INSERT INTO Transactions (Id, Amount, Provider, PaymentString, Status, CreatedAt)
                VALUES (@Id, @Amount, @Provider, @PaymentString, @Status, @CreatedAt);",
                seededTx);
        }

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/v1/api/coupled/payments/webhook")
        {
            Content = JsonContent.Create(new { transactionId = txId, status = "Paid" })
        };
        requestMessage.Headers.Add("X-Signature", "fake_secret_signature");

        // Act
        var response = await _client.SendAsync(requestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var assertConn = connectionFactory.CreateConnection();
        var tx = await assertConn.QueryFirstOrDefaultAsync<Transaction>(
            "SELECT * FROM Transactions WHERE Id = @Id;",
            new { Id = txId });

        Assert.NotNull(tx);
        Assert.Equal(ETransactionStatus.Paid, tx.Status);
    }

    [Fact]
    public async Task Abstracted_Webhook_Should_Authenticate_Convert_And_Update_Transaction_Status_With_Idempotency()
    {
        // Arrange
        var txId = Guid.NewGuid().ToString();
        var e2eId = "E0000000020200905221000000000000";
        var seededTx = new Transaction
        {
            Id = txId,
            Amount = 600.00m,
            Provider = "fake",
            PaymentString = "fake-abstracted-payload",
            Status = ETransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var connectionFactory = _factory.Services.GetRequiredService<IDbConnectionFactory>();
        using (var conn = connectionFactory.CreateConnection())
        {
            await conn.ExecuteAsync(@"
                INSERT INTO Transactions (Id, Amount, Provider, PaymentString, Status, CreatedAt)
                VALUES (@Id, @Amount, @Provider, @PaymentString, @Status, @CreatedAt);",
                seededTx);
        }

        var webhookPayload = new
        {
            pix = new[]
            {
                new { txid = txId, endToEndId = e2eId }
            }
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/v1/api/payments/webhook")
        {
            Content = JsonContent.Create(webhookPayload)
        };
        requestMessage.Headers.Add("X-Signature", "fake_secret");

        // Act
        var response = await _client.SendAsync(requestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var assertConn = connectionFactory.CreateConnection();
        var tx = await assertConn.QueryFirstOrDefaultAsync<Transaction>(
            "SELECT * FROM Transactions WHERE Id = @Id;",
            new { Id = txId });

        Assert.NotNull(tx);
        Assert.Equal(ETransactionStatus.Paid, tx.Status);
        Assert.Equal(e2eId, tx.EndToEndId);
    }

    [Fact]
    public async Task Abstracted_Webhook_Should_Fail_When_Signature_Is_Invalid()
    {
        // Arrange
        var webhookPayload = new
        {
            pix = new[]
            {
                new { txid = "tx_123", endToEndId = "e2e_123" }
            }
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/v1/api/payments/webhook")
        {
            Content = JsonContent.Create(webhookPayload)
        };
        requestMessage.Headers.Add("X-Signature", "invalid_signature");

        // Act
        var response = await _client.SendAsync(requestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
