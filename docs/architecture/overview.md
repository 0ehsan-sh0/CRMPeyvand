# System Architecture Overview

## 1. Introduction & Purpose

**CRMPeyvand (پیوند CRM)** is an enterprise desktop Customer Relationship Management (CRM) and sales accounting software tailored for Persian-speaking businesses. It orchestrates customer relationships, inventory stock control, Jalali sales billing, receivables tracking, customer interaction history, automated SMS notifications, and role-based access management.

The software is engineered as a modern, maintainable Windows desktop application powered by **.NET 10**, **WPF (XAML)**, and **Entity Framework 6.5**.

---

## 2. Architectural Style: Decoupled N-Tier (Layered) Architecture

CRMPeyvand enforces a strict four-layer architecture with unidirectional references. This pattern isolates presentation concerns from business rules and abstract data storage.

```
┌────────────────────────────────────────────────────────┐
│                   Presentation Layer                   │
│             CRMPeyvand (WPF / XAML / MVVM)             │
│   HandyControl UI • LiveCharts • QuestPDF Document UX  │
└───────────────────────────┬────────────────────────────┘
                            │ depends on
                            ▼
┌────────────────────────────────────────────────────────┐
│                  Business Logic Layer                  │
│                        BLL                             │
│   Pure Domain Policies • RBAC AccessGuard • Pricing    │
└───────────────────────────┬────────────────────────────┘
                            │ depends on
                            ▼
┌────────────────────────────────────────────────────────┐
│                   Data Access Layer                    │
│                        DAL                             │
│    EF6 DbContext • Dual-Provider (SQLite / SQL Server)  │
│        Dynamic Factory Config • Schema Migrations      │
└───────────────────────────┬────────────────────────────┘
                            │ depends on
                            ▼
┌────────────────────────────────────────────────────────┐
│                    Business Entities                   │
│                        BE                              │
│       POCO Domain Models • Enums • System Matrix       │
└────────────────────────────────────────────────────────┘
```

### Layer Responsibilities & Rules

| Layer | Assembly | Primary Responsibility | Dependency Rules |
|---|---|---|---|
| **Presentation** | `CRMPeyvand.dll` | User interface, windows, controls, data bindings, user-input parsing, and document viewer rendering. | References `BLL`, `DAL` (for models/queries), and `BE`. |
| **Business Logic (BLL)** | `BLL.dll` | Business policies, validation logic, RBAC evaluation, password cryptography, and orchestration of operations across domains. | References `DAL` and `BE`. No UI or WPF dependencies. |
| **Data Access (DAL)** | `DAL.dll` | Persistence orchestration, database connections, schema provisioning, raw EF6 queries, and provider resolution. | References `BE` and Entity Framework / ADO.NET packages. Has no reference to BLL or Presentation. |
| **Business Entities (BE)** | `BE.dll` | Plain Old CLR Objects (POCO), core domain entities, enums (`Section`, `Operation`, `ItemKind`, `PaymentInstrument`). | Zero project dependencies. Completely portable. |
| **Unit & Integration Tests** | `CRMPeyvand.Tests.dll` | Verification test suite for domain policies, pricing formulas, RBAC decisions, and database operations. | References `BE`, `BLL`, `DAL`, and `CRMPeyvand`. |

---

## 3. Solution Map

