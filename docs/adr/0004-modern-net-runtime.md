# Migrate to .NET 10 modern runtime and SDK-style project system

The solution leaves .NET Framework 4.7.2 for .NET 10 (`net10.0` for class libraries/tests and `net10.0-windows` for WPF/WinForms). Driver: maintainability, performance, security, and tech modernization.

## Decision Summary

1. **Target Runtime & SDK**: All 5 projects (`BE`, `BLL`, `DAL`, `CRMPeyvand`, `CRMPeyvand.Tests`) are migrated to modern SDK-style `.csproj` format targeting .NET 10 (`net10.0` / `net10.0-windows`).
2. **Data Access Layer**: Entity Framework is upgraded to `EntityFramework 6.5.1` (which officially supports .NET 6/8/9/10) along with `Microsoft.Data.SqlClient`. This preserves all existing EF6 Code-First migrations, `DbSet` entity configurations, and ADO.NET data access patterns with 100% behavioral fidelity while operating entirely on modern .NET 10.
3. **Stimulsoft Reporting Engine**: Stimulsoft Reports assemblies are updated to the .NET Core / .NET Standard 2.1 builds (`Stimulsoft.Base.dll`, `Stimulsoft.Report.dll`, `Stimulsoft.Report.Win.dll`), with template (`.mrt`) file resolution modernized to use `AppContext.BaseDirectory` and configured with `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>`.
4. **UI & Interop**: Enable `<UseWPF>true</UseWPF>` and `<UseWindowsForms>true</UseWindowsForms>` in presentation projects. Utilize `WinForms.DataVisualization` for WinForms charting, alongside modern `HandyControls` and `LiveCharts.Wpf`.

## Considered Options

- Staying on .NET Framework 4.7.2: rejected — no further runtime investment from Microsoft; blocks modern C# features and performance improvements.
- Full EF Core 10 rewrite in this phase: rejected — would require simultaneously rewriting all migrations, database initialization, and ADO.NET data table queries, violating the principle of isolating platform migrations from domain/data logic rewrites.
- Migrating to .NET 8 LTS only: evaluated as a viable alternative; .NET 10 was selected as the environment has SDK 10.0.400 and .NET 10 runtimes already installed and package ecosystem compatibility for all dependencies is verified.

