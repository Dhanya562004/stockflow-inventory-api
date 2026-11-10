# AI-Assisted Development Workflow

## 1. Overview & Philosophy

In modern software engineering, Artificial Intelligence tools (such as large language models, coding assistants, and automated review agents) serve as force multipliers for development velocity. However, production-grade systems require that developers exercise rigorous architectural oversight, validate every contribution against compilers and test suites, and take full responsibility for the code committed to the repository.

In **StockFlow**, AI tools were employed across four core disciplines:
- **Code Generation:** Bootstrapping repetitive boilerplate (DTO mappings, FluentValidation rules, repository CRUD signatures).
- **Debugging & Root-Cause Analysis:** Interpreting complex MSBuild target evaluations, runtime exceptions, and concurrency race conditions.
- **Test Generation:** Constructing property-based boundary tests, mock setups, and integration test permutations.
- **Review & Refactoring:** Auditing authorization attributes, identifying security gaps, and generating structured pull request templates.

---

## 2. Concrete Cases of Incomplete or Erroneous AI Output & Resolutions

The following real-world examples from the repository's Git history illustrate instances where AI code generation introduced subtle architectural or build defects, how they were detected through test verification, and how they were corrected.

### Case 1: Recursive Build Output Nesting & MSB3026 File-Lock Error
- **Git Reference:** Commit `7baf870` / PR #17 (`fix(build): exclude test and sidecar directories from root project items`)
- **What Was Generated:**
  When generating the initial project structure, `InventoryManagementSystemAPI.Tests` was placed as a child subdirectory within the root repository. To prevent duplicate compilation, the assistant added:
  ```xml
  <Compile Remove="InventoryManagementSystemAPI.Tests\**" />
  ```
- **The Defect:**
  The assistant omitted `<Content Remove>`, `<None Remove>`, and `<DefaultItemExcludes>`. In `Microsoft.NET.Sdk.Web`, all non-source files in child folders are automatically treated as `<Content>` items to be copied to the target output directory (`bin/Debug/net8.0/`).
  Consequently, with every build, `InventoryManagementSystemAPI.Tests/bin/` artifacts (including `MvcTestingAppManifest.json`) were copied into `bin/Debug/net8.0/InventoryManagementSystemAPI.Tests/...`. When the test project referenced the root project, it copied those contents back into its own output directory. This created a recursive directory loop 17 levels deep on disk, eventually causing MSBuild build failures:
  ```text
  warning MSB3026: Could not copy ".../MvcTestingAppManifest.json" to "...". The process cannot access the file because it is being used by another process.
  ```
- **How It Was Corrected:**
  Rather than suppressing the warning, the engineer diagnosed the SDK item-globbing behavior and updated `InventoryManagementSystemAPI.csproj` with:
  ```xml
  <PropertyGroup>
    <DefaultItemExcludes>$(DefaultItemExcludes);InventoryManagementSystemAPI.Tests\**;legacy\**;tests\**;playwright-report\**;docs\**</DefaultItemExcludes>
  </PropertyGroup>

  <ItemGroup>
    <Compile Remove="InventoryManagementSystemAPI.Tests\**;legacy\**;tests\**;playwright-report\**;docs\**" />
    <Content Remove="InventoryManagementSystemAPI.Tests\**;legacy\**;tests\**;playwright-report\**;docs\**" />
    <EmbeddedResource Remove="InventoryManagementSystemAPI.Tests\**;legacy\**;tests\**;playwright-report\**;docs\**" />
    <None Remove="InventoryManagementSystemAPI.Tests\**;legacy\**;tests\**;playwright-report\**;docs\**" />
  </ItemGroup>
  ```
  This cleanly isolated child projects from root project globbing and permanently resolved the lock condition.

---

### Case 2: RuntimeBinderException with Web API 2 Anonymous Types Across Assemblies
- **Git Reference:** Commit `d5d082b` / PR #15 (`feat: add legacy ASP.NET MVC 5 (.NET Framework 4.8) sidecar project`)
- **What Was Generated:**
  When scaffolding the legacy Web API 2 controller in `legacy/StockFlow.LegacyMvc5/Controllers/Api/LegacyStockApiController.cs`, the assistant returned an anonymous object inside `NegotiatedContentResult`:
  ```csharp
  return Content(HttpStatusCode.OK, new { success = true, newQuantity = newQty, message = "Stock adjusted successfully." });
  ```
- **The Defect:**
  In C#, anonymous types are marked `internal` to the declaring assembly. When the test project (`StockFlow.LegacyMvc5.Tests`) attempted to inspect properties of the returned content via `dynamic` or reflection, the runtime dynamic binder threw:
  ```text
  Microsoft.CSharp.RuntimeBinder.RuntimeBinderException: 'object' does not contain a definition for 'success'
  ```
- **How It Was Corrected:**
  The engineer replaced the anonymous object with a strongly typed DTO (`LegacyStockAdjustmentResponse`) marked `public` in the models layer:
  ```csharp
  public class LegacyStockAdjustmentResponse
  {
      public bool Success { get; set; }
      public int NewQuantity { get; set; }
      public string Message { get; set; } = string.Empty;
      public string ErrorMessage { get; set; } = string.Empty;
  }
  ```
  The controller was updated to return this strongly typed response, eliminating reflection issues and aligning with enterprise Web API 2 standards.

---

### Case 3: Dual-Runtime Target Framework Collisions in Root Workspace
- **Git Reference:** Commit `51a2fc2` (`feat: add legacy ASP.NET MVC 5 sidecar project`)
- **What Was Generated:**
  When adding the legacy .NET Framework 4.8 sidecar project into `./legacy`, the initial SDK-style project at the root level automatically traversed subdirectories and attempted to compile `.NET Framework 4.8` source files into the `.NET 8.0` build pipeline.
- **The Defect:**
  The compiler failed with `CS0579: Duplicate TargetFrameworkAttribute` and missing type errors because both runtimes attempted to resolve incompatible BCL types (`System.Web` vs `Microsoft.AspNetCore`).
- **How It Was Corrected:**
  The engineer decoupled the repository into two separate Visual Studio solutions:
  1. `InventoryManagementSystemAPI.sln`: Targets `.NET 8.0` modern web API and its xUnit test project.
  2. `legacy/StockFlow.Legacy.sln`: Isolated solution targeting `.NET Framework 4.8` for the sidecar and its Moq test project.
  This decoupled the build configurations, allowing modern Linux CI and legacy Windows CI to operate independently without cross-runtime pollution.

---

## 3. Human-in-the-Loop Verification Protocol

To ensure software quality when collaborating with AI tools, the development process follows strict verification gates:

1. **Compiler & Linter Gates:** Code is immediately compiled with `dotnet build` under strict warning settings before committing.
2. **Deterministic Automated Testing:** Every generated or modified feature must have corresponding unit and integration tests executed against real in-memory fakes or Docker SQL Server containers.
3. **Execution Plan & SQL Audit:** Raw SQL queries proposed by AI are audited for parameterization, index coverage, and SQL injection safety before EF Core registration.
4. **Security & Authorization Review:** Endpoints are independently audited to ensure that missing attributes do not default to anonymous access.
5. **Git Hygiene:** Every change is reviewed via `git diff --check` and committed with semantic conventional commit messages.
