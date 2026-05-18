---
trigger: glob
globs: /*.{ts,tsx,py,go,java,cpp,cs,rb,php}
---

# Rule: Tactical Design Pattern Harmonization Standards

You must evaluate and align all class designs, structural integrations, and cross-cutting concerns with standard tactical design patterns.

## 1. Pattern Implementation Triggers

Do not write custom procedural branching or hardcoded integrations when standard design patterns can solve the architectural problem. Refer to this matrix:

| Design Pattern | SOLID Principle Fulfilled | Clean Code Smell Eliminated |
| --- | --- | --- |
| Strategy | OCP | Complex `if/else` or `switch` blocks; cyclomatic noise. |
| Adapter | ISP / DIP | Third-party SDK signature pollution in core domain. |
| Factory Method | DIP | `new` keyword coupling inside domain/use cases. |
| Decorator | SRP / OCP | Logging, caching, or tracing boilerplate bleeding. |
| Command | SRP | Bloated coordinators handling multiple intents. |

## 2. Refactoring Guardrails

* **Condition Flattening**: Replace complex conditional blocks or state-based switch-cases with a polymorphic **Strategy** or **State** pattern.
* **Infrastructure Isolation**: Never allow concrete database adapters, network APIs, or third-party library signatures to bleed into the high-level application core. Isolate them inside structural **Adapters**.
* **Separation of Cross-Cutting Concerns**: Do not mix logging, performance metrics, authorization, or transaction scope logic directly into your core business domain service classes. Wrap the core service using a **Decorator** to handle these infrastructure concerns.
* **Limit Class Growth**: Ensure all concrete pattern classes remain highly focused, keeping individual method lengths under 15–20 lines of code.
