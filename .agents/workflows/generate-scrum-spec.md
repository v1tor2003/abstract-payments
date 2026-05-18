---
description: Guides the Tech Lead in generating a codebase-aware, highly deterministic Scrum task markdown file.
---

# Scrum Technical Spec Generator Playbook

This active workflow guides you through analyzing a feature request, identifying codebase touchpoints, and compiling a structured technical task conforming to architectural standards.

## Guardrails

* **Stack-Agnostic Context Mapping**: Inspect framework configs (e.g., `package.json`, `requirements.txt`, `Cargo.toml`) to align target code paths and mock technologies with the current project stack.


* **No Spec Speculation**: Ask clarifying questions if the feature requirements do not specify target validations, states, or systems.


* **Safe CLI Usage**: Use the terminal with the `// turbo` flag only for safe repository mapping, directory scanning, and import analysis tasks.



## Workflow Steps

### Step 1: Understand Context

Ask the Tech Lead to provide:

1. The raw user story, product requirement draft, or issue ticket.
2. The target folder or files they intend to modify.
Wait for the details before proceeding to scan files.



### Step 2: Analyze Project (Codebase Context Mapping)

Examine the active codebase directories using the following execution rules :

* **Import Dependency Check**: Scans imports of the target directory to verify architectural integrity. Formulate warnings if the change risks violating the Dependency Inversion Principle.


* **Interface Dissection**: Locate existing abstract ports, interfaces, or adapters related to the domain feature request. Extract the raw interface declarations.
* **Pattern Matcher**: Identify existing implementations of Design Patterns (e.g., Strategies, Decorators, or Factories) to recommend consistent styling.

### Step 3: Scaffold the Technical Spec

Pull the raw Scrum task markdown template from the `generating-scrum-specs` skill. Compile the task file by combining:

* The user story details.
* The extracted interface signatures under `Current Codebase Reference`.
* Structured Gherkin or deterministic Acceptance Criteria outlining precise exceptions and state modifications.
* Visually separated Unit (AAA, Stubs) and Integration testing constraints (Testcontainers, Network Mockers, table truncation lists).

### Step 4: Verify formatting

Ensure the completed task file conforms exactly to the SCRUM Technical Task Specification Template structure. Present the file path and compiled contents to the Tech Lead.