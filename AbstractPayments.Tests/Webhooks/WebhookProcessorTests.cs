namespace AbstractPayments.Tests.Webhooks;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbstractPayments.Core.Abstractions.Webhooks;
using AbstractPayments.Core.Abstractions;
using AbstractPayments.Core.Exceptions;
using AbstractPayments.Core.Extensions;
using AbstractPayments.Core.Extensions.Webhooks;
using AbstractPayments.Core.Models.Webhooks;
using AbstractPayments.Core.Processors.Webhooks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

public class WebhookProcessorTests
{
    private class DummySignatureValidator : IWebhookSignatureValidator
    {
        public bool ShouldValidate { get; set; } = true;
        public Task<bool> ValidateAsync(WebhookContext context) => Task.FromResult(ShouldValidate);
    }

    private class DummyEventConverter : IWebhookEventConverter
    {
        public WebhookEvent EventToReturn { get; set; } = new WebhookEvent("evt_123", "mercadopago", DateTime.UtcNow, "{}");

        public DummyEventConverter()
        {
        }

        public DummyEventConverter(WebhookEvent eventToReturn)
        {
            EventToReturn = eventToReturn;
        }

        public Task<WebhookEvent> ConvertAsync(WebhookContext context) => Task.FromResult(EventToReturn);
    }

    private class DummyEventHandler : IWebhookEventHandler
    {
        public int InvocationCount { get; private set; }
        public int FailCount { get; set; } = 0;

        public Task HandleAsync(WebhookEvent @event)
        {
            InvocationCount++;
            if (InvocationCount <= FailCount)
            {
                throw new Exception("Transient failure");
            }
            return Task.CompletedTask;
        }
    }

    private class DummyPixGateway : IPixGateway
    {
        public string Name => "testprovider";
        public Task<string> GeneratePaymentAsync() => Task.FromResult("qr-code");
        public Task<string> GetRefundAsync() => Task.FromResult("refund");
    }

    [Fact]
    public void DI_Fluent_Configuration_Should_Register_All_Keyed_Services()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAbstractPayments()
            .AddPayments(opts =>
            {
                opts.Pix.AddProvider<DummyPixGateway>("testprovider");
            })
            .AddEventsHandling(opts =>
            {
                opts.Endpoint = "/v1/api/payments/webhook";
                opts.SignatureValidators.UseStrategy<DummySignatureValidator>("testprovider");
                opts.Converters.AddConverter<DummyEventConverter>("testprovider");
                opts.Handlers.AddHandler<DummyEventHandler>("testprovider");
                opts.RetryCount = 3;
            });

        var provider = services.BuildServiceProvider();

        // Act & Assert Keyed Services resolution
        var validator = provider.GetKeyedService<IWebhookSignatureValidator>("validator:testprovider");
        var converter = provider.GetKeyedService<IWebhookEventConverter>("parser:testprovider");
        var handler = provider.GetKeyedService<IWebhookEventHandler>("handler:testprovider");
        var processor = provider.GetService<IWebhookProcessor>();
        var options = provider.GetService<IOptions<WebhookOptions>>();

        Assert.NotNull(validator);
        Assert.NotNull(converter);
        Assert.NotNull(handler);
        Assert.NotNull(processor);
        Assert.NotNull(options);

        Assert.Equal("/v1/api/payments/webhook", options.Value.Endpoint);
        Assert.Equal(3, options.Value.RetryCount);
    }

    [Fact]
    public async Task ProcessAsync_Should_Throw_WebhookSignatureValidationException_When_Signature_Is_Invalid()
    {
        // Arrange
        var services = new ServiceCollection();
        var dummyEvent = new WebhookEvent("evt_123", "testprovider", DateTime.UtcNow, "{}");

        services.AddAbstractPayments()
            .AddEventsHandling(opts =>
            {
                opts.SignatureValidators.UseStrategy<DummySignatureValidator>("testprovider", v => v.ShouldValidate = false);
            });

        services.AddKeyedSingleton<IWebhookEventConverter>("parser:testprovider", new DummyEventConverter(dummyEvent));
        services.AddKeyedSingleton<IWebhookEventHandler>("handler:testprovider", new DummyEventHandler());

        var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IWebhookProcessor>();
        var context = new WebhookContext("testprovider", "{}", new Dictionary<string, string>());

        // Act & Assert
        await Assert.ThrowsAsync<WebhookSignatureValidationException>(() => processor.ProcessAsync(context));
    }

    [Fact]
    public async Task ProcessAsync_Should_Throw_ProviderConfigurationException_When_Component_Is_Missing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAbstractPayments()
            .AddEventsHandling(opts =>
            {
                opts.SignatureValidators.UseStrategy<DummySignatureValidator>("testprovider");
            });

        var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IWebhookProcessor>();
        var context = new WebhookContext("testprovider", "{}", new Dictionary<string, string>());

        // Act & Assert
        await Assert.ThrowsAsync<ProviderConfigurationException>(() => processor.ProcessAsync(context));
    }

    [Fact]
    public async Task ProcessAsync_Should_Retry_And_Succeed_On_Transient_Failure()
    {
        // Arrange
        var services = new ServiceCollection();
        var handler = new DummyEventHandler { FailCount = 2 }; // Fails twice, succeeds on third attempt (attempt 2)
        var dummyEvent = new WebhookEvent("evt_123", "testprovider", DateTime.UtcNow, "{}");

        services.AddAbstractPayments()
            .AddEventsHandling(opts =>
            {
                opts.SignatureValidators.UseStrategy<DummySignatureValidator>("testprovider");
                opts.RetryCount = 3;
            });
        
        services.AddKeyedSingleton<IWebhookEventConverter>("parser:testprovider", new DummyEventConverter(dummyEvent));
        services.AddKeyedSingleton<IWebhookEventHandler>("handler:testprovider", handler);

        var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IWebhookProcessor>();
        var context = new WebhookContext("testprovider", "{}", new Dictionary<string, string>());

        // Act
        await processor.ProcessAsync(context);

        // Assert
        Assert.Equal(3, handler.InvocationCount); // 2 failures + 1 success = 3 invocations
    }

    [Fact]
    public async Task ProcessAsync_Should_Throw_WebhookProcessingException_When_Retries_Exhausted()
    {
        // Arrange
        var services = new ServiceCollection();
        var handler = new DummyEventHandler { FailCount = 5 }; // Exceeds retry count of 2
        var dummyEvent = new WebhookEvent("evt_123", "testprovider", DateTime.UtcNow, "{}");

        services.AddAbstractPayments()
            .AddEventsHandling(opts =>
            {
                opts.SignatureValidators.UseStrategy<DummySignatureValidator>("testprovider");
                opts.RetryCount = 2; // Will run: attempt 0 (fail), attempt 1 (fail), attempt 2 (fail) -> exhaust
            });
        
        services.AddKeyedSingleton<IWebhookEventConverter>("parser:testprovider", new DummyEventConverter(dummyEvent));
        services.AddKeyedSingleton<IWebhookEventHandler>("handler:testprovider", handler);

        var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IWebhookProcessor>();
        var context = new WebhookContext("testprovider", "{}", new Dictionary<string, string>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<WebhookProcessingException>(() => processor.ProcessAsync(context));
        Assert.NotNull(exception.InnerException);
        Assert.Equal("Transient failure", exception.InnerException.Message);
        Assert.Equal(3, handler.InvocationCount); // attempt 0, 1, 2
    }
}
