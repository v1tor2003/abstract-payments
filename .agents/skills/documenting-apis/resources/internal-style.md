# Internal Codebase Annotation Standards

Inline comments must stay as close to the target code as possible to avoid maintenance lag.

## 1. Structure Matrix

* **Entities**: Focus on invariants, business state transitions, and validation rules.
* **Use Cases (Interactors)**: Focus on step orchestration, boundary exceptions, transactional write locks, and asynchronous event triggers.
* **Ports/Interfaces**: Detail outbound requirements, query optimizations, and consumer contracts.


* **DTOs**: Annotate sanitization, payload size ceilings, and structural data ranges.

## Prompt Engineering Anchor

When instructed to write, expand, or refactor internal codebase annotations, inject these constraints:

Objective: Generate internal codebase documentation (or inline code comments) for the target component using an explicit reference style layout.

1. STYLE ALIGNMENT: Mirror formatting rules, naming terminology, metadata sections, and visual spacing patterns from existing project docstrings.
2. ARCHITECTURAL FOCUS: Document the architectural intent, boundary conditions, threading/transaction locks, and exception states. Do not explain obvious structural operations.