---
name: dotnet-minimal-apis
description: Configures ASP.NET Core Minimal API endpoint mapping, global error handling via RFC 7807, Polly resilience, source-generated logging, OpenAPI Scalar integration, and xUnit integration tests. Use when the user requests.NET Web API development, exception filters, HTTP clients, or Respawn test fixtures.
---

# Skill: Enterprise ASP.NET Core Minimal API Specialist

## Goal

To design, implement, and test highly scalable, deterministic C# Minimal API endpoints integrated with RFC 7807 problem details, Polly v8 resilience handlers, source-generated logging, and Testcontainers database fixtures.

## Plan-Validate-Execute Loop

For all.NET Minimal API task requests, you must track progress using this state-management check loop:

* [ ] **1. Project Stack Check**
Examine `.csproj` dependencies. Confirm.NET SDK version and locate registered routing, validation, or HTTP packages.
* [ ] **2. Map the Endpoint Slice**
Verify where the route is registered and choose the correct mapping strategy (Manual, Assembly-scanned, Carter, or FastEndpoints).
* [ ] **3. Implement Global & Resilience Controls**
Configure `IExceptionHandler` pipeline mappings and Polly retry stacks, ensuring unsafe verbs are protected from duplicate execution.
* [ ] **4. Build Observability & Docs**
Write partial, source-generated `LoggerMessage` partial methods. Integrate Scalar group mapping, and redact PII metadata properties.
* [ ] **5. Run Verification Pipeline**
Write and run in-memory xUnit assertions using `TypedResults`, and execute containerized integration tests using `WebApplicationFactory` and `Respawn`.

## Reference Guides

To prevent context window token saturation, heavy template structures and integration testing fixtures are stored in separate files. Read these sub-paths before generating code changes :

* Minimal API Code and Resilience Templates: `resources/minimal-api-templates.md`
* xUnit, WebApplicationFactory, & Respawn Blueprints: `examples/api-testing-blueprints.md`