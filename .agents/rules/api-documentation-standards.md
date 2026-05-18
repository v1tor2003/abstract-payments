---
trigger: glob
globs: /*.{js,ts,py,go,java,cs,cpp}
---

# Rule: Internal & Public API Documentation Standards

You must strictly enforce these code annotation and public interface presentation constraints during all code modification, REST controller generation, and documentation drafting tasks.

## 1. Inline Documentation & Code-as-Truth

* Inline comments/docstrings must never restate what is already obvious from reading clean code.
* Focus comments entirely on **why** a specific choice was made (architectural intent, boundary conditions, locks, transactions) rather than **what** lines of code are executing.
* Use standard language docstrings (e.g., JSDoc for TypeScript, docstrings for Python) that can be easily parsed by IDEs and static analysis tools.

## 2. Component Level Annotation Grid

Ensure internal components satisfy these documentation requirements:

| Component Type | Core Documentation Requirement | Focus Area |
| --- | --- | --- |
| **Domain Entity** | Invariants, state machine mutations, and enterprise business rules. | Business validation boundaries. |
| **Use Case (Interactor)** | Orchestration flow, external transactions, and boundary exception types. | Business flow sequence. |
| **Outbound Port / Interface** | Contract expectations, database query optimizations, or integration side effects. | Dependency Inversion decoupling. |
| **Inbound DTO** | Incoming payload serialization limits and sanitization guidelines. | Edge-case validation rules. |

## 3. Public-Facing Contract Isolation

* Keep internal system details (e.g., database table names, SQL errors, internal exception types, inner structures) isolated. Never leak them into public network boundaries.
* Public API schemas must be clean, secure, and fully standardized. Route exceptions must match unified RFC-style error signatures (e.g., RFC 7807 Problem Details).