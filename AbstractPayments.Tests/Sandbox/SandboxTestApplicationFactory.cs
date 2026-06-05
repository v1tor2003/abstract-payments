namespace AbstractPayments.Tests.Sandbox;

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AbstractPayments.Sandbox.Http;
using AbstractPayments.Sandbox.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Custom HttpMessageHandler stub for intercepting and mocking external HTTP gateway calls in tests.
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    public Func<HttpRequestMessage, HttpResponseMessage> Handler { get; set; } = 
        req => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = Handler(request);
        var content = response.Content != null ? await response.Content.ReadAsStringAsync() : "null";
        Console.WriteLine($"[TEST MOCK HTTP] Request: {request.Method} {request.RequestUri}");
        Console.WriteLine($"[TEST MOCK HTTP] Response Content: {content}");
        return response;
    }
}

/// <summary>
/// Custom application factory providing unique isolated SQLite databases and mocked HttpClient handlers for each test execution.
/// </summary>
public class SandboxTestApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Gets the unique SQLite file path for this test instance.
    /// </summary>
    public string DbFile { get; } = $"test_{Guid.NewGuid():N}.db";

    /// <summary>
    /// Mock HTTP handler to intercept gateway API calls.
    /// </summary>
    public MockHttpMessageHandler MockHttpHandler { get; } = new MockHttpMessageHandler();

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDbConnectionFactory));
            if (dbDescriptor != null)
            {
                services.Remove(dbDescriptor);
            }

            services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory($"Data Source={DbFile}"));

            // Inject the mock handler for ApiClient
            services.AddHttpClient<ApiClient>(client =>
            {
                client.BaseAddress = new Uri("http://localhost");
            })
            .ConfigurePrimaryHttpMessageHandler(() => MockHttpHandler);

            // Configure global static MercadoPago SDK HttpClient to route through our MockHttpMessageHandler
            var mockSdkClient = new HttpClient(MockHttpHandler);
            MercadoPago.Config.MercadoPagoConfig.HttpClient = new MercadoPago.Http.DefaultHttpClient(mockSdkClient);
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
