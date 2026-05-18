# Abstract.Payments.Core (for .NET 10)

## 1. Overview & Objectives

This project is a **.NET Class Library (C# 14)** providing a unified abstraction layer for integrating multiple payment gateways. It is designed to be distributed as a NuGet package for ASP.NET Core applications.

### 1.1 The Module-Plugin Hierarchy
The framework follows a strict hierarchical structure to ensure scalability and maintainability:

1.  **Module**: A functional domain or business area (e.g., `Payment`, `Ledger`, `KYC`).
2.  **Plugin**: A specific capability or feature within a module (e.g., `Pix`, `CreditCard`, `BankSlip`).
3.  **Implementation / Adapter**: The concrete logic that fulfills a plugin's contract, e.g:
    - For the **Payment Module**, the **Pix Plugin** can be implemented with one or more **Gateways** that supports it by the developer.
    - For the **Webhook Module**, the **Processor Plugin** can be implemented with queues and memory.

### 1.2 Architectural Goals
*   **Zero Provider Coupling:** Business logic must never depend on concrete gateway SDKs.
*   **Granular Capabilities (No God Classes):** Instead of monolithic interfaces, we break down responsibilities into specialized plugins.
*   **Developer-Driven Implementation:** The framework provides the "blueprints" (interfaces), and developers implement the "machinery" (gateways, processors, etc.) for their specific needs.
*   **Multi-Gateway Orchestration:** Support for fallbacks, routing, and concurrent registrations.

### 1.3 Initial Scope (MVP)
*   ✅ **Payment Module**: Currently supporting the `Pix` plugin.
*   ✅ **Webhook Module**: Standardized event processing and signature validation.
*   ❌ **Planned Modules**: Ledger, KYC, and additional Payment Method plugins (Credit Card, Bank Slip).

---

## 2. Architecture Philosophy: Why Plugins?

### 2.1 Avoiding the "God Class" Trap
A common pitfall in financial integrations is creating a single interface (like a "God Gateway") that attempts to handle every possible operation (Payments, Refunds, KYC, Ledger, etc.). This leads to:
*   **Bloated Interfaces:** Implementations are forced to throw `NotImplementedException` for methods they don't support.
*   **Tight Coupling:** Changes in one area (e.g., adding a new KYC field) force updates across all implementations.

### 2.2 The "Subset of Capabilities" Approach
By breaking the architecture into **Modules** and **Plugins**, we allow:
*   **Specialized Implementation:** A developer implementing a Pix gateway only needs to worry about the `IPixPaymentGateway` interface.
*   **Clean Separation:** Ledger logic stays in the Ledger module, and Payment logic stays in the Payment module.
*   **Scalability:** New payment methods or business domains can be added as new plugins without breaking existing ones.

---

## 2. Core Architectural Principles

* **Dependency Injection (DI) Native:** Built entirely around `Microsoft.Extensions.DependencyInjection`.
* **SOLID & Clean Architecture:** Interfaces belong to the Core; concrete SDKs belong to Infrastructure adapters.
* **Composition Over Inheritance:** Extending behavior via wrapped services rather than deep class hierarchies.
* **Strategy + Factory Patterns:** For runtime gateway resolution and automated converter injection.

---

## 3. Functional Requirements & Plugin Contracts

### 3.1 Base Gateway Identity
Gateways must be stateless and descriptive. The base interface simply identifies the provider and its capabilities.

```csharp
public interface IPaymentGateway
{
    string Name { get; }
    bool Supports(PaymentMethod method);
}
```

### 3.2 The Payment Module (Pix Plugin Example)
Instead of forcing all payment types into one massive interface, capabilities are treated as isolated plugins within a module. For example, the `Payment` module contains a `Pix` plugin contract. Provider adapters will implement this specific interface.

```csharp
public interface IPixPaymentGateway : IPaymentGateway
{
    Task<PixPaymentResult> GenerateAsync(PixPaymentRequest request);
    Task<PixPaymentStatusResult> GetStatusAsync(string externalId);
    Task<PixRefundResult> RefundAsync(PixRefundRequest request);
}
```

### 3.3 Automated Webhook Processing
Webhook processing handles incoming events from providers. It uses a Template Method for execution and an automated strategy for resolving data converters, avoiding hardcoded or manual maps.

```csharp
// Signature Validation
public interface IWebhookSignatureValidator
{
    bool Validate(WebhookRequest request);
}

// Automated Parser Strategy (Resolves the right converter dynamically)
public interface IWebhookEventParserStrategy
{
    IWebhookEventParser GetParser(string providerName); 
}

public interface IWebhookEventParser
{
    WebhookEvent Parse(WebhookRequest request);
}

// The Execution Template
public abstract class BaseWebhookProcessor : IWebhookProcessor
{
    private readonly IWebhookSignatureValidator _validator;
    private readonly IWebhookEventParserStrategy _parserStrategy;

    protected BaseWebhookProcessor(
        IWebhookSignatureValidator validator, 
        IWebhookEventParserStrategy parserStrategy)
    {
        _validator = validator;
        _parserStrategy = parserStrategy;
    }

    public async Task ProcessAsync(WebhookRequest request)
    {
        if (!_validator.Validate(request))
            throw new UnauthorizedAccessException("Invalid webhook signature.");

        var parser = _parserStrategy.GetParser(request.ProviderName);
        var evt = parser.Parse(request);

        await HandleAsync(evt);
    }

    // Default behavior is to update the Pix status. 
    // Developers can override this to add custom ledger/notification logic.
    protected abstract Task HandleAsync(WebhookEvent evt);
}
```

---

## 4. Multi-Gateway Orchestration & Resolution

To support multiple registered gateways (e.g., Mercado Pago and Stripe), the framework provides a factory for dynamic resolution and an orchestrator for routing/fallback logic.

```csharp
// Resolves the specific gateway at runtime
public interface IPaymentGatewayFactory
{
    T Get<T>(string name) where T : IPaymentGateway;
}

// Handles execution logic (e.g., trying a secondary gateway if the primary fails)
public interface IPixPaymentOrchestrator
{
    Task<PixPaymentResult> ExecuteAsync(PixPaymentRequest request);
}
```

---

## 5. Domain Models (Data Transfer Objects)

Inputs should not contain state tracking, and outputs must standardize provider differences (e.g., unifying Mercado Pago and Stripe error codes).

```csharp
public class PixPaymentRequest
{
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string PayerDocument { get; set; }
    public Dictionary<string, object>? Metadata { get; set; } // Provider-specific overrides
}

public class PixPaymentResult
{
    public bool Success { get; set; }
    public string ExternalId { get; set; }
    public string QrCode { get; set; }
    public string QrCodeImage { get; set; }
    public PaymentError? Error { get; set; }
}

public class PaymentError
{
    public string Code { get; set; }
    public string Message { get; set; }
    public string? ProviderError { get; set; } // Raw error from the SDK for debugging
}
```

---

## 6. Configuration API (Minimal API Integration)

The framework is registered using the Options Pattern. This allows the consumer application to seamlessly register the framework, configure webhook strategies, and plug in specific gateways.

```csharp
// Program.cs
builder.Services.AddAbstractPayments(options =>
{
    options.EnableWebhooks = true;
    
    // Automatically injects the requested signature validation strategy
    options.Webhooks.UseSignatureValidator<HeaderSignatureValidator>();

    // Registering Mercado Pago as a Pix Plugin
    options.AddGateway<IPixPaymentGateway, MercadoPagoGateway>("mercadopago", cfg =>
    {
        cfg.ClientId = builder.Configuration["MercadoPago:ClientId"];
        cfg.ClientSecret = builder.Configuration["MercadoPago:ClientSecret"];
    });

    // Registering EfiPay as a secondary Pix Plugin
    options.AddGateway<IPixPaymentGateway, EfiPayGateway>("efipay", cfg =>
    {
        cfg.ApiKey = builder.Configuration["EfiPay:ApiKey"];
    });
});
```

---

## 7. Suggested Folder Structure

The directory structure reflects the Module-Plugin hierarchy, ensuring a clean separation of concerns.

```text
/Abstract.Payments.Core
  /Abstractions           // Base interfaces (IPaymentGateway, IPaymentGatewayFactory)
  /Models                 // Shared domain models (PaymentError)
  /Modules                // Grouped by business domain
    /Payment              // The Payment Module
      /Plugins            // Capabilities within the module
        /Pix              
          /Contracts      // IPixPaymentGateway, IPixPaymentOrchestrator
          /Models         // PixPaymentRequest, PixPaymentResult
        /CreditCard       // (Planned)
    /Webhooks             // The Webhook Module
      /Contracts          // IWebhookProcessor, IWebhookEventParserStrategy
      /Processors         // BaseWebhookProcessor
  /Extensions             // DI Registration
    ServiceCollectionExtensions.cs
    PaymentFrameworkOptions.cs
```

---

## 8. Example Usage (Consumer App)

Because the architecture leverages DI and automated resolution, the implementation in a .NET 10 Minimal API is incredibly lightweight.

```csharp
app.MapPost("/api/checkout/pix", async (
    PixPaymentRequest request,
    IPaymentGatewayFactory factory) =>
{
    // Retrieve the desired gateway dynamically
    var gateway = factory.Get<IPixPaymentGateway>("mercadopago");

    var result = await gateway.GenerateAsync(request);

    return result.Success ? Results.Ok(result) : Results.BadRequest(result.Error);
});
```