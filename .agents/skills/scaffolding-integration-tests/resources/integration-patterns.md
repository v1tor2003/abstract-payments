# Advanced Integration Testing Patterns

## 1. Testcontainers Operational Lifecycle
* **No Host Port Collisions**: Always map internal container ports dynamically (e.g., let Testcontainers map Postgres `5432` to a random free port on the host machine).
* **Explicit Readiness Strategies**: Wait for TCP socket listening or healthy API response logs before proceeding to the Arrange phase. Never use static sleep timers.
* **Auto-Destruction (Ryuk/Reaper)**: Always enable dynamic container cleanup to prevent orphan processes from hanging on the host in the event of forced pipeline termination.

## 2. Authentication Bypass Strategies
* **Strategy A (Local Gateway Token Stub)**: Configure the testing server auth middleware to skip signature verification and parse claims directly from a dummy JWT payload generated locally.
* **Strategy B (DI Root Component Swap)**: Replace the standard authorization handler class in the test environment composition root with a `TestingAuthenticationHandler` that automatically appends mock security credentials into the request execution pipeline.

## 3. Network Mocking Architecture
* Point outbound client configuration URLs (such as payment gateways, analytics providers, or cloud buckets) directly to the loopback address of the running local mock server (e.g., WireMock).
* Assert on outbound payload schemas, query parameters, header attributes, and error-handling status paths to verify networking behavior on the wire.

---

## Prompt Engineering Anchor

When instructed to write, expand, or refactor integration tests in this workspace, append these constraints to your system memory:

Objective: Generate a deterministic integration test suite for the component attached below using Testcontainers and Wire-level Network Mockers.

1. NATIVE CONTAINER LIFECYCLES: Do not configure in-memory fallback databases. Utilize explicit Testcontainers orchestration patterns with random port configuration mappings and explicit readiness waiting blocks.
2. NETWORKING BOUNDARY TESTING: Do not allow standard inner-memory mocking frameworks to handle external system API client instances. Build network wire intercept rules via loopback mock server abstractions.
3. IDEMPOTENT SEEDING: Ensure all database seeds use randomly generated UUID/ULID structures instead of hardcoded sequential numbers. Provide setup/teardown code templates that truncate or roll back tables between executions.
4. AUTHENTICATION BYPASS: Do not integrate external OAuth network endpoints or live security client connections. Instead, stub or mock security middleware components by directly injecting un-signed JSON Web Tokens or Claims Principals into the testing server pipeline.
5. SYSTEM READ ASSERTIONS: Ensure validations verify the state mutations directly in the targeted infrastructure databases, rather than assuming application memory remains constant.

```