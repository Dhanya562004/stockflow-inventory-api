# Inventory Management System API

[![Build & Test](https://github.com/Dhanya562004/stockflow-inventory-api/actions/workflows/ci.yml/badge.svg)](https://github.com/Dhanya562004/stockflow-inventory-api/actions/workflows/ci.yml)

A production-inspired **Inventory Management System REST API** built with **ASP.NET Core 8**, **Entity Framework Core**, and **SQL Server**. This project demonstrates modern backend development practices including layered architecture, authentication, authorization, validation, and clean API design.

> **Credits:** Original project by Michael Mitchell ([github.com/mmitchell3397](https://github.com/mmitchell3397)). Extensions in this fork: stock adjustment rule, low-stock SQL report, xUnit tests.

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
- **Implementation:** Executes a parameterized raw SQL query through EF Core (`Database.SqlQuery<LowStockReportDto>`), performing a `JOIN` between products and stock data with a `GROUP BY` and a parameterized `HAVING` clause to prevent SQL injection.
- **Raw SQL Query:**
```sql
SELECT 
    p.Id AS ProductId,
    p.Name AS ProductName,
    p.SKU AS SKU,
    MIN(stock.QuantityInStock) AS CurrentStock,
    p.ReorderLevel AS ReorderLevel
FROM Products p
INNER JOIN Products stock ON p.Id = stock.Id
GROUP BY p.Id, p.Name, p.SKU, p.ReorderLevel
HAVING MIN(stock.QuantityInStock) <= @threshold
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

Swagger API documentation:
```text
https://localhost:xxxx/swagger
```

---

# Test Results

All tests are implemented in the `InventoryManagementSystemAPI.Tests` project using xUnit and FluentValidation:

```text
Test run for InventoryManagementSystemAPI.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    32, Skipped:     0, Total:    32, Duration: 274 ms - InventoryManagementSystemAPI.Tests.dll (net8.0)
```

### Test Coverage Highlights
- **Validation Tests (22 tests):** SKU max length, empty SKU, price > 0, cost > 0, quantity >= 0, reorder level >= 0, foreign key GUID rules, stock adjustment delta rules.
- **Product Service Tests (6 tests):** Duplicate SKU prevention, duplicate barcode detection, retrieval, update, and deletion rules without database dependencies.
- **Stock Adjustment Service Tests (6 tests):** Valid adjustments staying positive, adjustments hitting exactly zero, adjustments rejected due to negative balance, transient concurrency retry recovery, and exceeding retry limits (3 attempts).
- **Reports Tests (2 tests):** Low-stock report filtering products strictly at or below threshold and rejecting negative threshold inputs.

---

# Low-Stock SQL Query

The low-stock report endpoint (`GET /api/reports/low-stock?threshold=N`) executes the following parameterized raw SQL query via Entity Framework Core (`Database.SqlQuery<LowStockReportDto>`):

```sql
SELECT 
    p.Id AS ProductId,
    p.Name AS ProductName,
    p.SKU AS SKU,
    MIN(stock.QuantityInStock) AS CurrentStock,
    p.ReorderLevel AS ReorderLevel
FROM Products p
INNER JOIN Products stock ON p.Id = stock.Id
GROUP BY p.Id, p.Name, p.SKU, p.ReorderLevel
HAVING MIN(stock.QuantityInStock) <= @threshold
ORDER BY CurrentStock ASC
```

---

# What I Learned

This project helped strengthen my understanding of:

## ASP.NET Core

- Building RESTful APIs
- Middleware Pipeline
- Dependency Injection
- Controller Design

## Entity Framework Core

- Code-First Development
- Relationships
- Fluent API
- Migrations
- Database Seeding

## Software Architectureche

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
- Authorization Policies
- Custom Exceptions
- Logging with Serilog
- Pagination
- Filtering
- Searching
- Sorting
- Redis Caching
- API Versioning
- Unit Testing
- Integration Testing
- Docker Support
- CI/CD Pipeline
- Azure Deployment
- React Frontend
- Inventory Reporting Dashboard

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
