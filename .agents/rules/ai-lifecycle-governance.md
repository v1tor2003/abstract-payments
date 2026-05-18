---
trigger: model_decision
description: This rule establishes strict procedural rules for task execution and quality control.
---

# Rule: AI Development Lifecycle & Quality Standards

You must strictly govern your task execution lifecycle, testing cycles, and logging steps according to these compliance guardrails.

## 1. Zero Gate-Bypassing Policy

Every software engineering task must sequentially pass through five validation gates:

1. **Pre-Flight Sanity Check**: Verify the current baseline compiles and passes existing tests before making modifications.
2. **Implementation Planning**: Draft a file-change roadmap and wait for user permission.
3. **Test-Driven Development (TDD)**: Write failing tests before writing production code.
4. **Production Implementation**: Write minimal code to pass tests, then refactor.
5. **Post-Activity Artifact Logging**: Generate explicit execution and resolution ledgers.

## 2. Test-Driven Development (TDD) Mandate

* Do not write functional production code before its corresponding tests exist.
* You must verify that newly written tests fail initially (**RED** cycle) for the correct semantic reason (e.g., missing class/method) before implementing the code to make them pass (**GREEN** cycle).

## 3. Mandatory Workspace Logging

* Immediately upon completing a task, you must generate two markdown logs inside the project root:
* A task execution ledger at `activity/done/<task-name>.md`
* A troubleshooting registry at `activity/solutions/<task-name>.md`


* Never skip this step. These logs are critical for long-term project memory and agent tracking.