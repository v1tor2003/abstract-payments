namespace AbstractPayments.Core.Abstractions;

using System;

/// <summary>
/// Specifies the unique prefix associated with a specialized payment capability interface.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public class GatewayCapabilityAttribute : Attribute
{
    /// <summary>
    /// Gets the unique string prefix (e.g. "Pix", "Card").
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayCapabilityAttribute"/> class.
    /// </summary>
    /// <param name="prefix">The dynamic resolution capability prefix.</param>
    public GatewayCapabilityAttribute(string prefix)
    {
        Prefix = prefix;
    }
}
