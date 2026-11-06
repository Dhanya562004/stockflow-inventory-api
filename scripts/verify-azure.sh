#!/usr/bin/env bash
# ==============================================================================
# StockFlow Inventory Management System API - Azure Live Verification Script
# Verifies the live deployed API on Azure App Service
#
# Usage:
#   ./scripts/verify-azure.sh <API_URL>
#   or
#   API_URL="https://your-app.azurewebsites.net" ./scripts/verify-azure.sh
# ==============================================================================

set -euo pipefail

API_URL="${1:-${API_URL:-}}"

if [ -z "${API_URL}" ]; then
    echo "ERROR: Target API URL is unset."
    echo "Usage: ./scripts/verify-azure.sh <API_URL>"
    echo "Example: ./scripts/verify-azure.sh https://stockflow-api.azurewebsites.net"
    exit 1
fi

# Trim trailing slash if present
API_URL="${API_URL%/}"

echo "======================================================================"
echo "StockFlow Live Azure Verification"
echo "Target URL: ${API_URL}"
echo "======================================================================"

EXIT_CODE=0

# 1. Verify Swagger / OpenAPI endpoint
echo -n "Checking Swagger endpoint (${API_URL}/swagger/v1/swagger.json)... "
SWAGGER_HTTP=$(curl -s -o /dev/null -w "%{http_code}" --max-time 30 "${API_URL}/swagger/v1/swagger.json" || echo "000")
if [ "${SWAGGER_HTTP}" = "200" ]; then
    echo "OK (HTTP ${SWAGGER_HTTP})"
else
    echo "WARNING: HTTP ${SWAGGER_HTTP} (Swagger may be disabled in Production)"
fi

# 2. Verify Razor Stock view endpoint
echo -n "Checking Stock Razor View (${API_URL}/stock)... "
STOCK_HTTP=$(curl -s -o /dev/null -w "%{http_code}" --max-time 30 "${API_URL}/stock" || echo "000")
if [ "${STOCK_HTTP}" = "200" ]; then
    echo "PASSED (HTTP 200)"
else
    echo "FAILED (HTTP ${STOCK_HTTP})"
    EXIT_CODE=1
fi

# 3. Verify Products Public Catalog API
echo -n "Checking Products API (${API_URL}/api/products)... "
PRODUCTS_HTTP=$(curl -s -o /dev/null -w "%{http_code}" --max-time 30 "${API_URL}/api/products" || echo "000")
if [ "${PRODUCTS_HTTP}" = "200" ]; then
    echo "PASSED (HTTP 200)"
else
    echo "FAILED (HTTP ${PRODUCTS_HTTP})"
    EXIT_CODE=1
fi

echo "======================================================================"
if [ ${EXIT_CODE} -eq 0 ]; then
    echo "ALL VERIFICATION CHECKS PASSED!"
    echo "The StockFlow API is live, reachable, and healthy on Azure."
else
    echo "VERIFICATION CHECKS FAILED."
    echo "Review Azure App Service application logs: az webapp log tail --name <APP_NAME>"
fi
echo "======================================================================"

exit ${EXIT_CODE}
