namespace AbstractPayments.Core.Exceptions;

using System;

/// <summary>
/// Exception thrown when the resolved payment gateway name exists but does not implement the requested specialized capability interface type.
/// </summary>
public class GatewayTypeMismatchException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayTypeMismatchException"/> class.
    /// </summary>
    /// <param name="name">The registered gateway name.</param>
    /// <param name="expected">The requested capability interface contract.</param>
    /// <param name="actual">The actual concrete gateway type registered in DI.</param>
    public GatewayTypeMismatchException(string name, Type expected, Type actual)
        : base($"Gateway '{name}' is registered as type '{actual.FullName}' but was requested as type '{expected.FullName}'.")
    {}
}
