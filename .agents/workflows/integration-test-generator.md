---
description: Scaffolds isolated and idempotent integration test suites using Testcontainers and HTTP mock servers.
---

# Integration Test Generator Playbook

This active workflow guides you through analyzing a system component and scaffolding a deterministic integration test suite.

## Execution Guardrails

* **Stack-Agnostic Detection**: Inspect the workspace files (e.g., check `package.json`, `requirements.txt`, `Cargo.toml`, or configuration files) to identify the project's programming language and target testing framework before writing any tests.
* **No Real Networks**: Ensure all external HTTP requests are configured to point to a local wire-level mock server.
* **Safe Terminal Executions**: Use terminal commands with the `// turbo` flag only to compile, run tests, or spin up local Docker engines. Never run persistent background deletions under this flag.


## Workflow Steps

### Step 1: Understand Context

Ask the developer to specify the target component or API route they wish to write integration tests for. Clarify what external dependencies (databases, brokers, third-party APIs) this component interacts with.

### Step 2: Analyze Project Stack & Drivers

Examine the workspace configuration. Locate the active database client drivers, environment variable configurations, and test initialization files to align the new tests with the existing structure.

### Step 3: Propose the Infrastructure Matrix

Present a brief, text-based outline of:

1. The required Testcontainers configuration (including Alpine Docker images, dynamic ports, and wait-readiness strategies).
2. The HTTP API loopback mocker setups and target endpoints.
3. The table truncation list to reset database state between test cases.
Wait for the developer's confirmation.

### Step 4: Scaffold the Test Suite

Create the integration test class. Strictly enforce the **Arrange, Act, Assert (AAA)** structure:

* **Arrange**: Start containers, program mock wire responses, generate random UUIDs, seed localized data, and generate mock JWT authorization headers.
* **Act**: Execute the endpoint or use case.
* **Assert**: Directly query the Testcontainers database instance to verify structural mutations, and verify intercepted outbound HTTP traffic.

### Step 5: Verify the Test Suite Build

Ask the developer to run the tests in the terminal using the safe `// turbo` execution flag to verify that container orchestration and test assertions pass.