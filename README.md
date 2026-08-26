# CRMPeyvand (پیوند CRM)

<div align="center">

![C#](https://img.shields.io/badge/Language-C%23%2013-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![WPF](https://img.shields.io/badge/UI-WPF%20%2F%20XAML-0078D7?style=for-the-badge&logo=windows&logoColor=white)
![Entity Framework](https://img.shields.io/badge/ORM-Entity%20Framework%206.5-68217A?style=for-the-badge&logo=nuget&logoColor=white)
![xUnit](https://img.shields.io/badge/Tests-xUnit%202.9-blue?style=for-the-badge&logo=xunit&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

**A modular, enterprise-grade Persian desktop CRM and sales management application built with WPF, C#, and Entity Framework 6.**

[Features](#-key-features) • [Architecture](#-architecture) • [Tech Stack](#-technology-stack) • [Getting Started](#-getting-started) • [Running Tests](#-running-tests) • [معرفی فارسی](#-معرفی-پروژه-به-زبان-فارسی)

</div>

---

## 📌 Overview

**CRMPeyvand** is a comprehensive Customer Relationship Management (CRM) and sales accounting desktop software tailored for Persian-speaking businesses. It provides seamless management of customers, goods and services catalog with stock control, sales invoicing with Persian (Jalali) dates, customer interaction logging, reminders, SMS messaging, and granular role-based access control (RBAC).

Designed with a clean multi-tier architecture, robust domain logic, and a secure authentication system, CRMPeyvand provides high reliability, maintainability, and enterprise-grade performance.

---

## ✨ Key Features

### 👥 Customer Management
- **Complete Customer Profiles**: Name, phone numbers, registration date, notes, and activity history.
- **Dynamic Search & Filtering**: Instant customer lookup by name or contact number.
- **Purchase History**: Instant access to past invoices and transaction logs.
- **Export & Reports**: Export customer directories and summaries to PDF and printable report formats.

### 📦 Catalog & Inventory Control
- **Goods vs. Services Separation**: Differentiates physical *Goods* (with strict non-negative stock tracking) from *Services* (non-physical items without inventory limits).
- **Automated Stock Depletion**: Inventory decreases automatically upon invoice finalization.
- **Unit Pricing & Details**: Category grouping, unit pricing, and stock alerts.

### 🧾 Invoicing & Sales Billing
- **Persian (Jalali) Date Support**: Built-in solar calendar integration for invoice issuance and tracking.
- **Itemized Invoice Lines**: Explicit line items with unit price snapshots, quantity, and line totals.
- **Discount Engine**: Support for promotional discount codes (`OffCode`) and percentage/fixed discounts with automatic calculation.
- **Stimulsoft Reporting**: Professional printable invoice templates (`.mrt`) and export options (PDF, Excel, Print).

### 🏷️ Promotions & Discount Codes
- **Campaign Codes**: Create and manage discount codes with validity periods and usage limits.
- **Status Toggle**: Easily activate, deactivate, or expire promo codes.

### 📅 Activities & Reminders
- **Customer Activities**: Log calls, meetings, follow-ups, and notes categorized by activity type.
- **Proactive Reminders**: Set dated alerts with automated notification popups for scheduled tasks.

### 📱 SMS Messaging & Notifications
- **IPE SmsIr Gateway Integration**: Direct integration with SMS provider API for automated and manual messaging.
- **Event-Driven Messaging**: Send automated SMS upon customer registration, invoice issuance, or reminders.
- **Bulk & Single SMS**: Send targeted announcements or individual customer notices.

### 🔐 Granular Role-Based Access Control (RBAC)
- **Typed Permission Matrix**: Fine-grained access control based on `Section` × `Operation` enums:
  - **Sections**: Customers, Invoices, Products, Activities, Reminders, Users, SMS, Settings, Reports.
  - **Operations**: `View`, `Create`, `Edit`, `Delete`.
- **User Groups**: Assign reusable permission sets to employee groups.
- **Built-in Administrator Group**: Auto-seeded on First Run with full system privileges.
- **First-Run Wizard**: Guided initial setup for creating the system administrator.

### 🛡️ Security & Authentication
- **PBKDF2-SHA256 Password Hashing**: Passwords stored using salted PBKDF2 (RFC 2898) with 100,000 iterations and per-user cryptographic salts.
- **Remember-Me Session Tokens**: Secure persistent local login capabilities.

### 📊 Dashboard & Visual Analytics
- **LiveCharts Visualization**: Interactive charts for sales performance, daily/weekly/monthly revenue trends, customer growth, and top catalog items.
- **Quick Metric Cards**: Real-time stats on pending reminders, recent invoices, and customer activities.

### 🗄️ Database Management & Backup
- **Code-First Entity Framework 6**: Structured migrations and clean relational schema.
- **Backup & Restore Utility**: Create `.bak` snapshots of the SQL database and restore anytime directly from the application settings.

---

## 🏛 Architecture

CRMPeyvand is structured around a decoupled **Layered (N-Tier) Architecture**:

```
CRMPeyvand (Solution)
├── 🖥️ CRMPeyvand        # Presentation Layer (WPF / XAML, HandyControl, LiveCharts, Stimulsoft)
├── ⚙️ BLL                # Business Logic Layer (Domain rules, AccessGuard, Pricing, PasswordHasher)
├── 🗄️ DAL                # Data Access Layer (Entity Framework 6 DbContext, Migrations, Repositories)
├── 📦 BE                 # Business Entities (Domain Models: Customer, Invoice, User, CatalogItem, etc.)
└── 🧪 CRMPeyvand.Tests   # Unit & Integration Tests (xUnit)
```

### Architectural Decisions (ADRs)
- **ADR-0001**: Incremental in-place refactoring preserving Persian UI formatting and legacy flows.
- **ADR-0002**: Explicit `InvoiceLine` entity replacing implicit join tables for accurate historical pricing and stock decrements.
- **ADR-0003**: Strongly-typed `(Section, Operation)` permission matrix eliminating magic string comparisons.
- **ADR-0004**: Modern .NET 10 runtime & SDK-style project system.
- **ADR-0005**: Cryptographic PBKDF2-SHA256 hashing replacing reversible encoding.
- **ADR-0006**: Flagged `IsBuiltIn` Administrator group replacing title-string conventions.

---

## 💻 Technology Stack

| Component | Technology / Library |
|---|---|
| **Platform** | .NET 10 (Windows Desktop) |
| **Language** | C# 13+ |
| **UI Framework** | WPF (Windows Presentation Foundation) |
| **UI Controls** | HandyControl (`v3.5.3`), BehComponents |
| **Data Access & ORM** | Entity Framework 6 (`v6.5.1`) & Microsoft.Data.SqlClient |
| **Database** | Microsoft SQL Server (LocalDB / Express / Standard) |
| **Charts** | LiveCharts.Wpf (`v0.9.7`), WinForms.DataVisualization (`v1.9.2`) |
| **Reporting** | Stimulsoft Reports .NET Core / Win (`v2022.1.1`) |
| **SMS Provider** | IPE.SmsIr (`v1.0.5`) |
| **Typography** | Shabnam Persian Font (Embedded) |
| **Testing** | xUnit (`v2.9.2`), Microsoft.NET.Test.Sdk (`v17.11.1`) |

---

## 🚀 Getting Started

### Prerequisites
- **Operating System**: Windows 10 / 11 / Windows Server
- **IDE / CLI**: [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download)
- **Database Engine**: Microsoft SQL Server 2016+ or SQL Server LocalDB / Express

### 1. Clone the Repository
```bash
git clone https://github.com/0ehsan-sh0/CRMPeyvand.git
cd CRMPeyvand
```

### 2. Configure Database Connection
Open `CRMPeyvand/App.config` and `DAL/App.config` and adjust the connection string according to your local SQL Server instance:

```xml
<connectionStrings>
  <add name="conStr"
       connectionString="Data Source=.;Initial Catalog=CRMPeyvand;Integrated Security=true"
       providerName="Microsoft.Data.SqlClient" />
</connectionStrings>
```

### 3. Restore & Build Solution
You can build using the .NET CLI:

```bash
# Build Solution
dotnet build CRMPeyvand.sln
```

### 4. Apply Database Migrations
In Visual Studio **Package Manager Console** (set Default project to `DAL`):

```powershell
Update-Database
```

### 5. Launch Application & First Run
1. Start `CRMPeyvand` (`F5` in Visual Studio or run `CRMPeyvand.exe`).
2. On **First Run**, the application will automatically detect that no users exist and display the initial setup screen.
3. Register your primary **System Administrator** account. The application will seed the Built-in Administrator group with full access rights.

---

## 🧪 Running Tests

Unit tests are implemented with **xUnit** covering domain logic, stock policies, pricing engine, permission matrix, and security hashing.

Run tests using the .NET CLI:

```bash
dotnet test
```

Expected output:
```text
Passed!  - Failed: 0, Passed: 24, Skipped: 0, Total: 24
```

---

## 🇮🇷 معرفی پروژه به زبان فارسی

<div dir="rtl" style="text-align: right;">

### نرم‌افزار مدیریت ارتباط با مشتری و فروش پیوند (<span dir="ltr">CRMPeyvand</span>)

نرم‌افزار **پیوند** یک سیستم جامع، ماژولار و بومی تحت دسکتاپ برای مدیریت امور مشتریان، صدور فاکتورهای فروش با تاریخ شمسی، انبارداری و کاتالوگ کالا و خدمات، ثبت فعالیت‌ها و پیگیری‌ها، ارسال پیامک‌های هوشمند و مدیریت سطوح دسترسی کاربران بر پایه معماری چندلایه <span dir="ltr">(.NET 10 / WPF / EF 6.5)</span> است.

#### ویژگی‌های کلیدی:
- **مدیریت پیشرفته مشتریان**: ثبت مشخصات، سوابق خرید، تاریخچه تعاملات و جستجوی لحظه‌ای.
- **کاتالوگ کالا و خدمات**: تفکیک کالای فیزیکی (دارای موجودی و کسر خودکار از انبار هنگام فروش) و خدمات، با اعمال قوانین دقیق کنترل موجودی.
- **صدور و مدیریت فاکتور**: محاسبه خودکار اقلام، اعمال کدهای تخفیف، تبدیل تاریخ به تقویم هجری شمسی و چاپ فاکتور استاندارد با استیمول‌سافت (<span dir="ltr">Stimulsoft Reports</span>).
- **کدهای تخفیف (<span dir="ltr">OffCode</span>)**: ایجاد کدهای تخفیف درصدی و مبلغی با قابلیت تعیین سقف استفاده و بازه زمانی معتبر.
- **فعالیت‌ها و یادآورها**: ثبت تماس‌ها، جلسات و وظایف به تفکیک دسته‌بندی با اعلان و آلارم هوشمند.
- **سامانه پیامک**: ارتباط با وب‌سرویس <span dir="ltr">SmsIr</span> برای ارسال پیامک‌های خوش‌آمدگویی، صدور فاکتور و اطلاع‌رسانی انبوه.
- **سطوح دسترسی پیشرفته (<span dir="ltr">RBAC</span>)**: ماتریس دسترسی به تفکیک بخش‌ها (<span dir="ltr">Sections</span>) و عملیات (<span dir="ltr">View, Create, Edit, Delete</span>).
- **امنیت بالا**: هش‌کردن رمزهای عبور با الگوریتم قدرتمند <span dir="ltr">PBKDF2-SHA256</span> و ۱۰۰٬۰۰۰ تکرار به‌همراه <span dir="ltr">Salt</span> اختصاصی.
- **داشبورد آماری و نموداری**: نمایش نمودارهای زنده فروش، رشد مشتریان و آمارهای کلیدی با <span dir="ltr">LiveCharts</span>.
- **پشتیبان‌گیری و بازیابی پایگاه داده**: تهیه آسان نسخه پشتیبان (<span dir="ltr">.bak</span>) از دیتابیس <span dir="ltr">SQL Server</span> و بازیابی درون‌برنامه‌ای.

</div>

---

## 📄 License

This project is open-source software licensed under the [MIT License](LICENSE).
