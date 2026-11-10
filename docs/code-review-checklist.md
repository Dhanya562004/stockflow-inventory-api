# StockFlow Code Review Checklist

This checklist defines the engineering and code quality standards required for all pull requests to StockFlow. Reviewers and contributors must verify each category before approving changes.

---

## 1. Authorization & Authentication
- [ ] **Explicit Authorization Attributes:** Every controller action is explicitly decorated with either `[Authorize]` (with appropriate roles, e.g. `[Authorize(Roles = "Admin,Manager")]`) or `[AllowAnonymous]`. No endpoint is left implicitly public without architectural justification.
- [ ] **Least Privilege Principle:** Write operations (e.g., stock adjustments, product creation/deletion) require elevated roles (`Admin`, `Manager`). Read-only endpoints use basic authentication or documented anonymous access.
- [ ] **Role & Claim Verification:** Identity claims (user IDs, tenant IDs, roles) are extracted and verified from `ClaimsPrincipal` rather than trusted blindly from client request bodies.
- [ ] **No Secrets in Code:** No JWT signing keys, database passwords, or third-party API keys are hardcoded in source files or test files. All credentials reside in environment variables or user secrets.

---

## 2. Input Validation & Data Integrity
- [ ] **Boundary & Format Validation:** All request DTOs validate incoming boundaries (e.g. non-empty strings, positive quantities, non-zero adjustment deltas, valid SKU formats).
- [ ] **Entity Existence Checks:** Foreign keys and lookup IDs (e.g., `ProductId`, `LocationId`) are validated against database records before performing mutations.
- [ ] **Defensive Invariant Checks:** Domain entities enforce business invariants (e.g., stock levels cannot be negative unless explicitly permitted by domain rules).
- [ ] **Validation Layer Separation:** Validation rules are cleanly defined via FluentValidation validators or DataAnnotations, rather than scattered across controllers.

---

## 3. Concurrency & Database Transactions
- [ ] **Atomic Multi-Entity Updates:** When modifying multiple interdependent records (e.g., updating a warehouse-specific `StockLevel` and syncing catalog-level `Product.QuantityInStock`), all updates execute within an atomic transaction.
- [ ] **Optimistic Concurrency Protection:** Entities subject to concurrent mutations utilize `RowVersion` / `byte[]` timestamp tokens with EF Core concurrency conflict detection (`DbUpdateConcurrencyException`).
- [ ] **Retry Strategy with State Reload:** On concurrency conflict retries, fresh entity state is reloaded from the database before re-evaluating business rules and re-applying deltas.
- [ ] **Resilient Execution Strategies:** When using EF Core execution strategies (`EnableRetryOnFailure`), manual database transactions are wrapped in `executionStrategy.ExecuteAsync(...)`.
- [ ] **Preventing Duplicate Inserts:** Concurrent requests for new records (e.g., initializing a `StockLevel` for a new location) handle unique constraints safely or utilize upsert logic.

---

## 4. SQL Injection & Data Access Safety
- [ ] **Safe SQL Parameterization:** All raw SQL queries in ADO.NET (`SqlCommand`) or EF Core (`FromSqlRaw`, `FromSqlInterpolated`) strictly use parameterized inputs. No string interpolation or concatenation (`$"SELECT ... WHERE Id = {id}"`) is permitted with raw SQL.
- [ ] **EF Core Safe Querying:** Linq-to-Entities queries avoid translating raw user strings into dynamic SQL fragments.
- [ ] **Resource Disposal:** All `SqlConnection`, `SqlCommand`, and `SqlDataReader` instances implement deterministic disposal via `using` statements or declarations.

---

## 5. Error Handling & Security Hygiene
- [ ] **Centralized Exception Handling:** Exceptions are caught and transformed into standard HTTP problem responses by `ExceptionMiddleware`. Controllers do not emit raw unhandled stack traces.
- [ ] **Accurate HTTP Status Codes:** Responses use semantic status codes:
  - `400 Bad Request` for validation failures.
  - `401 Unauthorized` for missing or invalid tokens.
  - `403 Forbidden` for insufficient role privileges.
  - `404 Not Found` for missing resources.
  - `409 Conflict` for concurrency conflicts or domain rule violations (e.g., insufficient stock).
  - `500 Internal Server Error` for unexpected runtime failures.
- [ ] **Information Disclosure Prevention:** Error messages returned to clients never leak database connection strings, internal server paths, or SQL server version details.

---

## 6. Testing & Quality Assurance
- [ ] **Comprehensive Test Coverage:** Every bug fix includes a regression test; every new feature includes both unit and integration tests.
- [ ] **Edge Case Verification:** Test suites cover boundary values (e.g., 0 delta, boundary stock thresholds, concurrency retry exhaustion).
- [ ] **Environment Isolation:** Integration tests running against real SQL Server utilize `[SkippableFact]` to ensure test runs pass cleanly in environments where live SQL containers are not present.
- [ ] **Test Determinism:** Concurrency and integration tests clean up seeded test records and do not pollute shared test databases or depend on execution order.
- [ ] **No Flaky or Skipped Test Obfuscation:** Any intentionally skipped test provides an explicit, actionable skip reason (e.g., missing environment variable).
