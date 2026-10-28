# SQLite Fallback Support Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make CRM Peyvand run on SQLite out of the box with nothing to install, while keeping SQL Server available as an opt-in configured in a settings form.

**Architecture:** EF6 `DbContext` becomes provider-aware through a `DataSource` value object that is resolved once per process and persisted to disk. SQLite is the default; the schema is created by a hand-authored DDL script rather than EF migrations, because EF6's SQLite provider does not generate tables. All 18 raw-SQL grid queries are rewritten as LINQ projections that build their `DataTable` in C#, preserving the existing Persian column aliases so no UI file changes.

**Tech Stack:** .NET 10, Entity Framework 6.5.1, `System.Data.SQLite.EF6` 2.0.3, `SQLitePCLRaw.bundle_e_sqlite3`, xUnit.

**Spec:** Decisions below were taken with the maintainer and are binding. Domain vocabulary is in `CONTEXT.md`.

## Verified facts (do not re-litigate; each was proven by a compiled spike on this machine)

These were established empirically before writing this plan. Trust them.

1. **`System.Data.SQLite.EF6` 2.0.3 works on `net10.0`** and restores cleanly.
2. **`SQLitePCLRaw.bundle_e_sqlite3` must be referenced explicitly.** `System.Data.SQLite` 2.0.3's `netstandard2.1` target does *not* pull the native library transitively. Without it: `Unable to load DLL 'e_sqlite3'`.
3. **Register via `DbConfiguration` in code, not app.config.** Using `<system.data><DbProviderFactories>` in `App.config` fails on .NET 10 with `Unrecognized configuration section system.data` (System.Data.Common is a separate assembly now), and even once that section is declared the factory cannot be resolved to a provider invariant. A `DbConfiguration` subclass calling `SetProviderFactory` + `SetProviderServices` works.
4. **`Database.CreateIfNotExists()` creates ZERO tables on SQLite.** It returns without throwing and leaves the database empty. Schema creation is mandatory, by hand.
5. **`DbFunctions.TruncateTime(...)` fails on SQLite**: `no such function: TruncateTime`.
6. **`string.Contains(...)` in a LINQ-to-Entities `Where` fails on SQLite**: `no such function: CHARINDEX`. Search must therefore be done client-side after materialization.
7. **`decimal` round-trips exactly** through the SQLite provider (verified `1234.56m` in, `1234.56m` out, `==` true), including `ORDER BY`.
8. `e_sqlite3.dll` must be confirmed present in `dotnet publish` output, or the MSI will ship a broken app.

## Global Constraints

- **Do not change any `.xaml` UI file, and do not change any method signature in the BLL.** The UI binds `DataTable`s through `PublicMethods.dgvFiller` (54 call sites) with `AutoGenerateColumns = true`, so the Persian column aliases **are** the visible grid headers. Every rewritten `Read()`/`Search()` must return a `DataTable` whose column names and order match the SQL it replaces, exactly.
- **Table and column names must match the existing SQL Server schema exactly**, including the lowercase `id` used by 12 tables and uppercase `Id` used by `CatalogItems` and `InvoiceLines`, and including the EF6 pluralisation typo `RememberMes` (no trailing "s" on the "Me"). Renaming any of these silently orphans existing SQL Server data.
- **UI strings are Persian.** Every message shown to a user is Persian. Match the existing style: `MessageBox.Show("...", "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information)` and return Persian status strings from BLL/DAL methods.
- **No new NuGet package may pull in a native dependency that is not already verified in the "Verified facts" list above.**
- **Entity Framework Migrations remain in use for SQL Server only.** Do not add a second migration history, do not touch `DAL/Migrations/Configuration.cs` beyond Task 1, and do not rename `ContextKey`.
- **Every task ends with a green `dotnet test` run over the full suite** (66 existing tests must still pass).
- **Commit after every task**, using Conventional Commits matching existing history (`feat(scope):`, `fix(scope):`).

## File Structure

**New files:**

| Path | Responsibility |
|---|---|
| `DAL/DataSource.cs` | The provider choice: kind, connection string, disk persistence, connection testing. The single source of truth for "what database are we on". |
| `DAL/DataFolder.cs` | Resolves the writable data directory (`%ProgramData%` then `%LocalAppData%` fallback). Mirrors the existing `PublicMethods.UserPicturesDirectory` probe pattern. |
| `DAL/SqliteSchema.cs` | The hand-authored DDL for all 14 tables, applied on first run. |
| `DAL/GridTable.cs` | Builds a `DataTable` with explicit Persian column names from LINQ rows. |
| `CRMPeyvand.Tests/SqliteTestDb.cs` | Creates a throwaway SQLite database with the real schema, for every data-layer test. |
| `CRMPeyvand.Tests/GridQueriesTests.cs` | Tests for the rewritten grid queries. |

**Modified files:**

| Path | Change |
|---|---|
| `DAL/DB.cs` | Provider-aware constructors; provider-conditional schema initialisation; thread-safe resolution. |
| `DAL/Migrations/Configuration.cs` | Delete the dead `Seed()` override and the 5 stored procedures. |
| `DAL/CustomerDAL.cs`, `ActivityDAL.cs`, `ActivityCategoryDAL.cs` | Rewrite `Read()`/`Search()` as LINQ. |
| `DAL/UserDAL.cs`, `UserGroupDAL.cs`, `ReminderDAL.cs` | Rewrite `Read()`/`Search()` as LINQ (joins). |
| `DAL/CatalogItemDAL.cs`, `OffCodeDAL.cs`, `MessageDAL.cs` | Rewrite `Read()`/`Search()` as LINQ. |
| `DAL/InvoiceDAL.cs` | Rewrite `Read()`/`Search()` as LINQ (per-invoice sums). |
| `DAL/DashboardDAL.cs` | Replace `DbFunctions.TruncateTime` and the `DATEADD`/`GETDATE` scalar. |
| `DAL/SettingDAL.cs` | Provider branch in `BackUp`. |
| `DAL/DAL.csproj` | Add the two SQLite packages. |
| `CRMPeyvand/App.config` | Register the EF6 entityFramework provider section. |
| `CRMPeyvand/DataBaseForm.xaml` + `.cs` | Connection configuration UI. |
| `installer/Package.wxs` | Drop the SQL Server launch condition. |
| `README.md` | Document the database choice. |

**Explicitly NOT modified:** every `.xaml` file, every BLL file, every BE file, `DAL/Migrations/202608262307121_InitialCreate*.cs`.

---

### Task 1: Delete the dead stored procedures

`DAL/Migrations/Configuration.cs` creates five T-SQL stored procedures that **nothing calls** — they are leftovers from the retired Stimulsoft reporting engine. They are also full of T-SQL-only syntax (`CREATE OR ALTER PROCEDURE`, `SET NOCOUNT ON`, `dbo.`, `GETDATE()`, `DATEADD`, `ISNULL`, `CAST(... AS float)`) and would make a SQLite first-run fail outright.

**Files:**
- Modify: `DAL/Migrations/Configuration.cs`

**Interfaces:**
- Consumes: nothing
- Produces: `Migrations.Configuration` with no `Seed` override. The seed hook disappears; the seed was its only use.

- [ ] **Step 1: Confirm the procedures are unreferenced before deleting**

Run:
```powershell
Select-String -Path "DAL\*.cs","DAL\**\*.cs","BLL\*.cs","CRMPeyvand\*.cs","CRMPeyvand\**\*.cs" `
  -Pattern "ThisYearInvoices|ThisMonthInvoices|ThisWeekInvoices|ActivitiesView|ProductsTotal|CommandType\.StoredProcedure|EXECUTE?\s"
```
Expected: every hit is inside `DAL/Migrations/Configuration.cs` itself, or is the string literal `"ProductsTotal"` in `CRMPeyvand/ReportsWindow.xaml.cs` (which is a PDF filename prefix, not a call). **If any real call site appears, stop and report it instead of deleting.**

- [ ] **Step 2: Strip the Seed override and the procedure bodies**

Replace the whole of `DAL/Migrations/Configuration.cs` with:

```csharp
namespace DAL.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// EF6 migration configuration. Migrations are used for SQL Server only -
    /// the SQLite schema is created by DAL.SqliteSchema, because the EF6 SQLite
    /// provider does not generate tables (verified: CreateIfNotExists creates
    /// nothing).
    ///
    /// The Seed() override that used to live here created five stored procedures
    /// for the retired Stimulsoft reporting engine. Nothing called them; they
    /// were removed because their T-SQL made a SQLite first run impossible.
    /// </summary>
    internal sealed class Configuration : DbMigrationsConfiguration<DAL.DB>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "DAL.DB";
        }
    }
}
```

- [ ] **Step 3: Build the solution**

Run: `dotnet build CRMPeyvand.sln -c Release --nologo -v q`
Expected: `Build succeeded.` 0 errors. A pre-existing `CS0168` warning about `ex` in `LoginForm.xaml.cs` is expected and unrelated.

- [ ] **Step 4: Run the tests**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: `Passed!  - Failed: 0, Passed: 66, Skipped: 0, Total: 66`

- [ ] **Step 5: Commit**

```bash
git add DAL/Migrations/Configuration.cs
git commit -m "chore(migrations): remove unused report stored procedures

The five procedures (ThisYearInvoices, ThisMonthInvoices, ThisWeekInvoices,
ActivitiesView, ProductsTotal) were created by Seed() for the retired
Stimulsoft reporting engine. Nothing has called them since commit f6bca53
migrated reporting to QuestPDF.

They are also the single largest block of T-SQL-only syntax in the DAL
(CREATE OR ALTER PROCEDURE, SET NOCOUNT ON, dbo., GETDATE, DATEADD,
ISNULL), which makes them a hard blocker for a SQLite first run.

Migrations themselves are unchanged and still used for SQL Server."
```

---

### Task 2: SQLite schema bootstrap

Adds the two packages, registers the EF6 provider in code, and creates the schema by hand. This task's deliverable is: **a SQLite database with all 14 tables that EF6 can read and write**, proven by a test.

**Files:**
- Modify: `DAL/DAL.csproj`
- Modify: `DAL/DB.cs` (add only the `DbConnection` constructor and the SQLite factory registration; do not change the default provider yet)
- Create: `DAL/SqliteSchema.cs`
- Create: `CRMPeyvand.Tests/SqliteTestDb.cs`
- Create: `CRMPeyvand.Tests/SqliteSchemaTests.cs`

**Interfaces:**
- Consumes: nothing (works against a raw `System.Data.SQLite.SQLiteConnection`)
- Produces:
  - `public static class SqliteSchema` with `public static void Ensure(System.Data.Common.DbConnection connection)` and `public const string Invariant = "System.Data.SQLite";`
  - `public DB(System.Data.Common.DbConnection connection)` constructor on `DAL.DB`, `contextOwnsConnection: true`. This is what every later test and the SQLite path use.
  - `public static class SqliteTestDb` with `public static void WithDb(Action<DAL.DB> body)`.

- [ ] **Step 1: Add the failing test**

Create `CRMPeyvand.Tests/SqliteSchemaTests.cs`:

```csharp
using System;
using System.Data.Common;
using System.Linq;
using Xunit;

namespace CRMPeyvand.Tests
{
    public class SqliteSchemaTests
    {
        [Fact]
        public void Ensure_creates_every_table_the_context_maps()
        {
            string[] expected =
            {
                "AccessGrants", "UserGroups", "Users", "Activities", "ActivityCategories",
                "Customers", "Invoices", "InvoiceLines", "CatalogItems", "Reminders",
                "MessagePanels", "Messages", "OffCodes", "RememberMes",
            };

            var found = new System.Collections.Generic.List<string>();
            SqliteTestDb.WithRaw(connection =>
            {
                DAL.SqliteSchema.Ensure(connection);
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                            found.Add(reader.GetString(0));
                }
            });

            foreach (var table in expected)
                Assert.Contains(table, found);
        }

        [Fact]
        public void Ensure_is_idempotent()
        {
            SqliteTestDb.WithRaw(connection =>
            {
                DAL.SqliteSchema.Ensure(connection);
                DAL.SqliteSchema.Ensure(connection);   // must not throw
            });
        }

        [Fact]
        public void Ef_can_insert_and_read_a_customer()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.Customers.Add(new BE.Customer
                {
                    Name = "مشتری آزمایشی",
                    Phone = "09120000000",
                    RegDate = new DateTime(2026, 9, 29),
                });
                db.SaveChanges();

