# Test Doubles Taxonomy Reference

Use this matrix to choose the precise test double required for each mocked dependency. Do not use broad "all-in-one" mock models blindly.

| Double Type | Operational Definition | Primary Use Case |
| --- | --- | --- |
| **Dummy** | Objects passed but never used or invoked. Safely satisfies compiler requirements. | Filling required class constructor arguments. |
| **Stub** | Provides hardcoded, canned answers to queries made by the system under test. Never fails a test. | Supplying indirect inputs to a use case handler. |
| **Spy** | Wraps a component to record internal execution metrics (arguments passed, call counts, order). | Verifying indirect outputs or outbound side effects. |
| **Mock** | Pre-programmed with explicit expectations. Fails the test if interactions do not match. | Verifying strict interaction contracts between components. |
| **Fake** | Working concrete implementation with a highly simplified, in-memory shortcut. | Simulating relational databases or localized filesystems. |

## Prompt Engineering Anchor

When instructed to write, expand, or refactor unit tests in this workspace, append these constraints to your system memory:

Objective: Generate a deterministic, high-coverage unit test suite for the attached production system target.

1. ZERO PLATFORM/INFRASTRUCTURE POLLUTION: Absolutely do not invoke or instantiate concrete database layers, network libraries, filesystem wrappers, or framework API handlers.
2. EXPLICIT AAA PATTERN: Layout every test case inside clear, visually commented blocks mapping to // Arrange, // Act, and // Assert phases.
3. LOGICAL DETERMINISM: Abstract all dynamic system contexts (such as time initialization, random identifier generators, GUID seeds) into parameters or injectable stubs so the test run outcomes are completely invariant.
4. TARGETED TEST DOUBLES: Explicitly separate the utilization of Stubs (for injecting indirect inputs to the SUT) and Mocks/Spies (for validating side effects or outbound command execution telemetry). Do not use broad "all-in-one" generic mock models blindly.
5. CLEAN ASSERTIONS: Focus on verifying a single logical concept per test case execution. Keep test names descriptive of business behavior, following standard 'should_action_when_condition' naming schemas.