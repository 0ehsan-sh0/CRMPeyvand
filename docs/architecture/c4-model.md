# C4 Architectural Model & Dynamic Interaction Diagrams

This document details the architectural structure of **CRMPeyvand** following the **C4 Model** (Context, Containers, Components) and provides dynamic Mermaid sequence diagrams illustrating core runtime workflows.

---

## 1. C4 Level 1: System Context Diagram

The System Context diagram illustrates how CRMPeyvand fits into the overall operational environment, depicting users and external dependencies.

```mermaid
C4Context
    title System Context Diagram for CRMPeyvand

    Person(employee, "Employee / Cashier", "Operates the CRM to manage customers, invoices, catalog items, and payments.")
    Person(admin, "System Administrator", "Configures user groups, access permissions, database settings, and system backups.")

    System(crmPeyvand, "CRMPeyvand Desktop Application", "Provides CRM workflows, Persian sales invoicing, receivables management, reporting, and access control.")

    SystemDb(sqliteDb, "Local SQLite Database", "Embedded single-file database (CRMPeyvand.db) stored in %ProgramData% for standalone deployments.")
    SystemDb(sqlServerDb, "Enterprise SQL Server", "Remote/network Microsoft SQL Server for multi-workstation client-server deployments.")
    System_Ext(smsGateway, "IPE SmsIr SMS Gateway", "External REST API for transactional and bulk Persian SMS notifications.")

    Rel(employee, crmPeyvand, "Uses", "WPF Desktop GUI (RTL / Persian)")
    Rel(admin, crmPeyvand, "Administers", "WPF Desktop GUI")
    Rel(crmPeyvand, sqliteDb, "Reads/Writes", "System.Data.SQLite (Default Provider)")
    Rel(crmPeyvand, sqlServerDb, "Reads/Writes", "System.Data.SqlClient (Opt-in Provider)")
    Rel(crmPeyvand, smsGateway, "Sends SMS alerts", "HTTPS / REST API")
```

---

## 2. C4 Level 2: Container Diagram

The Container diagram zooms into the CRMPeyvand desktop executable and shows its internal boundary partitions and external integration points.

```mermaid
C4Container
    title Container Diagram for CRMPeyvand

    Person(user, "User / Employee", "Interacts with forms, tables, and reports")

    Container_Boundary(app_boundary, "CRMPeyvand Desktop (.NET 10 WPF Application)") {
        Container(presentation, "Presentation Layer", "WPF, XAML, HandyControl, LiveCharts", "Renders views, captures input, manages price grouping and view models.")
        Container(bll, "Business Logic Layer (BLL)", "C# Class Library", "Enforces pure business rules, pricing, settlement constraints, and security guards.")
        Container(dal, "Data Access Layer (DAL)", "EF 6.5.1, ADO.NET", "Orchestrates database connections, schema provisioning, queries, and backup snapshots.")
        Container(be, "Business Entities (BE)", "C# Class Library", "Defines domain POCOs, enums, and access grants.")
        Container(reporting, "Reporting Subsystem", "QuestPDF 2024", "Renders strongly-typed Persian PDF documents and receipts.")
    }

    SystemDb(localDb, "Local SQLite Database", "SQLite File", "Default zero-config local storage.")
    SystemDb(serverDb, "Microsoft SQL Server", "SQL Server DB", "Optional enterprise multi-user database.")
    System_Ext(smsApi, "IPE SMS API", "External HTTP Service", "SMS dispatch service.")

    Rel(user, presentation, "Interacts with", "WPF GUI")
    Rel(presentation, bll, "Calls business operations", "In-process C# Calls")
    Rel(presentation, reporting, "Invokes report generation", "In-process C# Calls")
    Rel(bll, dal, "Requests data persistence", "In-process C# Calls")
    Rel(bll, be, "Operates on", "Domain Entities")
    Rel(dal, be, "Maps records to", "Domain Entities")
    Rel(dal, localDb, "Executes queries via EF6/SQLite", "Direct File I/O")
    Rel(dal, serverDb, "Executes queries via EF6/SqlClient", "TDS Protocol / TCP")
    Rel(bll, smsApi, "Dispatches SMS messages", "HTTPS REST API")
```

