namespace AbstractPayments.Core.Exceptions;

using System;

/// <summary>
/// Exception thrown when a requested webhook processing service (validator, parser, or handler) is missing or misconfigured in the DI container.
/// </summary>
public class ProviderConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderConfigurationException"/> class.
    /// </summary>
    /// <param name="provider">The provider name.</param>
    /// <param name="component">The missing or misconfigured component name.</param>
    public ProviderConfigurationException(string provider, string component)
        : base($"Webhook component '{component}' for provider '{provider}' is not registered or is misconfigured in the DI container.")
    {}
}
