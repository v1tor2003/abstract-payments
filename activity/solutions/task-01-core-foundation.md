# Troubleshooting & Error-Resolution Registry - TASK-01

## Incident 1: Test Project Missing Dependency on BuildServiceProvider Extension Method

* **Root Cause:** 
  The unit test class `CoreAbstractionsTests.cs` uses `ServiceCollection.BuildServiceProvider()` to generate an in-memory dependency injection container to run assertions. However, compiling the test project initially failed with error `CS1061: 'ServiceCollection' does not contain a definition for 'BuildServiceProvider'`. This occurred because the concrete `BuildServiceProvider()` extension method is defined inside the NuGet package `Microsoft.Extensions.DependencyInjection`, which was not directly referenced by `AbstractPayments.Tests.csproj`. The test project only transitively referenced `Microsoft.Extensions.DependencyInjection.Abstractions` (which defines the `IServiceCollection` type but not the extension methods to build it).

* **Error Logs / Traces:**
  ```text
  C:\Users\vitor\code\tcc\framework\AbstractPayments.Tests\CoreAbstractionsTests.cs(41,33): error CS1061: 'ServiceCollection' does not contain a definition for 'BuildServiceProvider' and no accessible extension method 'BuildServiceProvider' accepting a first argument of type 'ServiceCollection' could be found (are you missing a using directive or an assembly reference?) [C:\Users\vitor\code\tcc\framework\AbstractPayments.Tests\AbstractPayments.Tests.csproj]
  ```

* **Resolution:**
  We added a direct package reference to `Microsoft.Extensions.DependencyInjection` (v9.0.2) to the `AbstractPayments.Tests.csproj` file:
  ```xml
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.2" />
  ```
  This immediately resolved the compilation error, allowing the service provider to build and execute all unit tests successfully.
