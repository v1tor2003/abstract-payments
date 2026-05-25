# TASK-02: Webhooks Events Module and Unified DI Configuration Engine

## 1. Scrum User Story

* **As a** Payment Platform Consumer (Developer)
* **I want to** configure payment plugins and webhook event handling fluently using a single unified Dependency Injection builder
* **So that** I can centralize webhook signature validation, event payload conversion, and event processing for multiple providers without coupling domain logic to specific gateway webhooks.

## 2. Architectural & Codebase Context

* **Target Domain / Context:** Webhook Ingestion & Decoupled Event Processing
* **Clean Architecture Layer:** Application Abstractions & Infrastructure Adapters (Layer 2 & Layer 3)
* **Affected Codebase Files:**
  * `📁 AbstractPayments.Core/Extensions/ServiceCollectionExtensions.cs` -> Extend to support AddPayments and AddEventsHandling builders.
  * `📁 AbstractPayments.Core/Extensions/IAbstractPaymentsBuilder.cs` -> Add fluent chaining methods for modules.
  * `📁 AbstractPayments.Core/Extensions/IPaymentsModuleBuilder.cs` -> Add support for sub-builders.
  * `📁 AbstractPayments.Core/Abstractions/Webhooks/` -> **[NEW]** Interfaces and models for Webhooks.
    * `IWebhookSignatureValidator.cs`
    * `IWebhookEventConverter.cs`
    * `IWebhookEventHandler.cs`
    * `IWebhookProcessor.cs`
  * `📁 AbstractPayments.Core/Models/Webhooks/` -> **[NEW]** Models for Webhook processing.
    * `WebhookContext.cs` (Wraps incoming raw payload/headers/query/provider)
    * `WebhookEvent.cs` (Unified abstract domain event representation)
  * `📁 AbstractPayments.Core/Processors/Webhooks/` -> **[NEW]** Core processor execution template.
    * `WebhookProcessor.cs`
  * `📁 AbstractPayments.Tests/Webhooks/` -> **[NEW]** Comprehensive webhook orchestration tests.

### Current Codebase Reference

```csharp
// From AbstractPayments.Core/Abstractions/IPixGateway.cs
[GatewayCapability("Pix")]
public interface IPixGateway : IPaymentGateway
{
    /// <summary>
    /// Generates a Pix payment.
    /// </summary>
    Task<string> GeneratePaymentAsync();

    /// <summary>
    /// Gets a refund status or execution for Pix.
    /// </summary>
    Task<string> GetRefundAsync();
}

// From AbstractPayments.Core/Extensions/PaymentModuleBuilderExtensions.cs
public static IPaymentModuleBuilder AddProvider<TContract, TImpl>(
    this IPaymentModuleBuilder builder,
    string name)
    where TContract : class, IPaymentGateway
    where TImpl : class, TContract
{
    var attribute = typeof(TContract).GetCustomAttribute<GatewayCapabilityAttribute>();

    if (attribute == null)
    {
        throw new InvalidOperationException($"The contract type '{typeof(TContract).FullName}' must be decorated with '{nameof(GatewayCapabilityAttribute)}'.");
    }

    string serviceKey = $"{attribute.Prefix}:{name}";

    builder.Services.AddKeyedScoped<IPaymentGateway, TImpl>(serviceKey);
    builder.Services.AddKeyedScoped<TContract, TImpl>(serviceKey);

    return builder;
}
```

## 3. Requirements Matrix (RF & RN)

### 3.1 Functional Requirements (RF)

