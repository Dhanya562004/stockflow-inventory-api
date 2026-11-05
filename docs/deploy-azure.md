# Azure App Service and Azure SQL Deployment Guide

> **Deployment Status:** This application is **NOT** deployed to Azure. This document provides the prepared, verified deployment script template and manual runbook.

---

## 1. Prerequisites
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) (v2.50+)
- .NET 8 SDK
- Active Azure Subscription

---

## 2. Parameter Placeholders
Replace the following placeholders before executing:
- `<RESOURCE_GROUP>`: e.g. `rg-stockflow-prod-eastus`
- `<LOCATION>`: e.g. `eastus`
- `<APP_SERVICE_PLAN>`: e.g. `plan-stockflow-linux`
- `<APP_NAME>`: e.g. `stockflow-inventory-api-prod` (globally unique)
- `<SQL_SERVER_NAME>`: e.g. `sql-stockflow-prod` (globally unique)
- `<SQL_DATABASE_NAME>`: e.g. `InventoryDB`
- `<ADMIN_USER>`: e.g. `sqladmin`
- `<ADMIN_PASSWORD>`: Secure strong password meeting Azure complexity requirements

---

## 3. Azure CLI Provisioning Commands

### Step 3.1: Create Resource Group
```bash
az group create \
  --name <RESOURCE_GROUP> \
  --location <LOCATION>
```

### Step 3.2: Provision Azure SQL Logical Server & Database
```bash
# Create Logical Server
az sql server create \
  --name <SQL_SERVER_NAME> \
  --resource-group <RESOURCE_GROUP> \
  --location <LOCATION> \
  --admin-user <ADMIN_USER> \
  --admin-password '<ADMIN_PASSWORD>'

# Configure Firewall Rule to Allow Azure Services
az sql server firewall-rule create \
  --resource-group <RESOURCE_GROUP> \
  --server <SQL_SERVER_NAME> \
  --name AllowAllWindowsAzureIps \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Create SQL Database
az sql db create \
  --resource-group <RESOURCE_GROUP> \
  --server <SQL_SERVER_NAME> \
  --name <SQL_DATABASE_NAME> \
  --service-objective Basic
```

### Step 3.3: Provision Linux App Service Plan & Web App (.NET 8)
```bash
# Create Linux App Service Plan
az appservice plan create \
  --name <APP_SERVICE_PLAN> \
  --resource-group <RESOURCE_GROUP> \
  --is-linux \
  --sku B1

# Create Web App with .NET 8 Runtime
az webapp create \
  --name <APP_NAME> \
  --resource-group <RESOURCE_GROUP> \
  --plan <APP_SERVICE_PLAN> \
  --runtime "DOTNETCORE:8.0"
```

### Step 3.4: Configure Connection String & Environment Settings
```bash
az webapp config connection-string set \
  --resource-group <RESOURCE_GROUP> \
  --name <APP_NAME> \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:<SQL_SERVER_NAME>.database.windows.net,1433;Initial Catalog=<SQL_DATABASE_NAME>;Persist Security Info=False;User ID=<ADMIN_USER>;Password='<ADMIN_PASSWORD>';MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

az webapp config appsettings set \
  --resource-group <RESOURCE_GROUP> \
  --name <APP_NAME> \
  --settings ASPNETCORE_ENVIRONMENT="Production" JWT__Key="<SUPER_SECRET_PRODUCTION_KEY>" JWT__Issuer="StockFlowAPI" JWT__Audience="StockFlowAPI"
```

### Step 3.5: Publish and Deploy Application
```bash
# Build & Publish
dotnet publish -c Release -o ./publish

# Create Deployment Zip Archive
cd ./publish
zip -r ../deploy.zip .
cd ..

# Deploy ZIP Package to App Service
az webapp deploy \
  --resource-group <RESOURCE_GROUP> \
  --name <APP_NAME> \
  --src-path ./deploy.zip \
  --type zip
```

---

## 4. Manual Steps Not Performable in This Environment
The following actions cannot be executed automatically within this agent environment and must be performed manually by an operator with Azure subscription credentials:
1. **Interactive Login:** Running `az login` to authenticate against Microsoft Entra ID.
2. **Subscription Selection:** Selecting the target billing subscription via `az account set --subscription <SUBSCRIPTION_ID>`.
3. **Provisioning Live Infrastructure:** Running billing-associated resource creation commands.
4. **Credential Secrets Injection:** Injecting real production database passwords and signing keys into Azure Key Vault or environment configurations.
