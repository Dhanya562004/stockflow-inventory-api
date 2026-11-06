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
- [Authentication & Authorization](#authentication--authorization)
- [Validation](#validation)
- [Database Seeding](#database-seeding)
- [Getting Started](#getting-started)
- [Test Results](#test-results)
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

All tests are implemented in the `InventoryManagementSystemAPI.Tests` project using xUnit, FluentValidation, and WebApplicationFactory:

```text
Test run for InventoryManagementSystemAPI.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    34, Skipped:     5, Total:    39, Duration: 2.1 s - InventoryManagementSystemAPI.Tests.dll (net8.0)
```

### Test Suite Breakdown
- **Validation Tests (22 tests):** SKU length & requirements, positive prices, costs, quantities, reorder levels, foreign key GUID rules, stock adjustment delta rules.
- **Product Service Tests (6 tests):** Duplicate SKU prevention, duplicate barcode detection, retrieval, update, and deletion rules without database dependencies.
- **Stock Adjustment Service Tests (6 tests):** Valid adjustments staying positive, adjustments hitting exactly zero, adjustments rejected due to negative balance, transient concurrency retry recovery, and exceeding retry limits (3 attempts).
- **Reports Tests (2 tests):** Low-stock report filtering products strictly at or below threshold and rejecting negative threshold inputs.
- **Stock View & Controller Tests (2 tests):** Razor view returns valid ViewResult and form action matches `POST /api/stock/adjust`.
- **Integration Tests (5 tests, skipped on local host without active Docker daemon, run in CI):** WebApplicationFactory endpoint coverage (200, 404, 409, 400) and 20-client concurrent adjustment test against SQL Server container.

---

# CI Pipeline

The project features an automated GitHub Actions CI pipeline ([`.github/workflows/ci.yml`](.github/workflows/ci.yml)) configured with caching, multiple parallel jobs, and strict quality gates:

| Job | Environment & Tools | What It Proves |
| :--- | :--- | :--- |
| **`.NET Build, Test & Coverage`** | Ubuntu, .NET 8, SQL Server 2022 service container, Coverlet | Verifies compilation in Release mode, runs all 48 .NET tests against a live SQL Server instance, guarantees zero integration tests are skipped in CI via automated TRX inspection, enforces a minimum 7.0% line coverage gate against the measured baseline (7.26%), and uploads TRX and Cobertura XML artifacts. |
| **`JavaScript Unit Tests`** | Ubuntu, Node.js 20, Jest, jsdom | Proves `wwwroot/js/stock.js` correctly handles every HTTP status code (200, 400 validation, 404, 409 negative balance, 500 retry), updates DOM via `textContent` (preventing XSS), manages button loading states, and renders product catalog tables and empty states without framework dependencies. |
| **`Playwright E2E Tests`** | Ubuntu, Node.js 20, .NET 8, Chromium | Starts the ASP.NET Core web server on Kestrel (`http://localhost:5228`), boots headless Chromium, navigates to `/stock`, fills the form, submits an adjustment, and verifies end-to-end rendering and quantity updates in a real browser. |

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
