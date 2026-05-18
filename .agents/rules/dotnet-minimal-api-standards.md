---
trigger: glob
globs: /*.cs
---

# Rule: ASP.NET Core Minimal API Engineering Standards

You must strictly enforce these architectural patterns, pipeline constraints, and compiler requirements during all C# code generation, refactoring, and project structuring tasks.

## 1. Minimal API Middleware Order

You must maintain the exact execution sequence of the ASP.NET Core middleware pipeline. Ensure that `Program.cs` registers middleware in this precise chronological order:

1. `UseDeveloperExceptionPage` (development environment only)
2. `UseRouting` (executes route matching)
3. `UseCors` (Cross-Origin Resource Sharing, registered prior to authentication)
4. `UseAuthentication` (identifies caller credentials)
5. `UseAuthorization` (validates RBAC access scopes)
6. Custom developer middleware / terminal endpoints
7. `UseEndpoints` (automatically appended; custom terminal endpoints must run after this)

## 2. Compile-Time Logging via Source Generators

Do not use traditional string-interpolated logging inside high-throughput pathways to prevent unnecessary heap allocations.

* Use partial methods decorated with `[LoggerMessage]` returning `void`.
* Logging methods and parameter names must not start with an underscore character (`_`).
* Logging methods must not be defined inside nested types or use generic parameters.
* Parameter signatures do not support keywords such as `params`, `scoped`, `out`, or `ref struct`.
* If a logging method is declared `static`, pass `ILogger` as the first argument using the `this` modifier.
* The Exception parameter must not have a matching placeholder inside the message format template. Doing so triggers a `SYSLIB0025` compiler warning.

## 3. Resilience Configuration Guardrails

* Outbound HTTP client registrations must utilize Polly-based standard resilience handlers via `AddStandardResilienceHandler()`.
* **Verbal Safety**: You must disable automated HTTP retries for unsafe HTTP verbs (e.g., `POST`, `PUT`, `PATCH`, `DELETE`) using `DisableForUnsafeHttpMethods()` to avoid duplicate database mutations.

## 4. Testing & Return Types

* Enforce strongly typed endpoint returns using `TypedResults` (e.g., `Results<Ok<T>, NotFound>`) instead of the untyped `Results` interface. This allows fast, in-memory unit testing in xUnit without bootstrapping the full HTTP server pipeline.