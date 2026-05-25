# Scrum Task: TASK-03 Minimal API Payment Integration & Abstraction Showcase

## Description
Establish a side-by-side comparison inside `AbstractPayments.Sandbox` to showcase the stark benefits of adopting the `AbstractPayments` decoupled plugin abstraction engine. We will build a direct, tightly coupled Mercado Pago integration and a highly abstract, factory-driven integration.

### Clean Architecture Layer Scopes
* **Infrastructure**: Dynamic SQLite connection factory, Dapper repositories, and simulated coupled Mercado Pago SDK clients.
* **Application / Presentation**: ASP.NET Core Minimal API endpoint handlers executing business logic and mapping payloads via `TypedResults`.
* **Domain / Framework**: Native implementation of the `IPixGateway` capability interface from `AbstractPayments.Core`.

---

## Current Codebase Reference

### Specialized Pix Capability Port
```csharp
namespace AbstractPayments.Core.Abstractions;

using System.Threading.Tasks;

[GatewayCapability("Pix")]
public interface IPixGateway : IPaymentGateway
{
    Task<string> GeneratePaymentAsync();
    Task<string> GetRefundAsync();
}
```

---

## Technical Tasks & Acceptance Criteria

### 1. Persistent Storage via SQLite & Dapper
* Scaffold `DbConnectionFactory` and Dapper-driven `TransactionRepository` under `AbstractPayments.Sandbox/Storage/`.
* Configure automated table schema creation on startup.

### 2. Coupled API Endpoint Integration
* Implement `/v1/api/coupled/payments/pix` POST/GET endpoints directly injecting `MercadoPagoDirectClient` mock.
* Return custom, bespoke response types mimicking a vendor-locked SDK.

### 3. Abstracted API Endpoint Integration
* Implement `MercadoPagoPixGateway` implementing `IPixGateway`.
* Configure `/v1/api/payments/pix` POST/GET endpoints dynamically resolving the gateway from the framework factory based on `Provider` field in the request JSON payload.

### 4. Code Standards Compliance
* Define zero-heap partial static `PaymentsLogger` compile-time logging.
* Maintain strict Minimal API pipeline registration chronological order in `Program.cs`.

---

## Testing Boundaries

### E2E Integration Testing
* **Infrastructure Isolation**: Spin up isolated, in-memory SQLite schema states per test inside `AbstractPayments.Tests/Sandbox/SandboxIntegrationTests.cs`.
* **State Cleanliness**: Initialize databases dynamically to prevent sequential ID collisions or concurrent run collisions.
