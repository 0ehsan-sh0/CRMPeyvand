# QuestPDF Reporting Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Completely replace legacy Stimulsoft reporting (`.mrt` files, hardcoded DLL paths, dynamic compilation) with a modern, code-first QuestPDF reporting engine supporting sales invoices, customer reports, activity logs, and periodic sales summaries with native Persian/RTL typography.

**Architecture:** Implement code-first `IDocument` layouts using QuestPDF fluent C# APIs. Feed all documents with strongly-typed ViewModels from BLL/DAL. Provide a unified PDF generation, interactive preview, and system printing workflow while removing all legacy Stimulsoft packages and `.mrt` templates.

**Tech Stack:** .NET 10 (`net10.0`, `net10.0-windows`), C# 13, QuestPDF (2024.12.x / Community License), SkiaSharp / HarfBuzz text shaping, xUnit.

**Spec:** [docs/adr/0007-questpdf-reporting-engine.md](file:///e:/Developing/Projects/CRMPeyvand/docs/adr/0007-questpdf-reporting-engine.md)

## Global Constraints

- Must run on modern .NET 10 (`net10.0` / `net10.0-windows`) without legacy `BinaryFormatter` or runtime `CodeDom` dependencies.
- Zero external software installations required on developer or client machines.
- All documents must render strictly Right-to-Left (`ContentDirection.RightToLeft`) with Persian numeral and calendar formatting (`PersianCalendar`, `fa-IR`).
- Maintain 100% test coverage with passing xUnit tests for every document type.
- Never use direct SQL queries inside document templates; all data flows through BLL domain models.

---

### Task 1: Package Modernization & License Setup

**Files:**
- Modify: `CRMPeyvand/CRMPeyvand.csproj:26-36`
- Modify: `CRMPeyvand.Tests/CRMPeyvand.Tests.csproj:27-33`
- Modify: `CRMPeyvand/App.xaml.cs:25-35`
- Test: `CRMPeyvand.Tests/Reporting/QuestPdfSetupTests.cs`

**Interfaces:**
- Produces: QuestPDF initialization in `App.xaml.cs` (`QuestPDF.Settings.License = LicenseType.Community;`).

- [x] **Step 1: Write the failing test for QuestPDF setup and basic PDF generation**

```csharp
// CRMPeyvand.Tests/Reporting/QuestPdfSetupTests.cs
using System;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Xunit;

namespace CRMPeyvand.Tests.Reporting
{
    public class QuestPdfSetupTests
    {
        [Fact]
        public void QuestPdf_GeneratesSimplePdfStream_Successfully()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Content().Text("تست سیستم گزارش‌گیری CRM پیوند");
                });
            }).GeneratePdf();

            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 0);
        }
    }
}
```

- [x] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter "FullyQualifiedName~QuestPdfSetupTests"`
Expected: Compilation failure (QuestPDF reference missing).

- [x] **Step 3: Update `CRMPeyvand.csproj` and `CRMPeyvand.Tests.csproj` and `App.xaml.cs`**

Remove:
- `Stimulsoft.Reports.Engine.NetCore`
- Hardcoded `Stimulsoft.Report.Win` reference

Add to both csproj files:
```xml
<PackageReference Include="QuestPDF" Version="2024.12.1" />
```

Update `CRMPeyvand/App.xaml.cs`:
```csharp
// Configure QuestPDF Community License
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
```

- [x] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~QuestPdfSetupTests"`
Expected: PASS.

- [x] **Step 5: Commit changes**

```bash
git add CRMPeyvand/CRMPeyvand.csproj CRMPeyvand.Tests/CRMPeyvand.Tests.csproj CRMPeyvand/App.xaml.cs CRMPeyvand.Tests/Reporting/QuestPdfSetupTests.cs
git commit -m "chore: replace Stimulsoft dependencies with QuestPDF 2024.12"
```

---

### Task 2: Reporting ViewModels & Domain DTOs

**Files:**
- Create: `CRMPeyvand/Reports/Models/InvoiceReportModel.cs`
- Create: `CRMPeyvand/Reports/Models/CustomerReportModel.cs`
- Create: `CRMPeyvand/Reports/Models/ActivityReportModel.cs`
- Create: `CRMPeyvand/Reports/Models/SalesSummaryReportModel.cs`
- Create: `CRMPeyvand/Reports/Models/CatalogItemReportModel.cs`
- Test: `CRMPeyvand.Tests/Reporting/ReportModelTests.cs`

**Interfaces:**
- Produces: Strongly-typed models for all CRM printable reports.

- [x] **Step 1: Write tests for report model initialization and computed fields**

```csharp
// CRMPeyvand.Tests/Reporting/ReportModelTests.cs
using System;
using System.Collections.Generic;
using CRMPeyvand.Reports.Models;
using Xunit;

namespace CRMPeyvand.Tests.Reporting
{
    public class ReportModelTests
    {
        [Fact]
        public void InvoiceReportModel_CalculatesTotalsCorrectly()
        {
            var model = new InvoiceReportModel
            {
                InvoiceNumber = "INV-1001",
                IssueDatePersian = "1405/06/05",
                CustomerName = "علی رضایی",
                CustomerPhone = "09123456789",
                Items = new List<InvoiceItemRowModel>
                {
                    new InvoiceItemRowModel { RowIndex = 1, ItemName = "نرم‌افزار CRM", Quantity = 2, UnitPrice = 5000000 },
                    new InvoiceItemRowModel { RowIndex = 2, ItemName = "پشتیبانی سالانه", Quantity = 1, UnitPrice = 2000000 }
                },
                DiscountAmount = 1000000
            };

            Assert.Equal(12000000, model.SubTotal);
            Assert.Equal(11000000, model.FinalTotal);
        }
    }
}
```

- [x] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter "FullyQualifiedName~ReportModelTests"`
Expected: FAIL (types not found).

- [x] **Step 3: Implement report models in `CRMPeyvand/Reports/Models/`**

Create `InvoiceReportModel.cs`:
```csharp
using System.Collections.Generic;
using System.Linq;

namespace CRMPeyvand.Reports.Models
{
    public class InvoiceItemRowModel
    {
        public int RowIndex { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice => Quantity * UnitPrice;
    }

    public class InvoiceReportModel
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string IssueDatePersian { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public List<InvoiceItemRowModel> Items { get; set; } = new();
        public double DiscountAmount { get; set; }
        public double SubTotal => Items.Sum(i => i.TotalPrice);
        public double FinalTotal => SubTotal - DiscountAmount;
        public string Note { get; set; } = string.Empty;
    }
}
```

Create `CustomerReportModel.cs`, `ActivityReportModel.cs`, `SalesSummaryReportModel.cs`, `CatalogItemReportModel.cs`.

- [x] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~ReportModelTests"`
Expected: PASS.

- [x] **Step 5: Commit changes**

```bash
git add CRMPeyvand/Reports/Models/ CRMPeyvand.Tests/Reporting/ReportModelTests.cs
git commit -m "feat: add strongly-typed report viewmodels for QuestPDF"
```

---

### Task 3: RTL Persian Layout Foundation & Invoice Document

**Files:**
- Create: `CRMPeyvand/Reports/Common/PersianReportStyle.cs`
- Create: `CRMPeyvand/Reports/Documents/InvoiceDocument.cs`
- Test: `CRMPeyvand.Tests/Reporting/InvoiceDocumentTests.cs`

**Interfaces:**
- Consumes: `InvoiceReportModel` from Task 2.
- Produces: `InvoiceDocument : IDocument` generating professional Persian sales invoices.

- [ ] **Step 1: Write test for InvoiceDocument rendering**

```csharp
// CRMPeyvand.Tests/Reporting/InvoiceDocumentTests.cs
using System.Collections.Generic;
using CRMPeyvand.Reports.Documents;
using CRMPeyvand.Reports.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Xunit;

namespace CRMPeyvand.Tests.Reporting
{
    public class InvoiceDocumentTests
    {
        [Fact]
        public void InvoiceDocument_RendersValidPdf()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var model = new InvoiceReportModel
            {
                InvoiceNumber = "1001",
                IssueDatePersian = "1405/06/05",
                CustomerName = "شرکت پویان",
                CustomerPhone = "09121112233",
                Items = new List<InvoiceItemRowModel>
                {
                    new InvoiceItemRowModel { RowIndex = 1, ItemName = "سرور ابری", Quantity = 1, UnitPrice = 15000000 }
                },
                DiscountAmount = 500000
            };

            var doc = new InvoiceDocument(model);
            byte[] pdf = doc.GeneratePdf();

            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 1000);
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter "FullyQualifiedName~InvoiceDocumentTests"`
Expected: FAIL (types missing).

- [ ] **Step 3: Implement `PersianReportStyle.cs` and `InvoiceDocument.cs`**

Create `PersianReportStyle.cs` with standard Persian colors, typography, and RTL table styling.
Create `InvoiceDocument.cs` implementing `QuestPDF.Infrastructure.IDocument` with Header, Content (Customer box, Line items table, Summary totals, Persian currency formatting), and Footer with page numbers.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~InvoiceDocumentTests"`
Expected: PASS.

- [ ] **Step 5: Commit changes**

```bash
git add CRMPeyvand/Reports/Common/ CRMPeyvand/Reports/Documents/InvoiceDocument.cs CRMPeyvand.Tests/Reporting/InvoiceDocumentTests.cs
git commit -m "feat: implement InvoiceDocument with QuestPDF RTL typography"
```

---

### Task 4: Management & Analytical Reports Documents

**Files:**
- Create: `CRMPeyvand/Reports/Documents/CustomerListDocument.cs`
- Create: `CRMPeyvand/Reports/Documents/ActivityListDocument.cs`
- Create: `CRMPeyvand/Reports/Documents/SalesSummaryDocument.cs`
- Create: `CRMPeyvand/Reports/Documents/CatalogItemListDocument.cs`
- Test: `CRMPeyvand.Tests/Reporting/ManagementReportsTests.cs`

**Interfaces:**
- Consumes: Models from Task 2.
- Produces: Documents for Customer lists, Activities, Periodic Sales (Weekly/Monthly/Yearly/DateRange), and Products.

- [ ] **Step 1: Write tests for all management report documents**

```csharp
// CRMPeyvand.Tests/Reporting/ManagementReportsTests.cs
using System.Collections.Generic;
using CRMPeyvand.Reports.Documents;
using CRMPeyvand.Reports.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Xunit;

namespace CRMPeyvand.Tests.Reporting
{
    public class ManagementReportsTests
    {
        [Fact]
        public void CustomerListDocument_RendersValidPdf()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var model = new CustomerReportModel
            {
                ReportTitle = "گزارش مشتریان",
                GeneratedDatePersian = "1405/06/05",
                Customers = new List<CustomerRowModel>
                {
                    new CustomerRowModel { RowIndex = 1, Name = "احسان شریفی", Phone = "09120000000", RegDatePersian = "1405/01/01" }
                }
            };

            var doc = new CustomerListDocument(model);
            byte[] pdf = doc.GeneratePdf();
            Assert.True(pdf.Length > 1000);
        }

        [Fact]
        public void SalesSummaryDocument_RendersValidPdf()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var model = new SalesSummaryReportModel
            {
                ReportTitle = "گزارش فروش دوره‌ای",
                StartDatePersian = "1405/06/01",
                EndDatePersian = "1405/06/05",
                UserSales = new List<UserSalesRowModel>
                {
                    new UserSalesRowModel { UserName = "کاربر ارشد", InvoicesCount = 12, TotalAmount = 45000000 }
                }
            };

            var doc = new SalesSummaryDocument(model);
            byte[] pdf = doc.GeneratePdf();
            Assert.True(pdf.Length > 1000);
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter "FullyQualifiedName~ManagementReportsTests"`
Expected: FAIL.

- [ ] **Step 3: Implement the document classes**

Implement `CustomerListDocument.cs`, `ActivityListDocument.cs`, `SalesSummaryDocument.cs`, `CatalogItemListDocument.cs`.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~ManagementReportsTests"`
Expected: PASS.

- [ ] **Step 5: Commit changes**

```bash
git add CRMPeyvand/Reports/Documents/ CRMPeyvand.Tests/Reporting/ManagementReportsTests.cs
git commit -m "feat: implement Customer, Activity, and Sales summary QuestPDF documents"
```

---

### Task 5: Print Preview & Export Service & UI Integration

**Files:**
- Create: `CRMPeyvand/Reports/Services/ReportViewerService.cs`
- Modify: `CRMPeyvand/InvoiceForm.xaml.cs:320-375`
- Modify: `CRMPeyvand/ReportsForm.cs:70-240`
- Delete: `CRMPeyvand/Reports/*.mrt` (all 10 `.mrt` files)
- Modify: `CRMPeyvand/CRMPeyvand.csproj:48-52` (remove `.mrt` copy target)
- Test: `CRMPeyvand.Tests/Reporting/InvoiceDocumentTests.cs`

**Interfaces:**
- Produces: `ReportViewerService.OpenOrPrint(IDocument document, string title)` to show system PDF / print dialog seamlessly.

- [ ] **Step 1: Implement `ReportViewerService.cs`**

```csharp
// CRMPeyvand/Reports/Services/ReportViewerService.cs
using System;
using System.Diagnostics;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace CRMPeyvand.Reports.Services
{
    public static class ReportViewerService
    {
        public static void OpenReportPdf(IDocument document, string filePrefix = "Report")
        {
            string tempPath = Path.Combine(Path.GetTempPath(), $"{filePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            document.GeneratePdf(tempPath);

            var psi = new ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}
```

- [ ] **Step 2: Refactor `InvoiceForm.xaml.cs` to use QuestPDF**

Replace `StiReport` block in `InvoiceForm.xaml.cs` with `InvoiceDocument` and `ReportViewerService.OpenReportPdf(doc, "Invoice")`.

- [ ] **Step 3: Refactor `ReportsForm.cs` to use QuestPDF**

Replace `RenderAndShowReport` and `pictureBox4_Click` in `ReportsForm.cs` with strongly-typed `BLL` reads mapped into `CustomerListDocument`, `ActivityListDocument`, `SalesSummaryDocument`, and `CatalogItemListDocument`.

- [ ] **Step 4: Delete legacy `.mrt` files and remove Stimulsoft references from `CRMPeyvand.csproj`**

- [ ] **Step 5: Run full test suite**

Run: `dotnet test`
Expected: All tests pass.

- [ ] **Step 6: Commit changes**

```bash
git add CRMPeyvand/Reports/ CRMPeyvand/InvoiceForm.xaml.cs CRMPeyvand/ReportsForm.cs CRMPeyvand/CRMPeyvand.csproj
git commit -m "feat: complete migration from Stimulsoft to QuestPDF reporting"
```
