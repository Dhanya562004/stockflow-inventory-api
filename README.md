# Inventory Management System API

[![Build & Test](https://github.com/Dhanya562004/stockflow-inventory-api/actions/workflows/ci.yml/badge.svg)](https://github.com/Dhanya562004/stockflow-inventory-api/actions/workflows/ci.yml)

A production-inspired **Inventory Management System REST API and MVC Web Application** built with **ASP.NET Core 8**, **Entity Framework Core**, and **SQL Server**. This project demonstrates modern backend development practices including layered architecture, authentication, authorization, validation, clean API design, and Razor views with plain JavaScript.

> [!NOTE]
> This application uses **ASP.NET Core 8 MVC and Web API** on the modern, cross-platform .NET 8 runtime (not legacy ASP.NET MVC on .NET Framework). No .NET Framework components or legacy APIs are used.

> **Credits:** Original project by Michael Mitchell ([github.com/mmitchell3397](https://github.com/mmitchell3397)). Extensions in this fork: stock adjustment rule, low-stock SQL report, xUnit tests, Razor UI, GitHub Actions CI.

---

# Table of Contents

- [Credits](#credits)
- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [NuGet Packages](#nuget-packages)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Database Design](#database-design)
- [API Resources](#api-resources)
- [Fork Extensions](#fork-extensions)
  - [1. Stock Adjustment Endpoint](#1-stock-adjustment-endpoint)
  - [2. Low-Stock Report (Raw SQL)](#2-low-stock-report-raw-sql)
  - [3. xUnit Test Suite](#3-xunit-test-suite)
  - [4. AI Inventory Replenishment Advisor](#4-ai-inventory-replenishment-advisor)
  - [5. Enterprise Architecture & Migration Guides](#5-enterprise-architecture--migration-guides)
    - [.NET Framework 4.8 to .NET 8 Migration Whitepaper](docs/dotnet-framework-to-dotnet8-migration.md)
    - [Multi-Channel Concurrency & Double-Selling Prevention](docs/cin7-multi-channel-inventory.md)
    - [SQL Server Indexing & Performance Analysis](docs/sql-server-indexing-and-performance.md)
    - [Azure Cloud SaaS Deployment Blueprint](docs/deploy-azure.md)
- [Legacy Sub-Project: ASP.NET MVC 5 (.NET Framework 4.8)](#legacy-sub-project-aspnet-mvc-5-net-framework-48)
- [Authentication & Authorization](#authentication--authorization)
- [Validation](#validation)
- [Database Seeding](#database-seeding)
- [Getting Started](#getting-started)
- [Test Results](#test-results)
- [Verification](#verification)
- [CI Pipeline](#ci-pipeline)
- [Low-Stock SQL Query](#low-stock-sql-query)
- [What I Learned](#what-i-learned)
- [Future Improvements](#future-improvements)
- [Author](#author)

---

# Overview

This project was created to learn and demonstrate professional backend development using the ASP.NET Core ecosystem.

The API provides complete inventory management functionality including:

- Product Management
- Category Management
- Supplier Management
- Purchase Orders
- Purchase Order Items
- User Authentication
- Role-Based Authorization

The project follows a layered architecture that separates concerns using repositories, services, DTOs, validators, and controllers.

---

# Features

- RESTful API
- ASP.NET Core 8
- Entity Framework Core
- SQL Server
- Repository Pattern
- Service Layer
- Dependency Injection
- DTO Mapping
- FluentValidation
- ASP.NET Core Identity
- JWT Authentication
- Role-Based Authorization
- Database Seeding
- Swagger/OpenAPI
- HTTP Request Testing
- Async/Await throughout the application

---

# Tech Stack

| Technology | Version |
|------------|---------|
| .NET | 8 |
| ASP.NET Core | 8 |
| Entity Framework Core | 8 |
| SQL Server | 2022+ |
| ASP.NET Core Identity | 8 |
| JWT Bearer Authentication | 8 |
| FluentValidation | 11.x |
| Swagger / OpenAPI | 6.x |

---

# NuGet Packages

```text
Microsoft.AspNetCore.Authentication.JwtBearer
Microsoft.AspNetCore.Identity.EntityFrameworkCore

Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.EntityFrameworkCore.Design
Microsoft.EntityFrameworkCore.Tools

FluentValidation
FluentValidation.AspNetCore
FluentValidation.DependencyInjectionExtensions

Swashbuckle.AspNetCore
```

---

# Architecture

## High-Level Architecture

```text
                Client
                   │
                   ▼
            ASP.NET Core API
                   │
        ┌──────────┴──────────┐
        ▼                     ▼
 Authentication        Authorization
        │
        ▼
    Controllers
        │
        ▼
      Services
        │
        ▼
    Repositories
        │
        ▼
 Entity Framework Core
        │
        ▼
     SQL Server
```

---

## Request Lifecycle

```text
HTTP Request

↓

Middleware

↓

Authentication

↓

Authorization

↓

Controller

↓

Service

↓

Repository

↓

DbContext

↓

SQL Server

↓

HTTP Response
```

---

# Project Structure

```text
InventoryManagementSystemAPI/

Controllers/

Configurations/

Data/
    Seed/

DTOs/
    Category/
    Product/
    PurchaseOrder/
    PurchaseOrderItem/
    Supplier/
    Identity/

Enums/

Exceptions/

Mappers/

Middleware/

Models/
    Identity/

Repositories/
    Interfaces/
    Implementations/

Responses/

Services/
    Interfaces/
    Implementations/

Validators/
    Category/
    Product/
    PurchaseOrder/
    PurchaseOrderItem/
    Supplier/
```
---

# Database Design

## Entities

- Product
- Category
- Supplier
- PurchaseOrder
- PurchaseOrderItem
- ApplicationUser
- ApplicationRole

---

## Relationships

Category

- One Category → Many Products

Supplier

- One Supplier → Many Products
- One Supplier → Many Purchase Orders

Purchase Order

- One Purchase Order → Many Purchase Order Items

Product

- One Product → Many Purchase Order Items

---

# API Resources

## Inventory

- Products
- Categories
- Suppliers
- Purchase Orders
- Purchase Order Items
- Stock Adjustments (`POST /api/stock/adjust`)
- Low-Stock Reports (`GET /api/reports/low-stock?threshold=N`)

## Identity

- Register
- Login
- Current User
- Change Password
- Assign Role

---

# Fork Extensions

### 1. Stock Adjustment Endpoint
- **Route:** `POST /api/stock/adjust`
- **Body:** `{ "productId": "GUID", "delta": int }`
- **Behavior:**
  - `200 OK`: When the resulting stock remains at or above zero, returns the new quantity.
  - `409 Conflict`: Rejects any adjustment that would result in a negative quantity with a clear message.
  - `404 Not Found`: Returned when the product ID does not exist.
  - `400 Bad Request`: Validation errors on invalid inputs (e.g. empty GUID or zero delta).
- **Concurrency & Transaction Safety:** Implements optimistic concurrency with SQL Server `rowversion` concurrency token on `Product.RowVersion`, wrapped in a database transaction with up to 3 retry attempts on `DbUpdateConcurrencyException`.

### 2. Low-Stock Report (Raw SQL)
- **Route:** `GET /api/reports/low-stock?threshold=N`
- **Implementation:** Executes a parameterized raw SQL query through EF Core (`Database.SqlQuery<LowStockReportDto>`), performing a `JOIN` between `StockLevels` and `Products` with a `GROUP BY` and a parameterized `HAVING` clause (`SUM(sl.Quantity) <= @threshold`) to prevent SQL injection and aggregate across multiple warehouse locations.
- **Raw SQL Query:**
```sql
SELECT 
    p.Id AS ProductId,
    p.Name AS ProductName,
    p.SKU AS SKU,
    CAST(SUM(sl.Quantity) AS INT) AS CurrentStock,
    p.ReorderLevel AS ReorderLevel
FROM StockLevels sl
INNER JOIN Products p ON sl.ProductId = p.Id
GROUP BY p.Id, p.Name, p.SKU, p.ReorderLevel
HAVING SUM(sl.Quantity) <= @threshold
ORDER BY CurrentStock ASC
```

### 3. xUnit Test Suite
- Test project: `InventoryManagementSystemAPI.Tests`
- Comprehensive unit tests for FluentValidation validators (Create/Update Product, AdjustStock).
- Service tests using in-memory fake repositories for `ProductService`, `StockService`, and `ReportService`.
- Concurrency simulation verifying positive adjustments, zero adjustments, negative rejections, and retry limits.

### 4. AI Inventory Replenishment Advisor (Full-Stack LLM Integration)
- **API Endpoint:** `GET /api/reports/low-stock/summary?threshold=N`
- **Frontend UI:** Interactive Razor MVC widget (`/stock`) powered by plain JavaScript (`stock.js`) and CSS (`stock.css`).
- **Capabilities:**
  - Aggregates at-risk inventory items below catalog reorder thresholds.
  - Sends structured prompts to OpenAI's GPT API through an abstracted `ILlmClient`.
  - Automatically parses and validates structured JSON responses into executive summaries, identified at-risk SKUs, and immediate purchase recommendations.
  - Features accessible ARIA live states (`aria-live="polite"`), loading spinners, and graceful fallback when the LLM service is offline (HTTP 503).

### 5. Enterprise Architecture & Modernization Whitepapers
- **[.NET Framework 4.8 to .NET 8 Migration Whitepaper](docs/dotnet-framework-to-dotnet8-migration.md):** Architectural guide detailing the transition from legacy ASP.NET MVC 5 / `Global.asax` / EF6 to modern ASP.NET Core 8 / `Program.cs` / EF Core using the Strangler Fig pattern with YARP.
- **[Multi-Channel Concurrency & Double-Selling Prevention](docs/cin7-multi-channel-inventory.md):** Deep dive into how optimistic concurrency (`rowversion`) eliminates stockouts and overselling across Shopify, Amazon, and retail POS.
- **[SQL Server Indexing & Performance Strategy](docs/sql-server-indexing-and-performance.md):** Analysis of raw SQL execution plans, covered indexes, and Read Committed Snapshot Isolation (RCSI) for high-scale SaaS catalogs.
- **[Microsoft Azure Deployment Guide](docs/deploy-azure.md):** Automated script-driven deployment targeting Azure App Service and Azure SQL Database.

---

# Legacy Sub-Project: ASP.NET MVC 5 (.NET Framework 4.8)

In enterprise SaaS ecosystems (such as Cin7), modern cloud platforms frequently evolve alongside legacy systems. To model this real-world architecture without compromising the modern application, the repository contains a fully isolated **Legacy Sub-Project** (`legacy/StockFlow.LegacyMvc5/`) representing the pre-migration baseline.

### 1. Structure of the Legacy Subsystem

```text
legacy/
├── StockFlow.Legacy.sln                       # Isolated solution for .NET Framework 4.8
├── StockFlow.LegacyMvc5/                      # ASP.NET MVC 5 & Web API 2 Project (net48)
│   ├── Global.asax & Global.asax.cs          # HttpApplication pipeline (Application_Start, etc.)
│   ├── Web.config                             # Traditional XML configuration (system.web, handlers)
│   ├── App_Start/
│   │   ├── RouteConfig.cs                     # RouteCollection.MapRoute ({controller}/{action}/{id})
│   │   ├── WebApiConfig.cs                    # HttpConfiguration.Routes.MapHttpRoute (api/{controller}/{id})
│   │   └── FilterConfig.cs                    # GlobalFilterCollection (HandleErrorAttribute)
│   ├── Controllers/
│   │   ├── StockController.cs                 # System.Web.Mvc.Controller (Razor actions)
│   │   └── Api/LegacyStockApiController.cs    # System.Web.Http.ApiController (REST endpoints)
│   ├── Data/
│   │   ├── ILegacyStockRepository.cs          # Synchronous repository interface
│   │   └── LegacyAdoNetStockRepository.cs     # Synchronous ADO.NET with SQL UPDLOCK transactions
│   ├── Models/                                # Legacy DTOs and DataAnnotations
│   └── Views/Stock/Index.cshtml               # MVC 5 Razor View with HtmlHelper & AntiForgeryToken
└── StockFlow.LegacyMvc5.Tests/                # Unit test project (net48)
    ├── Controllers/LegacyStockApiControllerTests.cs  # Web API 2 tests using Moq
    └── Controllers/LegacyStockControllerTests.cs     # MVC 5 controller tests using Moq
```

### 2. Architectural Comparison: Legacy MVC 5 vs. Modern .NET 8

| Feature | Legacy Sub-Project (`legacy/StockFlow.LegacyMvc5`) | Modern Production Application (`/`) |
| :--- | :--- | :--- |
| **Target Framework** | `.NET Framework 4.8` (`net48`) | `.NET 8.0` (`net8.0`) |
| **Web Framework** | ASP.NET MVC 5 (`System.Web.Mvc`) & Web API 2 (`System.Web.Http`) | ASP.NET Core 8 MVC & Web API (`Microsoft.AspNetCore.Mvc`) |
| **Startup / Entry Point** | `Global.asax.cs` (`HttpApplication` lifecycle) | `Program.cs` (`WebApplicationBuilder` unified host) |
| **Pipeline Architecture** | `IHttpModule` & `IHttpHandler` with 20+ IIS events | Non-blocking linear `RequestDelegate` Middleware pipeline |
| **Configuration** | XML `Web.config` (`<system.web>`, `<system.webServer>`) | Hierarchical `appsettings.json` + environment variables |
| **State Management** | `System.Web.HttpContext.Current`, `TempData`, `ViewBag` | Dependency-injected `IHttpContextAccessor`, Scoped services |
| **Data Access** | Synchronous ADO.NET with explicit `SqlTransaction` and `UPDLOCK` | Entity Framework Core 8 with native async & `RowVersion` concurrency |
| **Hosting Model** | Strongly coupled to Windows IIS (`w3wp.exe`) | Kestrel cross-platform asynchronous web server (Linux/Windows/Docker) |

### 3. Running Legacy Tests
The legacy project is kept isolated from the main .NET 8 solution and has its own dedicated solution file:
```bash
dotnet test legacy/StockFlow.Legacy.sln
```

---

# Authentication & Authorization

Authentication is implemented using:

- ASP.NET Core Identity
- JWT Bearer Tokens

Authorization is implemented using:

- Admin
- Manager
- User

Example:

```csharp
[Authorize]

[Authorize(Roles = "Admin")]

[Authorize(Roles = "Admin,Manager")]
```

---

# Validation

Validation is implemented using **FluentValidation**.

Examples include:

- Required fields
- String length
- Positive prices
- Positive quantities
- Valid email addresses
- Duplicate SKU detection
- Duplicate Barcode detection
- Duplicate Category detection
- Duplicate Supplier Email detection
- Valid foreign key references

---

# Database Seeding

Automatically seeds:

- Categories
- Suppliers
- Products
- Purchase Orders
- Purchase Order Items
- Roles
- Administrator account

---

# Getting Started

## 1. Clone Repository

```bash
git clone https://github.com/mmitchell3397/IMS-WebAPI.git
```

---

## 2. Restore Packages

```bash
dotnet restore
```

---

## 3. Configure SQL Server

Update `appsettings.json` with your SQL Server connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=InventoryDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

---

## 4. Apply Database Migrations

Apply Entity Framework Core migrations to your SQL Server instance:

```bash
dotnet ef database update
```

---

## 5. Run Test Suite

Execute all unit and service tests across the solution:

```bash
dotnet test
```

---

## 6. Run Application

```bash
dotnet run
```

Swagger API documentation (configured in `Properties/launchSettings.json`):
```text
https://localhost:7011/swagger
http://localhost:5228/swagger
```
Stock adjustment web interface:
```text
https://localhost:7011/stock
```

---

# Test Results

### .NET Tests (xUnit)

Latest local execution: 64 passed, 0 failed, 13 skipped (77 total).
- 11 SQL Server integration tests skipped locally.
- 2 live AI/LLM tests skipped because external API credentials were unavailable.

### JavaScript Tests (Jest)

12 passed, 0 failed.

### End-to-End Tests (Playwright)

1 browser test passed.

### Legacy .NET Framework Tests

10 passed, 0 failed, 0 skipped.

### Code Coverage

- CI line coverage threshold: 7.0%.
- Coverlet generates Cobertura coverage reports.
- CI uploads test and coverage artifacts.

---
# Verification

Reviewers can independently verify each component using the exact commands below:

### 1. Build and Run .NET Tests
```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

### 2. Measure Code Coverage
```bash
dotnet test --configuration Release --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

### 3. Run JavaScript Unit Tests
```bash
cd tests/js
npm test
cd ../..
```

### 4. Run Playwright E2E Tests
```bash
# Terminal 1: Start the application
dotnet run --urls "http://localhost:5228"

# Terminal 2: Run Playwright test
cd tests/js
npx playwright test
cd ../..
```

---

# CI Pipeline

The project uses GitHub Actions through [`.github/workflows/ci.yml`](.github/workflows/ci.yml). The workflow runs on pushes and pull requests targeting `main`.

| Job | Environment & Tools | What It Does |
| :--- | :--- | :--- |
| **.NET Build, Test & Coverage** | Ubuntu, .NET 8, SQL Server 2022, Coverlet | Builds the application, runs .NET tests against SQL Server, checks integration-test execution, enforces a minimum 7.0% line coverage threshold, and uploads test and coverage reports. |
| **JavaScript Unit Tests** | Ubuntu, Node.js 20, Jest, jsdom | Installs dependencies and runs frontend unit tests. |
| **Playwright E2E Tests** | Ubuntu, .NET 8, Node.js 20, Chromium | Builds and starts the application, checks server readiness, runs browser tests, and uploads the Playwright report. |

The workflow also uses dependency caching to reduce repeated restore and installation time.

---

# Low-Stock SQL Query

The low-stock report endpoint (`GET /api/reports/low-stock?threshold=N`) executes the following parameterized raw SQL query via Entity Framework Core (`Database.SqlQuery<LowStockReportDto>`):

```sql
SELECT 
    p.Id AS ProductId,
    p.Name AS ProductName,
    p.SKU AS SKU,
    CAST(SUM(sl.Quantity) AS INT) AS CurrentStock,
    p.ReorderLevel AS ReorderLevel
FROM StockLevels sl
INNER JOIN Products p ON sl.ProductId = p.Id
GROUP BY p.Id, p.Name, p.SKU, p.ReorderLevel
HAVING SUM(sl.Quantity) <= @threshold
ORDER BY CurrentStock ASC
```

---

# What I Learned

This project helped strengthen my understanding of:

## ASP.NET Core

- Building RESTful APIs and MVC Razor Views
- Middleware Pipeline
- Dependency Injection
- Controller Design

## Entity Framework Core

- Code-First Development
- Relationships
- Fluent API
- Migrations
- Database Seeding

## Software Architecture

- Repository Pattern
- Service Layer
- Separation of Concerns
- DTO Design
- Extension Method Mappers

## Security

- ASP.NET Core Identity
- JWT Authentication
- Role-Based Authorization
- Password Policies
- Claims-Based Identity

## Validation

- FluentValidation
- Business Rule Validation
- Input Validation
- Custom Validation

## General Development

- Async/Await
- LINQ
- SQL Server
- HTTP Status Codes
- REST Best Practices
- Swagger Documentation
- API Testing with `.http` files

---

# Future Improvements

- Global Exception Middleware
- Custom Authorization Policies
- Centralized Structured Logging (e.g. Serilog)
- Pagination, Filtering & Sorting for large catalogs
- Redis Caching for frequently read inventory
- API Versioning (URL or header-based)
- Standalone SPA Frontend (React or Next.js)
- Advanced Analytics & Automated Reorder Webhooks

---

# Author

**Michael Mitchell**

GitHub

```
https://github.com/mmitchell3397
```

---

## License

This project is intended for educational and portfolio purposes.
