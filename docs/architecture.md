# StockFlow System Architecture

StockFlow is an enterprise-grade Inventory Management System providing high-throughput stock tracking, multi-warehouse inventory reconciliation, optimistic concurrency control, and AI-assisted replenishment planning.

---

## 1. Solution Architecture Overview

StockFlow is structured around a modern cloud-native .NET 8 Web API while maintaining an isolated legacy .NET Framework 4.8 ASP.NET MVC 5 sidecar to demonstrate real-world brownfield enterprise coexistence and migration patterns.

```mermaid
flowchart TD
    subgraph Clients["Clients & Consumers"]
        WebSPA["Web Frontend / SPA"]
        MobileApp["Mobile Scanner App"]
        LegacyBrowser["Legacy Web Browser"]
        IntegrationClient["B2B ERP Integration"]
    end

    subgraph ModernSystem["Modern System (.NET 8 Web API)"]
        direction TB
        ApiGateway["Kestrel Web Server / Reverse Proxy"]
        AuthMiddleware["JWT Authentication & RBAC Middleware"]
        ExceptionMiddleware["Exception Handling Middleware"]
        StockController["StockController"]
        ReportsController["ReportsController"]
        AdvisorController["StockAdvisorController"]
        StockService["StockService (OCC & Retries)"]
        ReportService["ReportService (Optimized SQL)"]
        AdvisorService["StockAdvisorService"]
        EFCoreContext["ApplicationDbContext (EF Core 8)"]
        LlmClient["ILlmClient (OpenAI / Mock)"]
    end

    subgraph LegacySidecar["Legacy Sidecar (.NET Framework 4.8)"]
        direction TB
        IIS["IIS / System.Web"]
        LegacyMvc["ASP.NET MVC 5 Controllers"]
        LegacyAdoNet["LegacyAdoNetStockRepository"]
    end

    subgraph Storage["Data Tier"]
        SqlServer[("SQL Server Database\n(RowVersion Concurrency)")]
    end

    WebSPA --> ApiGateway
    MobileApp --> ApiGateway
    IntegrationClient --> ApiGateway
    LegacyBrowser --> IIS

    ApiGateway --> AuthMiddleware --> ExceptionMiddleware
    ExceptionMiddleware --> StockController
    ExceptionMiddleware --> ReportsController
    ExceptionMiddleware --> AdvisorController

    StockController --> StockService
    ReportsController --> ReportService
    AdvisorController --> AdvisorService

    StockService --> EFCoreContext
    ReportService --> EFCoreContext
    AdvisorService --> LlmClient
    AdvisorService --> ReportService

    EFCoreContext --> SqlServer
    IIS --> LegacyMvc --> LegacyAdoNet --> SqlServer
```

---

## 2. Request Lifecycle & Middleware Pipeline

All incoming HTTP requests to the modern Web API traverse a structured ASP.NET Core middleware pipeline designed for security, error isolation, and operational logging.

```mermaid
sequenceDiagram
    autonumber
    actor Client as API Client
    participant Auth as JwtBearer / RBAC
    participant Exc as ExceptionMiddleware
    participant Ctrl as StockController
    participant Svc as StockService
    participant DB as SQL Server (EF Core)

    Client->>Auth: POST /api/stock/adjust (Bearer JWT)
    alt Missing or Invalid Token
        Auth-->>Client: 401 Unauthorized
    else Valid Token, Insufficient Role
        Auth-->>Client: 403 Forbidden
    else Valid Token with Admin/Manager Role
        Auth->>Exc: Forward Request
        Exc->>Ctrl: Invoke AdjustStockAction
        Ctrl->>Svc: AdjustStockAsync(dto)
        loop Up to 3 Retries on Concurrency Collision
            Svc->>DB: Begin Transaction & Reload Entity State
            alt Insufficient Stock (Quantity + Delta < 0)
                Svc-->>Exc: Throw InsufficientStockException
                Exc-->>Client: 409 Conflict (Insufficient Stock)
            else Stock Sufficient
                Svc->>DB: UPDATE Products & StockLevels (Check RowVersion)
                alt Concurrency Collision (RowVersion mismatch)
                    DB-->>Svc: DbUpdateConcurrencyException
                    Svc->>Svc: Exponential Backoff & Jitter
                else Update Successful
                    DB-->>Svc: Commit Transaction
                    Svc-->>Ctrl: Return Updated StockDto
                    Ctrl-->>Client: 200 OK (Adjusted Stock)
                end
            end
        end
    end
```