---

## 3. C4 Level 3: Component Diagram (Sales & Receivables Subsystem)

This diagram breaks down the sales, invoicing, and receivables management subsystem into its collaborating classes.

```mermaid
flowchart TD
    subgraph UI ["Presentation Layer (WPF)"]
        InvoiceForm["InvoiceForm.xaml.cs<br/>(Sales Billing)"]
        PaymentsForm["PaymentsForm.xaml.cs<br/>(Receivables Ledger)"]
        MoneyHelper["Money.cs<br/>(Toman ↔ Rial Conversion)"]
    end

    subgraph BusinessLogic ["Business Logic Layer (BLL)"]
        InvoiceBLL["InvoiceBLL<br/>(Invoice Lifecycle)"]
        PaymentBLL["PaymentBLL<br/>(Payment Orchestration)"]
        Pricing["Pricing.cs<br/>(Discount & Payable Calculation)"]
        StockPolicy["StockPolicy.cs<br/>(Inventory Availability Rules)"]
        SettlementPolicy["SettlementPolicy.cs<br/>(Overpayment & Date Rules)"]
    end

    subgraph DataAccess ["Data Access Layer (DAL)"]
        InvoiceDAL["InvoiceDAL<br/>(Invoice Persistence)"]
        PaymentDAL["PaymentDAL<br/>(Payment Ledger Queries)"]
        BalanceQuery["BalanceQuery.cs<br/>(Derived Balance Projections)"]
        DbContext["DB : DbContext<br/>(EF6 Data Context)"]
    end

    subgraph Storage ["Database Engine"]
        StorageEngine[("SQLite / SQL Server")]
    end

    InvoiceForm -->|Calculates Totals| MoneyHelper
    InvoiceForm -->|Submits Sale| InvoiceBLL
    PaymentsForm -->|Records Receipt| PaymentBLL

    InvoiceBLL -->|Evaluates Discounts| Pricing
    InvoiceBLL -->|Checks Stock| StockPolicy
    InvoiceBLL -->|Persists Document| InvoiceDAL

    PaymentBLL -->|Validates Amount & Date| SettlementPolicy
    PaymentBLL -->|Records Transaction| PaymentDAL

    InvoiceDAL -->|Saves Invoices & Lines| DbContext
    PaymentDAL -->|Saves Payments| DbContext
    PaymentDAL -->|Computes Balances| BalanceQuery
    BalanceQuery -->|Queries Invoices & Payments| DbContext

    DbContext --> StorageEngine
```

---

## 4. Dynamic Runtime Workflows

### 4.1 First Run & Administrator Seeding Workflow

On first launch, CRMPeyvand detects whether any user account exists. If none are found, it guides the operator through initial system administrator onboarding.

```mermaid
sequenceDiagram
    autonumber
    actor Admin as System Administrator
    participant Login as LoginForm / App
    participant UserBLL as UserBLL
    participant UserDAL as UserDAL
    participant DB as DB (DbContext)
    participant Hasher as PasswordHasher

    Admin->>Login: Launch CRMPeyvand.exe
    Login->>UserBLL: HasAnyUser()
    UserBLL->>UserDAL: HasAnyUser()
    UserDAL->>DB: Check Users.Any()
    DB-->>UserDAL: Returns false (Empty Database)
    UserDAL-->>UserBLL: false
    UserBLL-->>Login: false

    Note over Login: No users exist.<br/>Display RegisterUC (First Run Mode)

    Admin->>Login: Submit Admin Credentials (Username, Password, Name)
    Login->>Hasher: Hash(password)
    Hasher-->>Login: "pbkdf2-sha256:100000:<salt>:<key>"
    Login->>UserBLL: RegisterFirstUser(adminUser)
    UserBLL->>UserDAL: CreateUser(adminUser)

    Note over UserDAL,DB: Seeds "مدیریت" (Built-in Administrator Group)<br/>with full AccessGrants across all Sections & Operations.<br/>IsBuiltIn = true.

    UserDAL->>DB: SaveChanges()
    DB-->>UserDAL: Saved
    UserDAL-->>UserBLL: Success
    UserBLL-->>Login: Success
    Login->>Admin: Navigate to MainWindow
```

