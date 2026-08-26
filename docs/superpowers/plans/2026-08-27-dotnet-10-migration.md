# .NET 10 Modernization & SDK-Style Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Migrate the entire CRMPeyvand solution from legacy .NET Framework 4.7.2 and old MSBuild format to .NET 10 (`net10.0` and `net10.0-windows`) using SDK-style projects, Entity Framework 6.5.1, Stimulsoft .NET Core/Standard assemblies, and modern packages while keeping all 24 unit tests passing and preserving all Persian UI and reporting behaviors.

**Architecture:** Convert each layer (`BE` -> `DAL` -> `BLL` -> `CRMPeyvand.Tests` -> `CRMPeyvand`) sequentially to SDK-style project format. Modernize data access to `Microsoft.Data.SqlClient` with `EntityFramework 6.5.1` (ensuring 100% Code-First migration and query compatibility), upgrade report loading paths to use `AppContext.BaseDirectory`, and enable dual WPF/WinForms support in presentation.

**Tech Stack:** .NET 10 (`net10.0`, `net10.0-windows`), C# 13+, Entity Framework 6.5.1, Microsoft.Data.SqlClient, WPF, Windows Forms, HandyControls, LiveCharts.Wpf, WinForms.DataVisualization, Stimulsoft Reports (.NET Core/Standard 2.1), xUnit.

**Spec:** [docs/adr/0004-modern-net-runtime.md](../../adr/0004-modern-net-runtime.md)

## Global Constraints

- Must target .NET 10 (`net10.0` for libraries/tests, `net10.0-windows` for WPF/WinForms application).
- Must convert all `.csproj` files to SDK-style `<Project Sdk="Microsoft.NET.Sdk">` and eliminate `packages.config` files.
- Must preserve all domain entity definitions, relationships, and property configurations in `BE`.
- Must preserve all 24 xUnit unit tests in `CRMPeyvand.Tests` and ensure they pass on `net10.0`.
- Must preserve Stimulsoft report generation for all 10 `.mrt` report templates without schema breakage.
- Must preserve Persian calendar formatting and RTL layout.

---

### Task 1: Migrate `BE` (Business Entities) to SDK-Style `net10.0`

**Files:**
- Modify: `BE/BE.csproj`
- Delete: `BE/Properties/AssemblyInfo.cs`

**Interfaces:**
- Consumes: Standard .NET 10 primitives and `System.ComponentModel.DataAnnotations`.
- Produces: `BE` domain classes (`Customer`, `Invoice`, `InvoiceLine`, `CatalogItem`, `User`, `UserGroup`, `AccessGrant`, `Activity`, `ActivityCategory`, `Reminder`, `OffCode`, `Message`, `MessagePanel`, `RememberMe`).

- [ ] **Step 1: Convert `BE/BE.csproj` to SDK-style format**

