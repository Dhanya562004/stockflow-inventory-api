# StockFlow Architectural Decision Records (ADRs)

This document captures the key architectural and design decisions made in StockFlow, the context behind them, alternatives evaluated, and associated trade-offs.

---

## Index of Decisions
- [ADR-001: Optimistic Concurrency via RowVersion vs. Pessimistic Locking](#adr-001-optimistic-concurrency-via-rowversion-vs-pessimistic-locking)
- [ADR-002: Parameterized Raw SQL for Low-Stock Aggregation Reporting](#adr-002-parameterized-raw-sql-for-low-stock-aggregation-reporting)
- [ADR-003: ILlmClient Abstraction for AI Replenishment Advisor](#adr-003-illmclient-abstraction-for-ai-replenishment-advisor)
- [ADR-004: Isolated Legacy .NET Framework 4.8 / ASP.NET MVC 5 Sidecar](#adr-004-isolated-legacy-net-framework-48--aspnet-mvc-5-sidecar)
- [ADR-005: Multi-Warehouse Inventory Model with Catalog Totals Synchronization](#adr-005-multi-warehouse-inventory-model-with-catalog-totals-synchronization)
- [ADR-006: HTTP 409 Conflict for Negative Stock Prevention](#adr-006-http-409-conflict-for-negative-stock-prevention)

---

## ADR-001: Optimistic Concurrency via RowVersion vs. Pessimistic Locking

### Status
Accepted

### Context
In high-throughput e-commerce and warehouse inventory environments, multiple concurrent threads or external API clients may attempt to adjust stock levels for the same SKU simultaneously. Without concurrency controls, interleaving read-modify-write operations lead to lost updates and severe inventory discrepancies (overselling).

We needed a concurrency control strategy that prevents lost updates while maintaining high throughput and minimizing database lock contention.

### Decision
Implement **Optimistic Concurrency Control (OCC)** using SQL Server `rowversion` / `byte[]` timestamp tokens mapped to EF Core entities (`Product.RowVersion` and `StockLevel.RowVersion`).
- When a stock adjustment occurs, EF Core includes the entity's original `RowVersion` in the SQL `UPDATE ... WHERE Id = @id AND RowVersion = @originalRowVersion`.
- If another transaction modified the row concurrently, zero rows are affected, causing EF Core to throw `DbUpdateConcurrencyException`.
- The application wraps the operation in an exponential backoff with jitter retry loop (up to 3 retries). On retry, the entity state is reloaded from the database to recalculate the delta against fresh inventory balances.
- If retries are exhausted, the API returns `HTTP 409 Conflict`.

### Alternatives Considered
1. **Pessimistic Locking (`SELECT ... WITH (UPDLOCK, ROWLOCK)` or Serializable Isolation):**
   - *Rejected because:* Holding exclusive locks during service execution degrades read/write concurrency, risks distributed deadlocks under high load, and impairs scalability across web server nodes.
2. **Atomic SQL Scripting (`UPDATE Products SET Quantity = Quantity + @delta WHERE ...`):**
   - *Rejected because:* StockFlow requires domain entity lifecycle hooks, transaction audit trails, warehouse-level reconciliation, and validation across multiple tables (`Product` and `StockLevel`), which pure atomic SQL scripts complicate.

### Consequences
- **Positive:** Maximum read and write concurrency under low-to-medium contention; non-blocking reads; eliminates database-level table and row deadlocks.
- **Trade-off:** High contention on a single SKU causes retries and increased CPU cycles. If contention exceeds retry limits, clients must handle HTTP 409 Conflict responses.

---

## ADR-002: Parameterized Raw SQL for Low-Stock Aggregation Reporting

### Status
Accepted

### Context
The low-stock report (`GET /api/reports/low-stock`) evaluates current inventory against reorder thresholds across thousands of products. It requires aggregating quantities across multiple warehouse `StockLevels`, evaluating products that have zero stock records, and joining `Categories` and `Suppliers` for reporting.

### Decision
Execute parameterized raw SQL via ADO.NET / EF Core `FromSqlInterpolated` for aggregated reporting, mapping results directly to a lightweight `LowStockItemDto`.

```sql
SELECT p.Id AS ProductId, p.Name AS ProductName, p.SKU,
       ISNULL(SUM(sl.Quantity), p.QuantityInStock) AS CurrentStock,
       p.ReorderLevel, c.Name AS CategoryName, s.Name AS SupplierName
FROM Products p
LEFT JOIN StockLevels sl ON sl.ProductId = p.Id
LEFT JOIN Categories c ON c.Id = p.CategoryId
LEFT JOIN Suppliers s ON s.Id = p.SupplierId
GROUP BY p.Id, p.Name, p.SKU, p.QuantityInStock, p.ReorderLevel, c.Name, s.Name
HAVING ISNULL(SUM(sl.Quantity), p.QuantityInStock) <= @threshold
```

### Alternatives Considered
1. **Full LINQ-to-Entities Projection via EF Core:**
   - *Rejected because:* Multi-table `LEFT JOIN` aggregations with `GROUP BY` and `HAVING` clauses often generate suboptimal SQL in EF Core, produce complex Cartesian subqueries, and allocate unnecessary tracking objects in memory.
2. **Precalculated Materialized Views or Read Replicas:**
   - *Rejected because:* Introduces operational complexity and delayed consistency that is unnecessary for our current inventory scale.

### Consequences
- **Positive:** Sub-millisecond query execution, zero entity tracking overhead, direct database index utilization, and minimal memory allocation.
- **Trade-off:** Raw SQL queries are not checked by the compiler during refactoring; schema changes require updating SQL strings and associated unit/integration tests.

---

## ADR-003: ILlmClient Abstraction for AI Replenishment Advisor

### Status
Accepted

### Context
StockFlow includes an AI Replenishment Advisor (`StockAdvisorService`) that analyzes low-stock items and suggests optimal reorder quantities with contextual justifications. Coupling the domain service directly to a third-party SDK (such as OpenAI or Azure OpenAI) creates architectural fragility, hinders automated CI testing, and incurs unwanted API costs during development.

### Decision
Introduce an `ILlmClient` abstraction defining an asynchronous contract:
```csharp
public interface ILlmClient
{
    Task<string> GetCompletionAsync(string prompt, CancellationToken cancellationToken = default);
}
```
- In production, `OpenAiLlmClient` communicates with the OpenAI REST API using standard HTTP client patterns.
- In unit and integration test suites, `MockLlmClient` provides deterministic, zero-latency, zero-cost synthetic responses.
- The service gracefully falls back to deterministic rule-based replenishment calculations if LLM calls fail or if API keys are unconfigured.

### Alternatives Considered
1. **Direct Coupling to OpenAI SDK:**
   - *Rejected because:* Prevents offline unit testing, breaks automated CI runs without live API keys, and tightly couples the codebase to a single vendor.
2. **Hardcoded Rule Engine Without AI:**
   - *Rejected because:* Natural language reasoning provides contextual summaries for warehouse managers (e.g. factoring supplier lead times and historical velocity), while the abstraction guarantees zero risk of vendor lock-in.

### Consequences
- **Positive:** 100% testable without network access or cloud API credentials; zero test costs; pluggable support for alternate models (e.g., Anthropic, Gemini, local Ollama).
- **Trade-off:** Live OpenAI API responses require manual or staging verification since automated CI relies on mock implementations.

---

## ADR-004: Isolated Legacy .NET Framework 4.8 / ASP.NET MVC 5 Sidecar

### Status
Accepted

### Context
Enterprise inventory ecosystems frequently contain brownfield .NET Framework systems that must coexist or integrate with modern .NET 8 cloud microservices. We needed to model this real-world architectural pattern without compromising the performance, dependency tree, or cross-platform CI of the modern .NET 8 codebase.

### Decision
Isolate the legacy ASP.NET MVC 5 / Web API 2 application in a dedicated directory (`legacy/StockFlow.LegacyMvc5`) with its own solution file (`legacy/StockFlow.Legacy.sln`) and an independent GitHub Actions workflow (`legacy-ci.yml` running on `windows-latest`).

### Alternatives Considered
1. **Multi-Targeting Modern Projects (`<TargetFrameworks>net8.0;net48</TargetFrameworks>`):**
   - *Rejected because:* Modern EF Core 8 and ASP.NET Core libraries are incompatible with `System.Web` and EF 6. Multi-targeting caused MSBuild SDK globbing conflicts, assembly binding errors, and blocked Linux-based CI builds.
2. **Immediate Full Rewrite / Decommissioning:**
   - *Rejected because:* In enterprise environments, legacy applications often remain operational for years during staged migrations; maintaining the legacy sidecar demonstrates brownfield ADO.NET patterns and side-by-side migration strategies.

### Consequences
- **Positive:** Modern .NET 8 CI runs fast on Linux containers; legacy .NET Framework 4.8 code builds cleanly on Windows runners with MSBuild; zero dependency pollution between old and new architectures.
- **Trade-off:** Requires maintaining two distinct CI workflow files and managing independent dependency configurations.

---

## ADR-005: Multi-Warehouse Inventory Model with Catalog Totals Synchronization

### Status
Accepted

### Context
Originally, StockFlow tracked stock exclusively at the product catalog level (`Product.QuantityInStock`). Supporting multi-warehouse distribution required introducing warehouse-specific `StockLevel` entities tied to physical `Location` entities, while maintaining backward compatibility with legacy consumers expecting catalog-level totals.

### Decision
Implement a hybrid normalization strategy:
- `StockLevel` records store physical warehouse inventory per `(ProductId, LocationId)`.
- `Product.QuantityInStock` is retained as the authoritative aggregate catalog stock.
- Warehouse-specific adjustments update both `StockLevel.Quantity` and `Product.QuantityInStock` atomically within a single database transaction.
- Adjustments omitting `LocationId` update catalog-level stock directly, preserving legacy API behavior.

### Alternatives Considered
1. **Pure Normalization (Dropping `Product.QuantityInStock`):**
   - *Rejected because:* Calculating total stock via dynamic `SUM(StockLevels.Quantity)` on every catalog query would degrade product listing performance and break contracts with existing API clients.
2. **Eventual Consistency via Message Bus:**
   - *Rejected because:* Asynchronous aggregation introduces race conditions and eventual consistency lag, allowing duplicate purchases or overselling before aggregate totals synchronize.

### Consequences
- **Positive:** Full backward compatibility with existing clients; fast catalog reads without dynamic aggregation; atomic guarantees between warehouse and catalog balances.
- **Trade-off:** Stock adjustment transactions must update and concurrency-lock both `Product` and `StockLevel` rows within the same atomic transaction.

---

## ADR-006: HTTP 409 Conflict for Negative Stock Prevention

### Status
Accepted

### Context
When a client requests a negative stock adjustment that exceeds available inventory, the operation must be rejected to prevent negative inventory balances. We needed to choose the appropriate HTTP semantics and error payload structure.

### Decision
Throw a domain-specific `InsufficientStockException` that is caught by `ExceptionMiddleware` and mapped to `HTTP 409 Conflict`:
```json
{
  "title": "Insufficient Stock",
  "status": 409,
  "detail": "Cannot adjust stock. Requested: -15, Available: 10."
}
```

### Alternatives Considered
1. **HTTP 400 Bad Request:**
   - *Rejected because:* The request syntax and DTO validation are completely valid. The rejection is due to the current state of the resource (conflict with current inventory balance).
2. **HTTP 422 Unprocessable Entity:**
   - *Rejected because:* In REST API conventions, HTTP 409 Conflict accurately represents state and concurrency conflicts.
3. **Allowing Negative Stock (Backorders):**
   - *Rejected because:* StockFlow's domain rules explicitly prohibit negative inventory unless a backorder feature is intentionally enabled.

### Consequences
- **Positive:** Semantic alignment with HTTP RFC specifications; API consumers can unambiguously differentiate syntax/validation errors (`400`) from inventory state conflicts (`409`).
- **Trade-off:** Clients must inspect error payloads to differentiate concurrency version collisions from insufficient stock conflicts (both return 409).
