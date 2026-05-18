---
name: architecting-solid-code
description: Refactors existing codebases or scaffolds new systems according to SOLID and Clean Architecture principles. Use when the user asks to restructure modules, decouple components, or create abstract design patterns.
---

# Skill: SOLID & Clean Architecture Synthesizer

## Goal

To refactor highly-coupled, rigid codebases into elastic, maintainable, and testable system designs that adhere strictly to clean architectural principles.

## Plan-Validate-Execute Loop

For all refactoring or scaffolding requests, you must run this state-tracking check loop:

* [ ] **1. Mapping & Smell Discovery**
Scan the target code. Map out the high-level policy vs. low-level execution paths. Check for "fat" interfaces and switch-based category handlers.
* [ ] **2. Draft the Abstract Blueprint**
Propose a clean set of interfaces, injected strategies, and separate handlers. Present this blueprint to the user for validation.
* [ ] **3. Surgical Refactoring**
Implement the changes using forward-slash `/` paths. Ensure code changes do not alter functional behavior.
* [ ] **4. Verification Run**
Use your terminal to verify that the project still builds/compiles without regressions.

## Architecture & Code References

To guarantee consistent code generation, you must directly read and integrate the golden refactoring examples and the Clean Architecture synergy matrices from these workspace resource sub-paths before outputting any code changes:

* Refer to refactoring patterns: `examples/solid-refactoring.md`
* Refer to Clean Architecture synergy & prompts: `resources/clean-architecture.md`