Replace `BE/BE.csproj` with:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>disable</Nullable>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>
</Project>
```

- [ ] **Step 2: Remove legacy `BE/Properties/AssemblyInfo.cs`**

Delete `BE/Properties/AssemblyInfo.cs` to prevent duplicate assembly attribute compilation errors in SDK-style projects.

- [ ] **Step 3: Build `BE` project**

Run: `dotnet build BE/BE.csproj`
Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Step 4: Commit `BE` migration**

```bash
git add BE/
git commit -m "refactor(be): migrate BE project to SDK-style net10.0"
```

---

### Task 2: Migrate `DAL` (Data Access Layer) to SDK-Style `net10.0` with EF 6.5.1

**Files:**
- Modify: `DAL/DAL.csproj`
- Modify: `DAL/CustomerDAL.cs`
- Modify: `DAL/InvoiceDAL.cs`
- Modify: `DAL/CatalogItemDAL.cs`
- Modify: `DAL/SettingDAL.cs`
- Modify: `DAL/DB.cs`
- Delete: `DAL/packages.config`
- Delete: `DAL/Properties/AssemblyInfo.cs`

**Interfaces:**
- Consumes: `BE`, `EntityFramework 6.5.1`, `Microsoft.Data.SqlClient`, `System.Configuration.ConfigurationManager`.
- Produces: `DAL` classes (`DB`, `CustomerDAL`, `InvoiceDAL`, `CatalogItemDAL`, `UserDAL`, `UserGroupDAL`, `ActivityDAL`, `ActivityCategoryDAL`, `ReminderDAL`, `OffCodeDAL`, `MessageDAL`, `MessagePanelDAL`, `SettingDAL`, `RememberMeDAL`).

- [ ] **Step 1: Convert `DAL/DAL.csproj` to SDK-style format with modern dependencies**

Replace `DAL/DAL.csproj` with:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>disable</Nullable>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="EntityFramework" Version="6.5.1" />
    <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
    <PackageReference Include="System.Configuration.ConfigurationManager" Version="10.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\BE\BE.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Clean up legacy files and update `System.Data.SqlClient` references to `Microsoft.Data.SqlClient`**

1. Delete `DAL/packages.config` and `DAL/Properties/AssemblyInfo.cs`.
2. In `DAL/CustomerDAL.cs`, `DAL/InvoiceDAL.cs`, `DAL/CatalogItemDAL.cs`, and `DAL/SettingDAL.cs`, replace `using System.Data.SqlClient;` with `using Microsoft.Data.SqlClient;`.

- [ ] **Step 3: Build `DAL` project**

Run: `dotnet build DAL/DAL.csproj`
Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Step 4: Commit `DAL` migration**

```bash
git add DAL/
git commit -m "refactor(dal): migrate DAL to SDK-style net10.0 with EF 6.5.1 and Microsoft.Data.SqlClient"
```

---

### Task 3: Migrate `BLL` (Business Logic Layer) to SDK-Style `net10.0`

**Files:**
- Modify: `BLL/BLL.csproj`
- Delete: `BLL/packages.config` (if present)
- Delete: `BLL/Properties/AssemblyInfo.cs`

**Interfaces:**
- Consumes: `BE`, `DAL`, `EntityFramework 6.5.1`.
- Produces: `BLL` services (`CustomerBLL`, `InvoiceBLL`, `CatalogItemBLL`, `UserBLL`, `UserGroupBLL`, `AccessGuard`, `Pricing`, `StockPolicy`, `PasswordHasher`, etc.).

- [ ] **Step 1: Convert `BLL/BLL.csproj` to SDK-style format**

Replace `BLL/BLL.csproj` with:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>disable</Nullable>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="EntityFramework" Version="6.5.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\BE\BE.csproj" />
    <ProjectReference Include="..\DAL\DAL.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Remove legacy `BLL/Properties/AssemblyInfo.cs`**

Delete `BLL/Properties/AssemblyInfo.cs`.

- [ ] **Step 3: Build `BLL` project**

Run: `dotnet build BLL/BLL.csproj`
Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Step 4: Commit `BLL` migration**

```bash
git add BLL/
git commit -m "refactor(bll): migrate BLL to SDK-style net10.0"
```

---

### Task 4: Upgrade `CRMPeyvand.Tests` to `net10.0` and Verify All Tests

**Files:**
- Modify: `CRMPeyvand.Tests/CRMPeyvand.Tests.csproj`

**Interfaces:**
- Consumes: `BE`, `BLL`, `DAL`, `xUnit 2.9.2`, `Microsoft.NET.Test.Sdk 17.11.1`.
- Produces: Verified unit tests for pricing, stock policies, RBAC access guards, and password security.

- [ ] **Step 1: Update TargetFramework in `CRMPeyvand.Tests/CRMPeyvand.Tests.csproj` to `net10.0`**

Update `CRMPeyvand.Tests/CRMPeyvand.Tests.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\BE\BE.csproj" />
    <ProjectReference Include="..\BLL\BLL.csproj" />
    <ProjectReference Include="..\DAL\DAL.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Run test suite on .NET 10**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj`
