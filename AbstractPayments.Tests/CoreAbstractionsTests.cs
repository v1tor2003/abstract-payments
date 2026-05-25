namespace AbstractPayments.Tests;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using AbstractPayments.Core.Abstractions;
using AbstractPayments.Core.Abstractions.Payments;
using AbstractPayments.Core.Models;
using AbstractPayments.Core.Exceptions;
using AbstractPayments.Core.Extensions;
using Xunit;

public class CoreAbstractionsTests
{
    [GatewayCapability("Pix")]
    private interface IPixDummyGateway : IPaymentGateway
    {
        Task<string> GeneratePaymentAsync();
    }

    [GatewayCapability("Card")]
    private interface ICardDummyGateway : IPaymentGateway
    {
        Task<string> PayAsync();
    }

    // Contract lacking GatewayCapabilityAttribute
    private interface IUnattributedDummyGateway : IPaymentGateway {}

    private class PixMercadoPago : IPixDummyGateway
    {
        public string Name => "mercadopago";
        public Task<string> GeneratePaymentAsync() => Task.FromResult("pix-qr-code");
    }

    private class CardMercadoPago : ICardDummyGateway
    {
        public string Name => "mercadopago";
        public Task<string> PayAsync() => Task.FromResult("card-charge-success");
    }

    private class UnattributedGateway : IUnattributedDummyGateway
    {
        public string Name => "mercadopago";
    }

    [Fact]
    public async Task Factory_Should_Resolve_Correct_Gateway_By_Capability_Key()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAbstractPayments()
            .AddPaymentModule(payment =>
            {
                // Register both capabilities under the same provider name "mercadopago" without collision
                payment.AddProvider<IPixDummyGateway, PixMercadoPago>("mercadopago")
                       .AddProvider<ICardDummyGateway, CardMercadoPago>("mercadopago");
            });

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IPaymentGatewayFactory>();

        // Act
        var pix = factory.Get<IPixDummyGateway>("mercadopago");
        var card = factory.Get<ICardDummyGateway>("mercadopago");

        // Assert
        Assert.NotNull(pix);
        Assert.Equal("mercadopago", pix.Name);
        Assert.Equal("pix-qr-code", await pix.GeneratePaymentAsync());

        Assert.NotNull(card);
        Assert.Equal("mercadopago", card.Name);
        Assert.Equal("card-charge-success", await card.PayAsync());
    }

    [Fact]
    public void Factory_Should_Throw_GatewayNotRegisteredException_When_Gateway_Not_Found()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAbstractPayments();

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IPaymentGatewayFactory>();

        // Act & Assert
        // Should look for "Pix:non-existent"
        var exception = Assert.Throws<GatewayNotRegisteredException>(() => factory.Get<IPixDummyGateway>("non-existent"));
        Assert.Contains("Pix:non-existent", exception.Message);
    }

    [Fact]
    public void Factory_Should_Throw_InvalidOperationException_When_Contract_Is_Not_Attributed()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = services.AddAbstractPayments();

        // Act & Assert Registration throws
        Assert.Throws<InvalidOperationException>(() =>
        {
            builder.AddPaymentModule(payment =>
            {
                payment.AddProvider<IUnattributedDummyGateway, UnattributedGateway>("mercadopago");
            });
        });

        // Act & Assert Resolution throws
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IPaymentGatewayFactory>();
        
        var exception = Assert.Throws<InvalidOperationException>(() => factory.Get<IUnattributedDummyGateway>("mercadopago"));
        Assert.Contains("GatewayCapabilityAttribute", exception.Message);
    }

    [Fact]
    public void Options_Validation_Should_Fail_When_Webhooks_Enabled_Without_Secret()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAbstractPayments(options =>
        {
            options.EnableWebhooks = true;
            options.WebhookSecret = null;
        });

        var provider = services.BuildServiceProvider();

        // Act & Assert
        var options = provider.GetRequiredService<IOptions<PaymentFrameworkOptions>>();
        Assert.Throws<OptionsValidationException>(() => _ = options.Value);
    }

    [Fact]
    public void Options_Validation_Should_Pass_When_Webhooks_Disabled_Without_Secret()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAbstractPayments(options =>
        {
            options.EnableWebhooks = false;
            options.WebhookSecret = null;
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<PaymentFrameworkOptions>>();

        // Act
        var value = options.Value;

        // Assert
        Assert.NotNull(value);
        Assert.False(value.EnableWebhooks);
    }
}