| ID | Target Behavior | User Action / Trigger | Expected System Output |
| :--- | :--- | :--- | :--- |
| RF-1 | Unified Fluent DI Setup | Chaining `.AddPayments(...)` and `.AddEventsHandling(...)` | Registers payments, signature validators, converters, and custom handlers dynamically under keyed scopes. |
| RF-2 | Keyed Provider Mapping | Setup multiple signature validators, converters, and handlers | Isolates implementation instances by provider identifier under specific keyed DI tags (`"validator:{provider}"`, `"parser:{provider}"`, `"handler:{provider}"`). |
| RF-3 | Centralized Processing Orchestration | Call `IWebhookProcessor.ProcessAsync` | Intercepts webhook requests, validates signature, resolves correct converter, normalizes payload, executes handler with retries. |
| RF-4 | Retry Mechanism Integration | A handler throws an exception during processing | Automatically retries execution of the resolved event handler up to `RetryCount` times before propagating the failure. |

### 3.2 Non-Functional & Technical Requirements (RN)

| ID | Quality Attribute | Technical Constraint | Verification Threshold |
| :--- | :--- | :--- | :--- |
| RN-1 | Strategy Extensibility | Abstract signature verification | Support multiple signature validator strategy kinds (e.g. `Hmac`, `Header`, `Rsa`) configured with custom options delegates. |
| RN-2 | Zero Domain Coupling | No hardcoded provider details in core processor | Dynamic resolution of converters and validators at runtime via keyed services based on incoming `providerName`. |
| RN-3 | Allocation Minimization | Webhook request parsing and conversion | Leverage `System.Text.Json` source generators or optimized UTF8 parsing to ensure zero-allocation payload scanning for core properties. |

## 4. Explicit Acceptance Criteria

*   **AC 1 [Invariant Check]:** If a webhook signature validation fails, the core `WebhookProcessor` must throw a `WebhookSignatureValidationException` immediately, short-circuiting further event processing (satisfies RF-3).
*   **AC 2:** If no converter is registered for the incoming `providerName`, the core processor must throw a `ProviderConfigurationException` detailing the missing conversion capability (satisfies RF-2, RF-3).
*   **AC 3:** If the custom event handler fails, the processor must catch the exception and retry execution up to `RetryCount` times. If all retry attempts fail, the final exception must be wrapped inside a `WebhookProcessingException` and propagated (satisfies RF-4).
*   **AC 4:** The configuration engine must support fluent registration exactly matches the user's design blueprint:
    ```csharp
    builder.Services.AddAbstractPayments()
        .AddPayments(opts => {
            opts.Pix.AddProvider<Impl>("key");
            opts.CreditCard.AddProvider<Impl>("key");
        })
        .AddEventsHandling(opts => {
            opts.Endpoint = "/v1/api/payments/webhook";
            opts.SignatureValidators.UseStrategy<Hmac>("key", opt => { ... });
            opts.Converters.AddConverter<Impl>("key");
            opts.Handlers.AddHandler<Impl>("key");
            opts.RetryCount = 3;
        });
    ```

## 5. Rigorous Testing Requirements

### 5.1 Unit Testing Boundary Constraints
*   **Isolation Focus:** Validate the core `WebhookProcessor` orchestration flow, keyed resolution logic, and retry behavior using the **Arrange-Act-Assert (AAA)** pattern in complete isolation.
*   **Test Doubles Required:**
    *   `Stub`: `IWebhookSignatureValidator` returning canned boolean (success/fail).
    *   `Stub`: `IWebhookEventConverter` returning pre-configured `WebhookEvent`.
    *   `Spy/Mock`: `IWebhookEventHandler` capturing the parsed event and tracking the number of invocations to verify the retry mechanism.
*   **No-Infra Rule:** Zero filesystem, database, or real HTTP server interaction during unit testing.

### 5.2 Integration Testing Boundary Constraints
*   **Infrastructure Engine:** Validate the HTTP endpoint using `Microsoft.AspNetCore.Mvc.Testing` (via `WebApplicationFactory`).
*   **Idempotency & Seeding:** For test cases involving standard handlers executing DB logging, execute database truncation/cleanup between test cases. Database tracking and transaction IDs must use randomized UUIDs.
*   **Network / Authentication Isolation:** Stub external provider calls (such as signature key rotation endpoints) using a wire-level mocker (e.g. WireMock) to isolate testing from external network dependencies.
