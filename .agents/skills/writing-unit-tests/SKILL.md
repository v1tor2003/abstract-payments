---
name: writing-unit-tests
description: Generates isolated, deterministic unit tests using the Arrange-Act-Assert pattern and test doubles. Use when the user requests unit tests, test coverage, or mock setups.
---

# Skill: Deterministic Unit Testing Expert

## Goal

To write or refactor unit test suites that are completely isolated from physical system layers, highly reliable, and compliant with the F.I.R.S.T. and AAA patterns.

## Plan-Validate-Execute Loop

For all test generation or mocking requests, you must run this state-tracking check loop:

* [ ] **1. Isolation Mapping**
Scan the target system under test (SUT) for direct environmental couplers (e.g., filesystems, network APIs, databases, or system time).
* [ ] **2. Select Test Doubles**
Decide which dependencies must be replaced. Map required doubles (Stubs, Spies, or Mocks) to decouple the SUT.
* [ ] **3. Write AAA Tests**
Generate the test suite using forward slashes `/` for all directory paths. Visually partition each test case with explicit commenting:
* `// Arrange`
* `// Act`
* `// Assert`


* [ ] **4. Code Verification**
Verify that the generated tests match the styling conventions of the project and compile cleanly.

## Reference Guides

To maintain context efficiency, detailed code blueprints and test double matrices are stored in separate subdirectories. Read these files before outputting code changes :

* Golden Unit Testing Blueprints: `examples/testing-blueprints.md`
* Test Double Taxonomy & Usage: `resources/test-doubles.md`
