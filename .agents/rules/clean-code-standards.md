---
trigger: glob
globs: /*.{ts,tsx,py,go,java,cpp,cs,rb,php}
---

# Rule: Micro-Level Clean Code & Readability Standards

You must strictly enforce these micro-level software craftsmanship guardrails during all code generation, refactoring, and file modification tasks.

## 1. Intent-Revealing Names

* Replace all ambiguous, single-character (except in brief loop indices), or encrypted variable names with self-documenting equivalents.
* Never include data types or structures in a name (e.g., use `accounts` instead of `accountList`).
* Use verbs or verb-phrases for methods (`calculateTotal`) and nouns for classes or properties (`Invoice`).

## 2. Abstraction & Function "Smallness"

* Functions must represent a single conceptual operation (Single Responsibility Principle) at a unified level of abstraction.
* Keep functions small: aim for a maximum of 15–20 lines of code per block.
* Minimize arguments: Niladic (0) or monadic (1) is preferred. If a function requires three or more parameters, combine them into a single configuration or options object.
* Eradicate hidden side effects. A function must not secretly modify global states, alter out-of-scope variables, or trigger unannounced I/O.

## 3. Inversion of Nesting (Early Returns)

* Eliminate nested `if/else` control flow branches.
* Invert conditionals immediately to execute short, early-exit Guard Clauses at the beginning of the function.

## 4. Error Isolation

* Exception handling is an independent execution concern.
* Try/catch blocks must not mix with standard business logic. If a function utilizes a try/catch block, that block must be the only structural pipeline inside the function body (delegate execution and handling to helper methods).