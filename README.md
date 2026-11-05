# Inventory Management System API

A production-inspired **Inventory Management System REST API** built with **ASP.NET Core 8**, **Entity Framework Core**, and **SQL Server**. This project demonstrates modern backend development practices including layered architecture, authentication, authorization, validation, and clean API design.

---

# Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [NuGet Packages](#nuget-packages)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Database Design](#database-design)
- [API Resources](#api-resources)
- [Authentication & Authorization](#authentication--authorization)
- [Validation](#validation)
- [Database Seeding](#database-seeding)
- [Getting Started](#getting-started)
- [API Demonstration](#api-demonstration)
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

## Identity

- Register
- Login
- Current User
- Change Password
- Assign Role

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

## Clone Repository

```bash
git clone https://github.com/mmitchell3397/IMS-WebAPI.git
```

---

## Restore Packages

```bash
dotnet restore
```

---

## Configure SQL Server

Update:

```text
appsettings.json
```

Configure:

```json
ConnectionStrings
```

---

## Apply Migrations

```bash
dotnet ef database update
```

---

## Run

```bash
dotnet run
```

Swagger:

```text
https://localhost:xxxx/swagger
```

---

# API Demonstration

The following screenshots demonstrate the application's functionality.

## Authentication

### Register

![Register](docs/images/register.jpeg)

Creates a new user account.

---

### Login

![Login](docs/images/login.jpeg)

Returns a JWT Bearer Token.

> **Note:** Make sure to click the ***Authorize*** button to add your ***JWT Bearer Token*** before continuing.
---

### Get Current User

![Current User](docs/images/current_user.jpeg)

Retrieves the authenticated user's information.

---

### Change Password

![Change Password](docs/images/change_password.jpeg)

Changes the authenticated user's password.

---

### Assign Role

![Assign Role](docs/images/assign_role.jpeg)

Administrator assigning a role to another user.

>**Note:** The ***IdentityDataSeeder.cs*** class includes an Admin account with a password and role assigned.
---

# Product Endpoints
>**Note:** All other models will follow the same pattern.

### Get All Products

![Get Products](docs/images/get_all_products.jpeg)

---

### Get Product By ID

![Get Product](docs/images/get_product_by_id.jpeg)

---

### Create Product

![Create Product](docs/images/create_product.jpeg)

>**NOTE:** Requires ***Admin*** or ***Manager*** level authorization.

---

### Update Product

![Update Product](docs/images/update_product.jpeg)

>**NOTE:** Requires ***Admin*** or ***Manager*** level authorization.

---

### Delete Product

![Delete Product](docs/images/delete_product.jpeg)

>**NOTE:** Requires ***Admin*** level authorization.

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
