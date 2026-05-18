# Clean Architecture Folder Placement Topology

Design patterns must be located inside their designated architectural layers. Follow this target directory structure:

src/
├── domain/
│   └── services/
│       └── tax_calculation.strategy.ext    # Domain Strategies / Business Rules
│
├── application/
│   ├── use_cases/
│   │   └── process_order.command.ext       # Application Commands / Interactors
│   └── ports/
│       └── message_broker.port.ext         # Abstract Target Interfaces / Ports
│
├── presentation/
│   └── presenters/
│       └── json_response.adapter.ext       # Presenter Adapters converting types to Views
│
└── infrastructure/
├── integrations/
│   └── rabbitmq_broker.adapter.ext     # Infrastructure Adapters satisfying ports
└── config/
└── identity_provider.factory.ext   # Factories configuring SDK lifecycles

## Prompt Engineering Anchor

When instructed to refactor, write, or evaluate design patterns inside this workspace, enforce compliance using these constraints:

Objective: Refactor the code block attached below by applying design patterns that simultaneously satisfy SOLID structural boundaries and Clean Code readability requirements.

1. ANTI-PATTERN ERADICATION: Replace explicit control flows (large if/else statements, complex switch blocks) with a polymorphic Strategy, Command, or State pattern.
2. ADAPT ENCAPSULATION: Isolate all dirty structural modifications, data formatting, and external SDK method calls inside a concrete Structural Adapter class.
3. COMPOSITION OVER INHERITANCE: Instead of deep base class extensions to inject plumbing logic (logging, transaction scopes), utilize the Decorator pattern.
4. CODE SEPARATION: Ensure all generated concrete pattern classes follow the rule of smallness (<20 lines per method body) with high structural cohesion.