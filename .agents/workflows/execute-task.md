---
description: Guides the agent through task execution using a strict five-phase TDD and artifact logging pipeline.
---

# Task Execution & Logging Playbook

This active workflow guides you through implementing a feature request or bug fix using strict Test-Driven Development (TDD) and logging protocols.

## Execution Guardrails

* **Stack-Agnostic Execution**: Inspect framework configuration files (e.g., `package.json`, `requirements.txt`, `Cargo.toml`) to identify the project's programming language and test runner before executing steps.


* **No Unapproved Writes**: You must wait for explicit user validation of your implementation plan before editing or creating codebase files.
* **Safe CLI Invocation**: Use the `// turbo` flag only for safe, non-destructive compile, build, or test commands.


## Workflow Steps

### Step 1: Pre-Flight Sanity Check (Phase 1)

1. Run the project's compilation/build command using the safe `// turbo` flag to ensure the baseline compiles.


2. Run the existing test suite.


3. **Halt Condition**: If the project fails to build or any existing test fails *before* you make changes, halt execution immediately, report the baseline regressions to the user, and do not write new code until the regression is resolved.

### Step 2: Implementation Planning (Phase 2)

1. Formulate a framework-agnostic architectural plan outlining:
* Target files/directories to modify or create.
* Testing boundaries to cross.
* Required test doubles (Stubs, Spies, Mocks).


2. Present this plan to the user in a clean list.
3. **Halt and Wait**: Explicitly pause and ask the user to type "proceed" or provide feedback. Do not modify files until approved.

### Step 3: Test-Driven Development (Phase 3)

1. Write the unit and/or integration tests for the task requirements *before* writing any production implementation.
2. Execute the new test suite and capture the failure (**RED** cycle). Confirm that the tests fail due to the missing implementation, not compilation errors.

### Step 4: Production Implementation & Refactoring (Phase 4)

1. Write the minimum necessary code to satisfy the failing tests (**GREEN** cycle).
2. Refactor the code according to SOLID, Clean Code, and Design Patterns.
3. Run the full test suite again to verify everything remains green.

### Step 5: Post-Activity Artifact Logging (Phase 5)

1. Access the `ai-task-execution` skill templates.


2. Generate the task completion summary file at `activity/done/<task-name>.md`.
3. Generate the troubleshooting registry file at `activity/solutions/<task-name>.md`.
4. Report successful completion and present the logged file paths to the user.
