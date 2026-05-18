---
trigger: glob
globs: /*.{test,spec}.{js,ts,py,go,java,cs,cpp}
---

# Rule: Deterministic Unit Testing & Isolation Standards

You must strictly enforce these unit testing guardrails during all test generation, test expansion, and test suites audits.

## 1. Core Philosophy & F.I.R.S.T. Principles

A unit test must verify the smallest testable piece of code in total isolation. If a test contacts a database, hits a network socket, reads the file system, or relies on global state configuration, it is an **Integration Test**, not a Unit Test.

* **Fast**: Tests must execute in milliseconds so thousands of them can run continuously without friction.
* **Independent**: Tests must have zero execution dependencies. They must run in any order, concurrently, without leaving shared side effects in memory.
* **Repeatable**: Tests must produce the identical outcome every single run, regardless of time zones, environment variables, or underlying hardware.
* **Self-Validating**: Test outputs must be a clear boolean result (Pass/Fail). Do not use manual console log parsing to evaluate success.
* **Timely**: Write tests aligned with or immediately prior to (TDD) production implementation.

## 2. Structural Isolation Guardrails

* **No Network or DB Calls**: Absolutely do not initialize or invoke real database connections, remote API clients, local filesystems, or global configurations. Swapping them with lightweight test doubles is mandatory.
* **Deterministic Execution**: Abstract all dynamic system environments (such as current datetime, random GUID generators, or process environments) into parameters or mock stubs so execution remains completely reproducible.
* **Single Concept Verification**: Target exactly one logical behavior per test case. Avoid bloated multi-assertion tests that test multiple logical pathways in a single block.