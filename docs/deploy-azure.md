# Azure Free-Tier Deployment Guide (Strict ₹0 Budget)

> **Deployment Status:** This application is **NOT** deployed to Azure. This document specifies the exact configuration, operational limitations, and verified scripts required to host the API on Azure using **ONLY free-tier resources (₹0 budget)**.

---

## 1. Budget & Free-Tier Resource Specification

To comply with the strict **₹0 budget constraint**, the deployment architecture uses exclusively:

| Component | Service & Tier | Free Limit & Auto-Pause Behavior | Budgeted Cost |
| :--- | :--- | :--- | :--- |
| **Hosting** | Azure App Service Linux **F1 (Free)** | 60 CPU minutes/day, 1 GB RAM, 1 GB Storage. Shuts down if daily CPU quota is reached. | **₹0** |
| **Database** | Azure SQL Database **Serverless GeneralPurpose Free Offer** | 100,000 vCore-seconds/month (~55 hrs @ 0.5 vCore), 32 GB max storage. Auto-pauses after 60 min of inactivity. Configured with `--free-limit-exhaustion-behavior AutoPause` so it pauses if the monthly limit is reached instead of incurring charges. | **₹0** |
| **Networking** | SQL Server Firewall Rule | Direct IP access for Azure internal services only (`0.0.0.0`). | **₹0** |
| **SSL / HTTPS** | Default `*.azurewebsites.net` certificate | Included free on all App Service plans. | **₹0** |

> [!CAUTION]
> **Never select Basic, Standard, or Premium tiers.**
> - App Service `B1` costs ~$13/month (~₹1,100/mo).
> - Azure SQL `Basic` DTU tier costs ~$4.99/month (~₹420/mo).
> - Never assume Azure trial credits satisfy a ₹0 budget requirement.

---

## 2. Incompatibilities, Subscription Constraints & Limitations

Before deploying, verify these technical constraints:

1. **Single Free Database Limit per Subscription:**
   - Microsoft Azure permits **exactly one lifetime free Azure SQL database per subscription**.
   - If your Azure subscription has already claimed a free SQL database, creating another one with `--use-free-limit true` will fail.
   - *Check in Azure Portal:* Search for **Azure SQL**, filter by free offer, or run `az sql db list` to inspect existing databases.

2. **Auto-Pause Cold Starts:**
   - The Serverless database auto-pauses after 60 minutes of inactivity to conserve free vCore seconds.
   - The first HTTP request after pause triggers a wake-up taking **30 to 60 seconds**. Clients or frontend applications must allow appropriate timeouts.

3. **Free Monthly Quota Exhaustion:**
   - If the database exhausts 100,000 vCore-seconds in a calendar month, the database **auto-pauses until the first day of the following calendar month**.
   - During this period, all database queries will fail, preventing unexpected charges.

4. **App Service F1 Daily Quota:**
   - 60 CPU minutes per 24-hour UTC day.
   - If exceeded, the App Service displays an HTTP 403 / 503 Quota Exceeded error until the quota resets at 00:00 UTC.

5. **Always On is Disabled:**
   - `Always On` is not supported on F1. The web worker will go to sleep after ~20 minutes of inactivity.

---

## 3. Verified Script Artifacts

The repository includes two scripts in the `scripts/` directory:

1. **[`scripts/deploy-azure.sh`](../scripts/deploy-azure.sh):**
   - Implements the complete provisioning workflow strictly enforcing `--sku F1` and `--use-free-limit true --free-limit-exhaustion-behavior AutoPause`.
   - Packages and deploys the .NET 8 application.
   - Validated syntax: `bash -n scripts/deploy-azure.sh` (Passed).

2. **[`scripts/verify-azure.sh`](../scripts/verify-azure.sh):**
   - Tests the live deployed URL: OpenAPI/Swagger (`/swagger/v1/swagger.json`), Razor stock view (`/stock`), and Products API (`/api/products`).
   - Fails cleanly with exit code 1 if `API_URL` is unset or unreachable.
   - Validated syntax: `bash -n scripts/verify-azure.sh` (Passed).

---

## 4. Manual Deployment Runbook (For Subscription Owner)

Because Azure CLI is not installed in this execution environment and interactive Microsoft Entra ID authentication (`az login`) requires operator browser confirmation, follow these manual steps to deploy:

### Step 4.1: Login and Select Target Subscription
```bash
# 1. Authenticate to Azure
az login

# 2. List your subscriptions and select the one intended for free hosting
az account list --output table
az account set --subscription "<YOUR_SUBSCRIPTION_ID>"

# 3. Check if your subscription already has an existing free SQL DB
az sql db list --query "[?useFreeLimit==\`true\`].{Name:name, Server:serverName}" -o table
```

### Step 4.2: Export Required Environment Variables
```bash
export RESOURCE_GROUP="rg-stockflow-free"
export LOCATION="eastus"                      # Ensure F1 Linux is available in chosen region
export APP_SERVICE_PLAN="plan-stockflow-free"
export APP_NAME="stockflow-api-$RANDOM"       # Must be globally unique
export SQL_SERVER_NAME="sql-stockflow-$RANDOM"# Must be globally unique
export SQL_DATABASE_NAME="InventoryDB"
export ADMIN_USER="sqladmin"
export ADMIN_PASSWORD="YourStrongPassword123!"# Must meet Azure SQL complexity rules
```

### Step 4.3: Execute Free Deployment
```bash
chmod +x scripts/deploy-azure.sh scripts/verify-azure.sh
./scripts/deploy-azure.sh
```

### Step 4.4: Run Live Verification
```bash
./scripts/verify-azure.sh "https://${APP_NAME}.azurewebsites.net"
```

---

## 5. Alternatives If Free Azure SQL Offer Is Unavailable

If your subscription has already utilized its single free Azure SQL Database, the following ₹0 options are available:

1. **SQLite Embedded Provider:**
   - Replace `Microsoft.EntityFrameworkCore.SqlServer` with `Microsoft.EntityFrameworkCore.Sqlite` in `Program.cs`.
   - Store the `.db` file in the App Service persistent `/home` directory.
   - **Cost:** ₹0 (uses existing App Service F1 storage; no external database service required).

2. **External Free SQL / Container Service:**
   - Connect the App Service to an external free development database provider (e.g., Neon, Supabase, or Render Free Tier).
