---
description: Guides the developer through scanning a production file and generating isolated, AAA-driven unit tests.
---

# Unit Test Generator Playbook

This active workflow guides you through analyzing a source file and scaffolding a deterministic, isolated unit test suite.

## Execution Guardrails

* **Stack-Agnostic Detection**: Inspect the workspace files (e.g., check `package.json`, `requirements.txt`, `Cargo.toml`, or configuration files) to identify the project's programming language and target testing framework (e.g., Jest, Pytest, JUnit) before writing any tests.


* **No Database/API Calls**: Strictly prohibit tests from attempting real I/O operations. Swapping them with target stubs is mandatory.


* **Safe Verification**: Use terminal commands with the `// turbo` flag only to execute the test suite or syntax compilers. Never run deletions or sweeping database resets under this flag.



## Workflow Steps

### Step 1: Understand Context

Ask the developer to specify the target source file they wish to test. If the scope is ambiguous, present a short list of modified files in the active branch.

### Step 2: Analyze Project & Dependencies

Examine the targeted source file. Specifically locate:

* External database queries or repositories.
* Third-party API SDK calls.
* Non-deterministic utilities (e.g., `Date.now()`, `uuid()`).
* Outbound event dispatchers or loggers.

### Step 3: Propose the Test Setup Matrix

Present a brief, text-based outline showing:

1. The target system under test (SUT).
2. The specific inputs and expected outcomes to cover (including success states and error edge cases).
3. The mock dependencies (Stubs for query inputs, Spies/Mocks for event outputs) needed for complete isolation.
Wait for the developer's confirmation.

### Step 4: Scaffold the Test Cases

Create the test suite using the identified framework conventions. For each test case, apply the **Arrange, Act, Assert (AAA)** pattern:

* **Arrange**: Setup the SUT, inputs, and isolated test doubles.
* **Act**: Execute the target method in a single line.
* **Assert**: Verify return values, state changes, or event spies.
Keep test naming descriptive, following the `should_action_when_condition` format.

### Step 5: Verify Test Suite Build

Ask the developer to run the tests in the terminal using the safe `// turbo` execution flag to ensure they pass without compilation or isolation issues.