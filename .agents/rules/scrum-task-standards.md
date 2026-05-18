---
trigger: model_decision
description: This rule acts as a quality assurance guardrail to ensure any task file created or updated in the repository complies with Scrum mapping criteria.
---

# Rule: SCRUM Technical Task Engineering Standards

You must strictly enforce these technical task decomposition, codebase mapping, and testing-dictation guidelines during all task engineering, backlog grooming, and issue formatting activities.

## 1. Context-Aware Ingestion

* Every generated task must bridge high-level product requirements with low-level execution details by mapping stories directly to target codebase architectures.
* Always identify the precise Clean Architecture layers impacted by the work. Never allow infrastructure implementation scopes (e.g., SQL queries, database migrations) to pollute domain core specifications.

## 2. Testing Constraints Dictation

Every task specification must explicitly dictate independent testing boundaries:

* **Unit Testing**: Enforce isolation using the AAA (Arrange-Act-Assert) pattern. Mandate the use of targeted test doubles (Stubs and Spies) over generic mock frameworks. Prohibit filesystem, database, or network interactions.
* **Integration Testing**: Enforce native containerized environments (via Testcontainers) with dynamic host-port mappings and explicit wait strategies. Require wire-level loopback HTTP interceptors (such as WireMock) for external networks, and forbid hardcoded database sequential IDs.

## 3. Real Code Snippet Ingestion

* Do not describe code contracts abstractly. Locate existing abstract ports, interfaces, or target adapters related to the feature request and display them inline within the task under `Current Codebase Reference`.