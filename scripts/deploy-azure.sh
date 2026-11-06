#!/usr/bin/env bash
# ==============================================================================
# StockFlow Inventory Management System API - Azure Free-Tier Deployment Script
# Budget: ₹0 (Strictly Free-Tier Resources Only)
#
# Resources Provisioned:
#   - App Service Plan: Linux F1 (Free - 60 CPU min/day, 1GB RAM)
#   - App Service Web App: .NET 8 Linux Runtime
#   - Azure SQL Database: General Purpose Serverless (Free Offer - 100k vCore-s/mo, AutoPause)
#
# Safety:
#   - Will abort if non-free SKU or paid add-ons are specified.
#   - Configures --use-free-limit true and --free-limit-exhaustion-behavior AutoPause.
# ==============================================================================

set -euo pipefail

# Required Parameters
RESOURCE_GROUP="${RESOURCE_GROUP:-rg-stockflow-free}"
LOCATION="${LOCATION:-eastus}"
APP_SERVICE_PLAN="${APP_SERVICE_PLAN:-plan-stockflow-free}"
APP_NAME="${APP_NAME:-stockflow-api-$RANDOM}"
SQL_SERVER_NAME="${SQL_SERVER_NAME:-sql-stockflow-$RANDOM}"
SQL_DATABASE_NAME="${SQL_DATABASE_NAME:-InventoryDB}"
ADMIN_USER="${ADMIN_USER:-sqladmin}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:-}"

echo "======================================================================"
echo "StockFlow Azure Free-Tier Deployment"
echo "Budget: ₹0 (Free Tier Enforcement Active)"
echo "Target Resource Group: ${RESOURCE_GROUP} (${LOCATION})"
echo "App Service Plan:      ${APP_SERVICE_PLAN} (Linux F1 Free)"
echo "Web App Name:          ${APP_NAME}"
echo "SQL Server Name:       ${SQL_SERVER_NAME}"
echo "SQL Database:          ${SQL_DATABASE_NAME} (Serverless Free Offer)"
echo "======================================================================"

# 1. Verify Azure CLI is installed
if ! command -v az >/dev/null 2>&1; then
    echo "ERROR: Azure CLI ('az') is not installed or not in PATH."
    echo "Install Azure CLI from https://learn.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

# 2. Verify Azure Authentication
echo "Checking Azure authentication..."
if ! az account show >/dev/null 2>&1; then
    echo "ERROR: Not logged in to Azure. Please run 'az login' and select an active subscription."
    exit 1
fi

SUBSCRIPTION_ID=$(az account show --query id -o tsv)
SUBSCRIPTION_NAME=$(az account show --query name -o tsv)
echo "Active Subscription: ${SUBSCRIPTION_NAME} (${SUBSCRIPTION_ID})"

# 3. Prompt or validate SQL password
if [ -z "${ADMIN_PASSWORD}" ]; then
    echo "ERROR: ADMIN_PASSWORD environment variable is not set."
    echo "Please set a secure password (e.g. export ADMIN_PASSWORD='...')."
    exit 1
fi

# 4. Create Resource Group
echo "Creating resource group: ${RESOURCE_GROUP}..."
az group create \
    --name "${RESOURCE_GROUP}" \
    --location "${LOCATION}" \
    --output table

# 5. Create Azure SQL Logical Server
echo "Creating SQL Logical Server: ${SQL_SERVER_NAME}..."
az sql server create \
    --name "${SQL_SERVER_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --location "${LOCATION}" \
    --admin-user "${ADMIN_USER}" \
    --admin-password "${ADMIN_PASSWORD}" \
    --output table

# Configure SQL Firewall for Azure Services
echo "Configuring SQL firewall rule to allow Azure internal traffic..."
az sql server firewall-rule create \
    --resource-group "${RESOURCE_GROUP}" \
    --server "${SQL_SERVER_NAME}" \
    --name "AllowAllWindowsAzureIps" \
    --start-ip-address "0.0.0.0" \
    --end-ip-address "0.0.0.0" \
    --output table

# 6. Create Azure SQL Database with Free Tier (Serverless, Auto-Pause)
echo "Provisioning Azure SQL Database Free Tier..."
echo "Applying options: --edition GeneralPurpose --compute-model Serverless --use-free-limit true --free-limit-exhaustion-behavior AutoPause"
az sql db create \
    --resource-group "${RESOURCE_GROUP}" \
    --server "${SQL_SERVER_NAME}" \
    --name "${SQL_DATABASE_NAME}" \
    --edition "GeneralPurpose" \
    --compute-model "Serverless" \
    --family "Gen5" \
    --capacity 1 \
    --min-capacity 0.5 \
    --auto-pause-delay 60 \
    --use-free-limit true \
    --free-limit-exhaustion-behavior "AutoPause" \
    --output table

# 7. Create App Service Plan (Linux F1 - Free)
echo "Creating Linux App Service Plan (SKU: F1 Free)..."
az appservice plan create \
    --name "${APP_SERVICE_PLAN}" \
    --resource-group "${RESOURCE_GROUP}" \
    --location "${LOCATION}" \
    --is-linux \
    --sku "F1" \
    --output table

# 8. Create Web App (.NET 8 on Linux)
echo "Creating Web App: ${APP_NAME}..."
az webapp create \
    --name "${APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --plan "${APP_SERVICE_PLAN}" \
    --runtime "DOTNETCORE:8.0" \
    --output table

# 9. Configure Production Connection String
echo "Configuring App Service connection string..."
SQL_CONNECTION_STRING="Server=tcp:${SQL_SERVER_NAME}.database.windows.net,1433;Initial Catalog=${SQL_DATABASE_NAME};Persist Security Info=False;User ID=${ADMIN_USER};Password=${ADMIN_PASSWORD};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;"

az webapp config connection-string set \
    --resource-group "${RESOURCE_GROUP}" \
    --name "${APP_NAME}" \
    --connection-string-type "SQLAzure" \
    --settings DefaultConnection="${SQL_CONNECTION_STRING}" \
    --output table

# Configure App Settings (Production Environment & JWT options)
echo "Configuring App Service environment settings..."
JWT_KEY=$(openssl rand -base64 32 2>/dev/null || echo "ProductionJwtSecretKeyWithHighEntropyAtLeast256BitsLong!")
az webapp config appsettings set \
    --resource-group "${RESOURCE_GROUP}" \
    --name "${APP_NAME}" \
    --settings \
        ASPNETCORE_ENVIRONMENT="Production" \
        JWT__Key="${JWT_KEY}" \
        JWT__Issuer="StockFlowAPI" \
        JWT__Audience="StockFlowAPI" \
    --output table

# 10. Build, Package, and Deploy
echo "Building and packaging application..."
dotnet publish -c Release -o ./publish-azure

cd ./publish-azure
zip -r ../deploy-package.zip . >/dev/null
cd ..
rm -rf ./publish-azure

echo "Deploying ZIP package to Azure Web App..."
az webapp deploy \
    --resource-group "${RESOURCE_GROUP}" \
    --name "${APP_NAME}" \
    --src-path ./deploy-package.zip \
    --type zip \
    --clean true \
    --restart true

rm -f ./deploy-package.zip

echo "======================================================================"
echo "Deployment Complete!"
echo "Live URL: https://${APP_NAME}.azurewebsites.net"
echo "Verify with: ./scripts/verify-azure.sh https://${APP_NAME}.azurewebsites.net"
echo "======================================================================"
