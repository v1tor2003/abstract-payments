# Golden Examples: Library Registrations & OpenAPI Integration

Use these programmatic patterns to write composable extension entry points.

## 1. Options Registration with Startup Validation & Post-Configuration

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace Enterprise.Diagnostics.Core
{
    public class DiagnosticsOptions
    {
        public const string DefaultSection = "DiagnosticsCore";

       
        public string ServiceName { get; set; } = string.Empty;

       
        public int MaxBatchSize { get; set; } = 10;

        public bool EnableDetailedTelemetry { get; set; } = false;
    }

    public interface IDiagnosticsService
    {
        void LogDiagnostics(string message);
    }

    public class DiagnosticsService : IDiagnosticsService
    {
        private readonly DiagnosticsOptions _options;

        public DiagnosticsService(IOptions<DiagnosticsOptions> options)
        {
            _options = options.Value;
        }

        public void LogDiagnostics(string message)
        {
            Console.WriteLine($" Batch Size={_options.MaxBatchSize}: {message}");
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEnterpriseDiagnostics(
            this IServiceCollection services, 
            string configSectionPath = DiagnosticsOptions.DefaultSection)
        {
            // Set up strongly typed Options with startup validation
            services.AddOptions<DiagnosticsOptions>()
              .BindConfiguration(configSectionPath)
              .ValidateDataAnnotations()
              .ValidateOnStart();

            // Register standard service implementation
            services.AddScoped<IDiagnosticsService, DiagnosticsService>();

            // Post-configure step to enforce enterprise policies globally
            services.PostConfigure<DiagnosticsOptions>(options =>
            {
                if (string.IsNullOrWhiteSpace(options.ServiceName))
                {
                    options.ServiceName = "Enterprise-Fallback-Service";
                }
            });

            // Native OpenAPI Metadata Customization
            services.ConfigureOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info.Title = "Enterprise Core API Services";
                    document.Info.Description = "Auto-generated schemas for enterprise diagnostics.";
                    document.Info.Contact = new OpenApiContact
                    {
                        Name = "Enterprise Architecture Board",
                        Email = "architecture@enterprise.com",
                        Url = new Uri("https://architecture.enterprise.com")
                    };
                    return Task.CompletedTask;
                });
            });

            return services;
        }
    }
}

```

## 2. Options Pattern Lifetime Reference

Use this model matrix to choose the appropriate dynamic configuration option for consuming services:

| Model | Lifetime | Reload Capability | Best Use Case |
| --- | --- | --- | --- |
| `IOptions<T>` | Singleton | Read-once during application startup; no reloads. | Static settings or simple utility providers. |
| `IOptionsSnapshot<T>` | Scoped | Recomputed per HTTP request; supports named options. | Request-bound environments needing quick recalculation. |
| `IOptionsMonitor<T>` | Singleton | Dynamic, real-time changes via configuration tokens. | Distributed enterprise microservices. |
