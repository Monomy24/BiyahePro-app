# 🚗 BiyahePro — Backend Engine & Core Database Setup

Welcome to the central repository for the **BiyahePro** ride-hailing backend ecosystem. This project features a high-performance database schema using time-sortable identifiers, spatial tracking data, and a secure ASP.NET Core Web API engine powered by Dapper Micro-ORM and SignalR WebSockets.

---

## 🛠️ Global Project Architecture Map

Ensure your folder tree layout side-by-side matches this schematic inside your root workspace:

```text
BiyahePro - new/                        ← Root Workspace Directory
├── README.md                           ← This file!
├── ridehailing-db/                     ← Phase 1: Database Scripts Folder
│   ├── 00_extensions.sql to 07_refresh_tokens.sql
└── RideHailing.API/                    ← Phase 2: .NET Core Project Folder
    ├── Controllers/, Hubs/, Models/
    ├── Middleware/, Repositories/, Services/
    ├── Program.cs & appsettings.json
```

---

## 🗄️ Phase 1: PostgreSQL & PostGIS Database Provisioning

This phase builds your relational database, adds location trackers, creates an isolated application user role, and inserts mock testing items.

### 🏃 Setup Commands
1. Open your terminal in VS Code (`Ctrl + ~`) and connect to your database system using the absolute path:
   ```powershell
   & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres
   ```
2. Paste and run these configuration scripts **one line at a time** inside the `postgres=#` interface:
   ```sql
   CREATE DATABASE ridehailing OWNER postgres;
   CREATE ROLE ridehailing_app WITH LOGIN PASSWORD 'Pisosabohol01';
   \c ridehailing
   ```
3. Load and compile all structure scripts in their correct file order to prevent cross-relationship breaks:
   ```sql
   \(\i 00_extensions.sql    \i 01_users.sql    \i 02_drivers_vehicles.sql    \i 03_trips.sql    \i 04_payments_ratings.sql    \i 05_app_settings.sql    \i 06_seed.sql    \i 07_refresh_tokens.\)sql
   ```
4. Verify your structural list displays **10 tables** by executing `\dt` (Press **Spacebar** to scroll, and **q** to close the viewer panel), then type `\q` to exit.

---

## 🏗️ Phase 2: ASP.NET Core API Server Construction

This phase compiles your application logic controllers, caching components, data transfer structures, and real-time mapping hooks.

### 🏃 Setup Commands
1. Jump your current terminal path into the correct backend folder:
   ```powershell
   cd "C:\Users\lague\Desktop\BiyahePro - new\RideHailing.API"
   ```
2. Verify that your `appsettings.json` file contains your connection keys and JWT safety block parameters.
3. Clean out old temporary compilation traces and trigger a fresh build check:
   ```powershell
   dotnet clean
   dotnet build
   ```
4. Boot up your application web server engine:
   ```powershell
   dotnet run
   ```
The application will light up and display that it is actively listening for web traffic on: **`http://localhost:5000`**

---

## 🧠 Core Engineering Features to Remember

* **UUIDv7 Standard Identifiers:** Primary keys are generated as sequential, time-sortable strings. They double as an organic creation timestamp, which speeds up search sorting without extra table columns.
* **PostGIS Radius Math:** Vehicle locations utilize geographical coordinate vectors. The repository handles efficient, ultra-fast proximity checks to discover nearby vehicles using `ST_DWithin` algorithms.
* **60-Second Configuration Cache:** App rules, fares, and surge rates are loaded through an optimized memory cache. This shields your database from repetitive hits on everyday price requests.
* **Write Audit Middleware:** A pipeline intercept log captures and records any modifying changes made by system administrators into a secure audit table.

---

## 🚨 Troubleshooting & Fix Reference Guide

### 1. "The term 'psql' is not recognized"
* **Cause:** Windows doesn't know where the PostgreSQL tool is hiding.
* **Fix:** Bypass global shortcuts by running commands through its explicit computer path: `& "C:\Program Files\PostgreSQL\18\bin\psql.exe"`.

### 2. "Did not find any tables"
* **Cause:** Running scripts while they still have a solid white circle (⚪) in your editor. This tells the database to read empty files from your drive.
* **Fix:** Always force-save all components (`Ctrl + K, S`) before deploying migrations.

### 3. Namespace Mismatch Errors (`CS0234` / `CS0246`)
* **Cause:** Files cannot see their neighbors because their header definitions are broken or sitting in incorrect directories.
* **Fix:** Ensure files start with an explicit layout name mapping line (e.g., `namespace RideHailing.API.Services;`) and verify folders use the plural names matching your architecture tree.

### 4. Code Changes Not Updating
* **Cause:** The compiler gets stuck reading an outdated system memory cache.
* **Fix:** Wipe out the old memory folders and trigger a clean build process:
  ```powershell
  Remove-Item -Path .\bin\,.\obj\ -Recurse -Force
  dotnet clean
  dotnet build
  ```
