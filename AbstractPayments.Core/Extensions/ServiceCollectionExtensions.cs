namespace AbstractPayments.Core.Extensions;

using System;
using AbstractPayments.Core.Abstractions;
using AbstractPayments.Core.Abstractions.Webhooks;
using AbstractPayments.Core.Extensions.Webhooks;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Service collection extension bootstrapping points for registering the AbstractPayments framework natively.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core AbstractPayments framework services, strategy factory, and startup validation configurations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">The configuration delegate for framework settings.</param>
    /// <returns>A fluent framework configuration builder.</returns>
    public static IAbstractPaymentsBuilder AddAbstractPayments(
        this IServiceCollection services,
        Action<PaymentFrameworkOptions>? configure = null)
    {
        var optionsBuilder = services.AddOptions<PaymentFrameworkOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        if (configure != null)
        {
            services.Configure(configure);
        }

        services.AddSingleton<IPaymentGatewayFactory, PaymentGatewayFactory>();

        return new AbstractPaymentsBuilder(services);
    }

    /// <summary>
    /// Scaffolds and configures the Payments Module scope, allowing fluent gateway capability plugin registrations.
    /// </summary>
    /// <param name="builder">The framework builder.</param>
    /// <param name="configure">The payments module builder delegate.</param>
    /// <returns>The original framework builder for fluent method-chaining.</returns>
    public static IAbstractPaymentsBuilder AddPaymentModule(
        this IAbstractPaymentsBuilder builder,
        Action<IPaymentModuleBuilder> configure)
    {
        var moduleBuilder = new PaymentModuleBuilder(builder.Services);
        configure(moduleBuilder);
        return builder;
    }

    /// <summary>
    /// Fluent builder extension to configure the Payments module plugins (Pix, CreditCard).
    /// </summary>
    /// <param name="builder">The framework builder.</param>
    /// <param name="configure">The payments sub-builder callback.</param>
    /// <returns>The original framework builder for fluent method-chaining.</returns>
    public static IAbstractPaymentsBuilder AddPayments(
        this IAbstractPaymentsBuilder builder,
        Action<IPaymentsModuleBuilder> configure)
    {
        var paymentsBuilder = new PaymentsModuleBuilder(builder.Services);
        configure(paymentsBuilder);
        return builder;
    }

    /// <summary>
    /// Fluent builder extension to configure Webhooks event handling, signature validation, and retry counts.
    /// </summary>
    /// <param name="builder">The framework builder.</param>
    /// <param name="configure">The events handling sub-builder callback.</param>
    /// <returns>The original framework builder for fluent method-chaining.</returns>
    public static IAbstractPaymentsBuilder AddEventsHandling(
        this IAbstractPaymentsBuilder builder,
        Action<IEventsHandlingBuilder> configure)
    {
        var eventsBuilder = new EventsHandlingBuilder(builder.Services);
        configure(eventsBuilder);

        builder.Services.Configure<WebhookOptions>(options =>
        {
            options.Endpoint = eventsBuilder.Endpoint;
            options.RetryCount = eventsBuilder.RetryCount;
        });

        builder.Services.AddScoped<IWebhookProcessor, Processors.Webhooks.WebhookProcessor>();

        return builder;
    }
}
