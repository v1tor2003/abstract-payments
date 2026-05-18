--- 
name: patterning-systems
description: Refactors coupled, procedural, or complex code by applying tactical software design patterns. Use when the user requests strategy setups, adapters, decorators, or pattern-based refactoring.
---

# Skill: Tactical Design Pattern Architect

## Goal

To refactor complex procedural code blocks into modular, elastic, and highly cohesive design pattern structures.

## Plan-Validate-Execute Loop

For all pattern refactoring requests, you must run this state-tracking check loop:

* [ ] **1. Complexity Analysis**
Analyze the target files. Calculate cyclomatic complexity $M=E-N+2P$ mentally to identify structural bottlenecks. Catalog the dependencies that must be inverted.
* [ ] **2. Select & Draft the Blueprint**
Draft the abstract interfaces and strategic concrete classes. Present the class structures and behavioral boundaries to the user before writing to the workspace.
* [ ] **3. Surgical Refactoring**
Implement the design patterns using forward slashes `/` for all directory paths to maintain cross-platform workspace compliance. Keep new methods under 15–20 lines of code.


* [ ] **4. Code verification**
Verify that the new files are correctly placed in the Clean Architecture topology and do not break compilation.

## Pattern & Topology References

To prevent context window saturation, the detailed code examples and folder topology conventions have been moved to sub-paths. You must read these files before outputting any code changes :

* Golden Pattern Blueprints: `examples/pattern-blueprints.md`
* Clean Architecture Folder Topology & System Anchor: `resources/topology-prompt.md`