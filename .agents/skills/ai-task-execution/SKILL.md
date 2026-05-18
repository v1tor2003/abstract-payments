---
name: ai-task-execution
description: Implements features using a five-phase TDD pipeline and creates done/solutions markdown logs. Use when executing programming assignments or writing artifact registries.
---

# Skill: Autonomous Task Execution & Registry Specialist

## Goal

To implement codebase tasks from requirement definitions using strict Test-Driven Development (TDD) and cataloging outcomes inside structured markdown ledgers.

## Plan-Validate-Execute Loop

For all development assignments, you must track your progress using this state-management check loop:

* [ ] **1. Pre-Flight Baseline Run**
Build the project and run existing tests using `// turbo`. Confirm the environment is green before proceeding.
* [ ] **2. Map & Present Blueprint**
Draft the implementation blueprint and wait for user approval.
* [ ] **3. Write Failing Test Case**
Write tests and verify they fail for the correct reasons (the RED cycle).
* [ ] **4. Code & Refactor**
Write the feature code until the tests pass (the GREEN cycle), then refactor.
* [ ] **5. Log Completion Artifacts**
Read the template structures in `resources/logging-templates.md` and write the completion ledger and error registries.

## Logging Template References

To protect the context window from token saturation, logging schemas and markdown blueprints are offloaded. Read this file before creating your post-activity log files :

* Done & Solutions Markdown Templates: `resources/logging-templates.md`