                var found = db.Customers.Single();
                Assert.Equal("مشتری آزمایشی", found.Name);
            });
        }

        [Fact]
        public void Ef_generates_incrementing_identity_keys()
        {
            SqliteTestDb.WithDb(db =>
            {
                for (var i = 0; i < 3; i++)
                    db.Customers.Add(new BE.Customer { Name = "c" + i, Phone = "0912000000" + i, RegDate = DateTime.Now });
                db.SaveChanges();

                var ids = db.Customers.OrderBy(c => c.id).Select(c => c.id).ToList();
                Assert.Equal(new[] { 1, 2, 3 }, ids);
            });
        }
    }
}
```

- [ ] **Step 2: Create the test helper**

Create `CRMPeyvand.Tests/SqliteTestDb.cs`:

```csharp
using System;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using DAL;

namespace CRMPeyvand.Tests
{
    /// <summary>
    /// Creates a throwaway SQLite database using the production schema, so data
    /// layer tests exercise the same DDL the application ships.
    /// </summary>
    public static class SqliteTestDb
    {
        private static string NewPath() => Path.Combine(
            Path.GetTempPath(),
            "crmpeyvand-test-" + Guid.NewGuid().ToString("N") + ".db");

        private static string ConnectionString(string path) =>
            new SQLiteConnectionStringBuilder { DataSource = path }.ToString()
            + ";providerName=" + SqliteSchema.Invariant;

        public static void WithRaw(Action<SQLiteConnection> body)
        {
            var path = NewPath();
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString(path)))
                {
                    connection.Open();
                    body(connection);
                }
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        public static void WithDb(Action<DB> body)
        {
            WithRaw(connection =>
            {
                SqliteSchema.Ensure(connection);
                using (var db = new DB(connection))
                    body(db);
            });
        }
    }
}
```

- [ ] **Step 3: Run the test to verify it fails**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: compile errors for missing `DAL.SqliteSchema` and `DB(DbConnection)`. That is the correct failure.

- [ ] **Step 4: Add the packages**

Run:
```powershell
dotnet add DAL/DAL.csproj package System.Data.SQLite.EF6 --version 2.0.3
dotnet add DAL/DAL.csproj package SQLitePCLRaw.bundle_e_sqlite3
```
The second package is mandatory (Verified fact 2) - `System.Data.SQLite` does not bring the native library on `netstandard2.1`.

- [ ] **Step 5: Write the schema DDL**

Create `DAL/SqliteSchema.cs`:

```csharp
using System;
using System.Data.Common;

namespace DAL
{
    /// <summary>
    /// The SQLite schema, written by hand.
    ///
    /// EF6's SQLite provider cannot generate tables: Database.CreateIfNotExists()
    /// returns without throwing and creates nothing (verified). EF6 Code First
    /// Migrations cannot be used either, because DAL/Migrations hardcodes
    /// "dbo." table names and its SqlGenerator is SQL Server only. So the DDL
    /// lives here and is applied on first run.
    ///
    /// Names must match the SQL Server schema exactly, including the lowercase
    /// "id" on 12 tables versus uppercase "Id" on CatalogItems and InvoiceLines,
    /// and including "RememberMes" (EF6's pluraliser produced that spelling and
    /// existing SQL Server data is keyed to it).
    /// </summary>
    public static class SqliteSchema
    {
        public const string Invariant = "System.Data.SQLite";

