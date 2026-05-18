---
trigger: glob
globs: /*.{cs,csproj}
---

# Rule: NuGet Library Authoring & Compilation Standards

You must strictly enforce these MSBuild metadata configurations, compilation safety parameters, and dependency registration guidelines during all class library modifications and project builds.

## 1. MSBuild Compilation Hygiene

Every shared class library project (`.csproj`) must utilize modern.NET SDK-style configurations. Ensure these compilation variables are explicitly declared:

| MSBuild Element | Typical Configuration Value | Purpose and Functional Impact |
| --- | --- | --- |
| `<TargetFramework>` | `net10.0` | Defines the target platform runtime and API accessibility limits. |
| `<Nullable>` | `enable` | Enforces compiler-level null safety, minimizing null reference risks. |
| `<ImplicitUsings>` | `enable` | Automatically references common namespaces based on SDK type. |
| `<LangVersion>` | `latest` | Instructs the compiler to utilize the newest C# syntax features. |
| `<PackageLicenseExpression>` | `MIT` | Declares legally compliant licenses using standard SPDX structures. |
| `<PackageReadmeFile>` | `README.md` | References a physical Markdown file embedded in the final package. |

## 2. API Compatibility & Baseline Validation

* You must configure native package validation inside the project builder. Set `<EnablePackageValidation>` to `true` to execute automatic compatibility checks after the pack task completes.
* Set `<ApiCompatStrictMode>` and `<ApiCompatValidateAssemblies>` to `true` to force strict compatibility checks.
* Prevent binary-breaking changes by comparing the newly compiled assembly against a prior package using the `<PackageValidationBaselineVersion>` element. Any contract violations must halt the build on a compiler error.

## 3. Dependency Injection & Namespace Guidelines

* Do not register custom library extensions inside the official Microsoft platform namespace. Reserving this namespace exclusively for native framework services maintains explicit boundary separation.
* All service collections extensions must reside in a static class following the `Extensions` suffix naming convention (e.g., `ServiceCollectionExtensions`).
* Registration methods must follow the `Add` naming format (e.g., `AddEnterpriseDiagnostics`).