Expected: `Passed! - Failed: 0, Passed: 24, Skipped: 0, Total: 24, Duration: ... - CRMPeyvand.Tests.dll (net10.0)`

- [ ] **Step 3: Commit `CRMPeyvand.Tests` modernization**

```bash
git add CRMPeyvand.Tests/
git commit -m "test: upgrade CRMPeyvand.Tests to net10.0 and verify test suite passes"
```

---

### Task 5: Migrate `CRMPeyvand` (WPF + WinForms) to SDK-Style `net10.0-windows`

**Files:**
- Modify: `CRMPeyvand/CRMPeyvand.csproj`
- Delete: `CRMPeyvand/packages.config`
- Delete: `CRMPeyvand/Properties/AssemblyInfo.cs`

**Interfaces:**
- Consumes: `BE`, `BLL`, `HandyControls`, `LiveCharts.Wpf`, `IPE.SmsIr`, `WinForms.DataVisualization`, `EntityFramework 6.5.1`, `Stimulsoft.Report` (.NET Core/Standard 2.1), `BehComponents.dll`.
- Produces: `CRMPeyvand.exe` (.NET 10 Windows Desktop application).

- [ ] **Step 1: Convert `CRMPeyvand/CRMPeyvand.csproj` to SDK-style format**

Update `CRMPeyvand/CRMPeyvand.csproj` with:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <UseWindowsForms>true</UseWindowsForms>
    <LangVersion>latest</LangVersion>
    <ApplicationIcon></ApplicationIcon>
    <StartupObject>CRMPeyvand.App</StartupObject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="EntityFramework" Version="6.5.1" />
    <PackageReference Include="HandyControls" Version="3.5.3" />
    <PackageReference Include="LiveCharts.Wpf" Version="0.9.7" />
    <PackageReference Include="IPE.SmsIr" Version="1.0.5" />
    <PackageReference Include="WinForms.DataVisualization" Version="1.9.2" />
    <PackageReference Include="System.Configuration.ConfigurationManager" Version="10.0.0" />
    <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
  </ItemGroup>

  <ItemGroup>
    <Reference Include="BehComponents">
      <HintPath>..\libs\BehComponents.dll</HintPath>
    </Reference>
    <Reference Include="Stimulsoft.Base">
      <HintPath>C:\Program Files (x86)\Stimulsoft Designer 2022.1.1\Libs\Reports.Net\.NETCoreApp3.1\Stimulsoft.Base.dll</HintPath>
    </Reference>
    <Reference Include="Stimulsoft.Report">
      <HintPath>C:\Program Files (x86)\Stimulsoft Designer 2022.1.1\Libs\Reports.Net\.NETCoreApp3.1\Stimulsoft.Report.dll</HintPath>
    </Reference>
    <Reference Include="Stimulsoft.Report.Win">
      <HintPath>C:\Program Files (x86)\Stimulsoft Designer 2022.1.1\Libs\Reports.Net\.NETCoreApp3.1\Stimulsoft.Report.Win.dll</HintPath>
    </Reference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\BE\BE.csproj" />
    <ProjectReference Include="..\BLL\BLL.csproj" />
  </ItemGroup>

  <ItemGroup>
    <None Update="Reports\*.mrt">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="App.config">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Remove legacy `CRMPeyvand/packages.config` and `CRMPeyvand/Properties/AssemblyInfo.cs`**

Delete `CRMPeyvand/packages.config` and `CRMPeyvand/Properties/AssemblyInfo.cs`.

- [ ] **Step 3: Build `CRMPeyvand` application**

Run: `dotnet build CRMPeyvand/CRMPeyvand.csproj`
Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Step 4: Commit `CRMPeyvand` migration**

