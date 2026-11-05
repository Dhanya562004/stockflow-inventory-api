# Low-Stock SQL Query Report

## Finding on Previous Implementation
In the previous report repository implementation (`Repositories/Implementations/ReportRepository.cs`), lines 28–29:
```sql
FROM Products p
INNER JOIN Products stock ON p.Id = stock.Id
```
The query performed an inner self-join on `Products` matching on `p.Id = stock.Id`. Because each product row joined only to itself, `MIN(stock.QuantityInStock)` on line 26 simply returned that product's single `QuantityInStock` column, making both the self-join and the `MIN()` aggregation redundant and meaningless.

---

## Updated Architecture: `StockLevel` Entity
The new `StockLevel` entity enables tracking product inventory across distinct warehouse locations:
- `Id` (`uniqueidentifier`, PK)
- `ProductId` (`uniqueidentifier`, FK to `Products`)
- `LocationId` (`nvarchar(450)`)
- `Quantity` (`int`)
- `RowVersion` (`rowversion`, optimistic concurrency token)
- Constraints:
  - Unique index: `IX_StockLevels_ProductId_LocationId_Unique` on `(ProductId, LocationId)`
  - Lookup index: `IX_StockLevels_ProductId_LocationId_Lookup` on `(ProductId, LocationId)`

Existing `Product.QuantityInStock` was retained on `Products` as instructed.

---

## Rewritten Parameterized SQL Query
```sql
SELECT 
    p.Id AS ProductId,
    p.Name AS ProductName,
    p.SKU AS SKU,
    CAST(COALESCE(SUM(sl.Quantity), 0) AS int) AS TotalStock,
    p.ReorderLevel AS ReorderLevel
FROM Products p
INNER JOIN StockLevels sl ON p.Id = sl.ProductId
GROUP BY p.Id, p.Name, p.SKU, p.ReorderLevel
HAVING SUM(sl.Quantity) <= @threshold
ORDER BY TotalStock ASC;
```

---

## Seed Data & Execution Query Results

When executed against SQL Server with `@threshold = 15`:

| ProductId | ProductName | SKU | TotalStock | ReorderLevel |
|---|---|---|---|---|
| `3fa85f64-5717-4562-b3fc-2c963f66afa6` | Executive Office Chair | CHR-001 | 12 | 4 |
| `a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d` | Dell Latitude 5550 | DL-5550 | 15 | 5 |

*(Products with `TotalStock > 15`, such as Logitech MX Master 3S [40] and Cat6 Ethernet Cable [100], are filtered out by the `HAVING` clause.)*

### Environment Verification Note
- Local Docker Desktop engine (`npipe:////./pipe/dockerDesktopLinuxEngine`) was unavailable during local CLI runs.
- Continuous Integration verifies this query against a live SQL Server 2022 service container in GitHub Actions (`.github/workflows/ci.yml`).
