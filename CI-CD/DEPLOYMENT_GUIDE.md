# CateringEcommerce Deployment Guide
**Complete CI/CD Pipeline for IIS Deployment**

---

## 📦 Quick Start

### First-Time Deployment

1. **Configure Build Agent (One-Time)**
   ```powershell
   # Install required software
   choco install dotnet-9.0-sdk -y
   choco install nodejs-lts -y
   choco install sql-server-management-studio -y
   ```

2. **Setup IIS (One-Time)**
   ```powershell
   cd D:\Pankaj\Project\CateringEcommerce\Deploy
   .\setup-iis.ps1 -Environment Production
   ```

3. **Initialize Database (One-Time)**
   ```powershell
   .\db-init.ps1 `
       -ServerName "localhost\SQLEXPRESS" `
       -DatabaseName "CateringEcommerceDB" `
       -ScriptsPath "..\Database" `
       -FlagFile "C:\Deployments\CateringEcommerce\.db_initialized"
   ```

4. **Configure Pipeline Secrets in Azure DevOps**
   - Go to Pipelines → Library → Add Variable Group: `CateringEcommerce-Production`
   - Add these secrets:
     - `DB_CONNECTION_STRING`
     - `JWT_SECRET_KEY`
     - `EMAIL_API_KEY`
     - `SMS_API_KEY`
     - `ENCRYPTION_KEY`

5. **Trigger First Deployment**
   ```bash
   git commit -m "Initial production deployment"
   git push origin main
   ```

---

## 🔄 Redeployment (Automatic)

Every push to `main` branch automatically:
1. ✅ Builds backend + frontend
2. ✅ Runs tests
3. ⏭️ **Skips database initialization** (already done)
4. ✅ Backs up current deployment
5. ✅ Deploys new version
6. ✅ Runs health checks
7. ✅ Auto-rollback if health checks fail

```bash
# Just commit and push - pipeline handles everything
git commit -m "feat: Add new feature"
git push origin main
```

---

## 🗂️ File Structure

```
CateringEcommerce/
│
├── CI-CD/                              # CI/CD Pipeline Files
│   ├── pipeline.yml                    # Main orchestrator
│   ├── variables.yml                   # Environment variables
│   ├── build-backend.yml               # Backend build steps
│   ├── build-frontend.yml              # Frontend build steps
│   ├── deploy-backend.yml              # Backend deployment
│   ├── deploy-frontend.yml             # Frontend deployment
│   ├── test.yml                        # Test execution
│   ├── README.md                       # Pipeline documentation
│   └── DEPLOYMENT_GUIDE.md             # This file
│
├── Deploy/                             # PowerShell Deployment Scripts
│   ├── setup-iis.ps1                   # ⚙️  IIS initial setup
│   ├── db-init.ps1                     # 🗄️  Database initialization (one-time)
│   ├── db-check.ps1                    # ✅ Database status check
│   ├── deploy-backend-iis.ps1          # 🚀 Backend deployment
│   ├── deploy-frontend-iis.ps1         # 🚀 Frontend deployment
│   ├── backup-site.ps1                 # 💾 Pre-deployment backup
│   ├── rollback.ps1                    # ⏮️  Rollback to previous version
│   ├── health-check.ps1                # 🏥 Post-deployment validation
│   └── config/
│       ├── appsettings.Production.json # Production app settings template
│       └── web.config                  # IIS configuration template
│
├── CateringEcommerce.API/              # Backend API
├── CateringEcommerce.Web/Frontend/     # React Frontend
└── Database/                           # SQL Scripts
```

---

## 🔧 Manual Operations

### Check Database Status
```powershell
.\db-check.ps1 `
    -ServerName "localhost\SQLEXPRESS" `
    -DatabaseName "CateringEcommerceDB" `
    -FlagFile "C:\Deployments\CateringEcommerce\.db_initialized"