```bash
git add CRMPeyvand/
git commit -m "refactor(ui): migrate CRMPeyvand WPF/WinForms project to net10.0-windows"
```

---

### Task 6: Modernize Stimulsoft Report Resolution and Verify Report Loading

**Files:**
- Modify: `CRMPeyvand/ReportsForm.cs:62-81`
- Modify: `CRMPeyvand/ReportsForm.cs:150-213`
- Modify: `CRMPeyvand/InvoiceForm.xaml.cs:305-320`

**Interfaces:**
- Consumes: Stimulsoft Reports engine, `.mrt` files located in output `Reports/` directory.
- Produces: Safe, cross-environment report rendering for all 10 templates.

- [ ] **Step 1: Update report path helper in `ReportsForm.cs` and `InvoiceForm.xaml.cs`**

Replace brittle path parent chains like `Directory.GetParent(Directory.GetParent(Directory.GetParent(Assembly.GetEntryAssembly().Location)...))` with `Path.Combine(AppContext.BaseDirectory, "Reports", mrtFileName)`:

In `ReportsForm.cs`:
```csharp
private void RenderAndShowReport(string mrtFileName)
{
    StiReport sti = new StiReport();
    string path = Path.Combine(AppContext.BaseDirectory, "Reports", mrtFileName);
    if (!File.Exists(path))
    {
        // Fallback for development if run from IDE
        path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\Reports", mrtFileName));
    }
    sti.Load(path);
    string connStr = ConfigurationManager.ConnectionStrings["conStr"]?.ConnectionString;
    if (!string.IsNullOrEmpty(connStr))
    {
        foreach (var database in sti.Dictionary.Databases.OfType<Stimulsoft.Report.Dictionary.StiSqlDatabase>())
        {
            database.ConnectionString = connStr;
        }
    }
    if (sti.Dictionary.Variables.Contains("Date"))
    {
        sti.Dictionary.Variables["Date"].Value = DateTime.Now.Date.ToString("yyyy/MM/dd");
    }
    sti.Render();
    sti.Show();
}
```

In `InvoiceForm.xaml.cs`:
```csharp
string reportPath = Path.Combine(AppContext.BaseDirectory, "Reports", "InvoicePrint.mrt");
if (!File.Exists(reportPath))
{
    reportPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\Reports", "InvoicePrint.mrt"));
}
StiReport sti = new StiReport();
sti.Load(reportPath);
```

- [ ] **Step 2: Build `CRMPeyvand` and verify report compilation**

Run: `dotnet build CRMPeyvand/CRMPeyvand.csproj`
Expected: `Build succeeded.`

- [ ] **Step 3: Commit report path modernization**

```bash
git add CRMPeyvand/ReportsForm.cs CRMPeyvand/InvoiceForm.xaml.cs
git commit -m "fix(reports): modernize Stimulsoft report loading path using AppContext.BaseDirectory"
```

---

### Task 7: Update README, Build Solution & Run Full Verification

**Files:**
- Modify: `README.md`

**Interfaces:**
- Consumes: Solution-level `CRMPeyvand.sln`.
- Produces: Complete, verified .NET 10 solution and up-to-date documentation.

- [ ] **Step 1: Update `README.md` badges, tech stack table, and build commands**

Update .NET Framework 4.7.2 badges and tables in `README.md` to `.NET 10`, `C# 13+`, `Entity Framework 6.5.1`, `HandyControls 3.5.3`.

- [ ] **Step 2: Build entire solution**

Run: `dotnet build CRMPeyvand.sln`
Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`

- [ ] **Step 3: Run entire test suite across the solution**

Run: `dotnet test CRMPeyvand.sln`
Expected: `Passed! - Failed: 0, Passed: 24, Skipped: 0, Total: 24`

- [ ] **Step 4: Final commit**

```bash
git add README.md CRMPeyvand.sln
git commit -m "docs: update README with .NET 10 and modernized tech stack information"
```
