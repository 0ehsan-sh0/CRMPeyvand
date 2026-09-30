# SQLite by default, SQL Server on request

CRMPeyvand supports two database providers behind one DAL and one set of entities: SQLite as the zero-configuration default, Microsoft SQL Server as a user-selectable alternative.

## Context & Problem

The application shipped against SQL Server alone. That made the installer refuse to run on any machine without a database server, and it made a fresh clone unusable until a DBA had provisioned one. The entities themselves were not SQL Server specific — nothing in the domain model needed `dbo` schemas, `datetime` precision, or SQL Server collation — so the coupling was in the plumbing rather than the model.

Making SQLite the default introduced three problems that needed solving before the mode switch could be trustworthy.

## Decision Summary

1. **SQLite is the default; SQL Server is opt-in.** No server is required to install, run, or clone-and-run. The choice is persisted in `provider.json` beside the database.
2. **The choice is reversible without retyping credentials.** `SqlServerConnectionString` is retained on the `DataSource` even while SQLite is active, so the settings screen's on/off switch does not force the user to re-enter a server, login, and password. Activating SQL Server with nothing saved is refused rather than producing a broken connection.
3. **Provider registration happens in code, not `App.config`.** On .NET 10 the `system.data` configuration section is not recognised because `System.Data.Common` is a separate assembly, and even once declared the factory cannot be resolved to a provider invariant. `DB`'s static constructor registers the SQLite factory and provider services instead, and deliberately does not displace the `App.config` entry so SQL Server still resolves.
4. **The SQL Server connection is a `System.Data.SqlClient` one.** See the consequence below for why.
5. **The SQLite schema is created by the app, the SQL Server schema by EF6.** `SqliteSchema.Ensure` runs on the way through to a SQLite connection; `MigrateDatabaseToLatestVersion` runs for SQL Server. EF6's SQLite provider cannot generate tables and the migration hardcodes `dbo.` table names, so the migration is installed only on the SQL Server path.
6. **The connection test exercises EF6, not raw ADO.NET.** A probe that only opens the connection will pass a configuration EF6 cannot use.

## Considered Options

- **SQL Server only (the status quo)**: rejected — a database server became a hard prerequisite for installing and first-run.
- **SQLite only**: rejected — the existing deployments have real data in SQL Server and need a path to keep serving it.
- **Two entirely separate data-access code paths**: rejected — the providers are interchangeable behind ADO.NET, and duplicating every query would guarantee the two drift.
- **Tiered / EF Core with provider-specific migrations**: not applicable — this is EF6 on .NET 10, where the SQLite provider cannot generate schema.

## Consequences

- **The two providers are not interchangeable in one detail, and getting it wrong is silent.** EF6's SQL Server provider services hard-cast to `System.Data.SqlClient.SqlConnection`. Handing them a `Microsoft.Data.SqlClient` connection fails every query with `NotSupportedException: Unable to determine the provider name for provider factory of type 'Microsoft.Data.SqlClient.SqlClientFactory'`, and registering that factory's invariant only moves the failure to an `InvalidCastException`. `DataSource.CreateConnection` therefore returns `System.Data.SqlClient.SqlConnection`, which arrives with `EntityFramework` — the same assembly those provider services ship in — so no new dependency is needed. `Microsoft.Data.SqlClient` remains a dependency for its own connection-string and command types.
- **The two connections are returned in different states, deliberately.** SQLite comes back **open**, because that is the point at which a fresh install gets its tables; a connection handed back closed would let the first query arrive before the schema exists. SQL Server comes back **closed**, for EF6 to open and run its migration on.
- **The connection string is stripped of its `providerName=` tail before use.** That token is how EF6 finds the provider; it is not an ADO.NET keyword, and both connection types refuse a connection string that still carries it.
- **The provider is resolved once per process behind a lock.** The EF6 initialiser runs inside `DbContext` construction, `MigrateDatabaseToLatestVersion` is not thread-safe, and the DAL classes construct a `DB` in field initialisers.
- **A corrupt or unreadable `provider.json` must not stop the app starting.** `DataSource.Load` catches and falls through to the SQLite default, so a bad settings file degrades to a working default rather than a failed launch.
- **An older settings file is migrated on load.** A file written before `SqlServerConnectionString` existed records an active SQL Server connection and nothing else; the saved connection is copied across, or the on/off switch could never be thrown back for an install that had already been configured.
- **The connection test had to change to be worth anything.** It previously opened the connection with raw ADO.NET and so reported a broken SQL Server setting as working — which is exactly how the provider-mismatch defect above reached a release. The probe now runs a statement through EF6 on a context that maps nothing and has a null initialiser, so it cannot create or migrate a database in order to find out.
- **A shared-machine caveat remains.** `CRMPeyvand.db` sits in `%ProgramData%\CRMPeyvand` so all employees share one database. A user who cannot write to that folder falls back to `%LocalAppData%\CRMPeyvand` and silently gets their own database.