        public const string Ddl = @"
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS UserGroups (
    id          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Title       TEXT NULL,
    IsBuiltIn   INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS AccessGrants (
    id          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Section     INTEGER NOT NULL,
    Operation   INTEGER NOT NULL,
    UserGroup_id INTEGER NULL REFERENCES UserGroups (id)
);

CREATE TABLE IF NOT EXISTS Users (
    id           INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Name         TEXT NULL,
    UserName     TEXT NULL,
    Password     TEXT NULL,
    Picture      TEXT NULL,
    RegDate      DATETIME NOT NULL,
    DeleteStatus INTEGER NOT NULL,
    UserGroup_id INTEGER NULL REFERENCES UserGroups (id)
);

CREATE TABLE IF NOT EXISTS Customers (
    id           INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Name         TEXT NULL,
    Phone        TEXT NULL,
    RegDate      DATETIME NOT NULL,
    DeleteStatus INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS ActivityCategories (
    id           INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    CategoryName TEXT NULL,
    DeleteStatus INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS Activities (
    id                   INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Title                TEXT NULL,
    Info                 TEXT NULL,
    RegDate              DATETIME NOT NULL,
    DeleteStatus         INTEGER NOT NULL,
    ActivityCategory_id  INTEGER NULL REFERENCES ActivityCategories (id),
    Customer_id          INTEGER NULL REFERENCES Customers (id),
    User_id              INTEGER NULL REFERENCES Users (id)
);

CREATE TABLE IF NOT EXISTS OffCodes (
    id           INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Code         TEXT NULL,
    IsPrice      INTEGER NOT NULL,
    Price        DECIMAL(18,2) NULL,
    Percent      INTEGER NULL,
    RegDate      DATETIME NOT NULL,
    ExpireDate   DATETIME NULL,
    LimitCount   INTEGER NULL,
    DeleteStatus INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS Invoices (
    id              INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    RegDate         DATETIME NOT NULL,
    IsCheckedout    INTEGER NOT NULL,
    CheckoutDate    DATETIME NULL,
    DeleteStatus    INTEGER NOT NULL,
    OffCode         TEXT NULL,
    DiscountAmount  DECIMAL(18,2) NOT NULL,
    Customer_id     INTEGER NULL REFERENCES Customers (id),
    User_id         INTEGER NULL REFERENCES Users (id)
);

CREATE TABLE IF NOT EXISTS CatalogItems (
    Id         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Name       TEXT NULL,
    Kind       INTEGER NOT NULL,
    SalePrice  DECIMAL(18,2) NOT NULL,
    Stock      INTEGER NOT NULL,
    DeleteStatus INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS InvoiceLines (
    Id            INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    InvoiceId     INTEGER NOT NULL,
    CatalogItemId INTEGER NOT NULL,
    Quantity      INTEGER NOT NULL,
    UnitPrice     DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (InvoiceId)     REFERENCES Invoices (id)     ON DELETE CASCADE,
    FOREIGN KEY (CatalogItemId) REFERENCES CatalogItems (Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Reminders (
    id           INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Title        TEXT NULL,
    Info         TEXT NULL,
    RegDate      DATETIME NOT NULL,
    RemindDate   DATETIME NOT NULL,
    DeleteStatus INTEGER NOT NULL,
    IsReminded   INTEGER NOT NULL,
    User_id      INTEGER NULL REFERENCES Users (id)
);

CREATE TABLE IF NOT EXISTS MessagePanels (
    id         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    APIToken   TEXT NULL,
    LineNumber TEXT NULL,
    RegDate    DATETIME NOT NULL,
    EditDate   DATETIME NOT NULL
);

CREATE TABLE IF NOT EXISTS Messages (
    id           INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Content      TEXT NULL,
    DeleteStatus INTEGER NOT NULL,
    RegDate      DATETIME NOT NULL
);

-- EF6 pluralised DbSet<RememberMe> to ""RememberMes"" (no trailing s on the Me).
-- The name is load-bearing: existing SQL Server rows live in a table of this name.
CREATE TABLE IF NOT EXISTS RememberMes (
    id            INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    UserName      TEXT NULL,
    IsRemembered  INTEGER NOT NULL,
    LastLoginTime DATETIME NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_AccessGrants_UserGroup_id ON AccessGrants (UserGroup_id);
CREATE INDEX IF NOT EXISTS IX_Users_UserGroup_id          ON Users (UserGroup_id);
CREATE INDEX IF NOT EXISTS IX_Activities_Category_id     ON Activities (ActivityCategory_id);
CREATE INDEX IF NOT EXISTS IX_Activities_Customer_id      ON Activities (Customer_id);
CREATE INDEX IF NOT EXISTS IX_Activities_User_id          ON Activities (User_id);
CREATE INDEX IF NOT EXISTS IX_Invoices_Customer_id        ON Invoices (Customer_id);
CREATE INDEX IF NOT EXISTS IX_Invoices_User_id            ON Invoices (User_id);
CREATE INDEX IF NOT EXISTS IX_InvoiceLines_InvoiceId      ON InvoiceLines (InvoiceId);
CREATE INDEX IF NOT EXISTS IX_InvoiceLines_CatalogItemId  ON InvoiceLines (CatalogItemId);
CREATE INDEX IF NOT EXISTS IX_Reminders_User_id           ON Reminders (User_id);
";

        public static void Ensure(DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (connection.State != ConnectionState.Open) connection.Open();

            using (var command = connection.CreateCommand())
            {
                // PRAGMA foreign_keys is a no-op inside a transaction, and the
                // batch below is not one, so this applies.
                command.CommandText = Ddl;
                command.ExecuteNonQuery();
            }
        }
    }
}
```

- [ ] **Step 6: Register the EF6 provider and add the connection constructor**

Replace `DAL/DB.cs` with the following. **The default provider is unchanged in this task - still `conStr` from App.config, i.e. SQL Server.** Only the plumbing is added.

```csharp
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.SQLite;
using BE;

namespace DAL
{
    public class DB : DbContext
    {
        /// <summary>
        /// The SQL Server connection string, kept for the existing settings screen
        /// and for the SQL Server provider. Task 4 replaces this with DataSource.
        /// </summary>
        public static string ConnectionString =
            ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;

        static DB()
        {
            // Registers the SQLite ADO.NET factory and EF6 provider services in
            // code. App.config registration does not work on .NET 10: the
            // system.data section is not recognised because System.Data.Common
            // is a separate assembly, and even once declared the factory cannot
            // be resolved to a provider invariant.
            DbConfiguration.SetConfiguration(new SqliteConfiguration());
        }

        private sealed class SqliteConfiguration : DbConfiguration
        {
            public SqliteConfiguration()
            {
                SetProviderFactory(
                    SqliteSchema.Invariant,
                    SQLiteFactory.Instance);
                SetProviderServices(
                    SqliteSchema.Invariant,
                    (DbProviderServices)Activator.CreateInstance(Type.GetType(
                        "System.Data.SQLite.EF6.SQLiteProviderServices, System.Data.SQLite.EF6",
                        throwOnError: true)));
            }
        }

        public DB() : base("conStr")
        {
        }

        /// <summary>
        /// Used by the SQLite path and by tests, which supply their own
        /// connection (a temporary file, in the test case).
        /// </summary>
        public DB(DbConnection connection) : base(connection, contextOwnsConnection: true)
        {
        }

        public DbSet<Activity> Activities { get; set; }
        public DbSet<ActivityCategory> ActivityCategories { get; set; }
        public DbSet<CatalogItem> CatalogItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<AccessGrant> AccessGrants { get; set; }
        public DbSet<OffCode> OffCodes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessagePanel> MessagePanels { get; set; }
        public DbSet<RememberMe> RememberMe { get; set; }
    }
}
```

- [ ] **Step 7: Run the test to verify it passes**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: `Passed!  - Failed: 0, Passed: 70, Skipped: 0, Total: 70` (66 existing + 4 new).

If you see `Unable to load DLL 'e_sqlite3'`, Step 4's second package was not added. If you see `Unrecognized configuration section system.data`, you are using an app.config registration rather than the `DbConfiguration` subclass.

- [ ] **Step 8: Verify the native library reaches the publish output**

Run:
```powershell
dotnet publish CRMPeyvand/CRMPeyvand.csproj -c Release -r win-x64 --self-contained false -p:DebugType=none -o artifacts/publish --nologo
Get-ChildItem artifacts/publish -Recurse -Filter "e_sqlite3.dll"
```
Expected: exactly one `e_sqlite3.dll` somewhere under the publish output. If it is missing, the MSI would ship an app that cannot open a database. Record its relative path - later tasks and the installer task depend on it.

- [ ] **Step 9: Commit**

```bash
git add DAL/DAL.csproj DAL/DB.cs DAL/SqliteSchema.cs CRMPeyvand.Tests/SqliteTestDb.cs CRMPeyvand.Tests/SqliteSchemaTests.cs
git commit -m "feat(dal): add SQLite provider and hand-written schema

System.Data.SQLite.EF6 2.0.3 on EF6 6.5.1, with the provider registered
through a DbConfiguration subclass rather than app.config: on .NET 10 the
system.data section is not recognised (System.Data.Common is a separate
assembly) and the factory cannot be resolved to a provider invariant even
once declared.

SQLitePCLRaw.bundle_e_sqlite3 is referenced explicitly because
System.Data.SQLite's netstandard2.1 target does not carry the native
library transitively; without it SQLiteConnection.Open throws
'Unable to load DLL e_sqlite3'. This is what keeps the promise that
SQLite needs nothing installed on the target machine.

The schema is hand-written because EF6's SQLite provider cannot create
tables: CreateIfNotExists returns without throwing and creates nothing.
Table and column names match the SQL Server schema exactly, including the
RememberMes pluralisation typo, so existing data is not orphaned.

The default provider is unchanged; the next task introduces DataSource."
```

---

### Task 3: `DataSource` — the provider choice

**Files:**
- Create: `DAL/DataFolder.cs`
- Create: `DAL/DataSource.cs`
- Create: `CRMPeyvand.Tests/DataSourceTests.cs`

**Interfaces:**
- Consumes: `DAL.SqliteSchema.Invariant`
- Produces:
  - `public enum DbProviderKind { Sqlite, SqlServer }`
  - `public sealed class DataSource` with:
    - `public static DataSource Current { get; }`
    - `public DbProviderKind Kind { get; set; }`
    - `public string ConnectionString { get; set; }`
    - `public static DataSource DefaultSqlite()`
    - `public static DataSource DefaultSqlServer()`
    - `public static void Use(DataSource dataSource)` — persists and resets `Current`
    - `public static void UseForTests(DataSource dataSource)` — sets `Current` without persisting
    - `public static string Test(DataSource dataSource)` — returns a Persian message, `null` on success
    - `public static string FilePath { get; }` — the JSON file location
  - `public static class DataFolder` with `public static string Resolve()`

- [ ] **Step 1: Write the failing tests**

Create `CRMPeyvand.Tests/DataSourceTests.cs`:

```csharp
using System;
using System.IO;
using System.Linq;
using DAL;
using Xunit;

namespace CRMPeyvand.Tests
{
    public class DataSourceTests : IDisposable
    {
        private readonly string _original = DataSource.Current.ConnectionString;
        private readonly DbProviderKind _originalKind = DataSource.Current.Kind;

        public void Dispose() =>
            DataSource.UseForTests(new DataSource
            {
                Kind = _originalKind,
                ConnectionString = _original,
            });

        [Fact]
        public void Default_is_sqlite()
        {
            Assert.Equal(DbProviderKind.Sqlite, DataSource.DefaultSqlite().Kind);
        }

        [Fact]
        public void Sqlite_default_connection_string_carries_the_provider_name()
        {
            // EF6 only resolves the SQLite provider because the connection string
            // names the invariant.
            Assert.Contains("providerName=" + SqliteSchema.Invariant,
                            DataSource.DefaultSqlite().ConnectionString);
        }

        [Fact]
        public void Sqlite_database_lives_beside_the_settings_file()
        {
            var ds = DataSource.DefaultSqlite();
            Assert.Contains(DataSource.SqliteFileName, ds.ConnectionString);
        }

        [Fact]
        public void Test_reports_success_in_persian_for_a_real_sqlite_file()
        {
            var path = Path.Combine(Path.GetTempPath(),
                                   "probe-" + Guid.NewGuid().ToString("N") + ".db");
            var ds = new DataSource
            {
                Kind = DbProviderKind.Sqlite,
                ConnectionString = new System.Data.SQLite.SQLiteConnectionStringBuilder
                {
                    DataSource = path,
                }.ToString() + ";providerName=" + SqliteSchema.Invariant,
            };
            try
            {
                Assert.Null(DataSource.Test(ds));
                Assert.True(File.Exists(path));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void Test_reports_failure_in_persian_for_an_unusable_sql_server()
        {
            var ds = new DataSource
            {
                Kind = DbProviderKind.SqlServer,
                ConnectionString =
                    "Data Source=.;Initial Catalog=CRMPeyvand;Integrated Security=true;" +
                    "TrustServerCertificate=True;Connect Timeout=1",
            };
            var message = DataSource.Test(ds);
            Assert.NotNull(message);
            // A Persian sentence, not an exception name.
            Assert.Contains("متصل", message);
        }

        [Fact]
        public void Use_writes_the_choice_to_disk_and_Current_follows()
        {
            var ds = new DataSource
            {
                Kind = DbProviderKind.Sqlite,
                ConnectionString = "Data Source=" + Path.Combine(Path.GetTempPath(), "x.db")
                                  + ";providerName=" + SqliteSchema.Invariant,
            };
            DataSource.Use(ds);
            Assert.Equal(DbProviderKind.Sqlite, DataSource.Current.Kind);
            Assert.True(File.Exists(DataSource.FilePath));
        }
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: compile errors for missing `DataSource`, `DataFolder`, `DbProviderKind`.

- [ ] **Step 3: Write `DataFolder`**

Create `DAL/DataFolder.cs`:

```csharp
using System;
using System.IO;

namespace DAL
{
    /// <summary>
    /// Resolves a writable directory for the database and the provider settings.
    ///
    /// Prefers %ProgramData%\CRMPeyvand so that everyone signing in to the same
    /// machine shares one database, which is what SQL Server mode gives them. The
    /// folder is created by whoever runs first, so writability is probed rather
    /// than assumed; a later user falls back to %LocalAppData% and gets their own
    /// database. Same pattern, and the same trade-off, as
    /// PublicMethods.UserPicturesDirectory.
    /// </summary>
    public static class DataFolder
    {
        public const string Name = "CRMPeyvand";

        public static string Resolve()
        {
            var shared = TryUnder(Environment.SpecialFolder.CommonApplicationData);
            if (shared != null) return shared;
            return TryUnder(Environment.SpecialFolder.LocalApplicationData);
        }

        private static string TryUnder(Environment.SpecialFolder root)
        {
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(root), Name);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                var probe = Path.Combine(path, ".write-test-" + Guid.NewGuid().ToString("N"));
                try
                {
                    using (File.Create(probe)) { }
                }
                finally
                {
                    File.Delete(probe);
                }

                return path;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
```

- [ ] **Step 4: Write `DataSource`**

Create `DAL/DataSource.cs`:

```csharp
using System;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DAL
{
    public enum DbProviderKind
    {
        Sqlite,
        SqlServer,
    }

    /// <summary>
    /// Which database the application is talking to, and how to reach it.
    ///
    /// The default is SQLite so that a fresh install works with nothing else
    /// installed. SQL Server is opt-in: the settings screen writes a
    /// SqlServer DataSource and it is used from then on.
    ///
    /// Resolution happens once per process behind a lock. That matters: the EF6
    /// initialiser runs inside DbContext construction and MigrateDatabaseToLatestVersion
    /// is not thread-safe, and thirteen DAL classes construct a DB in a field
    /// initialiser.
    /// </summary>
    public sealed class DataSource
    {
        public const string SqliteFileName = "CRMPeyvand.db";
        private const string SettingsFileName = "provider.json";

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DbProviderKind Kind { get; set; } = DbProviderKind.Sqlite;

        public string ConnectionString { get; set; } = string.Empty;

        private static readonly object Gate = new object();
        private static DataSource _current;

        public static string FilePath => Path.Combine(DataFolder.Resolve(), SettingsFileName);

        public static DataSource Current
        {
            get
            {
                if (_current != null) return _current;
                lock (Gate)
                {
                    return _current ?? (_current = Load());
                }
            }
        }

        public static DataSource DefaultSqlite() => new DataSource
        {
            Kind = DbProviderKind.Sqlite,
            ConnectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = Path.Combine(DataFolder.Resolve(), SqliteFileName),
            }.ToString() + ";providerName=" + SqliteSchema.Invariant,
        };

        /// <summary>
        /// The connection string that shipped in App.config, kept as the
        /// SQL Server default so an existing install keeps working untouched.
        /// </summary>
        public static DataSource DefaultSqlServer() => new DataSource
        {
            Kind = DbProviderKind.SqlServer,
            ConnectionString = DB.ConnectionString,
        };

        public static void Use(DataSource dataSource)
        {
            if (dataSource == null) throw new ArgumentNullException(nameof(dataSource));
            if (string.IsNullOrWhiteSpace(dataSource.ConnectionString))
                throw new ArgumentException("رشته اتصال نمی تواند خالی باشد", nameof(dataSource));

            lock (Gate)
            {
                var json = JsonSerializer.Serialize(dataSource, Options);
                File.WriteAllText(FilePath, json);
                _current = dataSource;
            }
        }

        /// <summary>Sets the in-memory choice only. Tests only.</summary>
        public static void UseForTests(DataSource dataSource)
        {
            lock (Gate) { _current = dataSource; }
        }

        /// <summary>
        /// Opens the connection and, for SQLite, applies the schema. Returns
        /// null on success, or a Persian sentence describing the problem.
        /// </summary>
        public static string Test(DataSource dataSource)
        {
            if (dataSource == null) return "پیکربندی پایگاه داده نامعتبر است";

            try
            {
                using (var connection = CreateConnection(dataSource))
                {
                    connection.Open();
                    if (dataSource.Kind == DbProviderKind.Sqlite)
                        SqliteSchema.Ensure(connection);
                }
                return null;
            }
            catch (Exception e)
            {
                return "اتصال به پایگاه داده برقرار نشد:\n" + e.Message;
            }
        }

        internal static DbConnection CreateConnection(DataSource dataSource)
        {
            var builder = new SQLiteConnectionStringBuilder(
                dataSource.ConnectionString.Split(new[] { ";providerName=" },
                                                 StringSplitOptions.None)[0]);

            // Microsoft.Data.SqlClient for SQL Server, raw ADO.NET for SQLite.
            return dataSource.Kind == DbProviderKind.Sqlite
                ? (DbConnection)new SQLiteConnection(builder.ToString())
                : new Microsoft.Data.SqlClient.SqlConnection(dataSource.ConnectionString);
        }

        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        private static DataSource Load()
        {
            try
            {
                var path = FilePath;
                if (File.Exists(path))
                {
                    var loaded = JsonSerializer.Deserialize<DataSource>(File.ReadAllText(path), Options);
                    if (loaded != null && !string.IsNullOrWhiteSpace(loaded.ConnectionString))
                        return loaded;
                }
            }
            catch (Exception)
            {
                // A corrupt or unreadable settings file must not stop the app
                // from starting; fall through to the default.
            }

            return DefaultSqlite();
        }
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: `Failed: 0, Passed: 76, Skipped: 0, Total: 76`.

- [ ] **Step 6: Commit**

```bash
git add DAL/DataFolder.cs DAL/DataSource.cs CRMPeyvand.Tests/DataSourceTests.cs
git commit -m "feat(dal): add DataSource for provider choice and persistence

Holds the database provider kind, its connection string, and the on-disk
choice, resolved once per process behind a lock. The lock is not
decoration: the EF6 initialiser runs inside DbContext construction,
MigrateDatabaseToLatestVersion is not thread-safe, and thirteen DAL
classes create a DB in a field initialiser.

DataFolder prefers %ProgramData% so everyone on a machine shares one
database, as SQL Server mode does, and probes writability rather than
assuming it - the first user to sign in owns that folder, so a later user
falls back to %LocalAppData% and gets their own database. Same trade-off,
and the same fallback, as the employee photos folder.

Default is SQLite. A corrupt provider.json falls back to the default
rather than blocking startup."
```

---

### Task 4: Make `DB` provider-aware

Wires `DataSource` into the `DbContext` and gives each provider the right schema strategy. **After this task the app runs on SQLite by default.**

**Files:**
- Modify: `DAL/DB.cs`
- Modify: `CRMPeyvand.Tests/SqliteTestDb.cs`
- Create: `CRMPeyvand.Tests/DbContextTests.cs`

**Interfaces:**
- Consumes: `DataSource.Current`, `SqliteSchema.Ensure`
- Produces: `public DB()` now resolves `DataSource.Current`; `public DB(DbConnection)` unchanged.

- [ ] **Step 1: Write the failing test**

Create `CRMPeyvand.Tests/DbContextTests.cs`:

```csharp
using System;
using System.IO;
using System.Linq;
using DAL;
using Xunit;

namespace CRMPeyvand.Tests
{
    public class DbContextTests
    {
        [Fact]
        public void A_fresh_sqlite_database_is_usable_through_the_default_constructor()
        {
            var path = Path.Combine(Path.GetTempPath(),
                                   "ctx-" + Guid.NewGuid().ToString("N") + ".db");
            DataSource.UseForTests(new DataSource
            {
                Kind = DbProviderKind.Sqlite,
                ConnectionString =
                    "Data Source=" + path + ";providerName=" + SqliteSchema.Invariant,
            });

            try
            {
                // The whole point: no schema step, no SQL Server, just open it.
                using (var db = new DAL.DB())
                {
                    db.Customers.Add(new BE.Customer
                    {
                        Name = "بدون اسکیما",
                        Phone = "09121111111",
                        RegDate = DateTime.Now,
                    });
                    db.SaveChanges();

                    Assert.Equal(1, db.Customers.Count());
                }
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}
```

- [ ] **Step 2: Run it to verify it fails**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~DbContextTests`
Expected: FAIL. The parameterless `DB()` still reads the SQL Server `conStr` from App.config, so it throws a SQL Server connection error (or "no SQL Server").

- [ ] **Step 3: Rewrite `DB` to be provider-aware**

Replace the constructor region of `DAL/DB.cs` (keep the 13 `DbSet`s and the static `DB()` registration block exactly as they are) with:

```csharp
        /// <summary>
        /// The SQL Server connection string from App.config. Retained so the
        /// settings screen can offer it as a starting point; DataSource decides
        /// which provider is actually in use.
        /// </summary>
        public static string ConnectionString =
            ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;

        static DB()
        {
            DbConfiguration.SetConfiguration(new SqliteConfiguration());
        }

        private sealed class SqliteConfiguration : DbConfiguration
        {
            public SqliteConfiguration()
            {
                SetProviderFactory(SqliteSchema.Invariant, SQLiteFactory.Instance);
                SetProviderServices(
                    SqliteSchema.Invariant,
                    (DbProviderServices)Activator.CreateInstance(Type.GetType(
                        "System.Data.SQLite.EF6.SQLiteProviderServices, System.Data.SQLite.EF6",
                        throwOnError: true)));
            }
        }

        /// <summary>
        /// Resolves the configured provider. SQLite is applied through
        /// DataSource.CreateConnection so the schema is ensured before the first
        /// query; SQL Server keeps its EF6 migrations, which are the only
        /// supported way to evolve that schema.
        /// </summary>
        public DB() : base(DataSource.CreateConnection(DataSource.Current), contextOwnsConnection: true)
        {
        }

        public DB(DbConnection connection) : base(connection, contextOwnsConnection: true)
        {
        }
```

Add `using System.Data.Entity.Infrastructure;` to the `using` block at the top of the file (it is not currently imported).

- [ ] **Step 4: Make `DataSource.CreateConnection` ensure the SQLite schema**

In `DAL/DataSource.cs`, change the `Test` method's body and add a shared helper so both paths behave identically. Replace `CreateConnection` with:

```csharp
        internal static DbConnection CreateConnection(DataSource dataSource)
        {
            if (dataSource == null) throw new ArgumentNullException(nameof(dataSource));

            if (dataSource.Kind == DbProviderKind.SqlServer)
                return new Microsoft.Data.SqlClient.SqlConnection(dataSource.ConnectionString);

            var path = new SQLiteConnectionStringBuilder(
                dataSource.ConnectionString.Split(new[] { ";providerName=" },
                                                 StringSplitOptions.None)[0])
                .DataSource;

            var connection = new SQLiteConnection(dataSource.ConnectionString);
            connection.Open();
            SqliteSchema.Ensure(connection);
            return connection;
        }
```

The `Open()` plus `Ensure` here is what makes a fresh install work with no separate migration step.

- [ ] **Step 5: Fix the test helper to match the new reality**

In `CRMPeyvand.Tests/SqliteTestDb.cs`, `WithDb` no longer needs to apply the schema itself, because `DataSource.CreateConnection` does. But `WithDb` uses the explicit `DB(DbConnection)` constructor, which does not. Leave `WithRaw` as the single place that calls `SqliteSchema.Ensure`, and change `WithDb` to keep calling it, so the two paths stay independently testable. No edit required.

- [ ] **Step 6: Run the tests**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: `Failed: 0, Passed: 77, Skipped: 0, Total: 77`.

- [ ] **Step 7: Verify the app itself starts on SQLite**

Run from the repository root:
```powershell
dotnet run --project CRMPeyvand/CRMPeyvand.csproj -c Release
```
Expected: the login/first-run window appears and no dialog mentions SQL Server. Close it. This is the first point at which the app is genuinely runnable with nothing installed.

- [ ] **Step 8: Commit**

```bash
git add DAL/DB.cs DAL/DataSource.cs CRMPeyvand.Tests/DbContextTests.cs
git commit -m "feat(dal): run the context on the configured provider

DbContext() now resolves DataSource.Current. SQLite connections are opened
and have the schema applied as they are created, so a fresh install needs
no separate migration step; SQL Server keeps MigrateDatabaseToLatestVersion,
which remains the only supported way to evolve that schema.

This makes SQLite the default: the app starts with no database installed
and nothing else to set up."
```

---

### Task 5: `GridTable` plus the three simplest grids

Introduces the helper that keeps the Persian grid headers intact, then converts Customer, ActivityCategory and Activity. These three have no joins or aggregates, so they establish the pattern.

**Files:**
- Create: `DAL/GridTable.cs`
- Create: `CRMPeyvand.Tests/GridQueriesTests.cs`
- Modify: `DAL/CustomerDAL.cs`, `DAL/ActivityCategoryDAL.cs`, `DAL/ActivityDAL.cs`

**Interfaces:**
- Consumes: `DAL.DB`, `SqliteTestDb.WithDb`
- Produces:
  - `public static class GridTable` with `public static DataTable Build(string[] columnNames, System.Collections.Generic.IEnumerable<object[]> rows)`
  - `CustomerDAL.Read()` / `.Search(string)` return the same columns as before: `نام`, `شماره تماس`, `تاریخ ثبت`
  - `ActivityCategoryDAL.Read()` / `.Search(string)`: `ردیف`, `نام دسته بندی`
  - `ActivityDAL.Read()` / `.Search(string)`: `ردیف`, `عنوان`, `توضبحات`, `دسته بندی`, `نام کاربر`, `تاریخ ثبت`

  Note the third column is spelled `توضبحات` (no ی) and the fifth is `Users.UserName`, not `Users.Name`. Both are wrong-looking but are what the grid shows today, so both are preserved verbatim.

- [ ] **Step 1: Write the failing test**

Create `CRMPeyvand.Tests/GridQueriesTests.cs`:

```csharp
using System;
using System.Data;
using System.Linq;
using DAL;
using Xunit;

namespace CRMPeyvand.Tests
{
    public class GridQueriesTests
    {
        [Fact]
        public void Build_preserves_column_names_and_order()
        {
            var table = GridTable.Build(
                new[] { "نام", "شماره تماس" },
                new[] { new object[] { "علی", "09120000000" } });

            Assert.Equal(new[] { "نام", "شماره تماس" },
                         table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            Assert.Single(table.Rows);
            Assert.Equal("علی", table.Rows[0]["نام"]);
        }

        [Fact]
        public void Build_with_no_rows_still_has_the_columns()
        {
            // The grid binds AutoGenerateColumns, so an empty result must still
            // produce the headers or the grid renders blank.
            var table = GridTable.Build(new[] { "نام", "شماره تماس" }, Enumerable.Empty<object[]>());
            Assert.Equal(2, table.Columns.Count);
            Assert.Equal(0, table.Rows.Count);
        }

        [Fact]
        public void Customer_Read_returns_the_persian_headers()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.Customers.Add(new BE.Customer
                {
                    Name = "رضا", Phone = "09121110000", RegDate = DateTime.Now,
                });
                db.SaveChanges();

                var table = new CustomerDAL(db).Read();
                Assert.Equal(new[] { "نام", "شماره تماس", "تاریخ ثبت" },
                             table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                Assert.Single(table.Rows);
            });
        }

        [Fact]
        public void Customer_Read_excludes_soft_deleted_rows()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.Customers.Add(new BE.Customer
                {
                    Name = "زنده", Phone = "09120000001", RegDate = DateTime.Now,
                });
                db.Customers.Add(new BE.Customer
                {
                    Name = "حذف‌شده", Phone = "09120000002",
                    RegDate = DateTime.Now, DeleteStatus = true,
                });
                db.SaveChanges();

                var table = new CustomerDAL(db).Read();
                Assert.Single(table.Rows);
                Assert.Equal("زنده", table.Rows[0]["نام"]);
            });
        }

        [Fact]
        public void Customer_Search_matches_persian_text_case_insensitively()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.Customers.Add(new BE.Customer
                {
                    Name = "مشتری الف", Phone = "09120000003", RegDate = DateTime.Now,
                });
                db.Customers.Add(new BE.Customer
                {
                    Name = "مشتری ب", Phone = "09120000004", RegDate = DateTime.Now,
                });
                db.SaveChanges();

                var table = new CustomerDAL(db).Search("الف");
                Assert.Single(table.Rows);
                Assert.Equal("مشتری الف", table.Rows[0]["نام"]);
            });
        }

        [Fact]
        public void Customer_Search_matches_the_phone_number()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.Customers.Add(new BE.Customer
                {
                    Name = "الف", Phone = "09121234567", RegDate = DateTime.Now,
                });
                db.SaveChanges();

                var table = new CustomerDAL(db).Search("1234");
                Assert.Single(table.Rows);
            });
        }
    }
}
```

- [ ] **Step 2: Run to verify failure**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~GridQueriesTests`
Expected: compile errors for `GridTable` and the `CustomerDAL(DB)` constructor.

- [ ] **Step 3: Write `GridTable`**

Create `DAL/GridTable.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    /// <summary>
    /// Builds the DataTable that backs a read-only grid.
    ///
    /// The UI fills grids with PublicMethods.dgvFiller using
    /// AutoGenerateColumns = true, so the column names here ARE the headers the
    /// employee sees. They must therefore match the SQL these queries replaced,
    /// character for character, or the grid silently renders an empty column.
    ///
    /// The row limit (1000 on most screens) is applied by the caller, not here.
    /// </summary>
    public static class GridTable
    {
        /// <summary>How many rows a grid shows. Matches the old "SELECT TOP (1000)".</summary>
        public const int DefaultRowLimit = 1000;

        public static DataTable Build(string[] columnNames, IEnumerable<object[]> rows)
        {
            if (columnNames == null) throw new ArgumentNullException(nameof(columnNames));

            var table = new DataTable();
            foreach (var name in columnNames)
                table.Columns.Add(name, typeof(object));

            if (rows == null) return table;

            foreach (var row in rows)
            {
                if (row == null) continue;
                if (row.Length != columnNames.Length)
                    throw new ArgumentException(
                        "row has " + row.Length + " values but there are " + columnNames.Length + " columns");

                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Substring match that behaves the same on both providers.
        ///
        /// string.Contains in a LINQ-to-Entities Where clause is not usable here:
        /// it compiles to CHARINDEX, which SQLite does not have (verified:
        /// "no such function: CHARINDEX"). Filtering after materialisation also
        /// gets Persian text right, because SQLite's LIKE is not Unicode-aware
        /// and the previous N'%...%' pattern was only correct under SQL Server's
        /// collation.
        /// </summary>
        public static bool Matches(string filter, params string[] candidates)
        {
            if (string.IsNullOrEmpty(filter)) return true;
            if (candidates == null) return false;

            foreach (var candidate in candidates)
            {
                if (string.IsNullOrEmpty(candidate)) continue;
                if (candidate.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }
    }
}
```

- [ ] **Step 4: Rewrite `CustomerDAL`**

In `DAL/CustomerDAL.cs`, replace the `Read()` and `Search(string)` methods (lines 41-51 and 99-112) and add a constructor. Replace the field declaration at the top with:

```csharp
        DB db;

        public CustomerDAL()
        {
            db = new DB();
        }

        /// <summary>Tests supply their own context.</summary>
        public CustomerDAL(DB db)
        {
            this.db = db;
        }
```

Then replace the two query methods with:

```csharp
        private static readonly string[] ReadColumns =
            { "نام", "شماره تماس", "تاریخ ثبت" };

        public DataTable Read()
        {
            var rows = db.Customers
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .Select(i => new object[] { i.Name, i.Phone, i.RegDate })
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }

        public DataTable Search(string Filter)
        {
            var rows = db.Customers
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .Select(i => new object[] { i.Name, i.Phone, i.RegDate })
                .AsEnumerable()
                .Where(r => GridTable.Matches(Filter, (string)r[0], (string)r[1]))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }
```

Also delete the now-unused `using Microsoft.Data.SqlClient;` and the `var commandbuilder = new SqlCommandBuilder(sqlAdapter);` dead local, and add `using System.Collections.Generic;` if it is missing.

- [ ] **Step 5: Rewrite `ActivityCategoryDAL` the same way**

Its SQL is `SELECT TOP (100) id AS ردیف, CategoryName AS [نام دسته بندی] FROM dbo.ActivityCategories WHERE (DeleteStatus = 0) ORDER BY id DESC`, plus a search on the computed name. Note the **100** row limit here, not 1000:

```csharp
        private static readonly string[] ReadColumns =
            { "ردیف", "نام دسته بندی" };

        public DataTable Read()
        {
            var rows = db.ActivityCategories
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(100)
                .Select(i => new object[] { i.id, i.CategoryName })
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }

        public DataTable Search(string Filter)
        {
            var rows = db.ActivityCategories
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(100)
                .Select(i => new object[] { i.id, i.CategoryName })
                .AsEnumerable()
                .Where(r => GridTable.Matches(Filter, (string)r[1]))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }
```

Add the same two-constructor pattern to this class.

- [ ] **Step 6: Rewrite `ActivityDAL`**

The old SQL (at `ActivityDAL.cs:33`) is:

```
SELECT TOP (1000)  dbo.Activities.id AS ردیف ,  dbo.Activities.Title AS عنوان,
       dbo.Activities.Info AS توضبحات, dbo.ActivityCategories.CategoryName AS [دسته بندی],
       dbo.Users.UserName AS [نام کاربر], dbo.Activities.RegDate AS [تاریخ ثبت]
FROM dbo.Activities
     INNER JOIN dbo.ActivityCategories ON dbo.Activities.ActivityCategory_id = dbo.ActivityCategories.id
     INNER JOIN dbo.Users ON dbo.Activities.User_id = dbo.Users.id
WHERE (dbo.Activities.DeleteStatus = 0) ORDER BY dbo.Activities.id DESC
```

There is **no Customer column** in this grid. Add the two-constructor pattern and:

```csharp
        // Copied verbatim from the SQL this replaces. Note "توضبحات" is spelled
        // without a ی, and the fifth column is Users.UserName rather than
        // Users.Name. Both look like mistakes and both are what employees see
        // today, so they are preserved rather than corrected here.
        private static readonly string[] ReadColumns =
            { "ردیف", "عنوان", "توضبحات", "دسته بندی", "نام کاربر", "تاریخ ثبت" };

        private IQueryable<object[]> ActivityRows()
        {
            return db.Activities
                .Include("ActivityCategory")
                .Include("User")
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .Select(i => new object[]
                {
                    i.id,
                    i.Title,
                    i.Info,
                    i.ActivityCategory == null ? null : i.ActivityCategory.CategoryName,
                    i.User == null ? null : i.User.UserName,
                    i.RegDate,
                });
        }

        public DataTable Read() => GridTable.Build(ReadColumns, ActivityRows().ToList());

        public DataTable Search(string Filter)
        {
            // The old query matched Title, Info, CategoryName and UserName -
            // not the customer, because the customer is not in this grid.
            var rows = ActivityRows()
                .AsEnumerable()
                .Where(r => GridTable.Matches(Filter,
                    (string)r[1], (string)r[2], (string)r[3], (string)r[4]))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }
```

**`Include` is required, not optional.** This class previously reached the category and user through raw SQL joins. Going through EF without `Include` returns nulls, because EF6 does not lazy load.

- [ ] **Step 7: Run the tests**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: `Failed: 0, Passed: 83, Skipped: 0, Total: 83`.

- [ ] **Step 8: Manually confirm one grid**

Run `dotnet run --project CRMPeyvand/CRMPeyvand.csproj -c Release`, register a first user, open the Customers window.
Expected: the three Persian headers `نام`, `شماره تماس`, `تاریخ ثبت` are unchanged, and search by name and by phone both work.

- [ ] **Step 9: Commit**

```bash
git add DAL/GridTable.cs DAL/CustomerDAL.cs DAL/ActivityCategoryDAL.cs DAL/ActivityDAL.cs CRMPeyvand.Tests/GridQueriesTests.cs
git commit -m "refactor(dal): convert customer, activity and category grids to LINQ

Replaces three SqlDataAdapter queries containing SELECT TOP, dbo. prefixes
and N'' literals. GridTable.Build keeps the Persian column names, which
matter because PublicMethods.dgvFiller binds with AutoGenerateColumns, so
the column names are the headers the employee sees.

Search filters after materialisation rather than with string.Contains:
Contains compiles to CHARINDEX, which SQLite does not have (verified), and
SQLite's LIKE is not Unicode-aware so the old N'%...%' pattern was only
ever correct under SQL Server's collation.

Each DAL class gains a constructor taking a DB so it can be tested; the
parameterless one is what the BLL uses and is unchanged in signature."
```

---

### Task 6: Users, user groups and reminders

Same transformation, but these have joins.

**Files:**
- Modify: `DAL/UserDAL.cs`, `DAL/UserGroupDAL.cs`, `DAL/ReminderDAL.cs`
- Modify: `CRMPeyvand.Tests/GridQueriesTests.cs`

**Interfaces:**
- Consumes: `GridTable`, `GridTable.Matches`
- Produces: unchanged public signatures; the same Persian columns as the SQL they replace:
  - `UserDAL`: `نام`, `نام کاربری`, `گروه کاربری`, `تاریخ ثبت`, filtered to `UserGroups.IsBuiltIn == 0`
  - `UserGroupDAL`: `نام گروه کاربری`, filtered to `IsBuiltIn == 0`
  - `ReminderDAL`: `ردیف`, `موضوع`, `توضیحات`, `تاریخ یادآوری`, `وضعیت یادآور`, `نام کاربری`, `RegDate` — seven columns, and the last one is **unaliased**, so the grid header literally reads `RegDate`. That is a pre-existing wart; preserve it, and note it as a follow-up rather than silently renaming a column users already see.

- [ ] **Step 1: Add failing tests**

Append to `CRMPeyvand.Tests/GridQueriesTests.cs`:

```csharp
        [Fact]
        public void User_Read_hides_the_built_in_administrator_group()
        {
            SqliteTestDb.WithDb(db =>
            {
                var builtIn = new BE.UserGroup { Title = "مدیریت", IsBuiltIn = true };
                var normal = new BE.UserGroup { Title = "کارشناس", IsBuiltIn = false };
                db.UserGroups.Add(builtIn);
                db.UserGroups.Add(normal);
                db.Users.Add(new BE.User
                {
                    Name = "مدیر", UserName = "admin", Password = "x",
                    RegDate = DateTime.Now, UserGroup = builtIn,
                });
                db.Users.Add(new BE.User
                {
                    Name = "کارمند", UserName = "staff", Password = "x",
                    RegDate = DateTime.Now, UserGroup = normal,
                });
                db.SaveChanges();

                var table = new UserDAL(db).Read();
                Assert.Single(table.Rows);
                Assert.Equal("کارمند", table.Rows[0]["نام"]);
                Assert.Equal("کارشناس", table.Rows[0]["گروه کاربری"]);
            });
        }

        [Fact]
        public void UserGroup_Read_hides_the_built_in_group()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.UserGroups.Add(new BE.UserGroup { Title = "مدیریت", IsBuiltIn = true });
                db.UserGroups.Add(new BE.UserGroup { Title = "کارشناس", IsBuiltIn = false });
                db.SaveChanges();

                var table = new UserGroupDAL(db).Read();
                Assert.Single(table.Rows);
                Assert.Equal("کارشناس", table.Rows[0]["نام گروه کاربری"]);
            });
        }

        [Fact]
        public void Reminder_Read_includes_the_owner_name()
        {
            SqliteTestDb.WithDb(db =>
            {
                var group = new BE.UserGroup { Title = "کارشناس" };
                db.UserGroups.Add(group);
                db.Users.Add(new BE.User
                {
                    Name = "کارمند", UserName = "staff", Password = "x",
                    RegDate = DateTime.Now, UserGroup = group,
                });
                db.Reminders.Add(new BE.Reminder
                {
                    Title = "تماس", Info = "یادآوری", RegDate = DateTime.Now,
                    RemindDate = DateTime.Now, User = db.Users.Local.First(),
                });
                db.SaveChanges();

                var table = new ReminderDAL(db).Read();
                Assert.Single(table.Rows);
                Assert.Equal("کارمند", table.Rows[0]["نام کاربری"]);
            });
        }
```

- [ ] **Step 2: Run to verify failure**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~GridQueriesTests`
Expected: compile errors for the missing `(DB)` constructors.

- [ ] **Step 3: Rewrite `UserDAL`**

Add the two-constructor pattern. Replace `Read()` and `Search(string)` (the SQL at `UserDAL.cs:64`):

```csharp
        private static readonly string[] ReadColumns =
            { "نام", "نام کاربری", "گروه کاربری", "تاریخ ثبت" };

        private IQueryable<object[]> UserRows()
        {
            return db.Users
                .Include("UserGroup")
                .Where(i => i.DeleteStatus == false)
                // The built-in administrator group is not a manageable group and
                // was excluded in the original query.
                .Where(i => i.UserGroup == null || i.UserGroup.IsBuiltIn == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .Select(i => new object[]
                {
                    i.Name,
                    i.UserName,
                    i.UserGroup == null ? null : i.UserGroup.Title,
                    i.RegDate,
                });
        }

        public DataTable Read() => GridTable.Build(ReadColumns, UserRows().ToList());

        public DataTable Search(string Filter)
        {
            var rows = UserRows()
                .AsEnumerable()
                .Where(r => GridTable.Matches(Filter, (string)r[0], (string)r[1], (string)r[2]))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }
```

- [ ] **Step 4: Rewrite `UserGroupDAL`**

```csharp
        private static readonly string[] ReadColumns = { "نام گروه کاربری" };

        public DataTable Read()
        {
            var rows = db.UserGroups
                .Where(i => i.IsBuiltIn == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .Select(i => new object[] { i.Title })
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }

        public DataTable Search(string Filter)
        {
            var rows = db.UserGroups
                .Where(i => i.IsBuiltIn == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .Select(i => new object[] { i.Title })
                .AsEnumerable()
                .Where(r => GridTable.Matches(Filter, (string)r[0]))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }
```

- [ ] **Step 5: Rewrite `ReminderDAL`**

The old SQL (at `ReminderDAL.cs:36`) selects **seven** columns and aliases none of the last one:

```
SELECT TOP (1000)  dbo.Reminders.id AS ردیف, dbo.Reminders.Title AS موضوع,
       dbo.Reminders.Info AS توضیحات, dbo.Reminders.RemindDate AS [تاریخ یادآوری],
       dbo.Reminders.IsReminded AS [وضعیت یادآور], dbo.Users.Name AS [نام کاربری],
       dbo.Reminders.RegDate
FROM dbo.Reminders INNER JOIN dbo.Users ON dbo.Reminders.User_id = dbo.Users.id
WHERE (dbo.Reminders.DeleteStatus = 0) ORDER BY dbo.Reminders.id DESC
```

```csharp
        // Copied verbatim from the SQL this replaces. The last column has no
        // alias in the original, so the grid header is the literal string
        // "RegDate". That is ugly but it is what employees see today, so it is
        // preserved; renaming it is a separate, deliberate change.
        private static readonly string[] ReadColumns =
            { "ردیف", "موضوع", "توضیحات", "تاریخ یادآوری", "وضعیت یادآور", "نام کاربری", "RegDate" };

        private IQueryable<object[]> ReminderRows()
        {
            return db.Reminders
                .Include("User")
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .Select(i => new object[]
                {
                    i.id,
                    i.Title,
                    i.Info,
                    i.RemindDate,
                    i.IsReminded,
                    i.User == null ? null : i.User.Name,
                    i.RegDate,
                });
        }

        public DataTable Read() => GridTable.Build(ReadColumns, ReminderRows().ToList());

        public DataTable Search(string Filter)
        {
            // The old query matched Title and Info only - not the user name.
            var rows = ReminderRows()
                .AsEnumerable()
                .Where(r => GridTable.Matches(Filter, (string)r[1], (string)r[2]))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }
```

**`Include("User")` is required** - the original reached the user's name through an INNER JOIN, and EF6 does not lazy load navigation properties.

- [ ] **Step 6: Run the tests**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: `Failed: 0, Passed: 86, Skipped: 0, Total: 86`.

- [ ] **Step 7: Commit**

```bash
git add DAL/UserDAL.cs DAL/UserGroupDAL.cs DAL/ReminderDAL.cs CRMPeyvand.Tests/GridQueriesTests.cs
git commit -m "refactor(dal): convert user, group and reminder grids to LINQ

Same treatment as the previous three: SELECT TOP and dbo. prefixes go, the
Persian headers are preserved, and search moves to GridTable.Matches.
Navigation properties are pulled with Include because EF6 does not lazy
load them, so without it the joins would come back null."
```

---

### Task 7: Catalog, discount codes and messages

**Files:**
- Modify: `DAL/CatalogItemDAL.cs`, `DAL/OffCodeDAL.cs`, `DAL/MessageDAL.cs`
- Modify: `CRMPeyvand.Tests/GridQueriesTests.cs`

**Interfaces:**
- Produces: same public signatures and same Persian columns:
  - `CatalogItemDAL`: `نام`, `قیمت`, `نوع`, `موجودی` - where `نوع` is the CASE on `Kind`: 1 = `محصول`, 2 = `خدمات`
  - `OffCodeDAL`: `کد تخفیف`, `مبلغ تخفیف`, `درصد تخفیف`, `محدودیت مصرف`, `تاریخ انقضا`, `تاریخ ثبت`
  - `MessageDAL`: `متن پیام`

- [ ] **Step 1: Add failing tests**

Append to `CRMPeyvand.Tests/GridQueriesTests.cs`:

```csharp
        [Fact]
        public void Catalog_Read_translates_the_Kind_code_to_a_persian_label()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.CatalogItems.Add(new BE.CatalogItem
                {
                    Name = "کالای اول", Kind = (int)BE.ItemKind.Good,
                    SalePrice = 100m, Stock = 5,
                });
                db.CatalogItems.Add(new BE.CatalogItem
                {
                    Name = "خدمت اول", Kind = (int)BE.ItemKind.Service,
                    SalePrice = 200m, Stock = 0,
                });
                db.SaveChanges();

                var table = new CatalogItemDAL(db).Read();
                var labels = table.Rows.Cast<DataRow>()
                                   .Select(r => (string)r["نوع"]).ToList();
                Assert.Contains("محصول", labels);
                Assert.Contains("خدمات", labels);
            });
        }

        [Fact]
        public void OffCode_Read_returns_all_discount_columns()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.OffCodes.Add(new BE.OffCode
                {
                    Code = "OFF10", Percent = 10, IsPrice = false,
                    RegDate = DateTime.Now, Price = 0m,
                });
                db.SaveChanges();

                var table = new OffCodeDAL(db).Read();
                Assert.Equal(
                    new[] { "کد تخفیف", "مبلغ تخفیف", "درصد تخفیف", "محدودیت مصرف", "تاریخ انقضا", "تاریخ ثبت" },
                    table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                Assert.Equal(10m, Convert.ToDecimal(table.Rows[0]["درصد تخفیف"]));
            });
        }

        [Fact]
        public void Message_Read_excludes_soft_deleted()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.Messages.Add(new BE.Message
                {
                    Content = "پیام زنده", RegDate = DateTime.Now,
                });
                db.Messages.Add(new BE.Message
                {
                    Content = "پیام حذف‌شده", RegDate = DateTime.Now, DeleteStatus = true,
                });
                db.SaveChanges();

                var table = new MessageDAL(db).Read();
                Assert.Single(table.Rows);
                Assert.Equal("پیام زنده", table.Rows[0]["متن پیام"]);
            });
        }
```

**If `BE.ItemKind` has different member names, read `BE/ItemKind.cs` and use the real ones.** Do not guess; the test must compile against the actual enum.

- [ ] **Step 2: Run to verify failure**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~GridQueriesTests`
Expected: compile errors for the missing `(DB)` constructors.

- [ ] **Step 3: Rewrite `CatalogItemDAL`**

The old SQL had `CASE Kind WHEN 1 THEN N'محصول' WHEN 2 THEN N'خدمات' END`. In LINQ that is a plain ternary. Add the two-constructor pattern and:

```csharp
        private static readonly string[] ReadColumns =
            { "نام", "قیمت", "نوع", "موجودی" };

        private static string KindLabel(int kind) =>
            kind == (int)BE.ItemKind.Good ? "محصول" : "خدمات";

        private IQueryable<object[]> CatalogRows()
        {
            return db.CatalogItems
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .Select(i => new object[]
                {
                    i.Name,
                    i.SalePrice,
                    KindLabel(i.Kind),
                    i.Stock,
                });
        }

        public DataTable Read() => GridTable.Build(ReadColumns, CatalogRows().ToList());

        public DataTable Search(string Filter)
        {
            // The old search matched on the *label* ("محصول"), not the raw
            // integer, so both the name and the label are checked here.
            var rows = CatalogRows()
                .AsEnumerable()
                .Where(r => GridTable.Matches(Filter, (string)r[0], (string)r[2]))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }
```

- [ ] **Step 4: Rewrite `OffCodeDAL`**

```csharp
        private static readonly string[] ReadColumns =
            { "کد تخفیف", "مبلغ تخفیف", "درصد تخفیف", "محدودیت مصرف", "تاریخ انقضا", "تاریخ ثبت" };

        private IQueryable<object[]> OffCodeRows()
        {
            return db.OffCodes
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .Select(i => new object[]
                {
                    i.Code,
                    i.Price,
                    i.Percent,
                    i.LimitCount,
                    i.ExpireDate,
                    i.RegDate,
                });
        }

        public DataTable Read() => GridTable.Build(ReadColumns, OffCodeRows().ToList());

        public DataTable Search(string Filter)
        {
            var rows = OffCodeRows()
                .AsEnumerable()
                .Where(r => GridTable.Matches(Filter, (string)r[0]))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }
```

- [ ] **Step 5: Rewrite `MessageDAL`**

```csharp
        private static readonly string[] ReadColumns = { "متن پیام" };

        private IQueryable<object[]> MessageRows()
        {
            return db.Messages
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .Select(i => new object[] { i.Content });
        }

        public DataTable Read() => GridTable.Build(ReadColumns, MessageRows().ToList());

        public DataTable Search(string Filter)
        {
            var rows = MessageRows()
                .AsEnumerable()
                .Where(r => GridTable.Matches(Filter, (string)r[0]))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }
```

Do **not** touch `MessageDAL.IsNotCheckedOut()`. It enumerates customers and their invoices in memory already, so it is provider-independent and out of scope.

- [ ] **Step 6: Run the tests**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: `Failed: 0, Passed: 89, Skipped: 0, Total: 89`.

- [ ] **Step 7: Commit**

```bash
git add DAL/CatalogItemDAL.cs DAL/OffCodeDAL.cs DAL/MessageDAL.cs CRMPeyvand.Tests/GridQueriesTests.cs
git commit -m "refactor(dal): convert catalog, discount and message grids to LINQ

The catalog query's CASE expression on Kind becomes a ternary that maps to
the same Persian labels, so searching for محصول still finds goods - the
old query matched on the label rather than the raw integer, and that
behaviour is preserved deliberately."
```

---

### Task 8: The invoice grid

The hardest one: the old query computed two columns per invoice with correlated subqueries (`تعداد کالاهای فاکتور`, `هزینه پرداختی`), used `ISNULL`, and searched with `CONVERT(nvarchar(max), i.id)`.

**Files:**
- Modify: `DAL/InvoiceDAL.cs`
- Modify: `CRMPeyvand.Tests/GridQueriesTests.cs`

**Interfaces:**
- Consumes: `GridTable`, `GridTable.Matches`
- Produces: `InvoiceDAL.Read()` / `.Search(string)` returning **seven** columns: `شماره فاکتور`, `وضعیت پرداخت`, `تاریخ پرداخت`, `کد تخفیف`, `تاریخ ثبت`, `تعداد کالاهای فاکتور`, `هزینه پرداختی`

  **There is no `قیمت کل` column and no customer or user name in this grid.** A `قیمت کل` alias exists only inside the dead stored procedure removed in Task 1, and the live query has no joins at all. Do not add columns.

- [ ] **Step 1: Add a failing test**

Append to `CRMPeyvand.Tests/GridQueriesTests.cs`:

```csharp
        [Fact]
        public void Invoice_Read_sums_line_quantities_per_invoice()
        {
            SqliteTestDb.WithDb(db =>
            {
                var item = new BE.CatalogItem
                {
                    Name = "کالا", Kind = (int)BE.ItemKind.Good,
                    SalePrice = 1000m, Stock = 10,
                };
                db.CatalogItems.Add(item);

                var invoice = new BE.Invoice
                {
                    RegDate = DateTime.Now, IsCheckedout = false, DiscountAmount = 0m,
                };
                invoice.Lines.Add(new BE.InvoiceLine { Quantity = 2, UnitPrice = 1000m, CatalogItem = item });
                invoice.Lines.Add(new BE.InvoiceLine { Quantity = 3, UnitPrice = 1000m, CatalogItem = item });
                db.Invoices.Add(invoice);
                db.SaveChanges();

                var table = new InvoiceDAL(db).Read();
                Assert.Single(table.Rows);
                Assert.Equal(5, Convert.ToInt32(table.Rows[0]["تعداد کالاهای فاکتور"]));
                Assert.Equal(5000m, Convert.ToDecimal(table.Rows[0]["هزینه پرداختی"]));
            });
        }

        [Fact]
        public void Invoice_Read_subtracts_the_discount_from_the_payable_total()
        {
            SqliteTestDb.WithDb(db =>
            {
                var item = new BE.CatalogItem
                {
                    Name = "کالا", Kind = (int)BE.ItemKind.Good,
                    SalePrice = 1000m, Stock = 10,
                };
                db.CatalogItems.Add(item);

                var invoice = new BE.Invoice
                {
                    RegDate = DateTime.Now, IsCheckedout = false, DiscountAmount = 750m,
                };
                invoice.Lines.Add(new BE.InvoiceLine { Quantity = 5, UnitPrice = 1000m, CatalogItem = item });
                db.Invoices.Add(invoice);
                db.SaveChanges();

                var table = new InvoiceDAL(db).Read();
                Assert.Equal(4250m, Convert.ToDecimal(table.Rows[0]["هزینه پرداختی"]));
            });
        }

        [Fact]
        public void Invoice_Read_shows_zero_not_null_for_an_invoice_with_no_lines()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.Invoices.Add(new BE.Invoice
                {
                    RegDate = DateTime.Now, IsCheckedout = false, DiscountAmount = 0m,
                });
                db.SaveChanges();

                var table = new InvoiceDAL(db).Read();
                Assert.Equal(0, Convert.ToInt32(table.Rows[0]["تعداد کالاهای فاکتور"]));
                Assert.Equal(0m, Convert.ToDecimal(table.Rows[0]["هزینه پرداختی"]));
            });
        }
```

`BE.Invoice` exposes `Lines` (initialised), `Customer`, `User` and `DiscountAmount`, and `BE.InvoiceLine.CatalogItem`; `LineTotal` is read-only and must never be assigned.

- [ ] **Step 2: Run to verify failure**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~GridQueriesTests`
Expected: compile errors for the missing `(DB)` constructor.

- [ ] **Step 3: Rewrite the invoice grid**

The old SQL (at `InvoiceDAL.cs:104`) is:

```
SELECT TOP (1000) id AS [شماره فاکتور], IsCheckedout AS [وضعیت پرداخت],
       CheckoutDate AS [تاریخ پرداخت], OffCode AS [کد تخفیف], RegDate AS [تاریخ ثبت],
       (SELECT ISNULL(SUM(l.Quantity),0) FROM dbo.InvoiceLines l WHERE l.InvoiceId = i.id)
           AS [تعداد کالاهای فاکتور],
       (SELECT ISNULL(SUM(l.Quantity*l.UnitPrice),0) FROM dbo.InvoiceLines l WHERE l.InvoiceId = i.id)
           - i.DiscountAmount AS [هزینه پرداختی]
FROM dbo.Invoices i WHERE (i.DeleteStatus = 0) ORDER BY i.id DESC
```

Note the column order: the two computed columns come **last**, not in the middle. Add the two-constructor pattern and:

```csharp
        // Copied verbatim from the SQL this replaces, including the fact that the
        // two computed columns come last. There is no "قیمت کل" here and no
        // customer or user column: the live query has no joins.
        private static readonly string[] ReadColumns =
        {
            "شماره فاکتور", "وضعیت پرداخت", "تاریخ پرداخت", "کد تخفیف", "تاریخ ثبت",
            "تعداد کالوهای فاکتور", "هزینه پرداختی",
        };

        private IQueryable<object[]> InvoiceRows()
        {
            return db.Invoices
                .Include("Lines")
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .Select(i => new object[]
                {
                    i.id,
                    i.IsCheckedout,
                    i.CheckoutDate,
                    i.OffCode,
                    i.RegDate,
                    // ISNULL(SUM(...), 0) becomes Sum, which returns 0 for an
                    // empty sequence rather than null.
                    i.Lines.Sum(l => l.Quantity),
                    // The old SQL cast through float. Keeping this in decimal
                    // means money totals no longer lose precision.
                    i.Lines.Sum(l => l.Quantity * l.UnitPrice) - i.DiscountAmount,
                });
        }

        public DataTable Read() => GridTable.Build(ReadColumns, InvoiceRows().ToList());

        public DataTable Search(string Filter)
        {
            // The old query matched CONVERT(nvarchar(max), i.id) only - the
            // invoice number, and nothing else. Keep that, so behaviour is
            // unchanged for anyone used to searching by number.
            var rows = InvoiceRows()
                .AsEnumerable()
                .Where(r => GridTable.Matches(Filter, Convert.ToString(r[0])))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }
```

**`Include("Lines")` is required** - the two computed columns come from the `InvoiceLines` table via correlated subqueries, and EF6 does not lazy load.

- [ ] **Step 4: Run the tests**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: `Failed: 0, Passed: 92, Skipped: 0, Total: 92`. If the totals tests fail, the arithmetic in step 3 does not match the old SQL - fix the query, not the tests.

- [ ] **Step 5: Manually verify the invoice grid**

Run the app, add a customer, a catalog item, then create an invoice with two lines. Open the Invoices window.
Expected: headers unchanged; the quantity column shows the sum of line quantities; the totals agree with the invoice detail window (`ReportsWindow`) for the same invoice.

- [ ] **Step 6: Commit**

```bash
git add DAL/InvoiceDAL.cs CRMPeyvand.Tests/GridQueriesTests.cs
git commit -m "refactor(dal): convert the invoice grid to LINQ

The old query computed two columns with correlated subqueries over
InvoiceLines and subtracted DiscountAmount. Those become Sum projections
over the included Lines collection.

decimal arithmetic is kept in decimal and no longer round-tripped through
CAST(... AS float), so money totals no longer lose precision. Empty
invoice line sets yield 0 via Sum's empty-sequence behaviour, which is
what ISNULL was doing before."
```

---

### Task 9: Dashboard queries

`DbFunctions.TruncateTime` and a `DATEADD`/`GETDATE` raw scalar are both unavailable on SQLite (verified: `no such function: TruncateTime`).

**Files:**
- Modify: `DAL/DashboardDAL.cs`
- Modify: `CRMPeyvand.Tests/GridQueriesTests.cs`

**Interfaces:**
- Produces: unchanged signatures. `SellsCountToday()`, `SellsCountWeek()`, `UserReminderCount(User)`, `GetUserReminder(User)`.

- [ ] **Step 1: Add a failing test**

Append to `CRMPeyvand.Tests/GridQueriesTests.cs`:

```csharp
        [Fact]
        public void Dashboard_counts_invoices_registered_today()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.Invoices.Add(new BE.Invoice
                {
                    RegDate = DateTime.Today.AddHours(9), IsCheckedout = false, DiscountAmount = 0m,
                });
                db.Invoices.Add(new BE.Invoice
                {
                    RegDate = DateTime.Today.AddDays(-3), IsCheckedout = false, DiscountAmount = 0m,
                });
                db.SaveChanges();

                var dal = new DashboardDAL(db);
                Assert.Equal("1", dal.SellsCountToday());
                Assert.Equal("2", dal.SellsCountWeek());
            });
        }

        [Fact]
        public void Dashboard_count_today_is_not_confused_by_the_time_of_day()
        {
            // DbFunctions.TruncateTime is unavailable on SQLite, so a range
            // predicate must replace it. Late-evening rows must still count.
            SqliteTestDb.WithDb(db =>
            {
                db.Invoices.Add(new BE.Invoice
                {
                    RegDate = DateTime.Today.AddHours(23).AddMinutes(59),
                    IsCheckedout = false, DiscountAmount = 0m,
                });
                db.SaveChanges();

                Assert.Equal("1", new DashboardDAL(db).SellsCountToday());
            });
        }
```

Add the `(DB db)` constructor to `DashboardDAL` alongside the existing parameterless one.

- [ ] **Step 2: Run to verify failure**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~GridQueriesTests`
Expected: compile error for the missing `(DB)` constructor.

- [ ] **Step 3: Replace `TruncateTime` with a range predicate**

In `SellsCountToday()` and `UserReminderCount()` replace
`System.Data.Entity.DbFunctions.TruncateTime(x) == today` with a half-open range.
Add this helper to the class:

```csharp
        /// <summary>
        /// True when <paramref name="value"/> falls on the same calendar day.
        ///
        /// Replaces DbFunctions.TruncateTime, which compiles to a SQL Server
        /// TRUNCATE and fails on SQLite with "no such function: TruncateTime".
        /// A half-open range is also index-friendly, where TruncateTime was not.
        /// </summary>
        private static bool IsSameDay(DateTime value, DateTime day) =>
            value >= day.Date && value < day.Date.AddDays(1);
```

Then rewrite the three call sites:

```csharp
        public string SellsCountToday()
        {
            try
            {
                using (var db = new DB())
                {
                    var today = DateTime.Today;
                    return db.Invoices.Count(i =>
                        i.DeleteStatus == false && IsSameDay(i.RegDate, today)).ToString();
                }
            }
            catch { return "0"; }
        }
```

and, in `UserReminderCount`, `... && IsSameDay(i.RemindDate, today) ...`, and in `GetUserReminder` the same.

**But** the tests need the injected `db`. Give the class two constructors and make the methods use the field, matching the other DAL classes:

```csharp
        DB db;

        public DashboardDAL()
        {
            db = new DB();
        }

        public DashboardDAL(DB db)
        {
            this.db = db;
        }
```

and drop the `using (var db = new DB())` blocks so the injected instance is used.

- [ ] **Step 4: Replace the `DATEADD` scalar**

`SellsCountWeek()` opened a `SqlCommand` with
`RegDate BETWEEN DATEADD(WEEK, -1, GETDATE()) AND GETDATE()`. Replace the whole method:

```csharp
        public string SellsCountWeek()
        {
            try
            {
                var now = DateTime.Now;
                return db.Invoices.Count(i =>
                    i.DeleteStatus == false &&
                    i.RegDate >= now.AddDays(-7) &&
                    i.RegDate <= now).ToString();
            }
            catch
            {
                return "0";
            }
        }
```

`AddDays(-7)` matches `DATEADD(WEEK, -1, GETDATE())`, and the closed upper bound matches the original `BETWEEN`. No `Microsoft.Data.SqlClient` usage remains in this file; remove that `using`.

- [ ] **Step 5: Run the tests**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: `Failed: 0, Passed: 94, Skipped: 0, Total: 94`.

- [ ] **Step 6: Commit**

```bash
git add DAL/DashboardDAL.cs CRMPeyvand.Tests/GridQueriesTests.cs
git commit -m "fix(dal): make the dashboard counts work on both providers

DbFunctions.TruncateTime has no SQLite implementation (verified: 'no
function: TruncateTime'), so same-day checks become a half-open date range
- which is also index-friendly, unlike wrapping the column in TRUNCATE.

SellsCountWeek used DATEADD(WEEK,-1,GETDATE()) in a raw SqlCommand; it is
now a LINQ range over the injected context. This removes the last
Microsoft.Data.SqlClient dependency from the read path."
```

---

### Task 10: Backup on SQLite

`BACKUP DATABASE` has no SQLite equivalent. On SQLite the database is a single file, so a copy is a complete backup - but it must happen with no open connection writing, so use the SQLite online backup API rather than `File.Copy`.

**Files:**
- Modify: `DAL/SettingDAL.cs`
- Modify: `CRMPeyvand.Tests/GridQueriesTests.cs`

**Interfaces:**
- Consumes: `DataSource.Current.Kind`
- Produces: `SettingDAL.BackUp(string path)` - unchanged signature, now branches on provider.

- [ ] **Step 1: Add a failing test**

Append to `CRMPeyvand.Tests/GridQueriesTests.cs`:

```csharp
        [Fact]
        public void BackUp_on_sqlite_produces_a_restorable_file()
        {
            var dbPath = Path.Combine(Path.GetTempPath(),
                                      "bk-" + Guid.NewGuid().ToString("N") + ".db");
            var backupPath = Path.Combine(Path.GetTempPath(),
                                          "bk-" + Guid.NewGuid().ToString("N") + ".bak");
            DataSource.UseForTests(new DataSource
            {
                Kind = DbProviderKind.Sqlite,
                ConnectionString =
                    "Data Source=" + dbPath + ";providerName=" + SqliteSchema.Invariant,
            });

            try
            {
                using (var db = new DAL.DB())
                {
                    db.Customers.Add(new BE.Customer
                    {
                        Name = "قبل از پشتیبان", Phone = "09120000077", RegDate = DateTime.Now,
                    });
                    db.SaveChanges();
                }

                var message = new SettingDAL().BackUp(backupPath);
                Assert.Null(message);
                Assert.True(File.Exists(backupPath));

                // The copy must be a real database with the row in it.
                using (var connection = new System.Data.SQLite.SQLiteConnection(
                           "Data Source=" + backupPath))
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM Customers";
                        Assert.Equal(1L, Convert.ToInt64(cmd.ExecuteScalar()));
                    }
                }
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
                if (File.Exists(backupPath)) File.Delete(backupPath);
            }
        }
```

- [ ] **Step 2: Run to verify failure**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~GridQueriesTests`
Expected: FAIL - `BACKUP DATABASE` against a SQLite file will not produce a `.bak`.

- [ ] **Step 3: Branch `BackUp` on the provider**

Replace `SettingDAL.BackUp` and give the class a testable shape:

```csharp
        DB db;

        public SettingDAL()
        {
            db = new DB();
        }

        public SettingDAL(DB db)
        {
            this.db = db;
        }

        /// <summary>
        /// Returns null on success, or a Persian sentence describing the failure.
        /// </summary>
        public string BackUp(string Path)
        {
            try
            {
                if (DataSource.Current.Kind == DbProviderKind.Sqlite)
                    return BackUpSqlite(Path);

                return BackUpSqlServer(Path);
            }
            catch (Exception e)
            {
                return "ذخیره پشتیبان با مشکلی مواجه شد:\n" + e.Message;
            }
        }

        private string BackUpSqlServer(string path)
        {
            using (var connection = new SqlConnection(DB.ConnectionString))
            using (var command = new SqlCommand())
            {
                // BACKUP DATABASE cannot be parameterized for the db name; it is
                // taken from the live connection, not user input. The path stays a
                // parameter.
                command.CommandText =
                    "BACKUP DATABASE [" + connection.Database + "] TO DISK = @path WITH INIT";
                command.Parameters.AddWithValue("@path", path);
                command.Connection = connection;
                connection.Open();
                command.ExecuteNonQuery();
            }

            return "ذخیره فایل با موفقیت انجام شد لطفا پوشه مورد نظر را بررسی کنید";
        }

        private string BackUpSqlite(string path)
        {
            // SQLite has no BACKUP DATABASE, and a plain File.Copy can capture a
            // torn file if anything is mid-write. The online backup API writes a
            // consistent snapshot while the database stays open.
            var source = new SQLiteConnectionStringBuilder(
                DataSource.Current.ConnectionString.Split(
                    new[] { ";providerName=" }, StringSplitOptions.None)[0])
                .DataSource;

            if (!File.Exists(source))
                return "فایل پایگاه داده یافت نشد";

            using (var sourceConnection = new SQLiteConnection(DataSource.Current.ConnectionString))
            {
                sourceConnection.Open();
                using (var destination = new SQLiteConnection(
                           new SQLiteConnectionStringBuilder { DataSource = path }.ToString()))
                {
                    destination.Open();
                    sourceConnection.BackupDatabase(destination);
                }
            }

            return "ذخیره فایل با موفقیت انجام شد لطفا پوشه مورد نظر را بررسی کنید";
        }
```

Add `using System.Data.SQLite;` to the file. `SQLiteConnection.BackupDatabase(SQLiteConnection)` is a synchronous wrapper over the online backup API.

- [ ] **Step 4: Run the tests**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: `Failed: 0, Passed: 95, Skipped: 0, Total: 95`.

- [ ] **Step 5: Commit**

```bash
git add DAL/SettingDAL.cs CRMPeyvand.Tests/GridQueriesTests.cs
git commit -m "feat(dal): back up the SQLite database by snapshot, not BACKUP DATABASE

SQLite has no BACKUP DATABASE. A plain File.Copy can capture a torn file if
a write is in flight, so this uses SQLite's online backup API, which
snapshots consistently with the database left open.

BackUp's signature and its Persian success/failure messages are unchanged,
so DataBaseForm needs no edit for the backup button."
```

---

### Task 11: Connection settings screen

Give the user the form they asked for: pick the provider, fill in server/user/password/database, test the connection, save.

**Files:**
- Modify: `CRMPeyvand/DataBaseForm.xaml`
- Modify: `CRMPeyvand/DataBaseForm.xaml.cs`

**Interfaces:**
- Consumes: `DataSource.Test`, `DataSource.Use`, `DataSource.DefaultSqlite`, `DataSource.DefaultSqlServer`, `DataSource.Current`, `DB.ConnectionString`
- Produces: a working "پیکربندی پایگاه داده" window. This window currently exists but is **never instantiated** (no `new DataBaseForm` anywhere) and has two empty handlers.

- [ ] **Step 1: Confirm the window is currently unreachable**

Run:
```powershell
Select-String -Path "CRMPeyvand\*.xaml.cs" -Pattern "new DataBaseForm"
```
Expected: no output. The window is dead code, so this task makes it reachable from Settings.

- [ ] **Step 2: Add the XAML**

Open `CRMPeyvand/DataBaseForm.xaml` and add, before the closing `</Window>`, a section that offers the two providers and the SQL Server fields. Match the styling already used in `Setting.xaml` (`ButtonBorder` / `Button` styles) rather than inventing new ones. The controls needed are:

- `ComboBox x:Name="cmbProvider"` with two `ComboBoxItem`s: `SQLite (پیش‌فرض)` and `SQL Server`
- Text boxes `txtServer`, `txtDatabase`, `txtUser`, `txtPassword` (`PasswordBox`)
- A `CheckBox x:Name="chkWindowsAuth"` labelled `استفاده از حساب ویندوز`
- Buttons: `btnTest` (`آزمایش اتصال`), `btnSave` (`ذخیره`), plus the existing `btnSMSActivation_Click` (backup) and the existing back-navigation handler

- [ ] **Step 3: Wire the code-behind**

In `CRMPeyvand/DataBaseForm.xaml.cs`, add a load method and the three handlers. Everything user-facing is Persian:

```csharp
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var current = DataSource.Current;
            if (current.Kind == DbProviderKind.SqlServer)
            {
                cmbProvider.SelectedIndex = 1;
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(
                    current.ConnectionString);
                txtServer.Text = builder.DataSource;
                txtDatabase.Text = builder.InitialCatalog;
                txtUser.Text = builder.UserID;
                chkWindowsAuth.IsChecked = builder.IntegratedSecurity;
            }
            else
            {
                cmbProvider.SelectedIndex = 0;
            }

            ProviderChanged();
        }

        private void ProviderChanged()
        {
            bool sqlServer = cmbProvider.SelectedIndex == 1;
            txtServer.IsEnabled = sqlServer;
            txtDatabase.IsEnabled = sqlServer;
            txtUser.IsEnabled = sqlServer && chkWindowsAuth.IsChecked == false;
            txtPassword.IsEnabled = sqlServer && chkWindowsAuth.IsChecked == false;
        }

        private DataSource BuildFromForm()
        {
            if (cmbProvider.SelectedIndex == 0)
                return DataSource.DefaultSqlite();

            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
            {
                DataSource = txtServer.Text,
                InitialCatalog = txtDatabase.Text,
                IntegratedSecurity = chkWindowsAuth.IsChecked == true,
            };
            if (builder.IntegratedSecurity == false)
            {
                builder.UserID = txtUser.Text;
                builder.Password = txtPassword.Text;
            }
            builder.TrustServerCertificate = true;
            builder.MultipleActiveResultSets = true;

            return new DataSource
            {
                Kind = DbProviderKind.SqlServer,
                ConnectionString = builder.ToString(),
            };
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            var message = DataSource.Test(BuildFromForm());
            System.Windows.MessageBox.Show(
                message ?? "اتصال با موفقیت برقرار شد",
                "اطلاعیه", MessageBoxButton.OK,
                message == null ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var candidate = BuildFromForm();
            var message = DataSource.Test(candidate);
            if (message != null)
            {
                System.Windows.MessageBox.Show(
                    message, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DataSource.Use(candidate);
            System.Windows.MessageBox.Show(
                "پیکربندی پایگاه داده ذخیره شد لطفا برنامه را دوباره اجرا کنید",
                "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
        }
```

Add event wiring in the constructor: `Loaded += Window_Loaded;` and the button `Click` handlers. The password field is deliberately not persisted in cleartext beyond the connection string the user typed; if that is unacceptable, store the password in Windows Credential Manager instead and say so - do not silently write it to `provider.json`.

- [ ] **Step 4: Open the window from Settings**

In `CRMPeyvand/Setting.xaml.cs`, add a button handler that opens it:

```csharp
        private void btnDatabase_Click(object sender, RoutedEventArgs e)
        {
            var form = new DataBaseForm();
            form.Owner = this;
            form.ShowDialog();
        }
```

Add the matching `Button` to `Setting.xaml` following the existing row pattern (`<Border Style="{StaticResource ButtonBorder}"><Button Style="{StaticResource Button}" Click="btnDatabase_Click" Content="پیکربندی پایگاه داده"/></Border>`), placed inside the existing `AccessGuard.Can(u, Section.Settings, ...)` gate if one is present there.

- [ ] **Step 5: Verify by hand**

Run `dotnet run --project CRMPeyvand/CRMPeyvand.csproj -c Release`. Open Settings, then the database configuration.
Expected: defaults to SQLite; choosing SQL Server enables the server fields; **آزمایش اتصال** against a real instance succeeds; a wrong password reports a Persian error and does not save; **ذخیره** persists and, after restarting, the app uses SQL Server.

- [ ] **Step 6: Commit**

```bash
git add CRMPeyvand/DataBaseForm.xaml CRMPeyvand/DataBaseForm.xaml.cs CRMPeyvand/Setting.xaml.cs
git commit -m "feat(ui): add a database configuration screen

DataBaseForm already existed with a wired backup button and two empty
handlers, but nothing ever instantiated it. It now hosts the provider
choice, the SQL Server connection fields, a connection test and a save.

Saving writes DataSource, which is why the message tells the user to
restart: DataSource is resolved once per process behind a lock so that
EF6's non-thread-safe initialiser cannot race."
```

---

### Task 12: Un-gate the installer

SQL Server is no longer required, so the MSI must stop refusing to install without it.

**Files:**
- Modify: `installer/Package.wxs`
- Modify: `README.md`

**Interfaces:**
- Consumes: nothing
- Produces: an MSI that installs with no database prerequisites.

- [ ] **Step 1: Remove the SQL Server launch condition**

In `installer/Package.wxs`, delete the `SQLINSTALLED64` / `SQLINSTALLED32` properties and the `Launch` condition that references them, and delete the long comment block explaining the `Instance Names\SQL` investigation. Keep the `HASDESKTOPRUNTIME` search and its `Launch` condition: the .NET Desktop Runtime is still genuinely required, and that check is reliable.

Also update the header comment block, which currently states "A local SQL Server instance is a hard prerequisite".

- [ ] **Step 2: Rebuild the installer and check the condition is gone**

```powershell
.\build-installer.ps1 -Version 1.1.0
```
Then confirm with a COM query that `SQLINSTALLED` appears nowhere and `HASDESKTOPRUNTIME` still does:
```powershell
$inst = New-Object -ComObject WindowsInstaller.Installer
$db = $inst.GetType().InvokeMember("OpenDatabase","InvokeMethod",$null,$inst,
      @("E:\Developing\Projects\CRMPeyvand\artifacts\CRMPeyvand.msi",0))
$v = $db.GetType().InvokeMember("OpenView","InvokeMethod",$null,$db,
      @("SELECT Property,Value FROM Property WHERE Property LIKE '%SQL%' OR Property LIKE '%RUNTIME%'"))
$v.GetType().InvokeMember("Execute","InvokeMethod",$null,$v,$null)
while ($true) {
  $r = $v.GetType().InvokeMember("Fetch","InvokeMethod",$null,$v,$null)
  if (-not $r) { break }
  $r.GetType().InvokeMember("StringData","GetProperty",$null,$r,@(1)) + " = " +
  $r.GetType().InvokeMember("StringData","GetProperty",$null,$r,@(2))
}
```
Expected: only `HASDESKTOPRUNTIME`.

- [ ] **Step 3: Verify `e_sqlite3.dll` is inside the MSI**

```powershell
msiexec /a artifacts\CRMPeyvand.msi /qn TARGETDIR="$env:TEMP\msicheck"
Get-ChildItem "$env:TEMP\msicheck" -Recurse -Filter "e_sqlite3.dll"
```
Expected: exactly one. **This is the single most important check in this task.** If the native library is not in the package, every install produces an app that cannot open a database, and the failure appears only at runtime on the user's machine.

- [ ] **Step 4: Update the README**

In the "Building the MSI Installer" section, change the prerequisites table to list only the .NET Desktop Runtime, and add a short "Choosing a database" note: SQLite is the default and needs nothing installed; SQL Server is configured in Settings → پیکربندی پایگاه داده; the SQLite file lives beside `provider.json` in `%ProgramData%\CRMPeyvand` (or `%LocalAppData%\CRMPeyvand` if that folder is not writable).

- [ ] **Step 5: Commit**

```bash
git add installer/Package.wxs README.md
git commit -m "feat(installer): drop the SQL Server prerequisite

SQLite is the default and ships its own native library, so refusing to
install without a local SQL Server instance is now wrong. The .NET Desktop
Runtime check stays: it is still required, and unlike the SQL Server check
it is reliable.

The change also removes a launch condition that never worked - it probed
HKLM InstanceNames, which MSI cannot read - and would have blocked a
correctly configured machine."
```

---

### Task 13: Final verification

Nothing new is built here. This task exists because the data layer had **zero** test coverage before this plan: all 66 original tests build `BE` objects in memory and never touch a database, so they would pass identically with the DAL deleted. They are a guard against breaking the pure logic, not against breaking data access.

**Files:**
- Modify: `README.md` only if something is found to be untrue.

- [ ] **Step 1: Full clean build**

```powershell
Remove-Item -Recurse -Force artifacts, installer/obj
dotnet build CRMPeyvand.sln -c Release --nologo -v q
```
Expected: `Build succeeded.` 0 errors.

- [ ] **Step 2: Full test suite**

```powershell
dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q
```
Expected: `Failed: 0`, total 94 or more.

- [ ] **Step 3: Confirm no raw SQL remains on the read path**

```powershell
Select-String -Path "DAL\*.cs" -Pattern "new SqlConnection\(|new SqlDataAdapter\(|new SqlCommand\(|SELECT TOP"
```
Expected: matches **only** in `DAL/SettingDAL.cs` (the `BACKUP DATABASE` branch, which is SQL Server only by design). Everything else must be clean.

- [ ] **Step 4: Confirm no leftover dead locals**

```powershell
Select-String -Path "DAL\*.cs" -Pattern "SqlCommandBuilder"
```
Expected: no output. Nine unused `SqlCommandBuilder` locals existed before this plan.

- [ ] **Step 5: Manual smoke test on SQLite**

Run `dotnet run --project CRMPeyvand/CRMPeyvand.csproj -c Release` and exercise, on a machine with **no** SQL Server involvement in the app:
1. First run creates a user
2. Add a customer, a catalog item, an activity, an activity category, a reminder
3. Create an invoice with two lines; confirm the invoice grid totals and the invoice PDF agree
4. Search each list by Persian text and by a number
5. Add a discount code and apply it
6. Run the backup button; confirm a restorable file appears
7. Restart and confirm the data is still there

- [ ] **Step 6: Manual smoke test on SQL Server**

Open Settings → پیکربندی پایگاه داده, point it at the existing instance and `CRMPeyvand` database, test, save, restart.
Expected: existing data is visible and unchanged. **The 14 table names and column names must match exactly or the app will report missing tables** - this is the highest-risk regression in the whole plan, because a rename is silent until runtime.

- [ ] **Step 7: Confirm the installer still gates on .NET only**

```powershell
.\build-installer.ps1 -Version 1.1.0
```
Install on a clean machine. Expected: no database-related refusal.

- [ ] **Step 8: Commit only if something was corrected**

```bash
git commit -am "docs: correct installer notes after SQLite default"
```

---

## Out of scope, deliberately

- **Persian installer UI.** The WixUI wizard is English. Adding an `fa-IR` `.wxl` is a separate, self-contained change.
- **Code signing.** Still unconfigured; the MSI is unsigned and SmartScreen will warn.
- **Rewriting the 24 string-based `.Include("Lines.CatalogItem")` calls to strongly-typed lambdas.** EF6 does not validate these paths at compile time, so a renamed navigation property fails only at runtime. Worth doing, but it is a refactor with no behaviour change and does not block this feature.
- **Making `%ProgramData%\CRMPeyvand` writable by all users.** Needs an ACL the MSI cannot set without `WixToolset.Util`, which does not load in this setup. The per-user fallback in `DataFolder` covers it.
- **Fixing the `RememberMes` pluralisation.** Renaming it would orphan existing SQL Server rows. Correct long-term, but it must be a migration, not a quiet change.