---

## 3. Data Model & Entity Relationships

The relational schema coordinates inventory across catalog summaries and physical warehouse locations:

```mermaid
erDiagram
    CATEGORIES ||--o{ PRODUCTS : "classifies"
    SUPPLIERS ||--o{ PRODUCTS : "supplies"
    PRODUCTS ||--o{ STOCK_LEVELS : "tracks per location"
    LOCATIONS ||--o{ STOCK_LEVELS : "stores inventory at"

    CATEGORIES {
        int Id PK
        string Name
        string Description
    }

    SUPPLIERS {
        int Id PK
        string Name
        string ContactEmail
        string Phone
    }

    PRODUCTS {
        int Id PK
        string Name
        string SKU UK
        int QuantityInStock
        int ReorderLevel
        decimal Price
        decimal Cost
        byte[] RowVersion
        int CategoryId FK
        int SupplierId FK
    }

    LOCATIONS {
        string Id PK
        string Name
        string Description
    }

    STOCK_LEVELS {
        int Id PK
        int ProductId FK
        string LocationId FK
        int Quantity
        byte[] RowVersion
    }
```

---

## 4. Multi-Warehouse Stock Synchronization

To preserve zero breaking changes for existing catalog consumers while offering warehouse-level granularity, StockFlow synchronizes catalog totals atomically:

1. **Warehouse-Specific Adjustments (`POST /api/stock/adjust` with `LocationId`):**
   - Validates that `LocationId` exists in `Locations`.
   - Loads the specific `StockLevel` row and the aggregate `Product` row within a single database transaction.
   - Verifies that `StockLevel.Quantity + Delta >= 0` and `Product.QuantityInStock + Delta >= 0`.
   - Modifies `StockLevel.Quantity` and `Product.QuantityInStock` simultaneously.
   - Evaluates optimistic concurrency tokens (`RowVersion`) on both entities before committing.

2. **Catalog-Level Adjustments (`POST /api/stock/adjust` without `LocationId`):**
   - Directly updates `Product.QuantityInStock` with concurrency retry handling.
   - Preserves backward compatibility for automated integrations that do not track physical warehouse bins.

---

## 5. AI Replenishment Advisor Architecture

The AI Replenishment Advisor integrates automated stock replenishment with LLMs through a provider-agnostic abstraction:

```mermaid
flowchart LR
    ReportService["ReportService\n(Low-Stock Query)"] --> AdvisorService["StockAdvisorService"]
    AdvisorService --> PromptBuilder["Replenishment Prompt\nBuilder & Sanitizer"]
    PromptBuilder --> ILlmClient{"ILlmClient\nInterface"}
    ILlmClient -->|Production| OpenAiClient["OpenAiLlmClient\n(REST API)"]
    ILlmClient -->|Testing / Local| MockClient["MockLlmClient\n(Deterministic DTO)"]
    ILlmClient -->|Fallback on Failure| RuleEngine["Deterministic Fallback\nRule Engine"]
    OpenAiClient --> OpenAICloud["OpenAI / LLM API"]
```

- **Resilience:** If the LLM provider experiences network latency, rate limits, or invalid API credentials, `StockAdvisorService` automatically falls back to an algorithmic calculation:
  $$\text{ReorderQuantity} = \max((\text{ReorderLevel} \times 2) - \text{CurrentStock}, 10)$$
- **Security:** Product identifiers and quantities are formatted without leaking internal database keys, customer identifiers, or proprietary margins to external LLMs.
