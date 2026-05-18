---
name: generating-scrum-specs
description: Generates codebase-aware, technically bounded SCRUM task specifications mapping stories to architectural layers. Use when the user requests Scrum task generation, technical spec writing, or backlog mapping.
---

# Skill: Technical Backlog Spec Architect

## Goal

To translate ambiguous product features into highly granular, technically isolated development tasks with explicit testing parameters and inline codebase contracts.

## Plan-Validate-Execute Loop

For all task mapping or spec engineering requests, you must run this state-management check loop:

* [ ] **1. Requirements Audit**
Read the raw user story or product requirement. Map the objective to specific Clean Architecture layers.
* [ ] **2. Map Codebase Context**
Scan target folders. Run dependency and import analysis to ensure no domain-to-infrastructure couplings are introduced.
* [ ] **3. Extract Contracts**
Locate and extract target interfaces or existing pattern strategies to serve as the development starting baseline.
* [ ] **4. Synthesize the Spec**
Read the template from `resources/task-template.md` and populate the fields. Ensure acceptance criteria specify exact custom exceptions and state names.
* [ ] **5. Validate & Place Spec**
Ensure the compiled markdown conforms to the golden examples in `examples/documented-task.md` before outputting.

## Reference Guides

To keep your active context window fast and context token sizes minimal, templates and golden example files are split :

* SCRUM Technical Task Template: `resources/task-template.md`
* Golden Example: Documented Flight Task: `examples/documented-task.md`
