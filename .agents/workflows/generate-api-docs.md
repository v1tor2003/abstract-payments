---
description: Automatically scans workspace files and generates internal inline comments or public OpenAPI specifications.
---

# API Documentation Generator Playbook

This active workflow guides you through analyzing an existing component and generating either internal codebase annotations or public Swagger/OpenAPI schemas.

## Execution Guardrails

* **Stack-Agnostic Exploration**: Always inspect the project's dependencies and config files (e.g., `package.json`, `requirements.txt`) to identify the language, framework, and target documentation standard before outputting text.


* **No Assumption of Frameworks**: Ask clarifying questions to determine if the developer wants TypeScript JSDocs, Python docstrings, or OpenAPI schema definitions.


* **Safe CLI Execution**: Use terminal commands with the `// turbo` flag only to run compilers or syntax/spec checkers on generated documentation.



## Workflow Steps

### Step 1: Understand Context

Ask the developer which source file, use case, or REST controller they wish to document. Clarify whether they require:

1. **Internal Developer Docs** (inline comments, class docstrings, ports/adapters).
2. **Public API Specs** (Swagger annotations, OpenAPI 3.x schema blocks, RFC 7807 payloads).
Wait for a response before scanning files.

### Step 2: Analyze File Boundaries

Examine the targeted code file. Specifically locate:

* Input validation mechanisms and serializable data payloads.
* Thrown boundary exception classes.
* Orchestrated database operations, transaction limits, or event triggers.

### Step 3: Present Documentation Plan

Draft a short outline detailing:

* The proposed docstring variables, parameters, and exceptions.
* The target OpenAPI path parameters, HTTP status responses, and validation constraints.
Wait for developer confirmation before writing to the codebase.

### Step 4: Generate & Inject Code

Apply the documentation following the target styles. Strictly enforce:

* Internal files: Document the **why**, transaction scopes, and exception states.
* Public controllers: Ensure exact mock values (e.g., `150.50` instead of `0` for price) and validation restrictions are annotated.

### Step 5: Verify Build

Ask the developer to run a syntax compile or schema validator run using `// turbo` to ensure the changes did not introduce syntax regressions.