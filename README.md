# PawNect

Pet management platform built with **.NET 10**, **Clean Architecture**, **ASP.NET Core** (Web API and MVC), and **Entity Framework Core**. The repository contains the backend API, Pet Parent web app, and Admin Portal.

## Repository layout

| Path | Description |
|------|-------------|
| [`Project/`](Project/) | Solution, source, scripts, and detailed documentation |
| [`Project/PawNect.sln`](Project/PawNect.sln) | Visual Studio / `dotnet` solution file |

Full architecture, database setup, API overview, and feature notes: **[`Project/README.md`](Project/README.md)**.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (local or remote) for the connection string in `appsettings`

## Quick start

1. Open a terminal in the solution folder:

   ```powershell
   cd Project
   ```

2. Apply EF Core migrations (from `Project`; see [`Project/README.md`](Project/README.md) for options):

   ```powershell
   .\Run-Migrations.ps1
   ```

3. Build and run API + Pet Parent + Admin in one step (Windows; uses Windows Terminal tabs when `wt` is available):

   ```powershell
   .\Run-PawNect.ps1
   ```

   Or double-click **`Run-PawNect.cmd`**.

4. Open in the browser:

   | App | URL |
   |-----|-----|
   | API (Swagger) | http://localhost:5000/swagger |
   | Pet Parent | http://localhost:5100 |
   | Admin Portal | http://localhost:5200 |

To run projects individually with `dotnet run`, see **Setup Instructions** in [`Project/README.md`](Project/README.md).

## License

Proprietary — PawNect 2026
