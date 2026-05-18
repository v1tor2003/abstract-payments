---
trigger: model_decision
description: This rule is meant to be used when the agent detects a task involving system design or class architecture
---

# Rule: SOLID Architectural Core Guardrails

You must evaluate and align all class designs, object models, and structural boundaries with the five SOLID design principles.

## 1. Single Responsibility Principle (SRP)

* A module (file, class, or service) must be responsible to exactly one actor or stakeholder.
* Isolate logical calculation, database state mutation, and external I/O operations into separate boundaries.

## 2. Open/Closed Principle (OCP)

* Entities must be open for extension but closed for modification.
* Replace procedural type inspections, `switch` blocks, or multi-branch `if/else` structures with polymorphic strategies or abstract interface implementations.

## 3. Liskov Substitution Principle (LSP)

* Subtypes must be transparently substitutable for their base types.
* Derived objects must honor base behavioral contracts. Do not throw `NotImplementedException` or narrow pre-conditions inside child classes.

## 4. Interface Segregation Principle (ISP)

* Clients must not be forced to depend on methods they do not use.
* Deconstruct fat, multi-purpose interfaces into granular, context-specific contracts.

## 5. Dependency Inversion Principle (DIP)

* High-level policy must never depend directly on low-level infrastructure detail (e.g., importing SQL engines, routers, or clients directly inside domain layers).
* Invert control structures by defining high-level port interfaces and injecting concrete adapters at runtime.