```
CRMPeyvand.sln
├── BE/                                    # Business Entities (Domain Models)
│   ├── AccessGrant.cs                     # RBAC permission grant
│   ├── Activity.cs                        # Customer interaction log
│   ├── ActivityCategory.cs                # Interaction categorization
│   ├── CatalogItem.cs                     # Goods & Services entity
│   ├── Customer.cs                        # Client profile entity
│   ├── Invoice.cs                         # Sales invoice header
│   ├── InvoiceLine.cs                     # Itemized invoice line item
│   ├── ItemKind.cs                        # Good vs Service enum
│   ├── Operation.cs                       # CRUD Operation enum (View, Create, Edit, Delete)
│   ├── Payment.cs                         # Financial receipt record
│   ├── PaymentInstrument.cs               # Tender instrument enum
│   ├── Section.cs                         # Protected application section enum
│   ├── User.cs                            # Employee login identity
│   └── UserGroup.cs                       # Role/Group entity
│
├── BLL/                                   # Business Logic Layer
│   ├── AccessGuard.cs                     # User permission checker
│   ├── AccessDecision.cs                  # Pure RBAC decision evaluator
│   ├── PasswordHasher.cs                  # Salted PBKDF2-SHA256 hasher
│   ├── Pricing.cs                         # Pure discount and payable arithmetic
│   ├── StockPolicy.cs                     # Inventory deduction and validation
│   ├── SettlementPolicy.cs                # Payment validation and balance rules
│   └── *BLL.cs                            # Domain service orchestrators
│
├── DAL/                                   # Data Access Layer
│   ├── DB.cs                              # Unified EF6 DbContext
│   ├── DataSource.cs                      # Provider configuration & runtime switch
│   ├── DataFolder.cs                      # Application storage path resolver
│   ├── SqliteSchema.cs                    # SQLite schema generator & DDL
│   ├── BalanceQuery.cs                    # Customer debt calculation projections
│   ├── SettingDAL.cs                      # Online backup and restore procedures
│   └── *DAL.cs                            # Data access repositories
│
├── CRMPeyvand/                            # Presentation Layer (WPF)
│   ├── App.xaml / App.xaml.cs             # Application lifecycle, culture bootstrap
│   ├── MainWindow.xaml / .cs              # Main dashboard host
│   ├── Money.cs                           # Rial ↔ Toman conversion & formatting
│   ├── PersianCulture.cs                  # Persian solar calendar provider
│   ├── PriceField.cs                      # Live comma-grouping price input handler
│   ├── *Form.xaml / .cs                   # WPF dialogs and management forms
│   └── Reports/                           # QuestPDF vector document definitions
│       ├── Common/                        # PersianReportStyle, layout constants
│       ├── Documents/                     # IDocument layout implementations
│       ├── Models/                        # Typed report view models
│       └── Services/                      # Factory mappers & viewer service
│
├── CRMPeyvand.Tests/                      # xUnit Test Suite
│   ├── AccessDecisionTests.cs
│   ├── PasswordHasherTests.cs
│   ├── PricingTests.cs
│   ├── SettlementPolicyTests.cs
│   ├── StockPolicyTests.cs
│   └── ...
│
├── docs/                                  # Project Documentation
│   ├── architecture/                      # Modular architecture specs (this suite)
│   ├── adr/                               # Architecture Decision Records
│   ├── archive/plans/                     # Completed implementation plans
│   └── releases/                          # Release notes
│
└── installer/                             # WiX Toolset v5 MSI packaging
```

---

## 4. Key Cross-Cutting Design Concepts

### 1. Dual-Currency Representation (Rial vs. Toman)
- **Database & External Documents**: Always **Rial (ریال)**. This guarantees exact integer accounting representation without floating-point inaccuracies. Official QuestPDF printed reports and receipts output figures in Rial.
- **User Interface**: Always **Toman (تومان)**. Standard Persian commercial usage represents prices in Toman (1 Toman = 10 Rial). The presentation layer (`Money.cs`) automatically displays `amount / 10` using floor division to prevent showing fractional currency.

### 2. Jalali (Persian Solar) Calendar System
The application runs under a custom `PersianCulture` initialized at startup in `App.xaml.cs`. Invoice dates, customer registration stamps, reminder intervals, and activity logs operate natively on the Solar Hijri calendar with RTL text alignment throughout the UI.

### 3. Dual-Database Flexibility
The application defaults to an embedded zero-configuration SQLite database (`CRMPeyvand.db`) stored under `%ProgramData%\CRMPeyvand`. Businesses requiring shared multi-user local area network access can switch to Microsoft SQL Server directly from the in-app settings screen without reinstalling the software.