---

### 4.2 Invoice Issuance & Payment Settlement Workflow

This sequence demonstrates issuing an invoice, decreasing inventory, and settling the balance authoritatively.

```mermaid
sequenceDiagram
    autonumber
    actor Cashier as Cashier / Sales Rep
    participant Form as InvoiceForm
    participant BLL as InvoiceBLL
    participant Stock as StockPolicy
    participant PricingEngine as Pricing
    participant Settle as SettlementPolicy
    participant DAL as InvoiceDAL
    participant DB as DB (DbContext)

    Cashier->>Form: Add Goods and Services to Invoice
    Form->>PricingEngine: ComputeDiscount(offCode, subTotal)
    PricingEngine-->>Form: discountAmount
    Form->>PricingEngine: ComputePayable(subTotal, discountAmount)
    PricingEngine-->>Form: payableAmount

    Cashier->>Form: Click Finalize Invoice (Optional Settle Checkbox Checked)
    Form->>BLL: Create(invoice, settleImmediately)

    loop For each physical Good
        BLL->>Stock: Validate(catalogItem, quantity)
        Stock-->>BLL: Valid (Stock >= Quantity)
    end

    alt Settle Immediately is Checked
        BLL->>Settle: ValidateRecording(payableAmount, payableAmount, today, today)
        Settle-->>BLL: Valid
    end

    BLL->>DAL: Create(invoice, settleImmediately)

    critical Database Transaction
        DAL->>DB: BeginTransaction()
        DAL->>DB: Invoices.Add(invoice)
        loop Decrement Stock for physical goods
            DAL->>DB: item.Stock -= line.Quantity
        end
        alt Settle Immediately
            DAL->>DB: Payments.Add(new Payment { Amount = payable, Instrument = Cash })
            DAL->>DB: invoice.IsCheckout = true
        end
        DAL->>DB: SaveChanges()
        DAL->>DB: Commit()
    end

    DB-->>DAL: Transaction Committed
    DAL-->>BLL: Success
    BLL-->>Form: Invoice Created Successfully
    Form->>Cashier: Show Success & Offer QuestPDF Printout
```

---

### 4.3 Dual-Database Provider Initialization & Fallback Workflow

CRMPeyvand determines the active database engine at startup through `DataSource.Current`. If the configured provider encounters an irrecoverable error, it fails gracefully.

```mermaid
sequenceDiagram
    autonumber
    participant App as Application Boot
    participant DS as DataSource
    participant DB as DB (Static Initializer)
    participant SqliteSchema as SqliteSchema
    participant Storage as File / SQL Server

    App->>DS: DataSource.Current
    DS->>Storage: Read provider.json (under %ProgramData%\CRMPeyvand)

    alt provider.json exists and Kind == SqlServer
        Storage-->>DS: Configured SQL Server Connection String
        DS-->>App: DataSource (Kind = SqlServer)
        App->>DB: Instantiates DB()
        Note over DB: Installs MigrateDatabaseToLatestVersion initializer.<br/>EF6 opens connection and migrates SQL Server schema.
    else provider.json is missing, corrupt, or Kind == Sqlite (Default)
        Note over DS: Fallback to DefaultSqlite (Zero-Config)
        DS-->>App: DataSource (Kind = Sqlite, file = CRMPeyvand.db)
        App->>DB: Instantiates DB()
        DB->>SqliteSchema: Ensure(sqliteConnection)
        SqliteSchema->>Storage: Execute CREATE TABLE IF NOT EXISTS DDL
        Storage-->>SqliteSchema: Schema Ready
        Note over DB: Returns open SQLite connection with schema verified.
    end
```
