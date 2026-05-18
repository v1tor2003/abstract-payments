# Mandatory Post-Activity Logging Templates

Immediately following a successful task run, you must generate and write these two files.

## 1. Task Completion Template

### 1.1 Task Completion Summary (Overview & Objectives)

A concise description of the functional requirements implemented. Placed at 
`activity/done/<task-name>.md`.

### 1.2 Structural Footprint (Files Created / Modified)

* `src/domain/...` (Specify changes)
* `src/application/...` (Specify changes)
* `tests/integration/...` (Specify changes)

### 1.3 Design Decisions & Patterns Used

* **Architectural Boundary:** [e.g., Inverted persistence dependencies using Outbound Ports]
* **Patterns Implemented:**

### 1.4 Verification Proof

* Build Status:
* Test Execution Footprint:

## 2. Troubleshooting & Error-Resolution Registry (`activity/solutions/<task-name>.md`)

# Troubleshooting & Error-Resolution Registry:

## Incident 1:

* **Root Cause:** Detailed explanation of why the compilation, test assertion, or infrastructure container crashed.
* Error Logs / Traces:
```text
[Insert the raw stack trace or terminal error here]

```

* **Resolution:** Precise steps taken to refactor the code or configuration to resolve the issue permanently.

---
## Prompt Engineering Anchor

When starting a coding assignment in this workspace, append these constraints to your active context:
```
Objective: Implement the requirements markdown file provided by the user using strict TDD and environmental safety controls.

1. PRE-FLIGHT VERIFICATION: Before modifying any files, verify that the project successfully compiles and all existing tests pass. Report anomalies immediately.
2. PLANNING GATE: Present a detailed file-modification map and architectural execution plan. Wait for explicit user validation before proceeding to modify files.
3. TEST-FIRST EXECUTION: Write your test assertions and test double configurations before writing production implementations. Verify the tests fail initially.
4. AUTOMATED LEADER LOGGING: Upon making your implementation pass successfully, you must immediately generate the following documentation artifacts in the workspace:
   - Save a task execution ledger to: activity/done/<task-name>.md
   - Save an error stack trace and resolution diary to: activity/solutions/<task-name>.md
5. Keep methods compact, modular, and fully aligned with clean code architecture schemas.

```