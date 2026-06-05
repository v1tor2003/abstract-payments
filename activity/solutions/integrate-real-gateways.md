# Troubleshooting & Error-Resolution Registry: Integrate Real Gateways

## Incident 1: WebhookProcessorTests Compilation Failure
* **Root Cause:** Updating `IPixGateway` to extend `IPaymentGateway<PixPaymentRequest, PixPaymentResult>` broke the mock `DummyPixGateway` class in the `AbstractPayments.Tests` project because it lacked the new signature implementation.
* **Error Logs / Traces:**
```text
C:\Users\vitor\code\tcc\framework\AbstractPayments.Tests\Webhooks\WebhookProcessorTests.cs(59,37): error CS0535: 'WebhookProcessorTests.DummyPixGateway' does not implement interface member 'IPaymentGateway<PixPaymentRequest, PixPaymentResult>.GeneratePaymentAsync(PixPaymentRequest)'
```
* **Resolution:** Updated `DummyPixGateway` in `WebhookProcessorTests.cs` to accept `PixPaymentRequest` and return `Task<PixPaymentResult>`, resolving compile-time compatibility.

---

## Incident 2: Relative URI InvalidOperationException in Tests
* **Root Cause:** In the newly implemented `GatewayApiClient`, relative paths were used for endpoints (e.g. `/v1/payments`), but the underlying `HttpClient` registered in the DI container (and test factory overrides) had a `null` `BaseAddress`, causing network execution crashes.
* **Error Logs / Traces:**
```text
Assert.Equal() Failure: Values differ
Expected: OK
Actual:   BadRequest
(Internal System Stacktrace: System.InvalidOperationException: An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set.)
```
* **Resolution:** Configured a default `BaseAddress` on the `HttpClient` during DI registrations inside both `Program.cs` and the test environment `SandboxTestApplicationFactory`.

---

## Incident 3: Deserialization Failure / Snake-Case JSON Property Mismatches
* **Root Cause:** Mock responses returned by the tests utilized lower_snake_case property names (e.g., `point_of_interaction`, `reference_id`), whereas C# properties used PascalCase. Because System.Text.Json default web defaults only resolve camelCase, properties deserialized as `null`, causing `NullReferenceExceptions` inside the adapters which mapped to 500 Internal Server Errors.
* **Error Logs / Traces:**
```text
Assert.Equal() Failure: Values differ
Expected: OK
Actual:   InternalServerError
(System.NullReferenceException: Object reference not set to an instance of an object at MercadoPagoPixGateway.GeneratePaymentAsync)
```
* **Resolution:** Annotated all request and response records in `MercadoPagoCommands.cs`, `PagSeguroCommands.cs`, and `EfiBankCommands.cs` using the `[JsonPropertyName("...")]` attribute to enforce exact JSON-to-C# mapping.
