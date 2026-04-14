# Deploy PawNect to Azure

## URL layout

| App | URL |
|-----|-----|
| PetParent (main) | https://pawnect.azurewebsites.net |
| Admin portal | https://pawnect.azurewebsites.net/admin |
| API | https://pawnect.azurewebsites.net/api |

## One-time Azure setup

1. **Path mappings** (required for /admin and /api to run as separate apps)  
   Azure Portal → your App Service **pawnect** → **Configuration** → **Path mappings** → **Virtual applications and directories**:
   - `/` → `site\wwwroot` (Application)
   - `/admin` → `site\wwwroot\admin` (Application)
   - `/api` → `site\wwwroot\api` (Application)

2. **Connection string**  
   Configuration → **Application settings** → **New application setting**:
   - Name: `ConnectionStrings__DefaultConnection`
   - Value: your Azure SQL (or other) connection string

3. **Optional**  
   Set `ASPNETCORE_ENVIRONMENT` to `Production` if not already.

## Deploy

1. Copy your publish profile (e.g. from Azure Portal → App Service → **Get publish profile**) to a path like `deploy\pawnect.PublishSettings` (or keep it in Downloads).

2. From the **Project** folder (repository root), run:

```powershell
.\deploy\deploy-azure.ps1 -PublishProfile "C:\Users\vinay\Downloads\Data\pawnect.PublishSettings"
```

Use `-Configuration Debug` if you want a debug build.

The script will:
- Publish all three apps (Release)
- Build a single `wwwroot` with root = PetParent, `admin` = AdminPortal, `api` = API
- Zip and upload via Azure Zip Deploy

**Security:** Do not commit `*.PublishSettings` (they contain credentials). `.gitignore` is set to exclude them.
