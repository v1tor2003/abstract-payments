---
description: Performs an architectural SOLID design compliance audit on a selected directory, module, or file.
---

# SOLID Architecture Audit Playbook

This workflow guides you through a comprehensive, step-by-step evaluation of the target codebase against the five SOLID principles.

## Guardrails

* **No Destructive Actions**: Do not delete or rewrite files during the audit phase unless explicitly instructed in a subsequent user prompt.
* **Stack-Agnostic Execution**: Inspect framework configuration files (e.g., `package.json`, `requirements.txt`, `Cargo.toml`) and conform to existing styling rules.
* **Safe CLI Usage**: Use the `// turbo` flag only for non-destructive local queries or compiler check commands.


## Workflows Steps

### Step 1: Understand Context

Ask the user to specify the target file, module, or directory to audit. If the scope is ambiguous, present a short checklist of subfolders to help the user choose.

### Step 2: Analyze Project & Dependency Graph

Locate imports, class declarations, and runtime dependencies inside the target files. Identify how components communicate and flag hard-coded instantiations (violations of DIP).

### Step 3: Conduct Principle Checklist Audit

Audit the selected code and compile a structured report evaluating each principle:

1. **SRP Audit**: Are business logic, database mutations, and API requests mixed in a single file?
2. **OCP Audit**: Will adding a new platform or provider require editing the core coordination code?
3. **LSP Audit**: Do child classes implement empty methods or throw unexpected errors when subbing for parents?
4. **ISP Audit**: Are consumer components forced to implement boilerplate helper methods they don't use?
5. **DIP Audit**: Are high-level orchestrators directly importing raw database drivers or clients?

### Step 4: Present Audit Matrix & Recommendations

Generate a Markdown table rating compliance for each principle (e.g., "Pass", "Weak", "Fail") and provide concise refactoring suggestions.

### Step 5: Verify

Ask the user if they want to load the `solid-architect` skill to automatically refactor a flagged component according to the recommended solutions.

## Principles

* **Question-Driven**: Clarify scope before executing deep analysis scans.
* **Progressive Disclosure**: Summarize findings first; present detailed code diffs only on request.