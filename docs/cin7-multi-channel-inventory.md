# Multi-Channel Inventory Synchronization & Concurrency Architecture

> **Domain Case Study: Solving the "Double-Selling" Dilemma in Modern E-Commerce Platforms**  
> *Targeted for Cin7 Multi-Channel SaaS Operations (Shopify, Amazon, WooCommerce, In-Store POS).*

---

## 1. The Multi-Channel Problem Statement

In omnichannel commerce (Cin7's core vertical), a merchant sells inventory across multiple disconnected storefronts and marketplaces simultaneously:
- **Shopify Online Store**
- **Amazon Marketplace**
- **In-Store Retail POS**
- **B2B Wholesale Portal**

All channels typically fulfill from a shared central warehouse inventory pool.

### The Race Condition Scenario:
Imagine a high-value product with **1 unit remaining in stock**:
1. At `12:00:00.100`, Customer A purchases the unit on **Shopify**.
2. At `12:00:00.105`, Customer B purchases the unit on **Amazon**.
3. Both channel listeners issue `POST /api/stock/adjust` with `delta = -1`.
4. In naive inventory systems without strict concurrency locking:
   - Request A reads `Quantity = 1`.
   - Request B reads `Quantity = 1`.
   - Request A writes `Quantity = 0` (Succeeds).
   - Request B writes `Quantity = 0` or `-1` (Succeeds).
5. **Outcome**: The seller has sold 2 physical units when only 1 exists, triggering fulfillment failure, order cancellation, and Amazon seller account demerits.

---

## 2. StockFlow's Concurrency Protection Architecture

StockFlow solves this race condition using **Optimistic Concurrency Control** backed by SQL Server's native `rowversion` column and Entity Framework Core 8.

```mermaid
sequenceDiagram
    autonumber
    actor Shopify as Shopify Webhook
    actor Amazon as Amazon Webhook
    participant API as StockFlow API (StockController)
    participant Service as StockService
    participant SQL as SQL Server (Products Table)

    Note over SQL: Product X: Quantity=1, RowVersion=0x0001
    
    Shopify->>API: POST /api/stock/adjust (delta=-1)
    Amazon->>API: POST /api/stock/adjust (delta=-1)
    
    API->>Service: AdjustStockAsync(Product X, -1)
    Service->>SQL: SELECT Product (reads Quantity=1, RV=0x0001)
    
    API->>Service: AdjustStockAsync(Product X, -1)
    Service->>SQL: SELECT Product (reads Quantity=1, RV=0x0001)
    
    Note over Service: Both calculate newQuantity = 0
    
    Service->>SQL: UPDATE Products SET Qty=0 WHERE Id=X AND RowVersion=0x0001
    SQL-->>Service: 1 row affected (Succeeds! New RV=0x0002)
    Service-->>Shopify: 200 OK (New Quantity = 0)
    
    Service->>SQL: UPDATE Products SET Qty=0 WHERE Id=X AND RowVersion=0x0001
    SQL-->>Service: 0 rows affected (Concurrency Conflict Detected!)
    
    Note over Service: EF Core throws DbUpdateConcurrencyException
    Service->>Service: Jittered Backoff Retry (re-reads Quantity=0, RV=0x0002)
    Note over Service: 0 + (-1) = -1 (Violates non-negative invariant!)
    
    Service-->>Amazon: 409 Conflict ("stock would go negative")
```

---

## 3. Implementation Breakdown in StockFlow

### 1. Database Model Concurrency Token
In `Models/Product.cs`, the entity declares a byte array decorated as a concurrency token:
```csharp
[Timestamp]
public byte[]? RowVersion { get; set; }
```
In SQL Server, EF Core configures this column as `ROWVERSION` (`TIMESTAMP`). The SQL Server storage engine automatically assigns a monotonically increasing binary number whenever a record is inserted or updated.

### 2. Jittered Exponential Backoff in `StockService.cs`
When concurrent webhooks arrive simultaneously, StockFlow does not fail immediately. It attempts an intelligent retry loop with randomized jitter:
```csharp
for (int attempt = 1; attempt <= maxRetries; attempt++)
{
    try
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product == null) return (false, 404, "product not found", 0);

        if (product.QuantityInStock + delta < 0)
        {
            return (false, 409, "stock would go negative", product.QuantityInStock);
        }

        product.QuantityInStock += delta;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, 200, "Stock adjusted successfully.", product.QuantityInStock);
    }
    catch (DbUpdateConcurrencyException) when (attempt < maxRetries)
    {
        // Add random jitter between 25ms and 75ms to desynchronize concurrent threads
        var jitterMs = Random.Shared.Next(25, 75);
        await Task.Delay(jitterMs, cancellationToken);
    }
}
```

### 3. Verification Under Load
StockFlow includes an automated integration test (`StockEndpointIntegrationTests.cs`) simulating **20 parallel concurrent stock deductions** against an item with only 10 available units. Under concurrent execution:
- Exactly **10 requests succeed (HTTP 200)**.
- Exactly **10 requests are rejected (HTTP 409 Conflict)** with `"stock would go negative"`.
- The final verified database quantity is guaranteed to be exactly **0** (never negative).

---

## 4. Multi-Warehouse & Channel Allocation Mapping

In a full enterprise deployment, this pattern extends to **Warehouse Locations**:
- `Available Stock` = `Total Physical Stock` - `Allocated / Reserved Stock`.
- Stock adjustments from POS directly decrement physical inventory.
- Stock adjustments from e-commerce channels place a reservation hold prior to warehouse dispatch, all guarded by the same atomic concurrency guarantees.
