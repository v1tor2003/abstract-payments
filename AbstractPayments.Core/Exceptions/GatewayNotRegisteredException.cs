namespace AbstractPayments.Core.Exceptions;

using System;

/// <summary>
/// Exception thrown when attempting to resolve a payment gateway that has not been registered in the DI container.
/// </summary>
public class GatewayNotRegisteredException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayNotRegisteredException"/> class.
    /// </summary>
    /// <param name="name">The name of the unregistered gateway.</param>
    public GatewayNotRegisteredException(string name)
        : base($"Payment gateway with name '{name}' is not registered in the DI container.")
    {}
}
