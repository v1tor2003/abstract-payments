# AbstractPayments.Tests

## 1. Overview

This project contains the test suite for the **AbstractPayments** framework. It is built using **xUnit** and targets **.NET 10**.

The testing strategy is divided into two main pillars to ensure the framework's reliability and the correctness of its plugin-based architecture:

1.  **Unit Testing**: Focused on isolating core logic, orchestrators, and webhook processing using mocks.
2.  **Integration Testing**: Focused on verifying Dependency Injection (DI) registrations, gateway resolution via the factory, and (optionally) concrete gateway connectivity.

---

## 2. Unit Testing Strategy

Unit tests should isolate the component under test by mocking its dependencies. We recommend using libraries like **Moq** or **NSubstitute**.

### 2.1 Testing Abstractions & Gateways
Since `IPaymentGateway` and its derivatives (e.g., `IPixPaymentGateway`) are the core of the plugin system, unit tests for consumer logic or orchestrators should mock these interfaces.

```csharp
[Fact]
public async Task PixOrchestrator_ShouldFallback_WhenPrimaryGatewayFails()
{
    // Arrange
    var primaryMock = new Mock<IPixPaymentGateway>();
    primaryMock.Setup(g => g.GenerateAsync(It.IsAny<PixPaymentRequest>()))
               .ReturnsAsync(new PixPaymentResult { Success = false });

    var secondaryMock = new Mock<IPixPaymentGateway>();
    secondaryMock.Setup(g => g.GenerateAsync(It.IsAny<PixPaymentRequest>()))
                 .ReturnsAsync(new PixPaymentResult { Success = true, ExternalId = "success-123" });

    // Orchestrator logic would use these mocks
    var orchestrator = new PixPaymentOrchestrator(new[] { primaryMock.Object, secondaryMock.Object });

    // Act
    var result = await orchestrator.ExecuteAsync(new PixPaymentRequest());

    // Assert
    Assert.True(result.Success);
    Assert.Equal("success-123", result.ExternalId);
}
```

### 2.2 Testing Webhook Processors
The `BaseWebhookProcessor` uses a **Template Method** pattern. Tests should verify that:
1.  The signature validator is called.
2.  The correct parser is resolved via the strategy.
3.  The `HandleAsync` method (implemented in the concrete processor) receives the parsed event.

```csharp
public class TestWebhookProcessor : BaseWebhookProcessor
{
    public WebhookEvent? CapturedEvent { get; private set; }

    public TestWebhookProcessor(IWebhookSignatureValidator v, IWebhookEventParserStrategy s) : base(v, s) { }

    protected override Task HandleAsync(WebhookEvent evt)
    {
        CapturedEvent = evt;
        return Task.CompletedTask;
    }
}
```

---

## 3. Integration Testing Strategy

Integration tests ensure that the framework's components work together correctly within the ASP.NET Core ecosystem.

### 3.1 Verifying DI Registrations
Use `ServiceCollection` to verify that the `AddAbstractPayments` extension method correctly registers the required services and options.

```csharp
[Fact]
public void AddAbstractPayments_ShouldRegisterCoreServices()
{
    var services = new ServiceCollection();
    services.AddAbstractPayments(options => {
        options.AddGateway<IPixPaymentGateway, MockGateway>("mock");
    });

    var provider = services.BuildServiceProvider();

    Assert.NotNull(provider.GetService<IPaymentGatewayFactory>());
    Assert.NotNull(provider.GetService<IPixPaymentOrchestrator>());
}
```

### 3.2 Testing the Gateway Factory
The `IPaymentGatewayFactory` is responsible for runtime resolution. Integration tests should verify that gateways registered by name can be retrieved correctly.

```csharp
[Fact]
public void GatewayFactory_ShouldResolveRegisteredGatewaysByName()
{
    // Setup DI with multiple gateways
    var factory = serviceProvider.GetRequiredService<IPaymentGatewayFactory>();

    var mp = factory.Get<IPixPaymentGateway>("mercadopago");
    var efi = factory.Get<IPixPaymentGateway>("efipay");

    Assert.IsType<MercadoPagoGateway>(mp);
    Assert.IsType<EfiPayGateway>(efi);
}
```

### 3.3 (Future) Infrastructure Adapters
When concrete infrastructure projects are added (e.g., `AbstractPayments.Infrastructure.Stripe`), integration tests should:
*   Use `MockHttp` or similar to simulate provider API responses.
*   Verify that the adapter correctly maps raw provider JSON to the framework's `Models`.

---

## 4. Running Tests

### 4.1 Via CLI
Run all tests in the solution:
```bash
dotnet test
```

Run with code coverage:
```bash
dotnet test /p:CollectCoverage=true
```

### 4.2 Via IDE
*   **Visual Studio**: Use **Test Explorer** (Ctrl+E, T).
*   **VS Code**: Use the **C# Dev Kit** or the **Test Explorer** extension.

---

## 5. Folder Structure for Tests

```text
/AbstractPayments.Tests
  /Unit
    /Modules
      /Pix               // Orchestrator and Gateway logic tests
    /Webhooks            // Processor and Strategy tests
  /Integration
    /Configuration       // DI registration tests
    /Factories           // Factory resolution tests
  /Mocks                 // Shared mock implementations (if any)
```
