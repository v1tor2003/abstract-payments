# Minimal API Technical Templates

## 1. Assembly Scanning Interface (`IEndpoint`)

To maintain vertical slice isolation, each implementation of `IEndpoint` must contain exactly one Minimal API definition.

```csharp
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        ServiceDescriptor serviceDescriptors = assembly.DefinedTypes
           .Where(type => type is { IsAbstract: false, IsInterface: false } && 
                           type.IsAssignableTo(typeof(IEndpoint)))
           .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
           .ToArray();

        services.TryAddEnumerable(serviceDescriptors);
        return services;
    }

    public static IApplicationBuilder MapEndpoints(this WebApplication app, RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpoint> endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        IEndpointRouteBuilder builder = routeGroupBuilder is null? app : routeGroupBuilder;

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }
}

```

---

## 2. Enterprise Global Exception Handler (RFC 7807)

Handles content negotiation fallbacks when the client's Accept header is incompatible with the default problem details JSON writer.

```csharp
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled system execution fault: {Message}", exception.Message);

        var (statusCode, title) = exception switch
        {
            ArgumentException or InvalidOperationException => (StatusCodes.Status400BadRequest, "Bad Request"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized Access"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatus.es/{statusCode}",
            Detail = exception.Message,
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        };

        var didWrite = await problemDetailsService.TryWriteAsync(context);

        if (!didWrite)
        {
            // Defensive fallback when client's Accept header is incompatible
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        }

        return true;
    }
}

```

---

## 3. Resilient Outbound HTTP Client

Registers Polly retry mechanisms, timeout strategies, and disables retries on non-idempotent unsafe HTTP verbs.

```csharp
builder.Services.AddHttpClient("ResilientService", client =>
{
    client.BaseAddress = new Uri("https://api.external.com");
})
.AddStandardResilienceHandler(options =>
{
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
    options.CircuitBreaker.FailureRatio = 0.15; // Open circuit if 15% of requests fail
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(45);
    options.CircuitBreaker.MinimumThroughput = 50;

    // Safety: Disable retries for unsafe HTTP verbs to avoid duplicate mutations
    options.Retry.DisableForUnsafeHttpMethods();
});

```