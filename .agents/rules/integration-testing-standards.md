---
trigger: model_decision
description: This rule establishes key architectural constraints for integration testing.
---

# Rule: Resilient Integration Testing & Infrastructure Isolation

You must strictly enforce these integration testing boundaries and state-isolation protocols during all test generation, Docker configuration, and pipeline orchestration tasks.

## 1. Core Integration Mandate

* Integration tests must verify interactions with real out-of-process dependencies (databases, message brokers, HTTP networks, identity providers).
* No integration test may depend on state mutated by a prior test or concurrently running workers. Absolute state isolation is mandatory.

## 2. Infrastructure Isolation & Mocking

* **Database & Queue Lifecycles**: Real external databases and brokers must run on native, disposable Docker instances. Do not fall back to in-memory mocks (e.g., SQLite, H2) that hide dialect-specific bugs.
* **External HTTP APIs**: Intercept outbound third-party HTTP/HTTPS traffic using local wire-level mock loopback servers. Real networks must never be reached during test execution.

## 3. Idempotent Seeding Guardrails

* **Zero Sequential IDs**: Do not assert on auto-incrementing integers (e.g., `id = 1`). All seed utilities must generate random, explicitly declared identifiers (UUIDs or ULIDs) to prevent state collisions.
* **Inline Seeding**: Avoid global SQL database seed files. Place data seeding logic inline and locally within the Arrange block of the test so dependencies are self-documenting.
* **Transaction Rollbacks**: Ensure every test runs in an isolated transaction that rolls back automatically, or executes a targeted table truncation script immediately upon test completion.

## 4. Authentication Bypass

* Bypassing authentication must be handled via local gateway token stubs (unsigned/locally signed JWTs with mock claims) or by swapping real authorization middleware with a testing mock claims handler at the dependency injection root.