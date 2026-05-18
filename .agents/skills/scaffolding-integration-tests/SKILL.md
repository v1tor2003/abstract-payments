---
name: scaffolding-integration-tests
description: Scaffolds deterministic integration tests using Testcontainers, idempotent data seeds, and wire-level HTTP mocks. Use when the user requests containerized tests, API mocking, database seeding, or OAuth stubs.
---

# Skill: Resilient Integration Testing Expert

## Goal

To write, refactor, and configure integration test suites that interact with real database engines and external HTTP interfaces in a completely isolated, idempotent, and parallel-safe sandbox.

## Plan-Validate-Execute Loop

For all integration test generation or configuration tasks, you must run this state-tracking check loop:

* [ ] **1. Dependency Audit**
Analyze the component to identify out-of-process boundaries (PostgreSQL, Redis, RabbitMQ, Stripe APIs).
* [ ] **2. Define Container Lifecycles**
Draft the Docker container setup with randomized port bindings and explicit readiness waiting blocks.
* [ ] **3. Formulate Mock Wire Rules**
Map out mock loops to intercept outbound HTTP sockets with canned JSON responses.
* [ ] **4. Write Idempotent Seeds**
Synthesize local, inline database seeds utilizing UUID/ULID structures, and append table-truncation tearDown hooks.
* [ ] **5. Verification Run**
Verify the containerized test suite compiles cleanly and runs successfully in the workspace.

## Reference Guides

To protect the context window from token saturation, detailed code blueprints and network-mocker structures are stored in separate files. Read these sub-paths before outputting code changes:

* Golden Integration Blueprints: `examples/integration-blueprints.md`
* Advanced Testing Patterns: `resources/integration-patterns.md`
