# Enterprise NuGet MSBuild Configuration Template

Configure your reusable class library's project file (`.csproj`) to match this standard build profile.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
  </PropertyGroup>

  <PropertyGroup>
    <PackageId>Enterprise.Diagnostics.Core</PackageId>
    <Version>2.1.0</Version>
    <Title>Enterprise Diagnostics Core Library</Title>
    <Authors>Enterprise Common Systems Team</Authors>
    <Company>Enterprise Corporation</Company>
    <Description>Advanced core telemetry and logging capabilities for microservices.</Description>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <RepositoryUrl>https://github.com/enterprise/diagnostics-core</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
  </PropertyGroup>

  <PropertyGroup>
    <EnablePackageValidation>true</EnablePackageValidation>
    <PackageValidationBaselineVersion>2.0.0</PackageValidationBaselineVersion>
    <ApiCompatStrictMode>true</ApiCompatStrictMode>
    <ApiCompatValidateAssemblies>true</ApiCompatValidateAssemblies>
  </PropertyGroup>

  <PropertyGroup>
    <PublishRepositoryUrl>true</PublishRepositoryUrl>
    <EmbedUntrackedSources>true</EmbedUntrackedSources>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
  </PropertyGroup>

  <PropertyGroup Condition="'$(TF_BUILD)' == 'true' Or '$(GITHUB_ACTIONS)' == 'true'">
    <ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>
  </PropertyGroup>

  <ItemGroup>
    <None Include="README.md" Pack="true" PackagePath="\" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.0" PrivateAssets="All" />
    <PackageReference Include="Microsoft.Extensions.Options.DataAnnotations" Version="10.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0" />
  </ItemGroup>

</Project>
