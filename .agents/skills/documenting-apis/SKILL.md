---
name: documenting-apis
description: Generates inline codebase docstrings for use cases and models, and writes OpenAPI/Swagger REST decorators. Use when the user requests codebase comments, API documentation, or Swagger schemas.
---

# Skill: Technical API & Codebase Documenter

## Goal

To write clean, scannable inline docstrings detailing architectural choices, and scaffold public OpenAPI/Swagger schemas representing predictable, secure contracts.

## Plan-Validate-Execute Loop

For all documentation generation or API spec mapping requests, you must run this state-tracking check loop:

* [ ] **1. Context Audit**
Read the target class or controller. Map out the architectural boundary (ports, adapters, entities) and exception boundaries.
* [ ] **2. Select Documentation Scope**
Clarify if this target is an internal domain module or an external-facing public gateway endpoint.
* [ ] **3. Apply Documentation Standards**
Synthesize the annotations using forward slashes `/` for paths. Avoid explaining obvious code. Focus strictly on intent, transaction locks, parameters, and exceptions.


* [ ] **4. Validate Schema Integration**
Verify that generated annotations or standalone specs compile cleanly within the workspace.

## Reference Guides

Detailed style templates, status code matrices, and code blueprints are offloaded to protect the active context window from token saturation :

* Golden Code & Schema Blueprints: `examples/doc-blueprints.md`
* Internal Docstring Styles & Anchors: `resources/internal-style.md`
* Public Swagger & Error Shapes: `resources/public-style.md`
