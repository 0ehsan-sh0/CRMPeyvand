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
- **Discount Engine**: Support for promotional discount codes (`Discount Code`) and percentage/fixed discounts with automatic calculation.
- **QuestPDF Reporting**: Invoices, customer lists, catalog lists, activity logs, and sales summaries are laid out in typed C# (`IDocument`), not in a binary template — see [ADR-0007](docs/adr/0007-questpdf-reporting-engine.md). Right-to-left Persian typography with embedded fonts, preview, and PDF/print export.
- **Readable Money**: Prices are grouped for reading wherever they appear — typed into a field, listed in a grid, or totalled on an invoice — and the separators come back off before the amount is saved.
- **Payments Against Invoices**: Record what a customer actually paid, in full or in part, against any invoice. The invoice's "paid" state is derived from those payments, so the two cannot disagree — see [ADR-0009](docs/adr/0009-payments-authoritative-over-checkout-flag.md).
- **Outstanding Balances**: Every invoice and every customer carries a derived balance. Overpayment is refused rather than held as credit, so a payment never exceeds what the invoice still owes.
- **رسید دریافت (Payment Receipt)**: A printed receipt per payment, carrying the invoice total, what that receipt paid, what is left, the Payment Instrument and who took it.

### 💰 Receivables
- **Ledger Screen**: A dedicated «دریافت‌ها» screen lists every payment, searchable by invoice number or customer name. Reachable from the sidebar, from shortcut `V`, or by right-clicking any unpaid invoice and choosing «ثبت وصولی».
- **Payment Instruments**: Cash, card, transfer, cheque or other, with an optional reference number — enough to answer "how much came in cash versus card" without dragging in full cheque tracking.
- **Void, Never Amend**: A payment may already have a printed receipt in the customer's hand, so a correction is a void plus a fresh payment rather than an edit.
- **مانده حساب Statement**: A report of every customer who still owes something, with their invoice count, total billed, total received and balance.
- **Cashier-Safe Access**: Payments are their own permission section, so a cashier can take money without being able to issue invoices — and therefore without being able to reduce stock.
- **Settle at the Counter**: The invoice screen's «وضعیت پرداخت» checkbox records the full payable as a real payment inside the same transaction that writes the invoice, so the commonest transaction in the shop is still one click.

### 🏷️ Promotions & Discount Codes
- **Campaign Codes**: Create and manage discount codes with validity periods and usage limits.
- **Status Toggle**: Easily activate, deactivate, or expire promo codes.

### 🔢 Price Entry & Display
- **Grouped as Typed**: The price field regroups with each keystroke (`100000` → `100,000`) and the caret stays on the digit being edited, rather than jumping to the end.
- **Separator-Free Saves**: Grouping is a display concern only. The separators are stripped before the amount is persisted, and a price too large to store is reported instead of throwing.
- **One Rule Everywhere**: One helper formats prices for the field, the grids, and the invoice labels, so a price cannot look one way where it was typed and another where it is listed. Grouping follows the current culture rather than a hard-coded comma.
- **Named Unit**: تومان is shown on the price field itself.
- **Stock Counts Left Alone**: Quantity columns are not formatted as money.

### 📅 Activities & Reminders
- **Customer Activities**: Log calls, meetings, follow-ups, and notes categorized by activity type.
- **Proactive Reminders**: Set dated alerts with automated notification popups for scheduled tasks.

### 📱 SMS Messaging & Notifications
- **IPE SmsIr Gateway Integration**: Direct integration with SMS provider API for automated and manual messaging.
- **Event-Driven Messaging**: Send automated SMS upon customer registration, invoice issuance, or reminders.
- **Bulk & Single SMS**: Send targeted announcements or individual customer notices.

### 🔐 Granular Role-Based Access Control (RBAC)
- **Typed Permission Matrix**: Fine-grained access control based on `Section` × `Operation` enums:
  - **Sections**: Customers, CatalogItems, Invoices, Activities, Reminders, Users, SmsPanel, Reports, Settings, Discounts, Payments.
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
- **Debtor Count**: A «مشتریان بدهکار» figure in the small stat box between the metric cards and the reminders panel, showing how many customers currently owe something, and clicking it opens the payments screen. Three cells beside it are reserved for future figures.

