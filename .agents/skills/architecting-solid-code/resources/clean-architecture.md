# Clean Architecture Synergy & Prompting Blueprint

## Macro-to-Micro Architectural Maptext
```
  Clean Architecture Layer             Manifested SOLID Principle
 ┌──────────────────────────┐         ┌────────────────────────────────────────────────────────┐
 │ Layer 1: Domain Core     │  ─────► │ SRP: Concentrates pure business entity state rules.    │
 ├──────────────────────────┤         ├────────────────────────────────────────────────────────┤
 │ Layer 2: Application     │  ─────► │ OCP: Interactors extend capabilities via new use cases.│
 ├──────────────────────────┤         ├────────────────────────────────────────────────────────┤
 │ Layer 3: Adapters        │  ─────► │ ISP: Controller/Presenter contracts stay context-lean. │
 ├──────────────────────────┤         ├────────────────────────────────────────────────────────┤
 │ Layer 4: Infrastructure  │  ─────► │ DIP: Plugs into Application abstract ports/interfaces. │
 └──────────────────────────┘         └────────────────────────────────────────────────────────┘
```

## Prompt Engineering Anchor

When instructed to refactor, write, or evaluate design solutions, always append these constraints to your system memory:

```text
1. SINGLE RESPONSIBILITY: Isolate structural behaviors. Logic calculation, persistence state mutations, and I/O delivery must reside in distinct objects.
2. OPEN/CLOSED: Eliminate any procedural type inspection, if/else structural chains on category states, or switch expressions. Use polymorphic strategy definitions.
3. LISKOV SUBSTITUTION: Avoid throwing NotImplementedException or breaking state assertions in child types. Subclasses must be transparently swappable.
4. INTERFACE SEGREGATION: Keep interface footprints highly localized. No class should implement methods it ignores.
5. DEPENDENCY INVERSION: High-level services must never import database drivers, framework routers, or physical service clients directly. Rely strictly on parameter-injected abstraction interfaces.
```