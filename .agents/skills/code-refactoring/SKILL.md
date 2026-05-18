---
name: code-refactoring
description: Refactors existing code blocks to maximize readability, minimize cyclomatic complexity, invert nested conditions, and isolate error paths. Use when the user requests code improvements, clean code audits, or structural refactoring.
---

# Skill: Micro-Refactoring & Complexity Reduction

## Goal

To systematically analyze arbitrary source code, isolate structural code smells, and safely refactor execution flows to match clean code paradigms without breaking functional requirements.

## Refactoring Workflow (Plan-Validate-Execute)

You must execute this task using a strict state-tracking loop. Copy and check off each step as you proceed:

* [ ] **Step 1: Code Ingestion & Abstract Mapping**
Analyze the user's code block or file. Map out abstraction levels, identify nested `if/else` trees, locate hidden side effects, and catalog magic values.
* [ ] **Step 2: Formulate the Clean Plan**
Write down a brief structural plan outlining:
* Proposed intent-revealing constants and helper function signatures.
* Inversion strategies for nested control blocks.
* Separation strategies for try/catch blocks.
Present this plan to the user before editing files.


* [ ] **Step 3: Surgical Execution**
Apply the refactoring changes. Ensure no new function exceeds 15–20 lines of code, and ensure all newly created functions strictly enforce a single responsibility.
* [ ] **Step 4: Functional Verification**
Double-check that the refactored code maintains 100% functional equivalence with the original source.

## Inversion & Abstraction Heuristics

* **Condition Inversion**: Replace `if (condition) { large_block }` with `if (!condition) return; large_block`.
* **Abstraction Extraction**: If a loop contains nested logic, extract the inner block of the loop into its own beautifully named monadic function.
* **Exceptions Over Codes**: Do not return error codes or status booleans. Throw descriptive, custom exceptions.

## Structural Examples

Refer directly to the Golden Examples located in the companion path below to guide your structural code synthesis:
*(examples/refactoring-pairs.md)