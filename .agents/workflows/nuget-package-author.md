---
description: Guides the developer in configuring MSBuild metadata, package validation, and publishing standards for.NET Nuget packages.
---

# NuGet Package Authoring Playbook

This active workflow guides you through configuring a reusable class library for NuGet distribution, setting up SourceLink, establishing package validation constraints, and designing options-based registrations.

## Execution Guardrails

* **Stack-Agnostic Setup**: Inspect the target workspace files (e.g., check existing `.csproj` framework versions and solution structure) before adding properties.


* **No Speculative Versioning**: Ask clarifying questions to determine the target license type, package baseline versions, and public repository URL.


* **Safe Verification**: Use terminal commands with the `// turbo` flag only to run compile, pack, or test commands.



## Workflow Steps

### Step 1: Understand Package Profile

Ask the developer to provide:

1. The unique Package ID and target framework.
2. The license model (SPDX expression) and public repository URL.
3. The options configuration keys and dependency injection scope.
Wait for a response before modifying files.



### Step 2: Configure Project Metadata (MSBuild)

1. Read the standard configuration file template from the `authoring-nuget-packages` skill.


2. Inject package metadata, compiler parameters (`Nullable`, `ImplicitUsings`), license parameters, and link the embedded `README.md` file.

### Step 3: Configure SourceLink and Debug Symbols

1. Add the matching `Microsoft.SourceLink.GitHub` package reference, setting its assets visibility to `PrivateAssets="All"` to prevent leaking dependencies.


2. Set up deterministic build settings, ensuring `<ContinuousIntegrationBuild>` evaluates to `true` when compiling on active CI servers (e.g., GitHub Actions, Azure Pipelines).
3. Determine the symbol format based on target feed support:
* **Private Feeds (Azure Artifacts)**: Embed symbols directly using `<DebugType>embedded</DebugType>`.
* **Public Registries (NuGet.org)**: Package separate symbol files using `<SymbolPackageFormat>snupkg</SymbolPackageFormat>`.



### Step 4: Implement Service Extensions and Options

1. Scaffold the Options configuration class. Leverage `.BindConfiguration` to map the sections.
2. Enable startup validation using `.ValidateDataAnnotations()` and `.ValidateOnStart()`.
3. Provide a post-configure callback block to enforce enterprise fallbacks.

### Step 5: Pack and Verify Compatibility

Instruct the developer to run `dotnet pack` using the safe `// turbo` flag to verify that MSBuild generates the package and executes native compatibility validations successfully.