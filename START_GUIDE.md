# Start Guide

## 1) Prerequisites
- .NET SDK 10.x (project targets `net10.0`)
- SQL Server (or SQL Express)
- Node.js 20+

## 2) Configure database connection
Update connection strings in:
- `src/Upload.Data.HttpApi.Host/appsettings.json`
- `src/Upload.Data.DbMigrator/appsettings.json`

Use the same `Default` connection string in both files.

## 3) Apply database migrations
From project root:

```powershell
dotnet run --project src/Upload.Data.DbMigrator/Upload.Data.DbMigrator.csproj
```

## 4) Run backend (API + auth server)
From project root:

```powershell
dotnet run --project src/Upload.Data.HttpApi.Host/Upload.Data.HttpApi.Host.csproj
```

Default URL is shown in console (commonly `https://localhost:44395`).

## 4.1) Configure Microsoft Social Login (optional)
Update these keys in:
- `src/Upload.Data.HttpApi.Host/appsettings.json`
- `src/Upload.Data.HttpApi.Host/appsettings.Development.json`

```json
"Authentication": {
  "Microsoft": {
    "Authority": "organizations",
    "TenantId": "organizations",
    "ClientId": "<your-azure-app-client-id>",
    "ClientSecret": "<your-azure-app-client-secret>"
  }
}
```

Notes:
- If you keep placeholder values (`SET_MICROSOFT_CLIENT_ID` / `SET_MICROSOFT_CLIENT_SECRET`), the **Microsoft** button is shown on login page but sign-in will fail until real values are set.
- Callback URL to configure in Azure App Registration: `https://localhost:44316/signin-microsoft`.

## 5) Run Angular UI
From `angular` folder:

```powershell
npm install
npm start
```

If `npm start` is not available on your machine path, use:

```powershell
node .\node_modules\@angular\cli\bin\ng serve
```

## 6) Login and test Excel business flow
1. Login from the default ABP login page.
2. Open **Data Upload** from menu.
3. Upload an `.xlsx` file where:
   - row 1 is header
   - data starts at row 2
   - columns are read as: A (`ColumnA`), B (`ColumnB`), C (`ColumnC`), D (`NumericValue`)
4. Open **My Dashboard** to see:
   - a paged table of `ColumnA`, `ColumnB`, `ColumnC`, `NumericValue`
   - a chart of summed `NumericValue` grouped by `ColumnA`

## 7) Notes
- Upload endpoint validates:
  - file is present
  - extension is `.xlsx`
  - file is not empty
  - parsed data rows exist
- Data is stored in two tables:
  - `AppExcelImportBatches`
  - `AppExcelDataRows`
- Dashboard data is filtered by current authenticated user only.



Microsoft tenant note:
- ABP database TenantId = null means host context; it is unrelated to Azure Entra tenant id.
- For host context, set `Authentication:Microsoft:Authority` to one of: `common`, `organizations`, `consumers`, or your Entra tenant GUID.
- If you use `common`/`consumers`, Azure App Registration must support personal Microsoft accounts.


