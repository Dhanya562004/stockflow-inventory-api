# Low-Stock Summary LLM Evaluation Report

## 1. Evaluation Methodology
To measure and verify the reliability, factuality, and schema compliance of the low-stock summary endpoint (`GET /api/reports/low-stock/summary`), a golden evaluation dataset containing 10 test scenarios was established in [`tests/eval/low-stock-golden.json`](../tests/eval/low-stock-golden.json).

Each scenario tests specific real-world warehouse conditions:
1. Single critically low product (CurrentStock: 1, Threshold: 5)
2. Multiple low items with identical stock levels (CurrentStock: 3 each, Threshold: 5)
3. Zero-stock product stockout (CurrentStock: 0, Threshold: 10)
4. Multi-item hierarchy with distinct lowest stock item (CurrentStock: 2, 7, 10)
5. Edge condition hitting threshold exactly (CurrentStock: 15, Threshold: 15)
6. Five low-stock peripheral accessories (CurrentStock: 1..5, Threshold: 5)
7. High-value computing components (CurrentStock: 0, 1, Threshold: 5)
8. Fast-moving consumable items (CurrentStock: 4, ReorderLevel: 30)
9. Industrial hardware fasteners (CurrentStock: 8, 9, Threshold: 10)
10. Mixed warehouse catalog with 4 low items among higher items (CurrentStock: 2, 3, 5, 8, Threshold: 8)

---

## 2. Deterministic Golden Evaluation Results (Fake Client)
- **Test Runner:** `InventoryManagementSystemAPI.Tests.Eval.LowStockEvaluationTests.GoldenEvaluation_DeterministicRun_All10ScenariosPassFactChecks`
- **Total Scenarios:** 10
- **Scenarios Passed:** 10 / 10 (100%)
- **Scenarios Failed:** 0
- **Verified Facts:**
  - Strict adherence to `LowStockSummaryAnalysisDto` JSON schema (`summary`, `items`, `recommendedAction`).
  - Total item count exactly matches filtered inventory count.
  - Recommended action explicitly names the lowest-stock SKU.
  - All listed low-stock SKUs are captured in the structured `items` array.

---

## 3. Live Model Evaluation Status
- **Status:** Skipped (Not Executed)
- **Reason:** `LLM_API_KEY` environment variable is not configured with available credits in this environment.
- **Hallucination / Quality Score Claim:** Per instructions, no hallucination rates or synthetic quality scores are claimed because live inference did not execute.
- **Failing Scenarios:** N/A (Live run skipped).

