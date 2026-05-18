---
description: Scaffolds modular and scalable Minimal API endpoints following clean architectural standards in ASP.NET Core.
---

# Minimal API Scaffolder Playbook

This active workflow guides you through analyzing a business requirement, detecting modular routing strategies, and scaffolding clean, high-performance C# Minimal API endpoints.

## Execution Guardrails

* **Stack-Agnostic Exploration**: Check the `.csproj` file to verify the target.NET SDK version (e.g.,.NET 8 or.NET 9) and inspect package references for Carter, FastEndpoints, or Polly before generating C# structures.


* **No Speculative Routing**: Ask the developer whether they want to use Reflection-based scanning, Manual extension methods, or third-party abstractions like Carter or FastEndpoints.


* **Safe CLI Usage**: Use the terminal with the `// turbo` flag only for safe repository discovery, compiler analysis, and running dotnet test runs.



## Workflow Steps

### Step 1: Understand Context

Ask the developer to describe:

1. The route path, HTTP method, request payload, and expected output model.
2. The preferred architectural modularity style:
* **Manual Mapping**: Static extension methods on `IEndpointRouteBuilder`.
* **Reflection Scanning**: Automatic runtime discovery via the `IEndpoint` interface.
* **Alternative Libraries**: Carter or FastEndpoints.
Wait for a response before scanning the codebase.





### Step 2: Analyze Codebase Patterns

Scan existing files inside the workspace to align naming styles, logging formats, folder structures, and OpenApi documentation conventions.

### Step 3: Propose the API Endpoint Blueprint

Present a brief, text-based outline of:

* The route group, group tags, and OpenAPI specifications.
* The request DTO validation rules.
* The returned `TypedResults` signatures.
Wait for developer confirmation before writing to the codebase.



### Step 4: Scaffold the Endpoint and Handler

Generate the required files using the selected modularity style:

* **Manual Map**: Create static route group extensions.
* **IEndpoint**: Generate a class implementing `IEndpoint` containing exactly one Minimal API definition to enforce vertical slice isolation.
* **OpenAPI Specs**: Register Scalar group mapping configurations.
* **Logging**: Set up compile-time partial logger message attributes.

### Step 5: Verify Build & Tests

Instruct the developer to run `dotnet build` and `dotnet test` using the safe `// turbo` flag to ensure no compilation errors or test isolation failures occur.