---
name: authoring-nuget-packages
description: Drafts reusable.NET class libraries with MSBuild properties, SourceLink, deterministic builds, and Fluent options registrations. Use when the user requests NuGet configuration, project packaging, options lifetime setups, or SourceLink integration.
---

# Skill: NuGet Package & MSBuild Configuration Specialist

## Goal

To configure.NET class libraries for highly compliant, debuggable, and backwards-compatible NuGet package distribution using SourceLink, MSBuild validations, and robust dependency injection options patterns.

## Plan-Validate-Execute Loop

For all package authoring or library scaffolding requests, you must run this state-tracking check loop:

* [ ] **1. Metadata Discovery**
Gather target package ID, version, licensing models, repository paths, and target frameworks.
* [ ] **2. Configure Build Elements**
Draft the SDK-style `.csproj` configurations, enabling nullable reference checks, SourceLink bindings, and deterministic builds.
* [ ] **3. Implement Options Lifetime**
Map configurations to the appropriate Options lifetime models:
* `IOptions<T>`: Singleton; read once at startup.
* `IOptionsSnapshot<T>`: Scoped; recomputed per request.
* `IOptionsMonitor<T>`: Singleton; dynamic reloads and change notifications.
* [ ] **4. Scaffold Registrations**
Construct static service collection extension methods using forward slashes `/` for paths. Ensure data annotations and startup validations are attached.


* [ ] **5. Build & Test Runs**
Verify that the packed artifacts generate successfully and pass active assembly validation checks.

## Template & Blueprint References

To maintain context window speed and optimize token usage, extensive project templates and C# examples are split :

* MSBuild Project Build Template: `resources/msbuild-template.md`
* Options & OpenAPI Customization Blueprints: `examples/options-and-openapi.md`
