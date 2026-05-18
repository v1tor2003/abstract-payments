---
description: Guides the developer through choosing, designing, and scaffolding standard tactical design patterns within their project.
---

# Design Pattern Scaffolder Playbook

This active workflow guides you through identifying structural bottlenecks and generating decoupled pattern classes.

## Execution Guardrails

* **Stack-Agnostic Detection**: Inspect the workspace files (e.g., check `package.json`, `requirements.txt`, or config files) to identify the project's programming language and styling conventions before generating code.


* **No Premature Architecture**: Ask clarifying questions to ensure the selected design pattern is appropriate for the scale of the task.


* **Safe Verification**: Use terminal commands with the `// turbo` flag only to run compilers or syntax checkers on scaffolded code. Never run deletions or global state resets under this flag.


## Workflow Steps

### Step 1: Discover and Identify the Smell

Ask the developer which files or behaviors they want to decouple. Analyze the selected files. Specifically check for:

* Nested `if/else` control trees or state switch-cases (Candidate for Strategy/State).
* Third-party library types imported inside high-level core files (Candidate for Adapter).
* Intermixed logs, metric counters, or database calls inside core business methods (Candidate for Decorator).

### Step 2: Propose the Architectural Selection

Present the recommended design pattern to the developer. Provide a brief text-based diagram mapping how the new Interfaces and Concrete classes will interact. Wait for the developer's confirmation.

### Step 3: Scaffold the Pattern Elements

Create the code structures using the language conventions identified in Step 1. Generate:

1. The abstract Strategy, Port, or Component Interface.
2. The core Concrete implementations.
3. The Context, Adapter, or Decorator wrapper class that consumers will interact with.

### Step 4: Map Folder Placement

Suggest correct locations for the new files matching clean architecture structures (e.g., strategies in `domain/services/`, adapters in `infrastructure/integrations/`).

### Step 5: Verify

Ask the developer to run a syntax compilation check or test run using `// turbo` to ensure the scaffolded pattern classes integrate seamlessly.