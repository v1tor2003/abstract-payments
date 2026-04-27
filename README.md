# AbstractPayments: Unified Payment Gateway Framework

> **Official Project Title:** Interface Comum para Integração com Gateways de Pagamento: Um Framework de Abstração Unificado Baseado em C# para a plataforma .NET

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-14.0-239120?style=flat&logo=csharp)
![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?style=flat&logo=docker)
![License](https://img.shields.io/badge/License-MIT-blue.svg)

## About the Project

The growth of e-commerce and systems like Pix in Brazil requires applications to integrate with multiple payment gateways (e.g., Stripe, Mercado Pago, EfiPay). However, the lack of standardization across these providers' APIs and SDKs often leads to high code coupling and "vendor lock-in". 

**AbstractPayments** is a unified C# framework built for the .NET platform. By leveraging Clean Architecture, SOLID principles, and native Dependency Injection, this package establishes a common, provider-agnostic interface. It allows developers to switch between payment gateways with minimal to no changes in the core application logic, significantly reducing technical debt and improving system resilience.

## Objectives

* **Unification:** Provide a common, agnostic interface for integrating multiple payment gateways.
* **Decoupling:** Isolate financial business rules from sudden API changes (breaking changes) introduced by external providers.
* **Maintainability:** Ensure high cohesion and low coupling across the application's components.
* **Extensibility:** Facilitate the creation of custom connectors for specialized providers.

## Architecture & Technologies

The framework is strictly designed around modern software engineering metrics to ensure a low Coupling Between Object classes (CBO) and high cohesion.

*   **Language/Runtime**: C# 14 / .NET 10
*   **Design Patterns**: Strategy, Factory, and Template Method
*   **Architectural Style**: Modular Monolith with Plugin-Style Adapters
*   **Testing**: xUnit with comprehensive mocking strategies

## Key Features

*   **Unified Pix Module**: Standardized payment generation and status checking.
*   **Automated Webhooks**: Strategy-based event parsing and signature validation.
*   **Multi-Gateway Orchestration**: Runtime resolution and fallback support.
*   **Zero Provider Coupling**: Business logic remains completely agnostic of gateway SDKs.

## Solution Structure

The repository is divided into three primary projects to separate concerns and facilitate testing:

*   **[AbstractPayments.Core](file:///c:/Users/vitor/code/tcc/framework/AbstractPayments.Core/README.md)**: The heart of the framework. Contains the abstractions, interfaces, and base implementations.
*   **[AbstractPayments.Tests](file:///c:/Users/vitor/code/tcc/framework/AbstractPayments.Tests/README.md)**: Comprehensive test suite validating both unit abstractions and integration flows.
*   **[AbstractPayments.Sandbox](file:///c:/Users/vitor/code/tcc/framework/AbstractPayments.Sandbox/README.md)**: A practical testing ground for simulating transactions and validating DI integration.

## Getting Started

### Prerequisites
*   [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional, for Sandbox containerization)

### Quick Start
1.  **Clone the repository**:
    ```bash
    git clone https://github.com/v1tor2003/abstract-payments.git
    ```
2.  **Explore the Core**: Check the [Core README](file:///c:/Users/vitor/code/tcc/framework/AbstractPayments.Core/README.md) to understand the architecture.
3.  **Run the Sandbox**: Navigate to `AbstractPayments.Sandbox` and see its [README](file:///c:/Users/vitor/code/tcc/framework/AbstractPayments.Sandbox/README.md) for execution details.
4.  **Run Tests**:
    ```bash
    dotnet test
    ```
    See the [Tests README](file:///c:/Users/vitor/code/tcc/framework/AbstractPayments.Tests/README.md) for more testing strategies.