```

### Manual Backup
```powershell
.\backup-site.ps1 `
    -ApiPath "C:\inetpub\wwwroot\CateringAPI" `
    -WebPath "C:\inetpub\wwwroot\CateringWeb" `
    -BackupPath "C:\Deployments\Backups" `
    -Version "1.0.50"
```

### Manual Rollback
```powershell
# Rollback to previous version
.\rollback.ps1 -Previous -Confirm

# Rollback to specific version
.\rollback.ps1 -Version "1.0.45" -Confirm

# Rollback to specific backup
.\rollback.ps1 -BackupName "backup_v1.0.45_2026-02-06_10-30-00" -Confirm
```

### Health Check
```powershell
.\health-check.ps1 `
    -ApiHealthEndpoint "https://api.enyvora.com/health" `
    -WebHealthEndpoint "https://enyvora.com" `
    -Timeout 60 `
    -Retries 3
```

---

## 🎯 How Database One-Time Execution Works

### The Problem
Database initialization should only run once, but code deployments happen frequently.

### The Solution
Three-layer protection:

1. **Flag File Check**
   - File: `C:\Deployments\CateringEcommerce\.db_initialized`
   - Created after successful initialization
   - Pipeline skips DB init if this file exists

2. **Database Table Check**
   - Table: `DeploymentHistory`
   - Contains initialization record
   - Query checks for `EventType = 'DATABASE_INITIALIZED'`

3. **Pipeline Logic**
   ```yaml
   - job: InitializeDatabase
     condition: eq(dependencies.PreDeployment.outputs['DBCheck.NeedsInitialization'], 'true')
   ```

### Workflow

**First Deployment:**
```
1. db-check.ps1 runs
   → Flag file NOT found
   → DeploymentHistory table NOT found
   → Sets NeedsInitialization = true

2. db-init.ps1 runs
   → Executes all SQL scripts
   → Creates DeploymentHistory table
   → Inserts initialization record
   → Creates flag file

3. Future deployments SKIP step 2
```

**Redeployments:**
```
1. db-check.ps1 runs
   → Flag file EXISTS
   → DeploymentHistory table EXISTS
   → Sets NeedsInitialization = false

2. db-init.ps1 SKIPPED
   → No database changes
   → Only application files deployed
```

### Force Re-initialization (DANGEROUS)
```powershell
# Step 1: Delete flag file
Remove-Item "C:\Deployments\CateringEcommerce\.db_initialized" -Force

# Step 2: Run with -Force parameter
.\db-init.ps1 `
    -ServerName "localhost\SQLEXPRESS" `
    -DatabaseName "CateringEcommerceDB" `
    -ScriptsPath "..\Database" `
    -FlagFile "C:\Deployments\CateringEcommerce\.db_initialized" `
    -Force

# ⚠️ WARNING: This will DROP ALL DATA!
```

---

## 🔐 Security Checklist

Before production deployment:

- [ ] All secrets in Azure Key Vault / Variable Groups
- [ ] No hardcoded passwords or API keys
- [ ] HTTPS enforced (certificates configured)
- [ ] SQL Server uses Windows Authentication
- [ ] IIS app pools run under least-privilege accounts
- [ ] File permissions restricted (IIS_IUSRS, IUSR only)
- [ ] web.config security headers configured
- [ ] CORS allows only production domains
- [ ] Rate limiting enabled
- [ ] OTP verification enabled
- [ ] 2FA enabled for admin accounts
- [ ] BCrypt password hashing (work factor 12)
- [ ] httpOnly cookies for JWT tokens

---

## 📊 Pipeline Stages Explained

### Stage 1: Build (CI)
| Step | Description | Duration |
|------|-------------|----------|
| Checkout | Clone repository | 10s |
| Restore Backend | NuGet packages | 30s |
| Restore Frontend | npm packages | 60s |
| Build Backend | .NET Release build | 45s |
| Build Frontend | Vite production build | 90s |
| Run Tests | Unit + Integration | 120s |
| Publish Artifacts | Create deployment packages | 30s |

