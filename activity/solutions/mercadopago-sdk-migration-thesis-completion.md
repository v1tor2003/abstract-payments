# Troubleshooting Registry: MercadoPago SDK Migration & Thesis Writing Completion

**Date:** 2026-06-05
**Task Name:** mercadopago-sdk-migration-thesis-completion

## Issues & Resolutions

### 1. SQLite Database Locking in Concurrent Test Runs
- **Symptom:** Runnig `dotnet test` occasionally failed with SQLite errors stating that the database file was locked.
- **Root Cause:** Both payment and webhook integration test suites ran in parallel, trying to read, write, and seed the same physical SQLite database file simultaneously.
- **Resolution:** Enforced serial test execution by creating a shared xUnit collection fixture `"Sandbox Tests"` and decorating both `SandboxPaymentIntegrationTests` and `SandboxWebhookIntegrationTests` with `[Collection("Sandbox Tests")]`.

### 2. Outbound Network Requests from MercadoPago SDK during Tests
- **Symptom:** Integration tests involving MercadoPago Pix generation were making real network requests or failing due to lack of network configuration.
- **Root Cause:** The official `mercadopago-sdk` uses its own internal HTTP client registered globally via the static configuration `MercadoPagoConfig.HttpClient`, bypassing the standard DI container where we injected our mocked clients.
- **Resolution:** Intercepted the SDK's global network operations in `SandboxTestApplicationFactory.cs` by wrapping the mocked `HttpClient` in `DefaultHttpClient` and registering it as the static SDK client:
  ```csharp
  var mockClient = _mockHttpMessageHandler.CreateClient();
  MercadoPagoConfig.HttpClient = new DefaultHttpClient(mockClient);
  ```

### 3. LaTeX Compilation Failure: Undefined TikZ Coordinate Variables
- **Symptom:** Running the compilation script returned `Undefined control sequence` pointing to `\endpoint`, `\processor`, `\validator`, etc.
- **Root Cause:** TikZ `\draw` paths were referencing node names prefixed with backslashes (e.g. `\endpoint`) instead of the correct coordinate offset variables defined at the top of the `tikzpicture` block (e.g. `\ep` or `\proc`).
- **Resolution:** Replaced the node-based variables with the correct numeric variables in all TikZ path calculations (e.g., changing `(\endpoint, 3.5)` to `(\ep, 3.5)`).

### 4. Hardcoded Document References
- **Symptom:** Academic references in the text to `Figura 1` and `Tabela 2` were hardcoded numbers, which would break if chapters/sections/figures were rearranged.
- **Root Cause:** The numbers were hardcoded instead of using LaTeX's dynamic `\ref` referencing mechanism.
- **Resolution:** Replaced the hardcoded references with `\ref{fig:clean_architecture}` and `\ref{tab:comparativo_arquitetura}` respectively, and fixed the spelling typo `denomindada` to `denominada`.
