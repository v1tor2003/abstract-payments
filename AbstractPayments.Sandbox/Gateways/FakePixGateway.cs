namespace AbstractPayments.Sandbox.Gateways;

using System;
using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions;

/// <summary>
/// Framework-compliant implementation of the Pix capability utilizing standard AbstractPayments interfaces.
/// </summary>
public class FakePixGateway : IPixGateway
{
    /// <inheritdoc />
    public string Name => "fake";

    /// <inheritdoc />
    public Task<string> GeneratePaymentAsync()
    {
        return Task.FromResult("fake-abstract-qrcode-success-xyz");
    }

    /// <inheritdoc />
    public Task<string> GetRefundAsync()
    {
        return Task.FromResult("fake-abstract-refund-success-xyz");
    }
}
