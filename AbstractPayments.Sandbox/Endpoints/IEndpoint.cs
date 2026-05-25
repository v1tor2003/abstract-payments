namespace AbstractPayments.Sandbox.Endpoints;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Defines a contract for individual vertical slice minimal API endpoints.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Maps the endpoint routes into the application route builder.
    /// </summary>
    void MapEndpoint(IEndpointRouteBuilder app);
}

/// <summary>
/// Extensions for automatic assembly-scanning registration of all endpoints.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Scans the assembly and registers all non-abstract implementations of IEndpoint as transient services.
    /// </summary>
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        var serviceDescriptors = assembly.DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } && 
                            type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);
        return services;
    }

    /// <summary>
    /// Resolves all registered IEndpoint services and invokes their MapEndpoint route mapping logic.
    /// </summary>
    public static IApplicationBuilder MapEndpoints(this WebApplication app, RouteGroupBuilder? routeGroupBuilder = null)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }
}
