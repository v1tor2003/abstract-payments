namespace AbstractPayments.Core.Extensions.Webhooks;

using System;
using AbstractPayments.Core.Abstractions.Webhooks;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Sub-builder contract for registering webhook signature validator strategies.
/// </summary>
public interface ISignatureValidatorsBuilder
{
    /// <summary>
    /// Gets the application service collection.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Registers a signature validator strategy for a specific provider.
    /// </summary>
    /// <typeparam name="TValidator">The concrete signature validator type.</typeparam>
    /// <param name="provider">The provider name.</param>
    /// <param name="configure">Optional customization callback to configure the validator strategy.</param>
    /// <returns>This builder for method chaining.</returns>
    ISignatureValidatorsBuilder UseStrategy<TValidator>(string provider, Action<TValidator>? configure = null)
        where TValidator : class, IWebhookSignatureValidator;
}

/// <summary>
/// Concrete internal implementation of the signature validators builder.
/// </summary>
internal class SignatureValidatorsBuilder : ISignatureValidatorsBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SignatureValidatorsBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public SignatureValidatorsBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <inheritdoc />
    public ISignatureValidatorsBuilder UseStrategy<TValidator>(string provider, Action<TValidator>? configure = null)
        where TValidator : class, IWebhookSignatureValidator
    {
        string serviceKey = $"validator:{provider}";

        if (configure != null)
        {
            Services.AddKeyedScoped<IWebhookSignatureValidator>(serviceKey, (sp, key) =>
            {
                var validator = ActivatorUtilities.CreateInstance<TValidator>(sp);
                configure(validator);
                return validator;
            });
        }
        else
        {
            Services.AddKeyedScoped<IWebhookSignatureValidator, TValidator>(serviceKey);
        }

        return this;
    }
}
