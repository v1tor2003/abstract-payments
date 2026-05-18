# TASK-402: Process Drone Flight Telemetry

## 1. Scrum User Story

* **As an** Operating Flight Unit
* **I want to** transmit real-time telemetry coordinates to the ingestion API
* **So that** my flight coordinates can be validated against legal airspace constraints and logged for auditing.

## 2. Architectural & Codebase Context

* **Target Domain / Context:** Drone Flight Telemetry Context
* **Clean Architecture Layer:** Application Use Case (Layer 2)
* **Affected Codebase Files:**
* `📁 src/domain/entities/drone_flight.js` -> Validate airspace boundaries and mutate speed state.
* `📁 src/application/ports/outbound/airspace_checker_port.js` -> Interface contract checking non-flight zone restrictions.



### Current Codebase Reference

```javascript

class AirspaceCheckerPort {
async isZoneAuthorized(latitude, longitude) {
throw new Error("Unimplemented port interface");
}
}
```
## 3. Requirements Matrix (RF & RN)

### 3.1 Functional Requirements (RF)

| ID | Target Behavior | User Action / Trigger | Expected System Output |
| --- | --- | --- | --- |
| RF-1 | Coordinate Validation | Transmit latitude/longitude payload | Validates range limits or throws coordinates error |
| RF-2 | Airspace Boundary Check | Coordinates map to restricted boundary | Restricts flight and triggers violation logs |
| RF-3 | Event Propagation | Transition drone state to restricted | Dispatches decoupled airspace violation telemetry |

### 3.2 Non-Functional & Technical Requirements (RN)

| ID | Quality Attribute | Technical Constraint | Verification Threshold |
| --- | --- | --- | --- |
| RN-1 | Operational Latency | Process and validate incoming DTO | Completion latency strictly under 200ms |
| RN-2 | Execution Security | Require verified JWT claims | Restricts route access to registered customers |
| RN-3 | Allocation Optimization | Logging operations on coordinates | Zero heap allocations via partial source-generators |

## 4. Explicit Acceptance Criteria

* **AC 1 [Invariant Check]:** If the telemetry input latitude exceeds 90 or longitude exceeds 180, the system must throw an `InvalidCoordinatesException` immediately (satisfies RF-1).
* **AC 2:** Upon confirming the coordinates violate an active no-fly zone, the drone flight entity must change state to `RESTRICTED_BOUNDS` and dispatch an `AirspaceViolationEvent` to the outbound port (satisfies RF-2, RF-3).
* **AC 3:** The REST controller must intercept raw payloads and parse latitude and longitude into float precision matching the application DTO camelCase schema (satisfies RN-1).

## 5. Rigorous Testing Requirements

### 5.1 Unit Testing Boundary Constraints

* **Isolation Focus:** Test the `ProcessDroneTelemetryUseCase` behavior in absolute isolation using the **Arrange, Act, Assert** pattern.
* **Test Doubles Required:** Inject a `Stub` representing the `AirspaceCheckerPort` to return a canned boolean response, and a `Spy` to verify that `AirspaceViolationEvent` was dispatched correctly.
* **No-Infra Rule:** Zero execution access to database wrappers or live REST controllers.

### 5.2 Integration Testing Boundary Constraints

* **Infrastructure Engine:** Orchestrate a live, ephemeral instance using **Testcontainers** running `postgres:16-alpine`. Use dynamic port allocation.
* **Idempotency & Seeding:** Execute localized truncations on the database tables after every test case. All seeded drone data rows must use randomly generated UUIDs.
* **Network / Authentication Isolation:** Intercept external outbound geo-services requests using WireMock. Stub the security check by injecting an unsigned JWT mock containing required authorization scopes.

---

### Tech Lead Copilot System Prompt Anchor

When initializing an issue grooming or task generation session inside your Google Antigravity workspace, append this updated system prompt anchor [4, 5]:

```text

Role: Distinguished Enterprise Tech Lead and Agile Scrum Master.
Objective: Generate or refine a highly granular, codebase-aware SCRUM technical task markdown file based on the codebase layout and the requirements provided.


1. RAW REQUIREMENTS: [Insert product requirements document, feature issue description, or developer conversation notes].
2. TARGET CODEBASE PATHS: [Insert relevant file paths, entities, or interface names current in the codebase].
3. SYSTEM ARCHITECTURE: Fulfills Clean Architecture, SOLID Principles, Clean Code micro-craftsmanship, and strict Test-First (TDD) methodologies.


1. ARCHITECTURAL LAYER IDENTITY: Explicitly tag which Clean Architecture layer this change belongs to. Never mix infrastructure alterations inside a pure Domain task specification.
2. CODEBASE INJECTION: Pull real, existing code signatures, data interface contracts, or abstract port definitions from the context and display them inline as technical starting baselines.
3. DETAILED REQUIREMENTS MATRIX: Formulate structured Functional (RF) and Non-Functional (RN) requirement tables using only single spaces between cell content and pipe separators. Define specific, testable behaviors and technical quality thresholds.
4. DETAILED ACCEPTANCE CRITERIA: Formulate granular, unambiguous invariant rules mapping back to the RF and RN identifiers. Do not use generic words like "handle processing properly." Specify error exceptions and state status values explicitly.
5. ISOLATED TESTING DICTATION: Mandate clear, separated Unit (AAA, Stubs) and Integration testing architectures (Testcontainers, Network Mockers, Local JWT Token Bypasses, Table Truncation Scopes).
6. FORMAT OUTPUT: Output purely valid, scannable Markdown conforming exactly to the SCRUM Technical Task Specification Template structure.

```