**Total CI Time:** ~6 minutes

### Stage 2: Deploy (CD)
| Step | Description | Duration |
|------|-------------|----------|
| DB Check | Verify initialization status | 5s |
| DB Init | **One-time only** | 2-5 min |
| Backup | Pre-deployment backup | 30s |
| Deploy Backend | Stop pool, copy files, start | 45s |
| Deploy Frontend | Stop site, copy files, start | 30s |
| Health Check | Verify endpoints | 30s |

**Total CD Time (First Deploy):** ~8 minutes
**Total CD Time (Redeploy):** ~3 minutes

---

## 🔍 Troubleshooting

### Pipeline Fails at Database Check
```powershell
# Check SQL Server connectivity
Test-NetConnection -ComputerName localhost -Port 1433

# Verify database exists
sqlcmd -S localhost\SQLEXPRESS -Q "SELECT name FROM sys.databases"

# Check flag file
Get-Content "C:\Deployments\CateringEcommerce\.db_initialized"
```

### Health Check Fails
```powershell
# Check IIS pools
Get-WebAppPoolState -Name "CateringEcommerce_API_Pool"
Get-WebAppPoolState -Name "CateringEcommerce_Web_Pool"

# Check IIS sites
Get-Website | Where-Object { $_.Name -like "*Catering*" }

# Check event logs
Get-EventLog -LogName Application -Source "IIS*" -Newest 20

# Manual health check
Invoke-WebRequest -Uri "https://api.enyvora.com/health"
```

### Rollback Not Working
```powershell
# List available backups
Get-ChildItem "C:\Deployments\Backups" -Directory | Sort CreationTime -Descending

# Verify backup contents
Get-ChildItem "C:\Deployments\Backups\backup_v1.0.45_*\api" -Recurse

# Force rollback
.\rollback.ps1 -Version "1.0.45" -Confirm
```

### App Pool Crashes Immediately
```powershell
# Check .NET runtime installed
dotnet --list-runtimes

# Check permissions
icacls "C:\inetpub\wwwroot\CateringAPI"

# Check stdout logs
Get-Content "C:\inetpub\wwwroot\CateringAPI\logs\stdout.log" -Tail 50
```

---

## 📈 Monitoring

### Application Insights (Optional)
Add to `appsettings.Production.json`:
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-key-here",
    "EnableAdaptiveSampling": true
  }
}
```

### Event Log Monitoring
```powershell
# Watch IIS errors
Get-EventLog -LogName Application -Source "IIS*" -EntryType Error -Newest 10

# Watch application errors
Get-EventLog -LogName Application -Source "ASP.NET*" -EntryType Error -Newest 10
```

### Performance Counters
```powershell
# Monitor app pool
Get-Counter "\ASP.NET Applications(__Total__)\Requests/Sec"
Get-Counter "\Web Service(_Total)\Current Connections"
```

---

## 🎓 Best Practices

1. **Always use the pipeline** - Don't manually copy files to production
2. **Test in staging first** - Use separate environment for testing
3. **Monitor health checks** - Set up alerts for failures
4. **Keep backups** - Retain at least 10 versions (automated)
5. **Use feature flags** - Enable/disable features without redeployment
6. **Version everything** - Clear versioning in artifacts and backups
7. **Document changes** - Meaningful commit messages
8. **Security first** - All secrets in secure storage
9. **Regular updates** - Keep dependencies up to date
10. **Disaster recovery plan** - Test rollback procedures regularly

---

## 📞 Support

**DevOps Team:** devops@enyvora.com
**On-Call:** +91-XXXX-XXXXXX
**Documentation:** Confluence → DevOps → CateringEcommerce
**Incidents:** Jira Service Desk

---

## 📝 Version History

| Version | Date | Pipeline Changes |
|---------|------|------------------|
| 1.0.0 | 2026-02-06 | Initial CI/CD pipeline setup |

---

**Last Updated:** February 6, 2026
**Maintained By:** DevOps Team