### 🗄️ Database Management & Backup
- **No Setup Required**: SQLite by default, with the schema created on first run; SQL Server as an opt-in configured in the app.
- **Code-First Entity Framework 6**: EF6 models both providers; structured migrations govern the SQL Server schema.
- **Backup & Restore Utility**: Take a restorable snapshot of the database from the settings screen. SQLite snapshots the file through SQLite's online backup API, so an employee can keep working while the copy is being taken; SQL Server uses `BACKUP DATABASE`.

---

## 🏛 Architecture

CRMPeyvand is structured around a decoupled **Layered (N-Tier) Architecture**:

```
CRMPeyvand (Solution)
├── 🖥️ CRMPeyvand        # Presentation Layer (WPF / XAML, HandyControl, LiveCharts, QuestPDF)
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
- [ADR-0007](docs/adr/0007-questpdf-reporting-engine.md): QuestPDF replacing Stimulsoft; report layouts as typed C# rather than binary `.mrt` templates.
- [ADR-0008](docs/adr/0008-dual-database-mode.md): SQLite by default with SQL Server on request, one DAL behind both.
- [ADR-0009](docs/adr/0009-payments-authoritative-over-checkout-flag.md): Payments are the record of what was received; the invoice's checkout flag is derived from them.

---

## 💻 Technology Stack

| Component | Technology / Library |
|---|---|
| **Platform** | .NET 10 (Windows Desktop) |
| **Language** | C# 13+ |
| **UI Framework** | WPF (Windows Presentation Foundation) |
| **UI Controls** | HandyControl (`v3.5.3`), BehComponents |
| **Data Access & ORM** | Entity Framework 6 (`v6.5.1`), System.Data.SQLite.EF6 (`v2.0.3`), SQLitePCLRaw.bundle_e_sqlite3 (`v3.0.5`), Microsoft.Data.SqlClient (`v5.2.2`, SQL Server only) |
| **Database** | SQLite (default) **or** Microsoft SQL Server (opt-in, user-selectable) |
| **Charts** | LiveCharts.Wpf (`v0.9.7`) |
| **Reporting** | QuestPDF (`v2024.12.1`) |
| **SMS Provider** | IPE.SmsIr (`v1.0.5`) |
| **Typography** | Shabnam Persian Font (Embedded) |
| **Testing** | xUnit (`v2.9.2`), Microsoft.NET.Test.Sdk (`v17.11.1`) |

---

## 🚀 Getting Started

### Prerequisites
- **Operating System**: Windows 10 / 11 / Windows Server
- **IDE / CLI**: [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download)
- **Database Engine**: none. SQLite is the default provider and ships inside the app,
  so a fresh clone runs with no database server installed. Microsoft SQL Server is an
  opt-in alternative chosen in the app (see [Choosing a database](#choosing-a-database));
  if you pick it, you need SQL Server 2016+ or LocalDB / Express yourself.

### 1. Clone the Repository
```bash
git clone https://github.com/0ehsan-sh0/CRMPeyvand.git
cd CRMPeyvand
```

### 2. Database
No configuration step. The app opens a SQLite database next to its settings file
(under `%ProgramData%\CRMPeyvand`, falling back to `%LocalAppData%\CRMPeyvand`) and
creates the schema itself on first run.

To use SQL Server instead, start the app once and choose it under
**Settings → پیکربندی پایگاه داده**, which also carries a connection test. The choice
is stored in `provider.json` alongside the database and applies from then on.

The `conStr` connection string still in `CRMPeyvand/App.config` is only the default
offered when SQL Server is selected; editing it is no longer a setup step.

### 3. Restore & Build Solution
You can build using the .NET CLI:

```bash
# Build Solution
dotnet build CRMPeyvand.sln
```

### 4. Database Migrations
No manual step. SQLite builds its own schema on first connection. On SQL Server the EF6
migrations run automatically at startup (`MigrateDatabaseToLatestVersion`), which also
creates and seeds the database if it is not there yet. `Update-Database` in the Visual
Studio **Package Manager Console** is only needed when authoring a new migration.

### 5. Launch Application & First Run
1. Start `CRMPeyvand` (`F5` in Visual Studio or run `CRMPeyvand.exe`).
2. On **First Run**, the application will automatically detect that no users exist and display the initial setup screen.
3. Register your primary **System Administrator** account. The application will seed the Built-in Administrator group with full access rights.

---

## 📦 Building the MSI Installer

`build-installer.ps1` publishes the app and wraps it in a Windows Installer package
using [WiX Toolset v5](https://wixtoolset.org). Output lands in `artifacts\CRMPeyvand.msi`.

```powershell
.\build-installer.ps1 -Version 1.0.0
```

Optional code signing (recommended — an unsigned MSI triggers SmartScreen warnings):

```powershell
.\build-installer.ps1 -Version 1.0.0 -SignPfx certs\peyvand.pfx -SignPfxPassword $env:PFX_PASSWORD
```

**Pass `-Version` on every release.** `MajorUpgrade` keys off the MSI `ProductVersion`,
so an unchanged version will not upgrade an existing installation. The value you pass is
authoritative and overrides `ProductVersion` in `installer\CRMPeyvand.Installer.wixproj`,
so the wixproj value only has to change if you build the installer project directly.

The script reads the version back out of the built MSI and fails if it is not the one you
asked for. That check exists because the version used to be silently dropped: WiX does not
treat `DefineConstants` as an input to its compile step, so a warm `installer\obj` made
the target up to date and the link step re-copied the **previous** MSI, version included —
every build after the first on a given machine emitted the wixproj default of `1.0.0`
regardless of `-Version`. The script therefore forces `-t:Rebuild`. A fresh CI runner never
saw the problem because its `obj` is always empty.

The MSI is **framework-dependent** and installs into `Program Files` by default. It
presents the standard Windows Installer wizard (Welcome → License → install folder →
Ready to install), and the folder page lets the user choose where the application goes.

It checks its prerequisites at launch and refuses to install — with a specific message —
if the runtime is missing:

| Prerequisite | Checked how |
| --- | --- |
| .NET 10 Desktop Runtime (x64) | `Microsoft.WindowsDesktop.App` shared framework folder |

The runtime check accepts any `Microsoft.WindowsDesktop.App` version, so a machine with
only an older desktop runtime will pass the check and then fail to launch the app.

There is no database prerequisite. SQLite is the default provider, it needs nothing
installed, and its native `e_sqlite3.dll` ships inside the package.

#### Choosing a database

CRMPeyvand runs against **either SQLite or Microsoft SQL Server**, chosen by the user.
Nothing about SQL Server is needed to install or run the app, and both providers are
covered by the same DAL and the same entities.

| | SQLite (default) | SQL Server (opt-in) |
| --- | --- | --- |
| **Chosen where** | The default; nothing to do | Settings → پیکربندی پایگاه داده |
| **Schema created by** | `SqliteSchema.Ensure` on first connection | EF6 `MigrateDatabaseToLatestVersion` at startup, which also seeds |
| **Server to install** | None | SQL Server 2016+, or LocalDB / Express |
| **Connection type** | `System.Data.SQLite.SQLiteConnection`, returned **open** so the schema exists before the first query | `System.Data.SqlClient.SqlConnection`, returned **closed** for EF6 to open and migrate |
| **Data file** | `CRMPeyvand.db` next to `provider.json` | Whatever the connection string names |

**The choice is reversible.** The SQL Server connection string is kept in
`provider.json` even while SQLite is active, so switching back and forth does not
mean retyping a server, a login, and a password each time. The settings screen's
on/off switch drives this; activating SQL Server with nothing saved is refused
rather than silently producing a broken connection.

**Where the setting lives:** `provider.json` in `%ProgramData%\CRMPeyvand`, falling
back to `%LocalAppData%\CRMPeyvand` if the shared folder is not writable. The
`CRMPeyvand.db` file sits beside it, so every employee on a machine shares one
database; a user who cannot write to the shared folder gets their own under
`%LocalAppData%` instead.

**Provider registration is done in code**, not in `App.config`. On .NET 10 the
`system.data` section is not recognised, because `System.Data.Common` is a separate
assembly, and even once declared the factory cannot be resolved to a provider
invariant. `DB`'s static constructor registers the SQLite factory and provider
services instead, and deliberately does **not** displace the `App.config` entry, so
SQL Server still resolves.

**The SQL Server connection is a `System.Data.SqlClient` one, not
`Microsoft.Data.SqlClient`.** EF6's SQL Server provider services hard-cast to
`System.Data.SqlClient.SqlConnection`, so handing them the other type fails every
query with *"Unable to determine the provider name for provider factory of type
'Microsoft.Data.SqlClient.SqlClientFactory'"* — and registering that factory's
invariant only moves the failure to an `InvalidCastException`. `System.Data.SqlClient`
arrives with `EntityFramework`, so this needs no extra dependency.

**The settings screen's connection test runs a statement through EF6**, not raw
ADO.NET. A test that only opens the connection will happily pass a configuration
that EF6 cannot use, which is how a broken SQL Server setting was previously
reported as working. The probe uses a context that maps nothing and has a null
initialiser, so it cannot create or migrate a database to find out.

### Where the installer stores things

| Path | Contents |
| --- | --- |
| `C:\Program Files\CRM Peyvand\` | Application and dependencies (user-selectable) |
| `C:\ProgramData\CRM Peyvand\UserPisc\` | Employee photos |
| Start Menu → `CRM Peyvand` | Shortcut |
| Public Desktop | Shortcut |

The desktop icon is written to the **public** desktop, not the installing user's, so
every employee sees it rather than just the administrator who ran the installer.

Employee photos are kept out of `Program Files` because that folder is read-only for
standard users. Because the MSI cannot set an ACL on the photos folder, the app probes
writability at runtime and falls back to `%LocalAppData%\CRMPeyvand\UserPisc` for any
user who cannot write to the shared folder. On a shared machine, later users can save
and see their own photo, but cannot see earlier users' photos.

### Releasing

Pushing a `v*` tag publishes the MSI to a GitHub Release:

```bash
git tag v1.0.0
git push origin v1.0.0
```

The tag must be exactly `vMAJOR.MINOR.PATCH`; the number after `v` becomes the MSI
`ProductVersion`. See `.github/workflows/release.yml`. To sign releases, add a base64
`.pfx` as the `MSI_CERT_PFX` repository secret and its password as `MSI_CERT_PASSWORD`.
Without a certificate the MSI is **unsigned** and SmartScreen will warn end users; the
workflow warns and carries on rather than failing.

Re-running a tag is safe: if a release already exists for it, the workflow uploads the new
MSI onto that release and replaces the old asset, instead of trying to create a second one.

Every push and pull request also builds the MSI via `.github/workflows/build-msi.yml`, so
packaging breakage surfaces on the branch that caused it. Those CI packages are versioned
`0.0.<run number>`, deliberately below every release — a run number as the major version
would eventually collide with a release version, and since `MajorUpgrade` keys off the
`UpgradeCode`, installing one after the other would fail with the downgrade error.

---

## 🧪 Running Tests

Unit tests are implemented with **xUnit** covering domain logic, stock policies, the pricing engine, the settlement policy and the payments ledger (recording, voiding, and the derived checkout flag against a real database file), the permission matrix, password hashing, money formatting, the data layer (the SQLite schema, the grid queries against a real database file, and the Persian-culture date ranges), and the provider selection behind dual-database mode.

The WPF-facing helpers are covered too. `Money` (grouping, parsing, the culture's own separator) is tested directly, and the price field's typing behaviour — caret tracking, and backspace with the caret just past a separator — is tested on a borrowed STA thread, because a window cannot be opened from a test run. The generated-grid price formatting is tested against a real `DataGrid` bound to a `DataTable`.

Run tests using the .NET CLI:

```bash
dotnet test
```

Expected output:

```text
Passed!  - Failed: 0, Passed: 304, Skipped: 0, Total: 304
```

---

## 🇮🇷 معرفی پروژه به زبان فارسی

<div dir="rtl" style="text-align: right;">

### نرم‌افزار مدیریت ارتباط با مشتری و فروش پیوند (<span dir="ltr">CRMPeyvand</span>)

نرم‌افزار **پیوند** یک سیستم جامع، ماژولار و بومی تحت دسکتاپ برای مدیریت امور مشتریان، صدور فاکتورهای فروش با تاریخ شمسی، انبارداری و کاتالوگ کالا و خدمات، ثبت فعالیت‌ها و پیگیری‌ها، ارسال پیامک‌های هوشمند و مدیریت سطوح دسترسی کاربران بر پایه معماری چندلایه <span dir="ltr">(.NET 10 / WPF / EF 6.5)</span> است.

#### ویژگی‌های کلیدی:
- **مدیریت پیشرفته مشتریان**: ثبت مشخصات، سوابق خرید، تاریخچه تعاملات و جستجوی لحظه‌ای.
- **کاتالوگ کالا و خدمات**: تفکیک کالای فیزیکی (دارای موجودی و کسر خودکار از انبار هنگام فروش) و خدمات، با اعمال قوانین دقیق کنترل موجودی.
- **صدور و مدیریت فاکتور**: محاسبه خودکار اقلام، اعمال کدهای تخفیف، تبدیل تاریخ به تقویم هجری شمسی و چاپ فاکتور استاندارد با موتور <span dir="ltr">QuestPDF</span> (چیدمان کدنویسی‌شده به‌جای فایل قالب، با پشتیبانی کامل راست‌به‌چپ و فونت فارسی).
- **نمایش خوانای مبالغ**: قیمت‌ها در هر جا که دیده می‌شوند — داخل فیلد ورودی، ستون جدول و جمع فاکتور — با جداکننده هزارگان نمایش داده می‌شوند و در زمان ذخیره، جداکننده‌ها حذف می‌شوند. واحد پول (تومان) روی فیلد قیمت درج شده است.
- **کدهای تخفیف**: ایجاد کدهای تخفیف درصدی و مبلغی با قابلیت تعیین سقف استفاده و بازه زمانی معتبر.
- **وصولی و مانده حساب**: ثبت پرداخت مشتری (کامل یا بخشی) بابت هر فاکتور، نگهداری مانده حساب هر مشتری و فاکتور، چاپ <span dir="ltr">رسید دریافت</span> و گزارش <span dir="ltr">مانده حساب</span> مشتریان. وضعیت «پرداخت شده» دیگر یک گزینه دستی نیست و از همین وصولی‌ها محاسبه می‌شود، بنابراین هرگز با فهرست وصولی‌ها اختلاف پیدا نمی‌کند. پرداخت بیش از مانده حساب پذیرفته نمی‌شود. وصولی قابل ویرایش نیست؛ اشتباه با «ابطال» و ثبت دوباره اصلاح می‌شود، چون ممکن است نسخه چاپ‌شده آن دست مشتری باشد. گزینه «وضعیت پرداخت» در صفحه فاکتور همچنان یک کلیک است، ولی اکنون یک وصولی واقعی در همان تراکنش ثبت می‌کند.
- **فعالیت‌ها و یادآورها**: ثبت تماس‌ها، جلسات و وظایف به تفکیک دسته‌بندی با اعلان و آلارم هوشمند.
- **سامانه پیامک**: ارتباط با وب‌سرویس <span dir="ltr">SmsIr</span> برای ارسال پیامک‌های خوش‌آمدگویی، صدور فاکتور و اطلاع‌رسانی انبوه.
- **سطوح دسترسی پیشرفته (<span dir="ltr">RBAC</span>)**: ماتریس دسترسی به تفکیک بخش‌ها (<span dir="ltr">Sections</span>) و عملیات (<span dir="ltr">View, Create, Edit, Delete</span>).
- **امنیت بالا**: هش‌کردن رمزهای عبور با الگوریتم قدرتمند <span dir="ltr">PBKDF2-SHA256</span> و ۱۰۰٬۰۰۰ تکرار به‌همراه <span dir="ltr">Salt</span> اختصاصی.
- **داشبورد آماری و نموداری**: نمایش نمودارهای زنده فروش، رشد مشتریان و آمارهای کلیدی با <span dir="ltr">LiveCharts</span>.
- **پشتیبان‌گیری و بازیابی پایگاه داده**: تهیه آسان نسخه پشتیبان از پایگاه داده و بازیابی درون‌برنامه‌ای. نسخه پشتیبان <span dir="ltr">SQLite</span> از طریق <span dir="ltr">API</span> آنلاین خودِ <span dir="ltr">SQLite</span> گرفته می‌شود، بنابراین کاربر می‌تواند هنگام تهیه نسخه به کار خود ادامه دهد؛ برای <span dir="ltr">SQL Server</span> از <span dir="ltr">BACKUP DATABASE</span> استفاده می‌شود.
- **پشتیبانی از دو پایگاه داده**: برنامه هم با <span dir="ltr">SQLite</span> (پیش‌فرض) و هم با <span dir="ltr">SQL Server</span> کار می‌کند و انتخاب از داخل خود برنامه (تنظیمات ← پیکربندی پایگاه داده) انجام می‌شود. رشته اتصال <span dir="ltr">SQL Server</span> حتی هنگام فعال بودن <span dir="ltr">SQLite</span> نگهداری می‌شود، بنابراین جابه‌جایی میان این دو بدون تایپ دوباره اطلاعات اتصال انجام می‌شود. آزمون اتصال در صفحه تنظیمات، دستور را از لایه <span dir="ltr">EF6</span> عبور می‌دهد تا تنظیمی که واقعاً کار نمی‌کند، موفق گزارش نشود.

</div>

---

## 📄 License

This project is open-source software licensed under the [MIT License](LICENSE).
