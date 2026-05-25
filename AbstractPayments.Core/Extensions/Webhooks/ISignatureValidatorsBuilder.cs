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
