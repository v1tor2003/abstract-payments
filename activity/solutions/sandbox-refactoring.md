# Troubleshooting Registry: Sandbox and Test Suite Refactoring

**Date:** 2026-06-04
**Task Name:** sandbox-refactoring

## Issues & Resolutions

### 1. Compile Error: Missing Namespace/Imports for Extracted DTO Records
- **Symptom:** During compilation, endpoints like `AbstractedPixEndpoints` complained that types `AbstractedPixRequest` and `AbstractedPixResponse` could not be found.
- **Root Cause:** DTO records were extracted from `Program.cs` into the `AbstractPayments.Sandbox.Requests` and `AbstractPayments.Sandbox.Responses` namespaces, but the corresponding endpoints did not import these namespaces.
- **Resolution:** Added `using AbstractPayments.Sandbox.Requests;` and `using AbstractPayments.Sandbox.Responses;` to `AbstractedPixEndpoints.cs` and `CoupledPixEndpoints.cs`.

### 2. Missing Reference to Provider Responses in Integration Tests
- **Symptom:** When splitting `SandboxIntegrationTests.cs`, the new file `SandboxPaymentIntegrationTests.cs` failed to compile because `MercadoPagoPixResponse`, `PagSeguroPixResponse`, and `EfiBankPixResponse` were not found.
- **Root Cause:** These responses are declared under `AbstractPayments.Sandbox.Http.Commands` rather than the main model/DTO folders.
- **Resolution:** Added the import `using AbstractPayments.Sandbox.Http.Commands;` to the `SandboxPaymentIntegrationTests.cs` file.

### 3. BaseAddress Mapping on Generic Client
- **Symptom:** Renaming `GatewayApiClient` to the generic `ApiClient` required overriding client setup in the Test Application Factory.
- **Root Cause:** Missing `BaseAddress` configuration when instantiating the mocked HttpClient causes `InvalidOperationException` in relative URI paths.
- **Resolution:** Ensured the Test Application Factory registers `ApiClient` with a valid mock host `BaseAddress = new Uri("http://localhost");` while attaching the mock message handler stub.

### 4. Codebase Removal of Simulated Mock ("Foo") Gateway
- **Symptom:** Retaining placeholder simulation gateway code ("Foo") in the codebase introduces noise now that real gateway strategies are integrated.
- **Root Cause:** Placeholder components can be removed, since they are redundant with the real gateways, but they must be kept in the thesis text as programming examples.
- **Resolution:** Deleted simulated `Foo` classes from the sandbox codebase. Updated `CoupledPixEndpoints` to validate request constraints (require explicit provider) and throw `ArgumentException` for unsupported providers, preventing fallback to dummy implementations. Added a clarifying footnote to the thesis.

### 5. Base Interface Pollution with Method-Level Generics
- **Symptom:** Adding generic `GeneratePaymentAsync` directly to `IPaymentGateway` caused other unrelated gateway stubs (e.g. `ICardDummyGateway`) in the test project to fail compilation.
- **Root Cause:** Making the base `IPaymentGateway` interface declare a generic `GeneratePaymentAsync` method incorrectly forced all sub-interfaces to inherit it, violating the Interface Segregation Principle.
- **Resolution:** Removed the method from `IPaymentGateway`, leaving it as a pure markup capability interface. Declared the generic `GeneratePaymentAsync<TRequest, TResponse>` directly inside the specialized `IPixGateway` interface, maintaining proper architectural isolation.
