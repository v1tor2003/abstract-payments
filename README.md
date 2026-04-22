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

## 🏗️ Architecture & Technologies

The framework is strictly designed around modern software engineering metrics to ensure a low Coupling Between Object classes (CBO) and high cohesion.

* **Language/Runtime:** C# 14 / .NET 10
* **Design Patterns:** Clean Architecture, SOLID Principles, Strategy/Adapter (for connectors)
* **Testing:** xUnit (Unit and Integration testing in Sandboxed environments)
* **CI/CD:** GitHub Actions for automated testing and NuGet distribution
* **Containerization:** Docker for isolated validation environments

## 📂 Solution Structure

The repository is divided into three primary projects to separate concerns and facilitate testing:

* `AbstractPayments.Core/`
  * The heart of the framework. Contains the abstractions, interfaces, and base implementations. It has absolutely no dependencies on external gateway SDKs.
* `AbstractPayments.Tests/`
  * Comprehensive test suite utilizing xUnit to validate contracts and ensure the framework meets architectural metrics.
* `AbstractPayments.Sandbox/`
  * A lightweight ASP.NET Core Minimal API configured to run via Docker. It serves as a practical testing ground to simulate transactions and validate the framework's Dependency Injection integration.

## 🚀 Getting Started

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for running the Sandbox)

### Running the Sandbox Environment

To test the framework's integration locally, you can spin up the Sandbox Minimal API using Docker:

```bash
# Build the Docker image
docker build -t abstract-payments-sandbox .

# Run the container
docker run -p 8080:8080 abstract-payments-sandbox