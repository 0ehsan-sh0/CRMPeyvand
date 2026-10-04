# Payments and Receivables Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let the business record money received from customers against invoices, see what each customer still owes, and print a receipt — replacing the invoice's boolean "paid" flag with a real payments ledger.

**Architecture:** A new `Payment` entity records one amount against exactly one `Invoice`, in its own transaction that also rewrites `Invoice.IsCheckedout` so the two can never disagree. Payment validation is a pure static policy in BLL, injected into the DAL as a delegate because `DAL` cannot reference `BLL`. Money owed is always derived (`Invoice.Balance`, `Customer.Balance`), never stored. A new «دریافت‌ها» screen is the ledger; the invoice screen's checkbox and a context-menu shortcut remain the fast paths for settling at the counter.

**Tech Stack:** .NET 10, WPF + XAML (RTL, HandyControl 3.5.3), Entity Framework 6 over SQLite (default) or SQL Server, QuestPDF 2024.12.1, xUnit 2.9.2 on VSTest.

**Spec:** This plan is the spec. `CONTEXT.md` (Receivables vocabulary) and `docs/adr/0009-payments-authoritative-over-checkout-flag.md` (why the flag is derived) are part of it and are already written.

---

## Global Constraints

- Every code sample is verbatim C# for this repository. Match its idioms: DAL/BLL mutators **return a Persian `string`** for success and failure alike; policy failures **throw `InvalidOperationException`** with a Persian message which the form catches. There is no `Result` type anywhere.
- Reference direction is `CRMPeyvand → BLL → DAL → BE`. **`DAL` must never reference `BLL`.** Business rules cross that boundary as `Func<>` delegates, exactly as `InvoiceDAL.Create` already does for `StockPolicy.Validate` and `Pricing.ComputeDiscount`.
- Every DAL class exposes two constructors: a parameterless one, and `/// <summary>Tests supply their own context.</summary> XxxDAL(DB db)`. Tests only ever use the second.
- Tests write to a **real throwaway SQLite file** through `SqliteTestDb.WithDb(db => { ... })`, seed via `db`, and assert via a DAL built on that same `db`. **BLL classes are never instantiated in tests** — their field initialisers open the process-wide database.
- Any test class using `SqliteTestDb` must carry `[Collection(DataSourceCollection.Name)]` and sit in namespace `CRMPeyvand.Tests`. Not optional: a DAL built with the parameterless overload creates `C:\ProgramData\CRMPeyvand\CRMPeyvand.db` on the developer's machine.
- Grid queries follow the `GridTable` contract: a `private static readonly string[] ReadColumns`, a private `…Rows()` returning `List<object[]>`, and `.Where(DeleteStatus == false).OrderByDescending(id).Take(GridTable.DefaultRowLimit).ToList()` **before** `.Select(object[])`, then `GridTable.Build`. `ReadColumns` entries **are** the visible headers.
- Text filtering is `GridTable.Matches(Filter, …)` applied **after** materialisation. `string.Contains` inside a LINQ-to-Entities `Where` compiles to `CHARINDEX`, which SQLite lacks.
- Money is `decimal` in BE and DAL and `DECIMAL(18,2)` in the database. Money is `double` in **report models** — that is the report convention and must be matched, with the cast done in the factory.
- Money entry goes through `Money.ParseWhole`, which returns `int?` (`null` when empty or out of range); display through `Money.Display`. Grid money columns are named in the `params string[] moneyColumns` overload of `PublicMethods.dgvFiller`.
- Plain numeric filters use `PublicMethods.FilterNumber`; grouped amount fields use `PriceField.GroupAsTyped` (TextChanged) plus `PriceField.Backspace` (PreviewKeyDown), as `ProductForm`'s price field does.
- Forms are a `Window` with `ResizeMode="NoResize" WindowStyle="None" WindowStartupLocation="CenterScreen" Background="Transparent" AllowsTransparency="True" FlowDirection="RightToLeft"`, a 12-column/12-row `Grid`, and the styles `GroupBox`, `TextBoxLabel`, `InputTextBoxBorder`, `InputTextBox`, `ButtonBorder`, `Button`, `DataGridBorder`, `DataGrid`, `ContextMenu`, `MenuItem`, `BackToHomeImage`.
- The logged-in user is **never passed to a form**. Read it in `Window_Loaded`: `MainWindow w = (MainWindow)Application.Current.MainWindow; u = w.loggedInUser;`
- Any file calling `AccessGuard` needs `using Section = BE.Section;` because `Section` collides.
- Screens open modally through MainWindow's private `void openform(Window f)`, always followed by `RefreshPage()`.
- `Escape` closes a form via `Window_PreviewKeyDown`.
- All user-facing strings are Persian, including the `"… با موفقیت انجام شد"` / `"… با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message` result convention.
- Run tests with `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`. The suite is VSTest, not Microsoft.Testing.Platform.
- Commit after every task. Never amend a failed commit; fix and make a new one.

---

## File Structure

**New files**

| Path | Responsibility |
| --- | --- |
| `BE/PaymentInstrument.cs` | The enum: how a Payment was tendered. |
| `BE/PaymentInstrumentTitles.cs` | The single Persian-caption map for that enum, visible to both DAL and UI. |
| `BE/Payment.cs` | The Payment entity: one amount against one Invoice, one User, soft delete. |
| `DAL/SettlementFlag.cs` | The only writer of `Invoice.IsCheckedout` and `CheckoutDate`. |
| `DAL/PaymentDAL.cs` | Record/void payments, the payment grid, per-customer balances. |
| `BLL/SettlementPolicy.cs` | Pure validation for recording a payment. |
| `BLL/PaymentBLL.cs` | BLL facade over `PaymentDAL`, injecting `SettlementPolicy`. |
| `CRMPeyvand/PaymentsForm.xaml` + `.xaml.cs` | The «دریافت‌ها» ledger screen and payment entry. |
| `CRMPeyvand/Reports/Models/PaymentReportModel.cs` | Report model for one رسید دریافت. |
| `CRMPeyvand/Reports/Models/CustomerBalanceReportModel.cs` | Report model for the مانده حساب statement. |
| `CRMPeyvand/Reports/Services/PaymentReportModelFactory.cs` | `Payment → PaymentReportModel`. |
| `CRMPeyvand/Reports/Documents/PaymentReceiptDocument.cs` | QuestPDF `IDocument` for the receipt. |
| `CRMPeyvand/Reports/Documents/CustomerBalanceDocument.cs` | QuestPDF `IDocument` for the statement. |
| `CRMPeyvand.Tests/SettlementPolicyTests.cs` | Tests for the pure policy. |
| `CRMPeyvand.Tests/InvoiceBalanceTests.cs` | Tests for the derived money. |
| `CRMPeyvand.Tests/PaymentDALTests.cs` | Record/void/grid/statement against real SQLite. |
| `CRMPeyvand.Tests/DashboardDALTests.cs` | The debtor count. |
| `CRMPeyvand.Tests/Reporting/PaymentReportTests.cs` | Receipt and statement document tests. |
| `DAL/Migrations/202610041200000_AddPayments.cs` + `.Designer.cs` | EF6 migration for the SQL Server path. No `.resx` — see Step 6. |

**Modified files**

| Path | Change |
| --- | --- |
| `BE/Section.cs` | Add `Payments = 11`. |
| `BE/Invoice.cs` | Add the `Payments` collection and `[NotMapped] Paid`, `Balance`, `IsSettled`. |
| `BE/Customer.cs` | Add `[NotMapped] PayableTotal`, `PaidTotal`, `Balance`. |
| `DAL/DB.cs` | Add `DbSet<Payment> Payments`. |
| `DAL/SqliteSchema.cs` | Add the `Payments` table and its index to `Ddl`. |
| `DAL/InvoiceDAL.cs` | Delete `Done`; take a `Payment` for settle-at-the-counter; `Include("Payments")`; nine grid columns. |
| `DAL/CustomerDAL.cs` | Add the «مانده حساب» column and the `Include`s it needs. |
| `DAL/DashboardDAL.cs` | Add `DebtorCustomerCount`. |
| `CRMPeyvand/CustomerForm.xaml.cs` | Group the new «مانده حساب» grid column. |
| `BLL/InvoiceBLL.cs` | `Create` gains the settle parameter; remove `Done`. |
| `BLL/DashboardBLL.cs` | Add `DebtorCustomerCount`. |
| `CRMPeyvand/InvoiceForm.xaml` + `.xaml.cs` | Money column names; the checkbox builds a `Payment`; the menu item opens the payments screen. |
| `CRMPeyvand/InvoiceDetailsForm.xaml` + `.xaml.cs` | Two new totals labels. |
| `CRMPeyvand/MainWindow.xaml` + `.xaml.cs` | Sidebar entry on row 10, `EnterV`, `case Key.V:`. The dashboard's fourth card is Task 9. |
| `CRMPeyvand/UsersForm.xaml.cs` | Persian caption for the new Section. |
| `CRMPeyvand/ReportsWindow.xaml` + `.xaml.cs` | Statement radio button and branch. |
| `CRMPeyvand/Reports/Models/InvoiceReportModel.cs` | `PaidAmount`, `RemainingBalance`. |
| `CRMPeyvand/Reports/Services/InvoiceReportModelFactory.cs` | Fill them. |
| `CRMPeyvand/Reports/Documents/InvoiceDocument.cs` | Print them. |
| `CRMPeyvand.Tests/SqliteSchemaTests.cs` | Add `Payments` to the expected table list; add a round-trip test. |
| `CRMPeyvand.Tests/GridQueriesTests.cs` | Update the invoice and customer column assertions. |
| `CRMPeyvand.Tests/InvoiceReportModelFactoryTests.cs` | Cover the new fields. |
| `README.md` | Feature list and Section list. |

---

### Task 1: The domain — Payment, Instrument, and the derived money

The entity, the enum, the Section, and the derived balances. Nothing persists yet.

**Files:**
- Create: `BE/Payment.cs`, `BE/PaymentInstrument.cs`, `BE/PaymentInstrumentTitles.cs`
- Modify: `BE/Invoice.cs`, `BE/Customer.cs`, `BE/Section.cs`
- Test: `CRMPeyvand.Tests/InvoiceBalanceTests.cs`

**Interfaces:**
- Consumes: nothing.
- Produces:
  - `BE.PaymentInstrument` — `Cash = 1, Card = 2, Transfer = 3, Cheque = 4, Other = 5`
  - `BE.PaymentInstrumentTitles.Of(PaymentInstrument) → string`
  - `BE.Payment` — `int Id`, `decimal Amount`, `DateTime RegDate`, `PaymentInstrument Instrument`, `string Reference`, `bool DeleteStatus`, `Invoice Invoice`, `User User`
  - `BE.Invoice.Payments` (`List<Payment>`, initialised empty), `BE.Invoice.Paid`, `.Balance`, `.IsSettled` (all `[NotMapped]`)
  - `BE.Customer.PayableTotal`, `.PaidTotal`, `.Balance` (all `[NotMapped]`)
  - `BE.Section.Payments = 11`

- [ ] **Step 1: Write the failing tests**

Create `CRMPeyvand.Tests/InvoiceBalanceTests.cs`:

```csharp
using BE;
using Xunit;

namespace CRMPeyvand.Tests
{
    public class InvoiceBalanceTests
    {
        private static Invoice InvoiceWithPayable(decimal payable)
        {
            var invoice = new Invoice { DiscountAmount = 0m };
            invoice.Lines.Add(new InvoiceLine { Quantity = 1, UnitPrice = payable });
            return invoice;
        }

        [Fact]
        public void An_invoice_with_no_payments_owes_everything()
        {
            var invoice = InvoiceWithPayable(5000m);

            Assert.Equal(0m, invoice.Paid);
            Assert.Equal(5000m, invoice.Balance);
            Assert.False(invoice.IsSettled);
        }

        [Fact]
        public void A_partial_payment_leaves_the_remainder_owed()
        {
            var invoice = InvoiceWithPayable(5000m);
            invoice.Payments.Add(new Payment { Amount = 2000m });

            Assert.Equal(2000m, invoice.Paid);
            Assert.Equal(3000m, invoice.Balance);
            Assert.False(invoice.IsSettled);
        }

        [Fact]
        public void Payments_covering_the_payable_settle_the_invoice()
        {
            var invoice = InvoiceWithPayable(5000m);
            invoice.Payments.Add(new Payment { Amount = 2000m });
            invoice.Payments.Add(new Payment { Amount = 3000m });

            Assert.Equal(5000m, invoice.Paid);
            Assert.Equal(0m, invoice.Balance);
            Assert.True(invoice.IsSettled);
        }

        [Fact]
        public void A_voided_payment_stops_counting_towards_the_paid_total()
        {
            var invoice = InvoiceWithPayable(5000m);
            invoice.Payments.Add(new Payment { Amount = 2000m });
            invoice.Payments.Add(new Payment { Amount = 3000m, DeleteStatus = true });

            Assert.Equal(2000m, invoice.Paid);
            Assert.Equal(3000m, invoice.Balance);
        }

        [Fact]
        public void Balance_never_goes_negative_even_if_payments_overshoot()
        {
            var invoice = InvoiceWithPayable(5000m);
            invoice.Payments.Add(new Payment { Amount = 9000m });

            Assert.Equal(9000m, invoice.Paid);
            Assert.Equal(0m, invoice.Balance);
            Assert.True(invoice.IsSettled);
        }

        [Fact]
        public void Balance_uses_the_discounted_payable_not_the_subtotal()
        {
            var invoice = InvoiceWithPayable(10000m);
            invoice.DiscountAmount = 4000m;

            Assert.Equal(6000m, invoice.Balance);
        }

        [Fact]
        public void Customer_balance_sums_only_their_live_invoices()
        {
            var customer = new Customer();

            var live = new Invoice { DiscountAmount = 0m };
            live.Lines.Add(new InvoiceLine { Quantity = 1, UnitPrice = 8000m });
            live.Payments.Add(new Payment { Amount = 3000m });

            var deleted = new Invoice { DeleteStatus = true, DiscountAmount = 0m };
            deleted.Lines.Add(new InvoiceLine { Quantity = 1, UnitPrice = 100000m });

            customer.Invoices.Add(live);
            customer.Invoices.Add(deleted);

            Assert.Equal(8000m, customer.PayableTotal);
            Assert.Equal(3000m, customer.PaidTotal);
            Assert.Equal(5000m, customer.Balance);
        }

        [Fact]
        public void Instrument_captions_are_the_words_the_grid_and_the_receipt_share()
        {
            Assert.Equal("نقد", PaymentInstrumentTitles.Of(PaymentInstrument.Cash));
            Assert.Equal("کارت", PaymentInstrumentTitles.Of(PaymentInstrument.Card));
            Assert.Equal("حواله", PaymentInstrumentTitles.Of(PaymentInstrument.Transfer));
            Assert.Equal("چک", PaymentInstrumentTitles.Of(PaymentInstrument.Cheque));
            Assert.Equal("سایر", PaymentInstrumentTitles.Of(PaymentInstrument.Other));
        }

        [Fact]
        public void An_unknown_instrument_number_reads_as_unspecified_rather_than_throwing()
        {
            Assert.Equal("نامشخص", PaymentInstrumentTitles.Of((PaymentInstrument)99));
        }

        [Fact]
        public void A_new_payment_starts_uncashed_and_current()
        {
            var payment = new Payment();

            Assert.Equal(PaymentInstrument.Cash, payment.Instrument);
            Assert.False(payment.DeleteStatus);
        }
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~InvoiceBalanceTests`
Expected: compile failure — `BE.Payment`, `BE.PaymentInstrument` and `BE.PaymentInstrumentTitles` do not exist.

- [ ] **Step 3: Add the enum**

Create `BE/PaymentInstrument.cs`:

```csharp
namespace BE
{
    public enum PaymentInstrument
    {
        Cash = 1, Card = 2, Transfer = 3, Cheque = 4, Other = 5
    }
}
```

- [ ] **Step 4: Add the single Persian caption map**

Create `BE/PaymentInstrumentTitles.cs`:

```csharp
namespace BE
{
    /// <summary>
    /// Persian captions for PaymentInstrument.
    ///
    /// The grid row and the printed receipt must call one instrument by one word,
    /// and BE is the only assembly both the DAL (which builds grid rows) and the
    /// UI (which builds the receipt) can see. A switch rather than a dictionary
    /// because the enum is closed, so a missing case shows up in review instead of
    /// rendering blank.
    /// </summary>
    public static class PaymentInstrumentTitles
    {
        public static string Of(PaymentInstrument instrument)
        {
            switch (instrument)
            {
                case PaymentInstrument.Cash: return "نقد";
                case PaymentInstrument.Card: return "کارت";
                case PaymentInstrument.Transfer: return "حواله";
                case PaymentInstrument.Cheque: return "چک";
                case PaymentInstrument.Other: return "سایر";
                default: return "نامشخص";
            }
        }
    }
}
```

- [ ] **Step 5: Add the entity**

Create `BE/Payment.cs`:

```csharp
namespace BE
{
    public class Payment
    {
        public Payment()
        {
            DeleteStatus = false;
            Instrument = PaymentInstrument.Cash;
            RegDate = DateTime.Now;
        }

        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime RegDate { get; set; }
        public PaymentInstrument Instrument { get; set; }
        public string Reference { get; set; }
        public bool DeleteStatus { get; set; }

        public Invoice Invoice { get; set; }
        public User User { get; set; }
    }
}
```

`RegDate` defaults to now in the constructor because `BE/Message.cs` and `BE/MessagePanel.cs` already do that; the form overwrites it with the date picker's value.

- [ ] **Step 6: Add the derived money to Invoice**

In `BE/Invoice.cs`, add the collection beside `Lines`:

```csharp
        public Customer Customer { get; set; }
        public User User { get; set; }
        public List<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
        public List<Payment> Payments { get; set; } = new List<Payment>();
```

Then add these below the existing `[NotMapped]` block:

```csharp
        /// <summary>
        /// Money actually received against this invoice, ignoring voided payments:
        /// a void is a soft delete, so the flag alone is not enough.
        ///
        /// Read paths use this. The write path in PaymentDAL recomputes the same
        /// figure as a SQL aggregate, because a payment being written is not yet in
        /// this collection.
        /// </summary>
        [NotMapped] public decimal Paid =>
            (Payments ?? new List<Payment>()).Where(p => !p.DeleteStatus).Sum(p => p.Amount);

        [NotMapped] public decimal Balance => Math.Max(0m, Payable - Paid);

        [NotMapped] public bool IsSettled => Balance <= 0m;
```

`BE/Invoice.cs` already has `using System.ComponentModel.DataAnnotations.Schema;`, `using System.Collections.Generic;`, `using System.Linq;` and `using System;`.

- [ ] **Step 7: Add the derived balance to Customer**

In `BE/Customer.cs`, add `using System.ComponentModel.DataAnnotations.Schema;` as the first using (the file starts with `using System;`), then add after the two collection properties:

```csharp
        /// <summary>
        /// What this customer still owes across their live invoices.
        ///
        /// All three walk Invoices, so any query reading them must Include the
        /// invoices' Lines and Payments too — EF6 does not lazy load. See
        /// CustomerDAL.Read and PaymentDAL.ReadCustomerBalances.
        /// </summary>
        [NotMapped] public decimal PayableTotal =>
            Invoices.Where(i => !i.DeleteStatus).Sum(i => i.Payable);

        [NotMapped] public decimal PaidTotal =>
            Invoices.Where(i => !i.DeleteStatus).Sum(i => i.Paid);

        [NotMapped] public decimal Balance => Math.Max(0m, PayableTotal - PaidTotal);
```

- [ ] **Step 8: Add the Section**

In `BE/Section.cs`, change the body to:

```csharp
    public enum Section
    {
        Customers = 1, CatalogItems = 2, Invoices = 3, Activities = 4, Reminders = 5,
        Users = 6, SmsPanel = 7, Reports = 8, Settings = 9, Discounts = 10, Payments = 11
    }
```

Nothing else is needed for a new Section: `RegisterUC.CreateAdminGroup` seeds by reflecting over `Enum.GetValues(typeof(Section))`, `UsersForm` builds its matrix from the same reflection, and `AccessDecision.Evaluate` short-circuits `true` for the built-in group, so no existing install is locked out. The Persian caption is Task 6.

- [ ] **Step 9: Run the tests to verify they pass**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~InvoiceBalanceTests`
Expected: PASS, 10 tests.

- [ ] **Step 10: Commit**

```bash
git add BE/Payment.cs BE/PaymentInstrument.cs BE/PaymentInstrumentTitles.cs BE/Invoice.cs BE/Customer.cs BE/Section.cs CRMPeyvand.Tests/InvoiceBalanceTests.cs
git commit -m "feat(receivables): add the Payment entity and the derived balances"
```

---

### Task 2: Persistence — DbSet, SQLite DDL, and the SQL Server migration

**Files:**
- Modify: `DAL/DB.cs`, `DAL/SqliteSchema.cs`, `CRMPeyvand.Tests/SqliteSchemaTests.cs`
- Create: `DAL/Migrations/202610041200000_AddPayments.cs`, `DAL/Migrations/202610041200000_AddPayments.Designer.cs`, `DAL/Migrations/202610041200000_AddPayments.resx`

**Interfaces:**
- Consumes: `BE.Payment` (Task 1).
- Produces: a `Payments` table on both providers, and `db.Payments` on the context.

- [ ] **Step 1: Write the failing tests**

In `CRMPeyvand.Tests/SqliteSchemaTests.cs`, change the `expected` array in `Ensure_creates_every_table_the_context_maps` to:

```csharp
            string[] expected =
            {
                "AccessGrants", "UserGroups", "Users", "Activities", "ActivityCategories",
                "Customers", "Invoices", "InvoiceLines", "CatalogItems", "Reminders",
                "MessagePanels", "Messages", "OffCodes", "RememberMes", "Payments",
            };
```

Then add this test at the end of the class:

```csharp
        [Fact]
        public void A_payment_round_trips_through_the_context_with_its_relations()
        {
            SqliteTestDb.WithDb(db =>
            {
                var customer = new BE.Customer { Name = "مشتری", Phone = "09120000001", RegDate = new DateTime(2026, 10, 4) };
                db.Customers.Add(customer);
                db.SaveChanges();

                var invoice = new BE.Invoice { RegDate = new DateTime(2026, 10, 4), DiscountAmount = 0m, Customer = customer };
                invoice.Lines.Add(new BE.InvoiceLine
                {
                    Quantity = 1,
                    UnitPrice = 4000m,
                    CatalogItem = new BE.CatalogItem { Name = "کالا", Kind = BE.ItemKind.Good, SalePrice = 4000m, Stock = 5 },
                });
                db.Invoices.Add(invoice);
                db.SaveChanges();

                db.Payments.Add(new BE.Payment
                {
                    Amount = 1500m,
                    RegDate = new DateTime(2026, 10, 4),
                    Instrument = BE.PaymentInstrument.Card,
                    Reference = "123456",
                    Invoice = invoice,
                });
                db.SaveChanges();

                var stored = db.Payments.Include("Invoice").Single();
                Assert.Equal(1500m, stored.Amount);
                Assert.Equal(BE.PaymentInstrument.Card, stored.Instrument);
                Assert.Equal("123456", stored.Reference);
                Assert.Equal(invoice.id, stored.Invoice.id);
            });
        }
```

`SqliteSchemaTests` carries no `[Collection]` attribute today and does not need one: it hands its own connection to `new DB(connection)`.

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~SqliteSchemaTests`
Expected: `Ensure_creates_every_table_the_context_maps` fails on `"Payments"`, and the new test fails to compile because `db.Payments` does not exist.

- [ ] **Step 3: Add the DbSet**

In `DAL/DB.cs`, add this line to the `DbSet` block, after `RememberMe`:

```csharp
        public DbSet<Payment> Payments { get; set; }
```

EF6 derives the table name from the property name pluralised, so the table is `Payments`.

- [ ] **Step 4: Add the SQLite table and index**

In `DAL/SqliteSchema.cs`, insert into the `Ddl` string literal **after** the `InvoiceLines` block (which ends at line 135) and **before** the `Reminders` block, so both foreign-key targets already exist:

```sql
CREATE TABLE IF NOT EXISTS Payments (
    Id           INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Amount       DECIMAL(18,2) NOT NULL,
    RegDate      DATETIME NOT NULL,
    Instrument   INTEGER NOT NULL,
    Reference    TEXT NULL,
    DeleteStatus INTEGER NOT NULL,
    Invoice_id   INTEGER NULL REFERENCES Invoices (id),
    User_id      INTEGER NULL REFERENCES Users (id)
);
```

Then add this to the index block, after the `IX_InvoiceLines_CatalogItemId` line:

```sql
CREATE INDEX IF NOT EXISTS IX_Payments_Invoice_id            ON Payments (Invoice_id);
```

Finally, update the class doc comment so it names the third `Id` table. It currently reads `including the lowercase "id" on 12 tables versus uppercase "Id" on CatalogItems and InvoiceLines`; make it:

```
    /// Names must match the SQL Server schema exactly, including the lowercase
    /// "id" on 12 tables versus uppercase "Id" on CatalogItems, InvoiceLines
    /// and Payments, and including "RememberMes" (EF6's pluraliser produced
    /// that spelling and existing SQL Server data is keyed to it).
```

- [ ] **Step 5: Run the tests to verify they pass**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~SqliteSchemaTests`
Expected: PASS, 5 tests.

- [ ] **Step 6: Write the EF6 migration for SQL Server**

`DAL/Migrations/Configuration.cs` sets `AutomaticMigrationsEnabled = false`, so a model change with no migration makes `MigrateDatabaseToLatestVersion` throw on the SQL Server path at startup. The app must keep working when a user switches provider.

Create `DAL/Migrations/202610041200000_AddPayments.cs`:

```csharp
using System.Data.Entity.Migrations;

namespace DAL.Migrations
{
    public partial class AddPayments : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RegDate = c.DateTime(nullable: false),
                        Instrument = c.Int(nullable: false),
                        Reference = c.String(),
                        DeleteStatus = c.Boolean(nullable: false),
                        Invoice_id = c.Int(),
                        User_id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Invoices", t => t.Invoice_id)
                .ForeignKey("dbo.Users", t => t.User_id)
                .Index(t => t.Invoice_id)
                .Index(t => t.User_id);
        }

        public override void Down()
        {
            DropForeignKey("dbo.Payments", "Invoice_id", "dbo.Invoices");
            DropForeignKey("dbo.Payments", "User_id", "dbo.Users");
            DropIndex("dbo.Payments", new[] { "Invoice_id" });
            DropIndex("dbo.Payments", new[] { "User_id" });
            DropTable("dbo.Payments");
        }
    }
}
```

`Down()` drops foreign keys, then indexes, then the table — the order `InitialCreate.Down()` uses.

Create `DAL/Migrations/202610041200000_AddPayments.Designer.cs`:

```csharp
// <auto-generated />
namespace DAL.Migrations
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;

    /// <summary>
    /// Migration metadata for AddPayments.
    ///
    /// Target is deliberately null. EF6 normally embeds the target model as a
    /// "Target" resource in a .resx beside this file, and that blob cannot be
    /// written by hand - so rather than ship a fabricated or stale one, there is
    /// none. See the KNOWN GAP note in 202610041200000_AddPayments.cs.
    ///
    /// Nothing at runtime reads it: MigrateDatabaseToLatestVersion applies Up()
    /// to the migrations the database has not recorded, and Up() carries the DDL.
    /// </summary>
    [GeneratedCode("EntityFramework.Migrations", "6.4.4")]
    public sealed partial class AddPayments : IMigrationMetadata
    {
        string IMigrationMetadata.Id
        {
            get { return "202610041200000_AddPayments"; }
        }

        string IMigrationMetadata.Source
        {
            get { return null; }
        }

        string IMigrationMetadata.Target
        {
            get { return null; }
        }
    }
}
```

**Do not create a `.resx` for this migration, and do not try to hand-write a model snapshot.** EF6 does not put the target model in the `.Designer.cs`: the existing `InitialCreate.Designer.cs` is only 29 lines of `IMigrationMetadata`, and the serialized model lives under a `Target` key in its `.resx` as an opaque base64 blob. That blob cannot be authored by hand.

The consequence must be written into the migration file's own doc comment, so the next developer meets it: with `Target` null, running `Add-Migration` again would believe no tables exist and try to create all fourteen. The remedy is to delete and re-add this migration from Visual Studio against a real SQL Server instance so a genuine snapshot is generated. `Up`/`Down` above are complete and correct, and they are all this app needs in order to run; only future scaffolding is affected.

Add that same warning as a `KNOWN GAP` block in the doc comment of `202610041200000_AddPayments.cs`.

**This migration cannot be verified on this machine.** `CRMPeyvand.Tests/LocalSqlServer.cs` returns `null` and its tests return early when no SQL Server instance holding the `CRMPeyvand` database is reachable. If you have Visual Studio and a local SQL Server, verify with `Update-Database -Script` and confirm the script contains `Payments`. Otherwise leave it as written and say in the PR that the SQL Server path is unverified.

- [ ] **Step 7: Run the whole suite**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: PASS, with the runner's total higher than the `243` recorded in the README.

- [ ] **Step 8: Commit**

```bash
git add DAL/DB.cs DAL/SqliteSchema.cs DAL/Migrations/202610041200000_AddPayments.cs DAL/Migrations/202610041200000_AddPayments.Designer.cs DAL/Migrations/202610041200000_AddPayments.resx CRMPeyvand.Tests/SqliteSchemaTests.cs
git commit -m "feat(receivables): persist payments on SQLite and SQL Server"
```

---

### Task 3: The settlement policy — pure validation

**Files:**
- Create: `BLL/SettlementPolicy.cs`
- Test: `CRMPeyvand.Tests/SettlementPolicyTests.cs`

**Interfaces:**
- Consumes: nothing.
- Produces: `BLL.SettlementPolicy.ValidateRecording(decimal amount, decimal balance, DateTime date, DateTime today) → InvalidOperationException`, returning `null` when the payment is allowed.

- [ ] **Step 1: Write the failing tests**

Create `CRMPeyvand.Tests/SettlementPolicyTests.cs`:

```csharp
using System;
using BLL;
using Xunit;

namespace CRMPeyvand.Tests
{
    public class SettlementPolicyTests
    {
        private static readonly DateTime Today = new DateTime(2026, 10, 4);

        private static string Rejection(decimal amount, decimal balance, DateTime date)
        {
            return SettlementPolicy
                .ValidateRecording(amount, balance, date, Today)
                ?.Message;
        }

        [Fact]
        public void A_payment_up_to_the_balance_is_allowed()
        {
            Assert.Null(Rejection(5000m, 5000m, Today));
            Assert.Null(Rejection(4999m, 5000m, Today));
        }

        [Fact]
        public void A_payment_beyond_the_balance_is_refused()
        {
            Assert.Equal(
                "مبلغ وصولی نمیتواند بیشتر از مانده حساب فاکتور باشد",
                Rejection(5001m, 5000m, Today));
        }

        [Fact]
        public void Nothing_is_allowed_against_a_settled_invoice()
        {
            Assert.NotNull(Rejection(1m, 0m, Today));
        }

        [Fact]
        public void A_zero_amount_is_refused()
        {
            Assert.Equal(
                "مبلغ وصولی باید بیشتر از صفر باشد",
                Rejection(0m, 5000m, Today));
        }

        [Fact]
        public void A_negative_amount_is_refused()
        {
            Assert.Equal(
                "مبلغ وصولی باید بیشتر از صفر باشد",
                Rejection(-500m, 5000m, Today));
        }

        [Fact]
        public void A_payment_dated_in_the_future_is_refused()
        {
            Assert.Equal(
                "تاریخ وصولی نمیتواند در آینده باشد",
                Rejection(1000m, 5000m, Today.AddDays(1)));
        }

        [Fact]
        public void Backdating_is_allowed_and_so_is_an_earlier_hour_today()
        {
            Assert.Null(Rejection(1000m, 5000m, Today.AddHours(9)));
            Assert.Null(Rejection(1000m, 5000m, Today.AddDays(-30)));
        }

        [Fact]
        public void The_amount_is_checked_before_the_date_so_a_bad_amount_is_reported_first()
        {
            Assert.Equal(
                "مبلغ وصولی باید بیشتر از صفر باشد",
                Rejection(0m, 5000m, Today.AddDays(5)));
        }
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~SettlementPolicyTests`
Expected: compile failure — `SettlementPolicy` does not exist.

- [ ] **Step 3: Write the policy**

Create `BLL/SettlementPolicy.cs`:

```csharp
using System;

namespace BLL
{
    /// <summary>
    /// What makes a payment a legal thing to record. Pure and static like Pricing
    /// and StockPolicy, so it can be tested without a database and so the DAL can
    /// take it as a delegate — the DAL cannot reference this assembly.
    ///
    /// The order of the checks is the order an employee needs them in: fix the
    /// amount before worrying about the date.
    /// </summary>
    public static class SettlementPolicy
    {
        public static InvalidOperationException ValidateRecording(
            decimal amount, decimal balance, DateTime date, DateTime today)
        {
            if (amount <= 0m)
                return new InvalidOperationException("مبلغ وصولی باید بیشتر از صفر باشد");

            if (date.Date > today.Date)
                return new InvalidOperationException("تاریخ وصولی نمیتواند در آینده باشد");

            if (amount > balance)
                return new InvalidOperationException("مبلغ وصولی نمیتواند بیشتر از مانده حساب فاکتور باشد");

            return null;
        }
    }
}
```

- [ ] **Step 4: Run the tests to verify they pass**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~SettlementPolicyTests`
Expected: PASS, 8 tests.

- [ ] **Step 5: Commit**

```bash
git add BLL/SettlementPolicy.cs CRMPeyvand.Tests/SettlementPolicyTests.cs
git commit -m "feat(receivables): validate a payment before it is recorded"
```

---

### Task 4: Recording and voiding payments in the data layer

The heart of the feature: `PaymentDAL`, the single place that writes `IsCheckedout`, and the invoice and customer grid changes that make the new money visible.

**Files:**
- Create: `DAL/SettlementFlag.cs`, `DAL/PaymentDAL.cs`
- Modify: `DAL/InvoiceDAL.cs`, `DAL/CustomerDAL.cs`
- Test: `CRMPeyvand.Tests/PaymentDALTests.cs`, `CRMPeyvand.Tests/GridQueriesTests.cs`

**Interfaces:**
- Consumes: `BE.Payment`, `BE.PaymentInstrumentTitles`, `BE.Invoice.Balance`, `BE.Customer.Balance` (Task 1); the `Payments` table (Task 2); `SettlementPolicy.ValidateRecording` (Task 3).
- Produces:
  - `DAL.SettlementFlag.Apply(Invoice invoice, decimal paid)` — internal static
  - `PaymentDAL(DB db)` and `PaymentDAL()`
  - `string PaymentDAL.Create(Payment payment, int invoiceId, Func<decimal, decimal, DateTime, DateTime, InvalidOperationException> validate)`
  - `string PaymentDAL.Void(int id)`
  - `DataTable PaymentDAL.Read()`, `DataTable PaymentDAL.Search(string Filter)`, `string PaymentDAL.Count()`
  - `Payment PaymentDAL.ReadById(int id)`, `List<Customer> PaymentDAL.ReadCustomerBalances()`
  - `InvoiceDAL.Create(Invoice, int, IReadOnlyList<InvoiceLine>, Func<CatalogItem,int,InvalidOperationException>, Func<OffCode,decimal,decimal>, Payment settledBy)` — the last parameter is new
  - `InvoiceDAL` loses `Done`; `Read`, `ReadDetails` and `CustomerDAL.Read`/`Search` gain columns

- [ ] **Step 1: Write the failing tests**

Create `CRMPeyvand.Tests/PaymentDALTests.cs`:

```csharp
using System;
using System.Data;
using System.Linq;
using BE;
using DAL;
using Xunit;

namespace CRMPeyvand.Tests
{
    [Collection(DataSourceCollection.Name)]
    public class PaymentDALTests
    {
        private static readonly Func<decimal, decimal, DateTime, DateTime, InvalidOperationException> Allow =
            BLL.SettlementPolicy.ValidateRecording;

        private static readonly DateTime PayDay = new DateTime(2026, 10, 4);

        private static Customer SeedCustomer(DB db, string phone = "09120000000", string name = "مشتری آزمایشی")
        {
            var customer = new Customer { Name = name, Phone = phone, RegDate = new DateTime(2026, 10, 1) };
            db.Customers.Add(customer);
            db.SaveChanges();
            return customer;
        }

        private static Invoice SeedInvoice(DB db, Customer customer, decimal unitPrice, decimal discount = 0m)
        {
            var invoice = new Invoice { RegDate = new DateTime(2026, 10, 1), DiscountAmount = discount, Customer = customer };
            invoice.Lines.Add(new InvoiceLine
            {
                Quantity = 1,
                UnitPrice = unitPrice,
                CatalogItem = new CatalogItem { Name = "کالا", Kind = ItemKind.Good, SalePrice = unitPrice, Stock = 10 },
            });
            db.Invoices.Add(invoice);
            db.SaveChanges();
            return invoice;
        }

        [Fact]
        public void Recording_a_payment_reduces_the_balance_and_leaves_the_invoice_unsettled()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);

                var result = new PaymentDAL(db).Create(
                    new Payment { Amount = 4000m, RegDate = PayDay, Instrument = PaymentInstrument.Cash },
                    invoice.id, Allow);

                Assert.Equal("ثبت وصولی با موفقیت انجام شد", result);

                var reloaded = db.Invoices.Include("Payments").Single();
                Assert.Equal(4000m, reloaded.Paid);
                Assert.Equal(6000m, reloaded.Balance);
                Assert.False(reloaded.IsCheckedout);
            });
        }

        [Fact]
        public void Settling_the_remainder_flips_the_checkout_flag_and_stamps_the_date()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);

                new PaymentDAL(db).Create(new Payment { Amount = 10000m, RegDate = PayDay }, invoice.id, Allow);

                var reloaded = db.Invoices.Single();
                Assert.True(reloaded.IsCheckedout);
                Assert.NotNull(reloaded.CheckoutDate);
            });
        }

        [Fact]
        public void The_balance_is_measured_after_the_discount_not_the_subtotal()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m, discount: 3000m);

                new PaymentDAL(db).Create(new Payment { Amount = 7000m, RegDate = PayDay }, invoice.id, Allow);

                Assert.True(db.Invoices.Single().IsCheckedout);
            });
        }

        [Fact]
        public void Two_payments_accumulate()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);
                var dal = new PaymentDAL(db);

                dal.Create(new Payment { Amount = 3000m, RegDate = new DateTime(2026, 10, 2) }, invoice.id, Allow);
                dal.Create(new Payment { Amount = 2000m, RegDate = new DateTime(2026, 10, 3) }, invoice.id, Allow);

                Assert.Equal(5000m, db.Invoices.Include("Payments").Single().Paid);
            });
        }

        [Fact]
        public void Voiding_a_payment_reopens_the_invoice()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);
                var dal = new PaymentDAL(db);

                dal.Create(new Payment { Amount = 10000m, RegDate = PayDay }, invoice.id, Allow);
                Assert.True(db.Invoices.Single().IsCheckedout);

                Assert.Equal("ابطال وصولی با موفقیت انجام شد", dal.Void(db.Payments.Single().Id));

                var reloaded = db.Invoices.Include("Payments").Single();
                Assert.False(reloaded.IsCheckedout);
                Assert.Null(reloaded.CheckoutDate);
                Assert.Equal(0m, reloaded.Paid);
            });
        }

        [Fact]
        public void Voiding_one_of_two_payments_leaves_the_invoice_settled()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);
                var dal = new PaymentDAL(db);

                dal.Create(new Payment { Amount = 4000m, RegDate = new DateTime(2026, 10, 2) }, invoice.id, Allow);
                dal.Create(new Payment { Amount = 6000m, RegDate = new DateTime(2026, 10, 3) }, invoice.id, Allow);

                dal.Void(db.Payments.OrderBy(p => p.Id).First().Id);

                Assert.True(db.Invoices.Single().IsCheckedout);
                Assert.Equal(6000m, db.Payments.Where(p => !p.DeleteStatus).Sum(p => p.Amount));
            });
        }

        [Fact]
        public void Voiding_something_that_does_not_exist_is_reported_not_thrown()
        {
            SqliteTestDb.WithDb(db =>
            {
                Assert.Equal("وصولی مورد نظر یافت نشد", new PaymentDAL(db).Void(999));
            });
        }

        [Fact]
        public void An_unknown_invoice_is_reported_not_thrown()
        {
            SqliteTestDb.WithDb(db =>
            {
                var message = new PaymentDAL(db).Create(new Payment { Amount = 1000m }, 999, Allow);

                Assert.Equal("فاکتور مورد نظر یافت نشد", message);
                Assert.Empty(db.Payments);
            });
        }

        [Fact]
        public void A_deleted_invoice_cannot_take_a_payment()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);
                invoice.DeleteStatus = true;
                db.SaveChanges();

                var message = new PaymentDAL(db).Create(new Payment { Amount = 1000m }, invoice.id, Allow);

                Assert.Equal("فاکتور حذف شده و قابل وصولی نیست", message);
                Assert.Empty(db.Payments);
            });
        }

        [Fact]
        public void An_overpayment_is_refused_and_stores_nothing()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);
                var dal = new PaymentDAL(db);

                Assert.Throws<InvalidOperationException>(
                    () => dal.Create(new Payment { Amount = 10001m, RegDate = PayDay }, invoice.id, Allow));

                Assert.Empty(db.Payments);
                Assert.False(db.Invoices.Single().IsCheckedout);
            });
        }

        [Fact]
        public void The_payment_keeps_its_instrument_and_reference()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);

                new PaymentDAL(db).Create(new Payment
                {
                    Amount = 1000m,
                    RegDate = PayDay,
                    Instrument = PaymentInstrument.Cheque,
                    Reference = "۱۲۳۴۵۶",
                }, invoice.id, Allow);

                var stored = db.Payments.Single();
                Assert.Equal(PaymentInstrument.Cheque, stored.Instrument);
                Assert.Equal("۱۲۳۴۵۶", stored.Reference);
            });
        }

        [Fact]
        public void The_payment_is_attributed_to_the_operator_who_took_it()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);
                var operatorUser = new User { Name = "احسان", UserName = "ehsan", RegDate = new DateTime(2026, 1, 1) };
                db.Users.Add(operatorUser);
                db.SaveChanges();

                new PaymentDAL(db).Create(
                    new Payment { Amount = 1000m, RegDate = PayDay, User = operatorUser },
                    invoice.id, Allow);

                Assert.Equal("ehsan", db.Payments.Include("User").Single().User.UserName);
            });
        }

        [Fact]
        public void The_grid_carries_the_receipt_number_the_invoice_the_customer_and_the_amount()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);

                new PaymentDAL(db).Create(new Payment
                {
                    Amount = 2500m,
                    RegDate = PayDay,
                    Instrument = PaymentInstrument.Card,
                    Reference = "7788",
                }, invoice.id, Allow);

                var table = new PaymentDAL(db).Read();

                Assert.Equal(
                    new[]
                    {
                        "شماره وصولی", "شماره فاکتور", "نام مشتری", "تاریخ وصول",
                        "مبلغ", "روش پرداخت", "شماره پیگیری", "ثبت توسط",
                    },
                    table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));

                Assert.Equal(invoice.id, Convert.ToInt32(table.Rows[0]["شماره فاکتور"]));
                Assert.Equal("مشتری آزمایشی", table.Rows[0]["نام مشتری"]);
                Assert.Equal(2500m, Convert.ToDecimal(table.Rows[0]["مبلغ"]));
                Assert.Equal("کارت", table.Rows[0]["روش پرداخت"]);
                Assert.Equal("7788", table.Rows[0]["شماره پیگیری"]);
            });
        }

        [Fact]
        public void The_grid_shows_the_most_recent_receipt_first()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);
                var dal = new PaymentDAL(db);
                dal.Create(new Payment { Amount = 1000m, RegDate = new DateTime(2026, 10, 2) }, invoice.id, Allow);
                dal.Create(new Payment { Amount = 2000m, RegDate = new DateTime(2026, 10, 3) }, invoice.id, Allow);

                var table = dal.Read();

                Assert.Equal(2000m, Convert.ToDecimal(table.Rows[0]["مبلغ"]));
                Assert.Equal(1000m, Convert.ToDecimal(table.Rows[1]["مبلغ"]));
            });
        }

        [Fact]
        public void A_voided_payment_leaves_the_grid()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, SeedCustomer(db), 10000m);
                var dal = new PaymentDAL(db);
                dal.Create(new Payment { Amount = 1000m, RegDate = PayDay }, invoice.id, Allow);

                dal.Void(db.Payments.Single().Id);

                Assert.Empty(dal.Read().Rows);
            });
        }

        [Fact]
        public void Searching_the_grid_matches_the_invoice_number_and_the_customer_name()
        {
            SqliteTestDb.WithDb(db =>
            {
                var first = SeedCustomer(db, "09120000001", "علی رضایی");
                var second = SeedCustomer(db, "09120000002", "سمیرا محمدی");

                var dal = new PaymentDAL(db);
                var firstInvoice = SeedInvoice(db, first, 5000m);
                var secondInvoice = SeedInvoice(db, second, 5000m);
                dal.Create(new Payment { Amount = 1000m, RegDate = PayDay }, firstInvoice.id, Allow);
                dal.Create(new Payment { Amount = 1000m, RegDate = PayDay }, secondInvoice.id, Allow);

                Assert.Single(dal.Search("رضایی").Rows);
                Assert.Single(dal.Search(secondInvoice.id.ToString()).Rows);
                Assert.Empty(dal.Search("هیچ‌کس").Rows);
            });
        }

        [Fact]
        public void The_customer_balance_list_keeps_only_those_who_owe_something()
        {
            SqliteTestDb.WithDb(db =>
            {
                var debtor = SeedCustomer(db, "09120000001");
                var settled = SeedCustomer(db, "09120000002");
                SeedInvoice(db, debtor, 10000m);
                SeedInvoice(db, settled, 5000m);

                var balances = new PaymentDAL(db).ReadCustomerBalances();

                Assert.Single(balances);
                Assert.Equal("09120000001", balances[0].Phone);
                Assert.Equal(10000m, balances[0].Balance);
            });
        }

        [Fact]
        public void The_customer_balance_list_ignores_voided_payments()
        {
            SqliteTestDb.WithDb(db =>
            {
                var customer = SeedCustomer(db);
                var invoice = SeedInvoice(db, customer, 10000m);
                new PaymentDAL(db).Create(new Payment { Amount = 10000m, RegDate = PayDay }, invoice.id, Allow);

                Assert.Empty(new PaymentDAL(db).ReadCustomerBalances());
            });
        }
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~PaymentDALTests`
Expected: compile failure — `PaymentDAL` does not exist.

- [ ] **Step 3: Write the one place that writes the flag**

Create `DAL/SettlementFlag.cs`:

```csharp
using BE;

namespace DAL
{
    /// <summary>
    /// The only writer of Invoice.IsCheckedout and Invoice.CheckoutDate.
    ///
    /// The flag is a cache, not a record: payments are the truth and this is
    /// derived from them. See
    /// docs/adr/0009-payments-authoritative-over-checkout-flag.md.
    ///
    /// Callers pass the paid total rather than letting this read Invoice.Paid,
    /// because on the write path the payment being recorded is not yet in the
    /// invoice's loaded collection, and on the void path the payment being voided
    /// still is. Taking the figure as an argument keeps the rule in one place and
    /// makes the caller responsible for getting the number right.
    /// </summary>
    internal static class SettlementFlag
    {
        internal static void Apply(Invoice invoice, decimal paid)
        {
            invoice.IsCheckedout = Math.Max(0m, invoice.Payable - paid) <= 0m;
            invoice.CheckoutDate = invoice.IsCheckedout
                ? (invoice.CheckoutDate ?? invoice.RegDate)
                : null;
        }
    }
}
```

- [ ] **Step 4: Write PaymentDAL**

Create `DAL/PaymentDAL.cs`:

```csharp
using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace DAL
{
    public class PaymentDAL
    {
        DB db;

        public PaymentDAL()
        {
            db = new DB();
        }

        /// <summary>Tests supply their own context.</summary>
        public PaymentDAL(DB db)
        {
            this.db = db;
        }

        /// <summary>
        /// Money already received against one invoice, optionally excluding one
        /// payment.
        ///
        /// A SQL aggregate rather than a sum over invoice.Payments, because on the
        /// write path the row being written is not in the collection and on the
        /// void path the row being voided still is. Asking the database settles
        /// both cases the same way, and it is the only figure that cannot be out
        /// of step with what is actually stored.
        /// </summary>
        private static decimal PaidSoFar(DB context, int invoiceId, int excludingPaymentId)
        {
            return context.Payments
                .Where(p => p.Invoice.id == invoiceId
                         && p.DeleteStatus == false
                         && (excludingPaymentId == 0 || p.Id != excludingPaymentId))
                .Sum(p => p.Amount);
        }

        /// <summary>
        /// Records a payment and re-derives the invoice's checkout flag in the
        /// same transaction, so the flag and the payment list can never be seen
        /// disagreeing.
        ///
        /// validate is injected from BLL (SettlementPolicy.ValidateRecording)
        /// because the DAL assembly cannot reference BLL (BLL references DAL). It
        /// is rethrown rather than returned: the form catches
        /// InvalidOperationException and shows the message as a warning, which is
        /// the pattern InvoiceDAL.Create established.
        /// </summary>
        public string Create(Payment payment, int invoiceId,
            Func<decimal, decimal, DateTime, DateTime, InvalidOperationException> validate)
        {
            // Per-call context, for the same reason InvoiceDAL.Create has one: a
            // rollback must not leave a pending insert in a long-lived tracker.
            using (var context = new DB())
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var invoice = context.Invoices
                        .Include("Lines")
                        .FirstOrDefault(i => i.id == invoiceId);

                    if (invoice == null)
                        return "فاکتور مورد نظر یافت نشد";

                    if (invoice.DeleteStatus)
                        return "فاکتور حذف شده و قابل وصولی نیست";

                    decimal balance = Math.Max(0m, invoice.Payable - PaidSoFar(context, invoiceId, 0));
                    var rejection = validate(payment.Amount, balance, payment.RegDate, DateTime.Now);
                    if (rejection != null) throw rejection;

                    payment.Invoice = invoice;
                    if (payment.User != null)
                    {
                        payment.User = context.Users.Find(payment.User.id);
                    }

                    context.Payments.Add(payment);
                    SettlementFlag.Apply(invoice, PaidSoFar(context, invoiceId, 0) + payment.Amount);

                    context.SaveChanges();
                    transaction.Commit();
                    return "ثبت وصولی با موفقیت انجام شد";
                }
                catch (InvalidOperationException)
                {
                    transaction.Rollback();
                    throw;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return "ثبت اطلاعات با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
                }
            }
        }

        /// <summary>
        /// Soft-deletes a payment and re-derives the flag, in one transaction.
        /// There is deliberately no edit: a payment may already have a printed
        /// receipt in the customer's hand, so a correction is a void plus a fresh
        /// payment rather than an amendment.
        /// </summary>
        public string Void(int id)
        {
            using (var context = new DB())
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var payment = context.Payments
                        .Include("Invoice.Lines")
                        .FirstOrDefault(p => p.Id == id);

                    if (payment == null)
                        return "وصولی مورد نظر یافت نشد";

                    var invoice = payment.Invoice;
                    payment.DeleteStatus = true;

                    SettlementFlag.Apply(invoice, PaidSoFar(context, invoice.id, payment.Id));

                    context.SaveChanges();
                    transaction.Commit();
                    return "ابطال وصولی با موفقیت انجام شد";
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return "ابطال وصولی با مشکلی روبرو شد لطفا برسی کنید:\n" + e.Message;
                }
            }
        }

        // Copied from the projection below, in the same order: the grid binds with
        // AutoGenerateColumns, so these strings are the visible headers.
        private static readonly string[] ReadColumns =
        {
            "شماره وصولی", "شماره فاکتور", "نام مشتری", "تاریخ وصول",
            "مبلغ", "روش پرداخت", "شماره پیگیری", "ثبت توسط",
        };

        /// <summary>
        /// The rows behind the grid, newest receipt first.
        ///
        /// Include("Invoice.Customer") is not optional: EF6 does not lazy load, so
        /// without it the customer column sees a null navigation.
        /// </summary>
        private List<object[]> PaymentRows()
        {
            return db.Payments
                .Include("Invoice.Customer")
                .Include("User")
                .Where(p => p.DeleteStatus == false)
                .OrderByDescending(p => p.Id)
                .Take(GridTable.DefaultRowLimit)
                .ToList()
                .Select(p => new object[]
                {
                    p.Id,
                    p.Invoice?.id,
                    p.Invoice?.Customer?.Name,
                    p.RegDate,
                    p.Amount,
                    PaymentInstrumentTitles.Of(p.Instrument),
                    p.Reference,
                    // Name-then-UserName, the fallback every other report uses.
                    p.User?.Name ?? p.User?.UserName,
                })
                .ToList();
        }

        public DataTable Read()
        {
            return GridTable.Build(ReadColumns, PaymentRows());
        }

        public DataTable Search(string Filter)
        {
            // Filtered after materialisation because string.Contains in a
            // LINQ-to-Entities Where needs CHARINDEX, which SQLite lacks. The
            // invoice number and the customer name are the two things an employee
            // knows when hunting for a receipt.
            var rows = PaymentRows()
                .Where(r => GridTable.Matches(
                    Filter,
                    Convert.ToString(r[1]),
                    Convert.ToString(r[2])))
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }

        public string Count()
        {
            return db.Payments.Where(p => p.DeleteStatus == false).Count().ToString();
        }

        /// <summary>Full payment for the receipt: the payment, its invoice and its customer.</summary>
        public Payment ReadById(int id)
        {
            return db.Payments
                .Include("Invoice.Customer")
                .Include("Invoice.Lines")
                .Include("Invoice.Payments")
                .Include("User")
                .FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Customers who still owe something, most indebted first.
        ///
        /// Balance is computed by Customer over its invoices' lines and payments,
        /// so all three collections are pulled in, and the result is materialised
        /// before filtering because a sum over a collection cannot be projected
        /// into SQL. Customers with nothing outstanding are dropped: this answers
        /// "who owes me", not "who do I serve".
        /// </summary>
        public List<Customer> ReadCustomerBalances()
        {
            return db.Customers
                .Where(c => c.DeleteStatus == false)
                .Include("Invoices.Lines")
                .Include("Invoices.Payments")
                .ToList()
                .Where(c => c.Balance > 0m)
                .OrderByDescending(c => c.Balance)
                .ToList();
        }
    }
}
```

- [ ] **Step 5: Change InvoiceDAL to accept a settle-at-the-counter payment**

In `DAL/InvoiceDAL.cs`, replace the `Create` signature and body (lines 30-71) with:

```csharp
        // Creates invoice + lines + optional settling payment atomically, validates/
        // decrements Goods stock, returns saved invoice with id.
        // validateStock / computeDiscount are injected from BLL (StockPolicy.Validate /
        // Pricing.ComputeDiscount) because the DAL assembly cannot reference BLL
        // (BLL references DAL). settledBy is the «وضعیت پرداخت» checkbox: when the
        // employee ticks it while composing the invoice, the full payable arrived at
        // the counter, so a real Payment is written here inside the transaction that
        // creates the invoice. It used to be a second call to Done() in a second
        // context afterwards, which was never atomic and recorded no amount.
        public Invoice Create(Invoice invoice, int customerId, IReadOnlyList<InvoiceLine> lines,
            Func<CatalogItem, int, InvalidOperationException> validateStock,
            Func<OffCode, decimal, decimal> computeDiscount,
            Payment settledBy)
        {
            // Per-call context: a rollback must not leave modified entities pending in a
            // shared long-lived tracker (phantom decrements would flush on the next SaveChanges).
            using (var db = new DB())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        invoice.Customer = db.Customers.Find(customerId);
                        invoice.User = db.Users.Find(invoice.User.id);
                        foreach (var line in lines)
                        {
                            var item = db.CatalogItems.Find(line.CatalogItemId);
                            var rejection = validateStock(item, line.Quantity);
                            if (rejection != null) throw rejection;
                            if (item.Kind == ItemKind.Good) item.Stock -= line.Quantity;
                            line.UnitPrice = item.SalePrice;
                            line.CatalogItem = item;
                            invoice.Lines.Add(line);
                        }
                        invoice.DiscountAmount = computeDiscount(
                            string.IsNullOrEmpty(invoice.OffCode)
                                ? null
                                : db.OffCodes.FirstOrDefault(o => o.Code == invoice.OffCode),
                            invoice.SubTotal);
                        db.Invoices.Add(invoice);
                        if (settledBy != null)
                        {
                            settledBy.Amount = invoice.Payable;
                            settledBy.RegDate = invoice.RegDate;
                            settledBy.User = invoice.User;
                            settledBy.Invoice = invoice;
                            db.Payments.Add(settledBy);
                            invoice.Payments.Add(settledBy);
                            SettlementFlag.Apply(invoice, invoice.Payable);
                        }
                        db.SaveChanges();
                        transaction.Commit();
                        return invoice;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
```

Then **delete the whole `Done` method** (lines 74-96). Nothing settles an invoice any more except recording money against it.

Then replace `ReadColumns` (lines 117-121) with:

```csharp
        private static readonly string[] ReadColumns =
        {
            "شماره فاکتور", "وضعیت پرداخت", "تاریخ پرداخت", "کد تخفیف", "تاریخ ثبت",
            "تعداد کالاهای فاکتور", "مبلغ کل فاکتور", "مبلغ پرداخت شده", "مانده حساب",
        };
```

The old last column «هزینه پرداختی» is renamed to «مبلغ کل فاکتور» because beside a real payments feature the word «پرداختی» reads as *paid*, which it never meant.

Then replace `InvoiceRows()` (lines 134-158) with:

```csharp
        private List<object[]> InvoiceRows()
        {
            return db.Invoices
                .Include("Lines")
                .Include("Payments")
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Take(GridTable.DefaultRowLimit)
                .ToList()
                .Select(i => new object[]
                {
                    i.id,
                    // Persian rather than the bool. The column held a bool in an
                    // object[] and rendered the literal text "True"/"False".
                    i.IsCheckedout ? "پرداخت شده" : "پرداخت نشده",
                    i.CheckoutDate,
                    i.OffCode,
                    i.RegDate,
                    // ISNULL(SUM(l.Quantity), 0) becomes Sum, which returns 0
                    // for an empty sequence rather than null.
                    i.Lines.Sum(l => l.Quantity),
                    // Payable rather than the raw expression this used to carry:
                    // it is clamped, and clamping matters now that a payment is
                    // measured against it.
                    i.Payable,
                    i.Paid,
                    i.Balance,
                })
                .ToList();
        }
```

Then add `.Include("Payments")` to `ReadDetails` (lines 195-202):

```csharp
        public Invoice ReadDetails(int id)
        {
            return db.Invoices
                .Include("Customer")
                .Include("User")
                .Include("Lines.CatalogItem")
                .Include("Payments")
                .FirstOrDefault(i => i.id == id);
        }
```

`ReadById` (lines 189-192) needs no change: the context menu reads only `id` from it.

- [ ] **Step 6: Add the balance column to the customer grid**

In `DAL/CustomerDAL.cs`, replace `ReadColumns`:

```csharp
        private static readonly string[] ReadColumns =
            { "نام", "شماره تماس", "تاریخ ثبت", "مانده حساب" };
```

Replace `Read`:

```csharp
        public DataTable Read()
        {
            var rows = db.Customers
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Include("Invoices.Lines")
                .Include("Invoices.Payments")
                .Take(GridTable.DefaultRowLimit)
                .ToList()
                .Select(i => new object[] { i.Name, i.Phone, i.RegDate, i.Balance })
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }
```

Replace `Search`:

```csharp
        public DataTable Search(string Filter)
        {
            var rows = db.Customers
                .Where(i => i.DeleteStatus == false)
                .OrderByDescending(i => i.id)
                .Include("Invoices.Lines")
                .Include("Invoices.Payments")
                .Take(GridTable.DefaultRowLimit)
                .ToList()
                .AsEnumerable()
                .Where(i => GridTable.Matches(Filter, i.Name, i.Phone))
                .Select(i => new object[] { i.Name, i.Phone, i.RegDate, i.Balance })
                .ToList();

            return GridTable.Build(ReadColumns, rows);
        }
```

The original had `.AsEnumerable()` **before** `.Take(...)`, which meant the row limit was applied in memory over the whole table. `Take` now precedes it so the limit is the database's job; mention this behaviour change in the commit message.

`ReadWithDateTime` needs no change — it feeds the customer report, which does not show balances.

- [ ] **Step 7: Group the customer grid's new money column**

`CRMPeyvand/CustomerForm.xaml.cs` fills its grid with the plain `PublicMethods.dgvFiller(dg, dt)` overload at five call sites (lines 69, 86, 95, 121, 123 and 155), so the new «مانده حساب» column would render as an ungrouped number like `12000000`. Add a helper beside the form's other members and use it at each of those sites:

```csharp
        private const string BalanceColumn = "مانده حساب";

        private void FillCustomers(DataTable table)
        {
            PublicMethods.dgvFiller(dgvCustomer, table, BalanceColumn);
        }
```

Replace each `PublicMethods.dgvFiller(dgvCustomer, …)` with `FillCustomers(…)`.

- [ ] **Step 8: Update the pinned grid assertions**

Find every affected assertion rather than guessing at line numbers:

```bash
Select-String -Path CRMPeyvand.Tests\GridQueriesTests.cs -Pattern 'هزینه پرداختی|شماره فاکتور|شماره تماس'
```

The invoice column assertion becomes:

```csharp
            Assert.Equal(
                new[]
                {
                    "شماره فاکتور", "وضعیت پرداخت", "تاریخ پرداخت", "کد تخفیف", "تاریخ ثبت",
                    "تعداد کالاهای فاکتور", "مبلغ کل فاکتور", "مبلغ پرداخت شده", "مانده حساب",
                },
                table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
```

The customer column assertion becomes:

```csharp
            Assert.Equal(
                new[] { "نام", "شماره تماس", "تاریخ ثبت", "مانده حساب" },
                table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
```

Any test that indexes into the old customer projection by position must move to reading by column name. `Assert.Equal(0, dal.Search("True").Rows.Count)` on the invoice grid still passes and needs no edit, because no cell contains the text `True` any more.

**Between this task and Task 7 the invoice grid's three money columns are ungrouped**, because `InvoiceForm.FillInvoices` still names only the old column. That is a known transient state, not a defect to fix here; Task 7 Step 2 completes it.

- [ ] **Step 9: Run the tests to verify they pass**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: PASS.

- [ ] **Step 10: Commit**

```bash
git add DAL/SettlementFlag.cs DAL/PaymentDAL.cs DAL/InvoiceDAL.cs DAL/CustomerDAL.cs CRMPeyvand/CustomerForm.xaml.cs CRMPeyvand.Tests/PaymentDALTests.cs CRMPeyvand.Tests/GridQueriesTests.cs
git commit -m "feat(receivables): record and void payments and derive the checkout flag

Also moves CustomerDAL's Take() ahead of AsEnumerable(), so the 1000-row
limit is applied by the database rather than in memory over the whole table."
```

---

### Task 5: The BLL facade and the dashboard debtor count

**Files:**
- Create: `BLL/PaymentBLL.cs`
- Modify: `BLL/InvoiceBLL.cs`, `BLL/DashboardBLL.cs`, `DAL/DashboardDAL.cs`, `CRMPeyvand/InvoiceForm.xaml.cs`
- Test: `CRMPeyvand.Tests/DashboardDALTests.cs`

**Interfaces:**
- Consumes: `PaymentDAL`, `SettlementFlag` (Task 4), `SettlementPolicy` (Task 3).
- Produces:
  - `PaymentBLL.Create(Payment, int invoiceId) → string`, `Void(int) → string`, `Read() → DataTable`, `Search(string) → DataTable`, `Count() → string`, `ReadById(int) → Payment`, `ReadCustomerBalances() → List<Customer>`
  - `InvoiceBLL.Create(Invoice, int, IReadOnlyList<InvoiceLine>, Payment settledBy) → Invoice` — the fourth parameter is new — and **no** `Done`
  - `DashboardBLL.DebtorCustomerCount() → string`

- [ ] **Step 1: Write the failing test**

Create `CRMPeyvand.Tests/DashboardDALTests.cs`:

```csharp
using System;
using System.Linq;
using BE;
using DAL;
using Xunit;

namespace CRMPeyvand.Tests
{
    [Collection(DataSourceCollection.Name)]
    public class DashboardDALTests
    {
        private static void SeedDebt(DB db, string phone, decimal unitPrice, decimal paid)
        {
            var customer = new Customer { Name = phone, Phone = phone, RegDate = new DateTime(2026, 10, 1) };
            var invoice = new Invoice { RegDate = new DateTime(2026, 10, 1), DiscountAmount = 0m, Customer = customer };
            invoice.Lines.Add(new InvoiceLine
            {
                Quantity = 1,
                UnitPrice = unitPrice,
                CatalogItem = new CatalogItem { Name = "کالا", Kind = ItemKind.Good, SalePrice = unitPrice, Stock = 10 },
            });
            invoice.Payments.Add(new Payment { Amount = paid, RegDate = new DateTime(2026, 10, 2) });
            db.Customers.Add(customer);
            db.Invoices.Add(invoice);
            db.SaveChanges();
        }

        [Fact]
        public void The_debtor_count_ignores_customers_who_have_paid_in_full()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedDebt(db, "09120000001", 10000m, 4000m);
                SeedDebt(db, "09120000002", 5000m, 5000m);

                Assert.Equal("1", new DashboardDAL(db).DebtorCustomerCount());
            });
        }

        [Fact]
        public void The_debtor_count_ignores_voided_payments()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedDebt(db, "09120000001", 10000m, 0m);
                db.Payments.Single().DeleteStatus = true;
                db.SaveChanges();

                Assert.Equal("0", new DashboardDAL(db).DebtorCustomerCount());
            });
        }

        [Fact]
        public void The_debtor_count_ignores_deleted_invoices()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedDebt(db, "09120000001", 10000m, 0m);
                db.Invoices.Single().DeleteStatus = true;
                db.SaveChanges();

                Assert.Equal("0", new DashboardDAL(db).DebtorCustomerCount());
            });
        }

        [Fact]
        public void The_debtor_count_is_zero_for_an_empty_book()
        {
            SqliteTestDb.WithDb(db =>
            {
                Assert.Equal("0", new DashboardDAL(db).DebtorCustomerCount());
            });
        }
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~DashboardDALTests`
Expected: compile failure — `DebtorCustomerCount` does not exist.

- [ ] **Step 3: Add the query**

In `DAL/DashboardDAL.cs`, add:

```csharp
        /// <summary>
        /// How many customers owe something, as a counter string.
        ///
        /// Counted in memory over the book rather than in SQL because the balance
        /// is a sum over an invoice's lines and payments, and EF6 cannot project
        /// that into one grouped aggregate on either provider. This is the only
        /// dashboard figure that materialises; the rest of this class is plain
        /// Count, so on a very large book this is the expensive card. Try/catch to
        /// "0" like its neighbours.
        /// </summary>
        public string DebtorCustomerCount()
        {
            try
            {
                return db.Customers
                    .Where(c => c.DeleteStatus == false)
                    .Include("Invoices.Lines")
                    .Include("Invoices.Payments")
                    .ToList()
                    .Count(c => c.Balance > 0m)
                    .ToString();
            }
            catch
            {
                return "0";
            }
        }
```

- [ ] **Step 4: Expose it through the BLL**

In `BLL/DashboardBLL.cs`, add:

```csharp
        public string DebtorCustomerCount()
        {
            return dal.DebtorCustomerCount();
        }
```

- [ ] **Step 5: Run the tests to verify they pass**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~DashboardDALTests`
Expected: PASS, 4 tests.

- [ ] **Step 6: Write PaymentBLL**

Create `BLL/PaymentBLL.cs`:

```csharp
using BE;
using DAL;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    public class PaymentBLL
    {
        PaymentDAL dal = new PaymentDAL();

        public string Create(Payment payment, int invoiceId)
        {
            return dal.Create(payment, invoiceId, SettlementPolicy.ValidateRecording);
        }

        public string Void(int id)
        {
            return dal.Void(id);
        }

        public DataTable Read()
        {
            return dal.Read();
        }

        public DataTable Search(string Filter)
        {
            return dal.Search(Filter);
        }

        public string Count()
        {
            return dal.Count();
        }

        public Payment ReadById(int id)
        {
            return dal.ReadById(id);
        }

        public List<Customer> ReadCustomerBalances()
        {
            return dal.ReadCustomerBalances();
        }
    }
}
```

- [ ] **Step 7: Change InvoiceBLL**

In `BLL/InvoiceBLL.cs`, replace `Create`:

```csharp
        /// <summary>
        /// settledBy is non-null when the «وضعیت پرداخت» checkbox was ticked,
        /// meaning the full payable arrived at the counter as the invoice was
        /// written.
        /// </summary>
        public Invoice Create(Invoice invoice, int customerId, IReadOnlyList<InvoiceLine> lines, Payment settledBy)
        {
            return dal.Create(invoice, customerId, lines, StockPolicy.Validate, Pricing.ComputeDiscount, settledBy);
        }
```

and remove:

```csharp
        public string Done(int id)
        {
            return dal.Done(id);
        }
```

- [ ] **Step 8: Keep the app compiling, with the checkbox behaviour temporarily disabled**

`CRMPeyvand/InvoiceForm.xaml.cs` still calls the old three-argument `Create` and still calls the now-deleted `Done`. Make the two minimal edits so the solution builds and this task stays committable; Task 7 replaces them with the real behaviour.

Pass `null` at both call sites — one in `btnAdd_Click`, one in `Print_MouseLeftButtonDown`:

```csharp
                    savedInvoice = Ibll.Create(invoice, cbCustomer.id, draftLines.ToList(), null);
```

Delete this block from both methods:

```csharp
                if (IsCheckedOutImage.Visibility == Visibility.Visible)
                {
                    Ibll.Done(savedInvoice.id);
                }
```

Delete the whole `miDone_Click` method:

```csharp
        private void miDone_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Ibll.Done(invoiceEdit.id));
        }
```

Delete the `miDone` gate from `Window_Loaded`:

```csharp
            if (!AccessGuard.Can(u, Section.Invoices, Operation.Edit))
            {
                miDone.IsEnabled = false;
            }
            else
            {
                miDone.IsEnabled = true;
            }
```

The `miDone` XAML element stays for now so the markup still compiles; Task 7 renames and rewires it. Until then the checkbox does nothing — do not ship that state.

- [ ] **Step 9: Build and run everything**

Run: `dotnet build CRMPeyvand.sln`
Expected: succeeds.
Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: PASS.

- [ ] **Step 10: Commit**

```bash
git add BLL/PaymentBLL.cs BLL/InvoiceBLL.cs BLL/DashboardBLL.cs DAL/DashboardDAL.cs CRMPeyvand/InvoiceForm.xaml.cs CRMPeyvand.Tests/DashboardDALTests.cs
git commit -m "feat(receivables): expose payments through the BLL and count debtors"
```

---

### Task 6: The «دریافت‌ها» screen

**Files:**
- Create: `CRMPeyvand/PaymentsForm.xaml`, `CRMPeyvand/PaymentsForm.xaml.cs`
- Modify: `CRMPeyvand/MainWindow.xaml`, `CRMPeyvand/MainWindow.xaml.cs`, `CRMPeyvand/UsersForm.xaml.cs`

**Interfaces:**
- Consumes: `PaymentBLL`, `InvoiceBLL.ReadDetails`, `AccessGuard`, `Section.Payments` (Tasks 1 and 5).
- Produces: `PaymentsForm()` and `PaymentsForm(int invoiceId)`; a sidebar entry on `Key.V`.

No test can cover this task — a WPF window cannot be opened from a test run, which is why `PriceFieldTests` borrows an STA thread. Verification is `dotnet build` plus opening the app. Say so in the commit message.

The code-behind calls `PaymentReportModelFactory` and `PaymentReceiptDocument`, which Task 8 creates. Do Task 8's two report files first if you are executing strictly in order; otherwise create them now as stubs that return an empty model and fill them in during Task 8.

- [ ] **Step 1: Create the window markup**

Create `CRMPeyvand/PaymentsForm.xaml`:

```xml
<Window
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:hc="https://handyorg.github.io/handycontrol" x:Class="CRMPeyvand.PaymentsForm"
        mc:Ignorable="d"
        ResizeMode="NoResize"
        WindowStyle="None"
        WindowStartupLocation="CenterScreen"
        Background="Transparent"
        AllowsTransparency="True"
        FlowDirection="RightToLeft"

        Title="PaymentsForm" Height="620" Width="1000" Loaded="Window_Loaded" PreviewKeyDown="Window_PreviewKeyDown">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition/>
            <ColumnDefinition/>
            <ColumnDefinition/>
            <ColumnDefinition/>
            <ColumnDefinition/>
            <ColumnDefinition/>
            <ColumnDefinition/>
            <ColumnDefinition/>
            <ColumnDefinition/>
            <ColumnDefinition/>
            <ColumnDefinition/>
            <ColumnDefinition/>
        </Grid.ColumnDefinitions>
        <Grid.RowDefinitions>
            <RowDefinition/>
            <RowDefinition Height="50"/>
            <RowDefinition/>
            <RowDefinition/>
            <RowDefinition/>
            <RowDefinition/>
            <RowDefinition/>
            <RowDefinition/>
            <RowDefinition/>
            <RowDefinition/>
            <RowDefinition/>
            <RowDefinition/>
        </Grid.RowDefinitions>
        <!--Background-->
        <Border Grid.ColumnSpan="12" Grid.RowSpan="12" BorderThickness="5" BorderBrush="#eeeeee" CornerRadius="10">
            <Rectangle Fill="#eeeeee"/>
        </Border>
        <!--Background-->

        <!--ثبت وصولی-->
        <GroupBox Style="{StaticResource GroupBox}" Header="ثبت وصولی" Grid.ColumnSpan="12" Grid.RowSpan="5" Margin="10,5,10,20" />

        <Label Style="{StaticResource TextBoxLabel}" Grid.Column="0" Content="شماره فاکتور :" HorizontalAlignment="Left" Grid.RowSpan="2" VerticalAlignment="Center" Margin="18,0,0,0" Grid.ColumnSpan="2" Width="90"/>
        <Border Style="{StaticResource InputTextBoxBorder}" HorizontalAlignment="Left" Grid.RowSpan="2" VerticalAlignment="Top" Grid.Column="1" Grid.ColumnSpan="5" Margin="31,36,0,0">
            <TextBox Style="{StaticResource InputTextBox}" x:Name="txtInvoiceNumber" TabIndex="0" TextChanged="txtInvoiceNumber_TextChanged" ToolTip="شماره فاکتوری که بابت آن وصولی ثبت میشود"/>
        </Border>
        <Label Style="{StaticResource TextBoxLabel}" x:Name="lblInvoiceInfo" Content="" HorizontalAlignment="Left" Grid.RowSpan="2" VerticalAlignment="Center" Margin="18,0,0,0" Grid.Column="6" Grid.ColumnSpan="5" Width="300"/>

        <Label Style="{StaticResource TextBoxLabel}" Grid.Column="0" Content="مبلغ وصولی :" HorizontalAlignment="Left" VerticalAlignment="Center" Margin="18,0,0,0" Grid.Row="2" Grid.ColumnSpan="2" Width="90"/>
        <Border Style="{StaticResource InputTextBoxBorder}" HorizontalAlignment="Left" VerticalAlignment="Center" Grid.Column="1" Grid.ColumnSpan="3" Grid.Row="2" Margin="34,0,0,0">
            <TextBox Style="{StaticResource InputTextBox}" x:Name="txtAmount" TabIndex="1" TextChanged="txtAmount_TextChanged" PreviewKeyDown="txtAmount_PreviewKeyDown"/>
        </Border>
        <Label Style="{StaticResource TextBoxLabel}" Grid.Column="4" Content="تومان" HorizontalAlignment="Left" VerticalAlignment="Center" Margin="8,0,0,0" Grid.Row="2" Grid.ColumnSpan="2" Width="50"/>

        <Label Style="{StaticResource TextBoxLabel}" Grid.Column="6" Content="روش پرداخت :" HorizontalAlignment="Left" VerticalAlignment="Center" Margin="18,0,0,0" Grid.Row="2" Grid.ColumnSpan="2" Width="90"/>
        <Border Style="{StaticResource InputTextBoxBorder}" HorizontalAlignment="Left" VerticalAlignment="Center" Grid.Column="8" Grid.ColumnSpan="3" Grid.Row="2" Margin="8,0,0,0">
            <ComboBox x:Name="cbInstrument" TabIndex="2"/>
        </Border>

        <Label Style="{StaticResource TextBoxLabel}" Grid.Column="0" Content="تاریخ وصول :" HorizontalAlignment="Left" VerticalAlignment="Center" Margin="18,0,0,0" Grid.Row="3" Grid.ColumnSpan="2" Width="90"/>
        <hc:PersianDatePicker x:Name="dpPaymentDate" FontFamily="Shabnam FD" FontWeight="Bold" Grid.Column="1" Grid.ColumnSpan="3" Grid.Row="3" Margin="31,8,0,8" Height="32" TabIndex="3"/>

        <Label Style="{StaticResource TextBoxLabel}" Grid.Column="6" Content="شماره پیگیری (اختیاری) :" HorizontalAlignment="Left" VerticalAlignment="Center" Margin="18,0,0,0" Grid.Row="3" Grid.ColumnSpan="3" Width="150"/>
        <Border Style="{StaticResource InputTextBoxBorder}" HorizontalAlignment="Left" VerticalAlignment="Center" Grid.Column="9" Grid.ColumnSpan="3" Grid.Row="3" Margin="8,0,0,0">
            <TextBox Style="{StaticResource InputTextBox}" x:Name="txtReference" TabIndex="4"/>
        </Border>

        <Border Style="{StaticResource ButtonBorder}" Grid.Column="0" Grid.ColumnSpan="7" HorizontalAlignment="Left" Grid.Row="4" VerticalAlignment="Center" Margin="70,0,0,0" Width="300">
            <Button Style="{StaticResource Button}" x:Name="btnAdd" Content="ثبت وصولی" TabIndex="5" Click="btnAdd_Click" Width="300"/>
        </Border>
        <Border Style="{StaticResource ButtonBorder}" Grid.Column="7" Grid.ColumnSpan="5" HorizontalAlignment="Left" Grid.Row="4" VerticalAlignment="Center" Margin="30,0,0,0" Width="300">
            <Button Style="{StaticResource Button}" x:Name="btnPrintReceipt" Content="چاپ رسید دریافت" TabIndex="6" Click="btnPrintReceipt_Click" Width="300"/>
        </Border>
        <!--ثبت وصولی-->

        <!--جستجو-->
        <GroupBox Style="{StaticResource GroupBox}" Header="جستجوی وصولی ها" Grid.Row="5" Grid.RowSpan="2" Grid.ColumnSpan="12" Margin="10,25,10,0"/>
        <Border Style="{StaticResource InputTextBoxBorder}" Grid.Column="0" Grid.ColumnSpan="12" HorizontalAlignment="Left" VerticalAlignment="Center" Grid.Row="6" Margin="20,0,0,0" Width="940">
            <TextBox Style="{StaticResource InputTextBox}" x:Name="txtSearch" TabIndex="7" TextChanged="txtSearch_TextChanged" ToolTip="جستجو بر اساس شماره فاکتور یا نام مشتری"/>
        </Border>
        <Image Source="/Images/Search.png" Grid.Column="11" Grid.Row="6" Height="25" Margin="0,12,0,12"/>
        <!--جستجو-->

        <!--نمایش وصولی ها-->
        <Border Style="{StaticResource DataGridBorder}" Grid.Column="0" Grid.ColumnSpan="12" Grid.Row="7" Grid.RowSpan="4" Margin="10,10,10,0">
            <DataGrid Style="{StaticResource DataGrid}" x:Name="dgvPayments" ContextMenuOpening="dgvPayments_ContextMenuOpening" Grid.ColumnSpan="12" Margin="0,0,-1,0" Grid.RowSpan="4">
                <DataGrid.ContextMenu>
                    <ContextMenu Style="{StaticResource ContextMenu}">
                        <MenuItem Style="{StaticResource MenuItem}" x:Name="miPrint" Header="چاپ رسید دریافت" Click="miPrint_Click">
                            <MenuItem.Icon>
                                <Image Source="/Images/printer.png"/>
                            </MenuItem.Icon>
                        </MenuItem>
                        <MenuItem Style="{StaticResource MenuItem}" x:Name="miVoid" Header="ابطال وصولی" Click="miVoid_Click">
                            <MenuItem.Icon>
                                <Image Source="/Images/Delete.png"/>
                            </MenuItem.Icon>
                        </MenuItem>
                    </ContextMenu>
                </DataGrid.ContextMenu>
            </DataGrid>
        </Border>
        <!--نمایش وصولی ها-->

        <!--منوی پایین-->
        <Image Style="{StaticResource BackToHomeImage}" x:Name="BackToHome" MouseLeftButtonDown="BackToHome_MouseLeftButtonDown" Grid.Column="11" Margin="15,44,10,6" Grid.Row="10" Width="40" Height="40" Grid.RowSpan="2"/>
        <Label Style="{StaticResource TextBoxLabel}" FontSize="14" Grid.Column="0" Grid.ColumnSpan="3" Content="وصولی های ثبت شده :" HorizontalAlignment="Left" Grid.Row="11" VerticalAlignment="Top" Margin="10,6,0,0" Grid.RowSpan="1" Width="162" />
        <Label Style="{StaticResource TextBoxLabel}" x:Name="lblCount" FontSize="20" FontWeight="Bold" Grid.Column="2" Content="0" HorizontalAlignment="Left" Grid.Row="11" VerticalAlignment="Top" Margin="22,3,0,0" Grid.ColumnSpan="3" Width="203"/>
        <!--منوی پایین-->
    </Grid>
</Window>
```

- [ ] **Step 2: Create the code-behind**

Create `CRMPeyvand/PaymentsForm.xaml.cs`:

```csharp
using BE;
using Section = BE.Section;
using BLL;
using CRMPeyvand.Reports.Documents;
using CRMPeyvand.Reports.Services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CRMPeyvand
{
    /// <summary>
    /// The «دریافت‌ها» ledger: record money received against an invoice, print the
    /// receipt, void a mistake.
    ///
    /// A payment is never edited. It may already have a printed receipt in the
    /// customer's hand, so a correction is a void plus a fresh payment.
    /// </summary>
    public partial class PaymentsForm : Window
    {
        /// <summary>The grid is the source of invoices, so 0 means "not chosen yet".</summary>
        private const int NoInvoice = 0;

        private const string AmountColumn = "مبلغ";

        private readonly int preselectedInvoiceId;
        private Invoice selectedInvoice;
        private Payment paymentEdit;
        private Payment lastRecorded;
        private User u = new User();

        private readonly PaymentBLL Pbll = new PaymentBLL();
        private readonly InvoiceBLL Ibll = new InvoiceBLL();

        public PaymentsForm() : this(NoInvoice)
        {
        }

        /// <summary>Opened from an unpaid invoice row, with that invoice filled in.</summary>
        public PaymentsForm(int invoiceId)
        {
            InitializeComponent();
            PublicMethods.ChangeToPersianCulture();
            this.preselectedInvoiceId = invoiceId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow w = (MainWindow)Application.Current.MainWindow;
            u = w.loggedInUser;

            btnAdd.IsEnabled = AccessGuard.Can(u, Section.Payments, Operation.Create);
            btnPrintReceipt.IsEnabled = AccessGuard.Can(u, Section.Payments, Operation.View);
            miPrint.IsEnabled = AccessGuard.Can(u, Section.Payments, Operation.View);
            miVoid.IsEnabled = AccessGuard.Can(u, Section.Payments, Operation.Delete);

            // Captions rather than enum values, because the caption is what the
            // employee reads. The order matches SelectedInstrument below.
            cbInstrument.ItemsSource = new[]
            {
                PaymentInstrumentTitles.Of(PaymentInstrument.Cash),
                PaymentInstrumentTitles.Of(PaymentInstrument.Card),
                PaymentInstrumentTitles.Of(PaymentInstrument.Transfer),
                PaymentInstrumentTitles.Of(PaymentInstrument.Cheque),
                PaymentInstrumentTitles.Of(PaymentInstrument.Other),
            };
            cbInstrument.SelectedIndex = 0;

            dpPaymentDate.SelectedDate = DateTime.Today;

            if (preselectedInvoiceId != NoInvoice)
            {
                txtInvoiceNumber.Text = preselectedInvoiceId.ToString();
                LoadInvoice();
            }

            FillPayments(Pbll.Read());
            lblCount.Content = Pbll.Count();
        }

        private void FillPayments(DataTable table)
        {
            PublicMethods.dgvFiller(dgvPayments, table, AmountColumn);
        }

        /// <summary>
        /// Resolves the typed invoice number and shows what is still owed on it, so
        /// the amount can default to the whole remainder instead of being typed
        /// blind.
        /// </summary>
        private void LoadInvoice()
        {
            selectedInvoice = null;
            lblInvoiceInfo.Content = "";

            if (!int.TryParse(txtInvoiceNumber.Text, out int invoiceId))
            {
                return;
            }

            selectedInvoice = Ibll.ReadDetails(invoiceId);
            if (selectedInvoice == null || selectedInvoice.DeleteStatus)
            {
                MessageBox.Show("فاکتور مورد نظر یافت نشد", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtInvoiceNumber.Clear();
                return;
            }

            lblInvoiceInfo.Content =
                $"مشتری: {selectedInvoice.Customer?.Name} - مانده حساب: {Money.Display(selectedInvoice.Balance)}";

            if (selectedInvoice.IsSettled)
            {
                MessageBox.Show("این فاکتور تسویه شده است", "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                txtAmount.Clear();
                return;
            }

            txtAmount.Text = Money.Group(selectedInvoice.Balance.ToString("0"));
        }

        private void txtInvoiceNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            PublicMethods.FilterNumber(textBox);
            LoadInvoice();
        }

        private void txtAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            PriceField.GroupAsTyped(textBox);
        }

        private void txtAmount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (PriceField.Backspace(textBox, e.Key))
            {
                e.Handled = true;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!AccessGuard.Can(u, Section.Payments, Operation.Create))
            {
                return;
            }

            if (selectedInvoice == null || selectedInvoice.IsSettled)
            {
                MessageBox.Show("لطفا شماره فاکتوری که هنوز تسویه نشده را وارد کنید", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // The field is grouped for reading, so the separators come back off
            // before it is a number again.
            int? amount = Money.ParseWhole(txtAmount.Text);
            if (!amount.HasValue)
            {
                MessageBox.Show("مبلغ وصولی را وارد کنید", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var payment = new Payment
            {
                Amount = amount.Value,
                RegDate = dpPaymentDate.SelectedDate ?? DateTime.Today,
                Instrument = SelectedInstrument(),
                Reference = string.IsNullOrWhiteSpace(txtReference.Text) ? null : txtReference.Text.Trim(),
                User = u,
            };

            try
            {
                string result = Pbll.Create(payment, selectedInvoice.id);
                MessageBox.Show(result, "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);

                lastRecorded = Pbll.ReadById(payment.Id);
                ResetEntry();
                LoadInvoice();
                FillPayments(Pbll.Read());
                lblCount.Content = Pbll.Count();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private PaymentInstrument SelectedInstrument()
        {
            switch (cbInstrument.SelectedIndex)
            {
                case 1: return PaymentInstrument.Card;
                case 2: return PaymentInstrument.Transfer;
                case 3: return PaymentInstrument.Cheque;
                case 4: return PaymentInstrument.Other;
                default: return PaymentInstrument.Cash;
            }
        }

        private void ResetEntry()
        {
            txtAmount.Clear();
            txtReference.Clear();
            dpPaymentDate.SelectedDate = DateTime.Today;
            cbInstrument.SelectedIndex = 0;
            paymentEdit = null;
            txtInvoiceNumber.Focus();
        }

        private void btnPrintReceipt_Click(object sender, RoutedEventArgs e)
        {
            Payment target = lastRecorded ?? paymentEdit;
            if (target == null)
            {
                MessageBox.Show("ابتدا یک وصولی را از جدول انتخاب کنید", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PrintReceipt(target);
        }

        private void PrintReceipt(Payment payment)
        {
            try
            {
                var model = PaymentReportModelFactory.FromPayment(payment);
                ReportViewerService.OpenReportPdf(new PaymentReceiptDocument(model), $"Receipt_{model.ReceiptNumber}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در چاپ رسید دریافت:\n" + ex.Message, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearch.Text != string.Empty)
            {
                FillPayments(Pbll.Search(txtSearch.Text));
            }
            else FillPayments(Pbll.Read());
        }

        private void dgvPayments_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            string receipt = PublicMethods.ReadTheEntityCode(dgvPayments, 0);
            if (receipt != null)
            {
                dgvPayments.ContextMenu.IsEnabled = true;
                paymentEdit = Pbll.ReadById(Convert.ToInt32(receipt));
            }
            else
            {
                dgvPayments.ContextMenu.IsEnabled = false;
                paymentEdit = null;
            }
        }

        private void miPrint_Click(object sender, RoutedEventArgs e)
        {
            if (paymentEdit != null && AccessGuard.Can(u, Section.Payments, Operation.View))
            {
                PrintReceipt(paymentEdit);
            }
        }

        private void miVoid_Click(object sender, RoutedEventArgs e)
        {
            if (!AccessGuard.Can(u, Section.Payments, Operation.Delete) || paymentEdit == null)
            {
                return;
            }

            MessageBoxResult confirmation = MessageBox.Show(
                "آیا از ابطال این وصولی مطمئن هستید ؟", "هشدار",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

            if (confirmation == MessageBoxResult.Yes)
            {
                MessageBox.Show(Pbll.Void(paymentEdit.Id), "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                paymentEdit = null;
                LoadInvoice();
                FillPayments(Pbll.Read());
                lblCount.Content = Pbll.Count();
            }
        }

        private void BackToHome_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.Close();
                    break;
            }
        }
    }
}
```

- [ ] **Step 3: Add the sidebar entry**

In `CRMPeyvand/MainWindow.xaml`, insert after the Reports `Image`/`Label` pair (line 82). Row 10 is empty in the sidebar column, so no `RowDefinition` change is needed:

```xml
        <Image Source="/Images/Checked.png" x:Name="PaymentIcon" Grid.Column="11" Grid.Row="10" Grid.ColumnSpan="1" Height="36" Width="45" Margin="19,0,21,0" VerticalAlignment="Center" Cursor="Hand" MouseLeftButtonDown="PaymentIcon_MouseLeftButtonDown" ToolTip="کلید میانبر V"/>
        <Label Style="{StaticResource MainWindowLabels}" x:Name="PaymentLabel" Content="دریافت ها" FontFamily="Shabnam FD" Grid.Row="10" Grid.ColumnSpan="2" Grid.Column="10" Height="40" Margin="0,20,64,0" HorizontalAlignment="Right" VerticalAlignment="Top" FontSize="11" Foreground="#4A8FE7" MouseEnter="Label_MouseEnter" MouseLeave="Label_MouseLeave" FontWeight="Bold" Cursor="Hand" MouseLeftButtonDown="PaymentIcon_MouseLeftButtonDown" ToolTip="کلید میانبر V"/>
```

`Checked.png` already ships in `CRMPeyvand/Images/`, so nothing is added to the project.

- [ ] **Step 4: Wire the handler, the flag, the guard and the shortcut**

In `CRMPeyvand/MainWindow.xaml.cs`:

Add the flag beside the others (lines 40-48):

```csharp
        bool EnterV;
```

Add the handler beside the other sidebar handlers, after `InvoiceIcon_MouseLeftButtonDown`:

```csharp
        private void PaymentIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PaymentsForm l = new PaymentsForm();
            openform(l);
            RefreshPage();
        }
```

Add the guard inside `LoadPage()`, after the `Section.Invoices` block:

```csharp
                if (!AccessGuard.Can(loggedInUser, Section.Payments, Operation.View))
                {
                    EnterV = false;
                    PaymentIcon.IsEnabled = false;
                    PaymentLabel.IsEnabled = false;
                    PaymentIcon.Opacity = 0.7;
                    PaymentLabel.Opacity = 0.7;
                }
                else
                {
                    EnterV = true;
                    PaymentIcon.IsEnabled = true;
                    PaymentLabel.IsEnabled = true;
                    PaymentIcon.Opacity = 1;
                    PaymentLabel.Opacity = 1;
                }
```

And add the disabled pair to the `else` branch taken when `loggedInUser == null`:

```csharp
                PaymentIcon.IsEnabled = false;
                PaymentLabel.IsEnabled = false;
```

Add the shortcut to `Window_PreviewKeyDown`, after the `case Key.F:` block:

```csharp
                case Key.V:
                    if (EnterV)
                    {
                        PaymentsForm list = new PaymentsForm();
                        openform(list);
                        RefreshPage();
                    }
                    break;
```

- [ ] **Step 5: Add the Persian caption**

In `CRMPeyvand/UsersForm.xaml.cs`, add to `SectionCaptions`:

```csharp
            { Section.Payments, "بخش وصولی ها" },
```

Without this the permissions matrix silently shows the English identifier, because `SectionCaptions.TryGetValue` falls back to `section.ToString()`.

- [ ] **Step 6: Build and run the tests**

Run: `dotnet build CRMPeyvand.sln`
Expected: succeeds.
Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: PASS.

- [ ] **Step 7: Verify by hand, then commit**

Launch the app and log in, then check: the sidebar shows «دریافت‌ها» with `Checked.png` and a «کلید میانبر V» tooltip; `V` opens it; typing an invoice number shows the customer and their مانده حساب and pre-fills the amount; saving adds a grid row and updates the count; right-click voids behind a confirmation; **Users → groups → the matrix now has a «بخش وصولی ها» row**.

```bash
git add CRMPeyvand/PaymentsForm.xaml CRMPeyvand/PaymentsForm.xaml.cs CRMPeyvand/MainWindow.xaml CRMPeyvand/MainWindow.xaml.cs CRMPeyvand/UsersForm.xaml.cs
git commit -m "feat(ui): add the دریافت‌ها screen and its permission section

Not covered by tests: a WPF window cannot be opened from a test run, so this
screen was verified by opening the app."
```

---

### Task 7: The invoice screen settles properly

**Files:**
- Modify: `CRMPeyvand/InvoiceForm.xaml`, `CRMPeyvand/InvoiceForm.xaml.cs`, `CRMPeyvand/InvoiceDetailsForm.xaml`, `CRMPeyvand/InvoiceDetailsForm.xaml.cs`, `CRMPeyvand/Reports/Models/InvoiceReportModel.cs`

**Interfaces:**
- Consumes: `InvoiceBLL.Create(..., Payment settledBy)` (Task 5), `PaymentsForm(int)` (Task 6).
- Produces: no new public types. `InvoiceReportModel` gains `PaidAmount` and `RemainingBalance`, which Task 8 fills and prints.

Like Task 6 this cannot be unit tested; verification is `dotnet build` plus opening the app.

- [ ] **Step 1: Add the two report-model fields**

In `CRMPeyvand/Reports/Models/InvoiceReportModel.cs`, add after `DiscountAmount`:

```csharp
        public double PaidAmount { get; set; }
        public double RemainingBalance { get; set; }
```

`double` because every other money field on this model is `double`; the cast from `decimal` happens in the factory.

- [ ] **Step 2: Teach the invoice grid its new money columns**

In `CRMPeyvand/InvoiceForm.xaml.cs`, replace the `AmountColumn` constant and `FillInvoices`:

```csharp
        /// <summary>
        /// The DAL's own names for the money columns, so the grid groups the
        /// columns the query produced rather than names repeated here. The line
        /// grid declares its own columns and already carries "N0".
        /// </summary>
        private const string InvoiceTotalColumn = "مبلغ کل فاکتور";
        private const string PaidColumn = "مبلغ پرداخت شده";
        private const string BalanceColumn = "مانده حساب";

        /// <summary>
        /// Every fill has to name the money columns again: a grid generated from a
        /// DataTable throws its columns away and rebuilds them each time, and
        /// searching narrows the rows.
        /// </summary>
        private void FillInvoices(DataTable table)
        {
            PublicMethods.dgvFiller(dgvInvoices, table, InvoiceTotalColumn, PaidColumn, BalanceColumn);
        }
```

- [ ] **Step 3: Make the checkbox record a real payment**

In `CRMPeyvand/InvoiceForm.xaml.cs`, add a field next to the other instance fields:

```csharp
        private DateTime invoiceRegDate;
```

Add this helper next to `ResetTotals`:

```csharp
        /// <summary>
        /// The «وضعیت پرداخت» checkbox as a Payment, or null when it is unticked.
        ///
        /// It used to mean "call InvoiceDAL.Done afterwards", which set a boolean
        /// and recorded no amount, in a second transaction. It now means the full
        /// payable arrived at the counter, and InvoiceDAL.Create writes the payment
        /// inside the transaction that creates the invoice. Settling at the counter
        /// is the commonest transaction in the shop, so this stays a one-click path.
        /// </summary>
        private Payment SettledBy()
        {
            if (IsCheckedOutImage.Visibility != Visibility.Visible)
            {
                return null;
            }

            return new Payment
            {
                Instrument = PaymentInstrument.Cash,
                RegDate = invoiceRegDate,
                User = u,
            };
        }
```

Replace `btnAdd_Click` (lines 221-254) with:

```csharp
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (txtCustomer.SelectedItem != null && dgvProduts.Items.Count != 0)
            {
                Invoice invoice = new Invoice();
                invoiceRegDate = DateTime.Now;
                invoice.RegDate = invoiceRegDate;
                invoice.OffCode = countOff();
                invoice.User = u;
                Invoice savedInvoice;
                try
                {
                    savedInvoice = Ibll.Create(invoice, cbCustomer.id, draftLines.ToList(), SettledBy());
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                MessageBox.Show($"فاکتور با شماره {savedInvoice.id} ثبت شد", "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                FillInvoices(Ibll.Read());
                lblCount.Content = Ibll.CountInvoices();
                dgvProduts.ItemsSource = null;
                draftLines.Clear();
                lstResult.Items.Clear();
                ResetTotals();
                txtOff.Clear();
                txtCustomer.Focus();
            }
            else MessageBox.Show("لطفا تمامی فیلد های مورد نیاز را پر کنید");
        }
```

Replace `Print_MouseLeftButtonDown` (lines 328-374) with the same body plus the print block:

```csharp
        private void Print_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (txtCustomer.SelectedItem != null && dgvProduts.Items.Count != 0)
            {
                Invoice invoice = new Invoice();
                invoiceRegDate = DateTime.Now;
                invoice.RegDate = invoiceRegDate;
                invoice.OffCode = countOff();
                invoice.User = u;
                Invoice savedInvoice;
                try
                {
                    savedInvoice = Ibll.Create(invoice, cbCustomer.id, draftLines.ToList(), SettledBy());
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                MessageBox.Show($"فاکتور با شماره {savedInvoice.id} ثبت شد", "اطلاعیه", MessageBoxButton.OK, MessageBoxImage.Information);
                FillInvoices(Ibll.Read());
                lblCount.Content = Ibll.CountInvoices();
                try
                {
                    string invNum = savedInvoice.id.ToString();
                    var reportModel = InvoiceReportModelFactory.FromInvoice(savedInvoice);

                    var doc = new InvoiceDocument(reportModel);
                    ReportViewerService.OpenReportPdf(doc, $"Invoice_{invNum}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطا در چاپ فاکتور:\n" + ex.Message, "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                dgvProduts.ItemsSource = null;
                draftLines.Clear();
                lstResult.Items.Clear();
                ResetTotals();
                txtOff.Clear();
                txtCustomer.Focus();
            }
            else MessageBox.Show("لطفا تمامی فیلد های مورد نیاز را پر کنید");
        }
```

The printed invoice now shows paid and balance because `savedInvoice.Payments` holds the payment when the checkbox was ticked, which Task 8's factory change reads.

- [ ] **Step 4: Turn the context-menu item into "record a payment"**

In `CRMPeyvand/InvoiceForm.xaml`, replace the `miDone` `MenuItem`:

```xml
                        <MenuItem Style="{StaticResource MenuItem}" x:Name="miRecordPayment" Header="ثبت وصولی" Click="miRecordPayment_Click">
                            <MenuItem.Icon>
                                <Image Source="/Images/Checked.png"/>
                            </MenuItem.Icon>
                        </MenuItem>
```

In `CRMPeyvand/InvoiceForm.xaml.cs`, add after `miDeleteInvoice_Click`:

```csharp
        private void miRecordPayment_Click(object sender, RoutedEventArgs e)
        {
            if (!AccessGuard.Can(u, Section.Payments, Operation.Create))
            {
                MessageBox.Show("شما به ثبت وصولی دسترسی ندارید", "هشدار", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (invoiceEdit != null && invoiceEdit.id != 0)
            {
                PaymentsForm form = new PaymentsForm(invoiceEdit.id);
                form.ShowDialog();
                FillInvoices(Ibll.Read());
                lblCount.Content = Ibll.CountInvoices();
            }
        }
```

And add its gate to `Window_Loaded`, where the `miDone` gate used to be:

```csharp
            if (!AccessGuard.Can(u, Section.Payments, Operation.Create))
            {
                miRecordPayment.IsEnabled = false;
            }
            else
            {
                miRecordPayment.IsEnabled = true;
            }
```

- [ ] **Step 5: Show paid and balance on the details screen**

In `CRMPeyvand/InvoiceDetailsForm.xaml`, add two more label/value pairs to the `WrapPanel` in the «مشخصات فاکتور» `GroupBox`, directly after the `تاریخ پرداخت` pair:

```xml
                <StackPanel Orientation="Horizontal" Margin="0,4,34,4">
                    <Label Content="مبلغ پرداخت شده :" />
                    <Label x:Name="lblPaidAmount" FontWeight="Bold"/>
                </StackPanel>
                <StackPanel Orientation="Horizontal" Margin="0,4,34,4">
                    <Label Content="مانده حساب :" />
                    <Label x:Name="lblBalance" FontWeight="Bold"/>
                </StackPanel>
```

The `WrapPanel` wraps, so a wider totals row is safe here.

In `CRMPeyvand/InvoiceDetailsForm.xaml.cs`, add to `ShowDetails()`, after `lblFinalTotal.Content`:

```csharp
            lblPaidAmount.Content = model.PaidAmount.ToString("N0");
            lblBalance.Content = model.RemainingBalance.ToString("N0");
```

- [ ] **Step 6: Build and test**

Run: `dotnet build CRMPeyvand.sln`
Expected: succeeds.
Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: PASS. The factory does not fill the two new fields yet, so the printed figures stay zero until Task 8; that is the only known incomplete behaviour at this point.

- [ ] **Step 7: Verify by hand, then commit**

Tick «وضعیت پرداخت» and save an invoice: the grid shows «پرداخت شده» with a zero «مانده حساب», and a Payment row exists. Leave it unticked and save: «پرداخت نشده» with the full amount owed. Right-click an unpaid invoice → «ثبت وصولی» opens the payments screen with the number already filled in. Open «مشاهده» on an invoice and confirm the two new labels appear.

```bash
git add CRMPeyvand/InvoiceForm.xaml CRMPeyvand/InvoiceForm.xaml.cs CRMPeyvand/InvoiceDetailsForm.xaml CRMPeyvand/InvoiceDetailsForm.xaml.cs CRMPeyvand/Reports/Models/InvoiceReportModel.cs
git commit -m "feat(ui): settle an invoice with a real payment from the invoice screen"
```

---

### Task 8: The printed documents

The receipt, the statement, and the invoice document gaining paid and balance.

**Files:**
- Create: `CRMPeyvand/Reports/Models/PaymentReportModel.cs`, `CRMPeyvand/Reports/Models/CustomerBalanceReportModel.cs`, `CRMPeyvand/Reports/Services/PaymentReportModelFactory.cs`, `CRMPeyvand/Reports/Documents/PaymentReceiptDocument.cs`, `CRMPeyvand/Reports/Documents/CustomerBalanceDocument.cs`, `CRMPeyvand.Tests/Reporting/PaymentReportTests.cs`
- Modify: `CRMPeyvand/Reports/Services/InvoiceReportModelFactory.cs`, `CRMPeyvand/Reports/Documents/InvoiceDocument.cs`, `CRMPeyvand/ReportsWindow.xaml`, `CRMPeyvand/ReportsWindow.xaml.cs`, `CRMPeyvand.Tests/InvoiceReportModelFactoryTests.cs`

**Interfaces:**
- Consumes: `BE.Payment`, `BE.PaymentInstrumentTitles`, `PaymentBLL` (Tasks 1, 5, 6); `InvoiceReportModel.PaidAmount`/`.RemainingBalance` (Task 7).
- Produces: `PaymentReportModelFactory.FromPayment(Payment) → PaymentReportModel`, `PaymentReceiptDocument`, `CustomerBalanceDocument`, and a filled-in `InvoiceReportModelFactory` and `InvoiceDocument`.

- [ ] **Step 1: Write the failing tests**

Create `CRMPeyvand.Tests/Reporting/PaymentReportTests.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using BE;
using CRMPeyvand.Reports.Documents;
using CRMPeyvand.Reports.Models;
using CRMPeyvand.Reports.Services;
using QuestPDF.Infrastructure;
using Xunit;

namespace CRMPeyvand.Tests.Reporting
{
    public class PaymentReportTests
    {
        public PaymentReportTests()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private static Payment PaymentWith(decimal invoiceTotal, decimal alreadyPaid, decimal amount)
        {
            var customer = new Customer { Name = "علی رضایی", Phone = "09121234567" };
            var invoice = new Invoice { RegDate = new DateTime(2026, 10, 1), DiscountAmount = 0m, Customer = customer };
            invoice.Lines.Add(new InvoiceLine
            {
                Quantity = 1,
                UnitPrice = invoiceTotal,
                CatalogItem = new CatalogItem { Name = "کالا", Kind = ItemKind.Good, SalePrice = invoiceTotal },
            });
            invoice.Payments.Add(new Payment { Amount = alreadyPaid, RegDate = new DateTime(2026, 10, 2) });
            invoice.Payments.Add(new Payment { Amount = amount, RegDate = new DateTime(2026, 10, 4) });

            return invoice.Payments[1];
        }

        private static void AssertRendersPdf(IDocument document)
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pdf");
            try
            {
                document.GeneratePdf(path);
                var bytes = File.ReadAllBytes(path);
                Assert.True(bytes.Length > 0);
                Assert.Equal((byte)'%', bytes[0]);
                Assert.Equal((byte)'P', bytes[1]);
                Assert.Equal((byte)'D', bytes[2]);
                Assert.Equal((byte)'F', bytes[3]);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void The_receipt_carries_the_numbers_the_customer_needs()
        {
            var model = PaymentReportModelFactory.FromPayment(PaymentWith(10000m, 4000m, 6000m));

            Assert.Equal("رسید دریافت", model.ReceiptTitle);
            Assert.Equal("10000", model.InvoiceTotal.ToString("0"));
            Assert.Equal("10000", model.InvoicePaidAfterThisReceipt.ToString("0"));
            Assert.Equal("6000", model.Amount.ToString("0"));
            Assert.Equal("0", model.RemainingBalance.ToString("0"));
            Assert.Equal("علی رضایی", model.CustomerName);
        }

        [Fact]
        public void The_receipt_shows_what_is_still_owed_after_a_partial_payment()
        {
            var model = PaymentReportModelFactory.FromPayment(PaymentWith(10000m, 4000m, 2000m));

            Assert.Equal("6000", model.InvoicePaidAfterThisReceipt.ToString("0"));
            Assert.Equal("4000", model.RemainingBalance.ToString("0"));
        }

        [Fact]
        public void The_receipt_names_the_instrument_and_the_operator()
        {
            var payment = PaymentWith(10000m, 0m, 1000m);
            payment.Instrument = PaymentInstrument.Cheque;
            payment.Reference = "۷۷۸۸";
            payment.User = new User { Name = "احسان", UserName = "ehsan" };

            var model = PaymentReportModelFactory.FromPayment(payment);

            Assert.Equal("چک", model.InstrumentTitle);
            Assert.Equal("۷۷۸۸", model.Reference);
            Assert.Equal("احسان", model.ReceivedBy);
        }

        [Fact]
        public void A_receipt_falls_back_to_the_username_when_the_operator_has_no_name()
        {
            var payment = PaymentWith(10000m, 0m, 1000m);
            payment.User = new User { UserName = "ehsan" };

            Assert.Equal("ehsan", PaymentReportModelFactory.FromPayment(payment).ReceivedBy);
        }

        [Fact]
        public void A_null_payment_still_produces_a_printable_receipt()
        {
            var model = PaymentReportModelFactory.FromPayment(null);

            Assert.Equal("رسید دریافت", model.ReceiptTitle);
            AssertRendersPdf(new PaymentReceiptDocument(model));
        }

        [Fact]
        public void The_receipt_renders()
        {
            AssertRendersPdf(new PaymentReceiptDocument(
                PaymentReportModelFactory.FromPayment(PaymentWith(10000m, 4000m, 6000m))));
        }

        [Fact]
        public void The_statement_totals_the_balance_column()
        {
            var model = new CustomerBalanceReportModel
            {
                Customers = new List<CustomerBalanceRowModel>
                {
                    new CustomerBalanceRowModel { RowIndex = 1, Name = "علی", Balance = 5000m },
                    new CustomerBalanceRowModel { RowIndex = 2, Name = "سمیرا", Balance = 2500m },
                },
            };

            Assert.Equal(2, model.DebtorCount);
            Assert.Equal(7500m, model.TotalBalance);
        }

        [Fact]
        public void The_statement_renders()
        {
            var model = new CustomerBalanceReportModel
            {
                GeneratedDatePersian = "1405/07/12",
                Customers = new List<CustomerBalanceRowModel>
                {
                    new CustomerBalanceRowModel
                    {
                        RowIndex = 1,
                        Name = "علی رضایی",
                        Phone = "09121234567",
                        InvoiceCount = 3,
                        TotalBilled = 12000m,
                        TotalReceived = 7000m,
                        Balance = 5000m,
                    },
                },
            };

            AssertRendersPdf(new CustomerBalanceDocument(model));
        }

        [Fact]
        public void An_empty_statement_renders_rather_than_throwing()
        {
            AssertRendersPdf(new CustomerBalanceDocument(null));
        }
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~PaymentReportTests`
Expected: compile failure — the report types do not exist.

- [ ] **Step 3: Write the receipt model**

Create `CRMPeyvand/Reports/Models/PaymentReportModel.cs`:

```csharp
namespace CRMPeyvand.Reports.Models
{
    /// <summary>
    /// One رسید دریافت. Money is double, as it is on every other report model in
    /// this project; the entity side is decimal and the cast happens in the factory.
    /// </summary>
    public class PaymentReportModel
    {
        public string ReceiptTitle { get; set; } = "رسید دریافت";
        public string ReceiptNumber { get; set; } = string.Empty;
        public string PaymentDatePersian { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string InvoiceDatePersian { get; set; } = string.Empty;
        public double InvoiceTotal { get; set; }
        public double InvoicePaidAfterThisReceipt { get; set; }
        public double Amount { get; set; }
        public double RemainingBalance { get; set; }
        public string InstrumentTitle { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string ReceivedBy { get; set; } = string.Empty;
    }
}
```

- [ ] **Step 4: Write the receipt factory**

Create `CRMPeyvand/Reports/Services/PaymentReportModelFactory.cs`:

```csharp
using BE;
using CRMPeyvand.Reports.Common;
using CRMPeyvand.Reports.Models;
using System;

namespace CRMPeyvand.Reports.Services
{
    /// <summary>
    /// Single mapping from a persisted Payment to the printable receipt, so the
    /// screen and the PDF can never disagree about a payment. Null-tolerant in the
    /// same way InvoiceReportModelFactory is.
    /// </summary>
    public static class PaymentReportModelFactory
    {
        public static PaymentReportModel FromPayment(Payment payment)
        {
            if (payment == null)
                return new PaymentReportModel();

            var invoice = payment.Invoice;

            // What the invoice has received including this receipt. Invoice.Paid
            // already covers the payment when the caller loaded the whole graph;
            // the extra term covers the caller that holds a payment saved after
            // the invoice's payments were read.
            double paidAfter = invoice == null
                ? 0d
                : (double)(invoice.Paid + (invoice.Payments == null || invoice.Payments.Contains(payment)
                        ? 0m
                        : payment.Amount));

            double total = invoice == null ? 0d : (double)invoice.Payable;

            return new PaymentReportModel
            {
                ReceiptNumber = payment.Id.ToString(),
                PaymentDatePersian = PersianReportStyle.FormatPersianDate(payment.RegDate),
                CustomerName = invoice?.Customer?.Name ?? string.Empty,
                CustomerPhone = invoice?.Customer?.Phone ?? string.Empty,
                InvoiceNumber = invoice?.id.ToString() ?? string.Empty,
                InvoiceDatePersian = invoice == null
                    ? string.Empty
                    : PersianReportStyle.FormatPersianDate(invoice.RegDate),
                InvoiceTotal = total,
                InvoicePaidAfterThisReceipt = paidAfter,
                Amount = (double)payment.Amount,
                RemainingBalance = Math.Max(0d, total - paidAfter),
                InstrumentTitle = PaymentInstrumentTitles.Of(payment.Instrument),
                Reference = payment.Reference ?? string.Empty,
                // Name-then-UserName, the fallback every other report uses.
                ReceivedBy = payment.User?.Name ?? payment.User?.UserName ?? string.Empty,
            };
        }
    }
}
```

- [ ] **Step 5: Write the receipt document**

Create `CRMPeyvand/Reports/Documents/PaymentReceiptDocument.cs`:

```csharp
using System;
using CRMPeyvand.Reports.Common;
using CRMPeyvand.Reports.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CRMPeyvand.Reports.Documents
{
    /// <summary>
    /// QuestPDF document for a single رسید دریافت. The customer keeps this, so it
    /// states what the invoice came to, what this receipt pays, and what is left.
    /// </summary>
    public class PaymentReceiptDocument : IDocument
    {
        public PaymentReportModel Model { get; }

        public PaymentReceiptDocument(PaymentReportModel model)
        {
            Model = model ?? new PaymentReportModel();
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = $"رسید دریافت {Model.ReceiptNumber}".Trim(),
            Author = "CRMPeyvand",
            Subject = "رسید دریافت",
            CreationDate = DateTime.Now
        };

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2f, Unit.Centimetre);
                page.PageColor(PersianReportStyle.ColorWhite);
                page.DefaultTextStyle(PersianReportStyle.DefaultTextStyle);
                page.ContentFromRightToLeft();

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.PaddingBottom(12).BorderBottom(1).BorderColor(PersianReportStyle.ColorBorder).PaddingBottom(8).Column(col =>
            {
                col.Item().Text(string.IsNullOrWhiteSpace(Model.ReceiptTitle) ? "رسید دریافت" : Model.ReceiptTitle)
                    .Style(PersianReportStyle.TitleTextStyle);
                col.Item().Text("سامانه مدیریت مشتریان پیوند").Style(PersianReportStyle.SubtitleTextStyle);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(16).Column(column =>
            {
                column.Spacing(14);
                column.Item().Element(ComposeDetailsCard);
                column.Item().Element(ComposeAmountsCard);
                column.Item().Element(ComposeSignatures);
            });
        }

        private void ComposeDetailsCard(IContainer container)
        {
            container.Border(1)
                .BorderColor(PersianReportStyle.ColorBorder)
                .Background(PersianReportStyle.ColorWhite)
                .Padding(10)
                .Column(col =>
                {
                    col.Item().Text("مشخصات وصولی").SemiBold().FontColor(PersianReportStyle.ColorPrimary);
                    col.Spacing(4);

                    Detail(col, "شماره رسید", Model.ReceiptNumber);
                    Detail(col, "تاریخ وصول", Model.PaymentDatePersian);
                    Detail(col, "نام مشتری", Model.CustomerName);
                    Detail(col, "شماره تماس", Model.CustomerPhone);
                    Detail(col, "شماره فاکتور", Model.InvoiceNumber);
                    Detail(col, "تاریخ فاکتور", Model.InvoiceDatePersian);
                    Detail(col, "روش پرداخت", Model.InstrumentTitle);
                    Detail(col, "شماره پیگیری", Model.Reference);
                    Detail(col, "دریافت توسط", Model.ReceivedBy);
                });
        }

        private static void Detail(ColumnDescriptor col, string label, string value)
        {
            col.Item().Row(r =>
            {
                r.AutoItem().Text(label + ": ").Style(PersianReportStyle.SummaryLabelStyle);
                r.RelativeItem().Text(string.IsNullOrWhiteSpace(value) ? "-" : value);
            });
        }

        private void ComposeAmountsCard(IContainer container)
        {
            container.Border(1)
                .BorderColor(PersianReportStyle.ColorBorder)
                .Background(PersianReportStyle.ColorZebraBg)
                .Padding(10)
                .Column(col =>
                {
                    col.Spacing(4);

                    col.Item().Row(r =>
                    {
                        r.AutoItem().Text("مبلغ کل فاکتور:").Style(PersianReportStyle.SummaryLabelStyle);
                        r.RelativeItem().AlignLeft().Text(PersianReportStyle.FormatCurrency(Model.InvoiceTotal)).Style(PersianReportStyle.SummaryValueStyle);
                    });

                    col.Item().Row(r =>
                    {
                        r.AutoItem().Text("مبلغ وصولی:").SemiBold().FontColor(PersianReportStyle.ColorAccent);
                        r.RelativeItem().AlignLeft().Text(PersianReportStyle.FormatCurrency(Model.Amount)).Style(PersianReportStyle.TotalHighlightStyle);
                    });

                    col.Item().LineHorizontal(0.5f).LineColor(PersianReportStyle.ColorBorder);

                    col.Item().Row(r =>
                    {
                        r.AutoItem().Text("مانده حساب فاکتور:").Style(PersianReportStyle.SummaryLabelStyle);
                        r.RelativeItem().AlignLeft().Text(PersianReportStyle.FormatCurrency(Model.RemainingBalance)).Style(PersianReportStyle.SummaryValueStyle);
                    });
                });
        }

        private void ComposeSignatures(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor(PersianReportStyle.ColorBorder).Padding(8).Column(col =>
                {
                    col.Item().Text("مهر و امضاء فروشنده").Style(PersianReportStyle.SummaryLabelStyle);
                    col.Item().Height(40);
                });

                row.ConstantItem(20);

                row.RelativeItem().Border(1).BorderColor(PersianReportStyle.ColorBorder).Padding(8).Column(col =>
                {
                    col.Item().Text("مهر و امضاء دریافت کننده").Style(PersianReportStyle.SummaryLabelStyle);
                    col.Item().Height(40);
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.BorderTop(1).BorderColor(PersianReportStyle.ColorBorder).PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Text("سامانه مدیریت مشتریان پیوند").FontSize(8).FontColor(PersianReportStyle.ColorMuted);

                row.RelativeItem().AlignLeft().Text(text =>
                {
                    text.Span("صفحه ").FontSize(8).FontColor(PersianReportStyle.ColorMuted);
                    text.CurrentPageNumber().FontSize(8).FontColor(PersianReportStyle.ColorMuted);
                    text.Span(" از ").FontSize(8).FontColor(PersianReportStyle.ColorMuted);
                    text.TotalPages().FontSize(8).FontColor(PersianReportStyle.ColorMuted);
                });
            });
        }
    }
}
```

- [ ] **Step 6: Write the statement model**

Create `CRMPeyvand/Reports/Models/CustomerBalanceReportModel.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;

namespace CRMPeyvand.Reports.Models
{
    /// <summary>
    /// One customer's outstanding position. Per customer rather than per invoice,
    /// because the question this answers is "who owes me and how much".
    /// </summary>
    public class CustomerBalanceRowModel
    {
        public int RowIndex { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int InvoiceCount { get; set; }
        public double TotalBilled { get; set; }
        public double TotalReceived { get; set; }
        public double Balance { get; set; }
    }

    public class CustomerBalanceReportModel
    {
        public string ReportTitle { get; set; } = "گزارش مانده حساب مشتریان";
        public string GeneratedDatePersian { get; set; } = string.Empty;
        public List<CustomerBalanceRowModel> Customers { get; set; } = new();
        public int DebtorCount => Customers.Count;
        public double TotalBalance => Customers.Sum(c => c.Balance);
    }
}
```

- [ ] **Step 7: Write the statement document**

Create `CRMPeyvand/Reports/Documents/CustomerBalanceDocument.cs`:

```csharp
using System;
using CRMPeyvand.Reports.Common;
using CRMPeyvand.Reports.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CRMPeyvand.Reports.Documents
{
    /// <summary>
    /// QuestPDF document for the مانده حساب statement.
    /// </summary>
    public class CustomerBalanceDocument : IDocument
    {
        public CustomerBalanceReportModel Model { get; }

        public CustomerBalanceDocument(CustomerBalanceReportModel model)
        {
            Model = model ?? new CustomerBalanceReportModel();
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = string.IsNullOrWhiteSpace(Model.ReportTitle) ? "گزارش مانده حساب مشتریان" : Model.ReportTitle,
            Author = "CRMPeyvand",
            Subject = "گزارش مانده حساب مشتریان",
            CreationDate = DateTime.Now
        };

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(PersianReportStyle.ColorWhite);
                page.DefaultTextStyle(PersianReportStyle.DefaultTextStyle);
                page.ContentFromRightToLeft();

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.PaddingBottom(12).BorderBottom(1).BorderColor(PersianReportStyle.ColorBorder).PaddingBottom(8).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(string.IsNullOrWhiteSpace(Model.ReportTitle) ? "گزارش مانده حساب مشتریان" : Model.ReportTitle)
                        .Style(PersianReportStyle.TitleTextStyle);
                    col.Item().Text("سامانه مدیریت مشتریان پیوند").Style(PersianReportStyle.SubtitleTextStyle);
                });

                row.ConstantItem(180).AlignLeft().Column(col =>
                {
                    if (!string.IsNullOrWhiteSpace(Model.GeneratedDatePersian))
                    {
                        col.Item().Row(r =>
                        {
                            r.AutoItem().Text("تاریخ گزارش: ").Style(PersianReportStyle.SummaryLabelStyle);
                            r.RelativeItem().Text(Model.GeneratedDatePersian).Bold();
                        });
                    }
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(10).Column(column =>
            {
                column.Spacing(12);
                column.Item().Element(ComposeTable);
                column.Item().Element(ComposeSummaryCard);
            });
        }

        private void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40); // ردیف
                    columns.RelativeColumn(3);  // نام مشتری
                    columns.RelativeColumn(2);  // شماره تماس
                    columns.ConstantColumn(60); // تعداد فاکتور
                    columns.RelativeColumn(2);  // مبلغ کل
                    columns.RelativeColumn(2);  // مبلغ وصولی
                    columns.RelativeColumn(2);  // مانده حساب
                });

                table.Header(header =>
                {
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("ردیف").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("نام مشتری").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("شماره تماس").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("تعداد فاکتور").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("مبلغ کل").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("مبلغ وصولی").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("مانده حساب").Style(PersianReportStyle.TableHeaderTextStyle);
                });

                if (Model.Customers != null && Model.Customers.Count > 0)
                {
                    for (int i = 0; i < Model.Customers.Count; i++)
                    {
                        var item = Model.Customers[i];
                        bool isZebra = i % 2 == 1;
                        int rowIndex = item.RowIndex > 0 ? item.RowIndex : i + 1;

                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(rowIndex.ToString());
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).PaddingRight(6).Text(item.Name ?? string.Empty);
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(item.Phone ?? string.Empty);
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(PersianReportStyle.FormatNumber(item.InvoiceCount));
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(PersianReportStyle.FormatCurrency(item.TotalBilled));
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(PersianReportStyle.FormatCurrency(item.TotalReceived));
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(PersianReportStyle.FormatCurrency(item.Balance));
                    }
                }
                else
                {
                    table.Cell().ColumnSpan(7).Element(c => PersianReportStyle.TableBodyCell(c, false))
                        .AlignCenter().Padding(12)
                        .Text("هیچ مشتری بدهکاری وجود ندارد.")
                        .Italic().FontColor(PersianReportStyle.ColorMuted);
                }
            });
        }

        private void ComposeSummaryCard(IContainer container)
        {
            container.Border(1)
                .BorderColor(PersianReportStyle.ColorBorder)
                .Background(PersianReportStyle.ColorZebraBg)
                .Padding(8)
                .Column(col =>
                {
                    col.Spacing(4);
                    col.Item().Row(r =>
                    {
                        r.AutoItem().Text("تعداد مشتریان بدهکار: ").Style(PersianReportStyle.SummaryLabelStyle);
                        r.RelativeItem().Text(PersianReportStyle.FormatNumber(Model.DebtorCount)).Style(PersianReportStyle.TotalHighlightStyle);
                    });
                    col.Item().Row(r =>
                    {
                        r.AutoItem().Text("جمع مانده حساب: ").Style(PersianReportStyle.SummaryLabelStyle);
                        r.RelativeItem().Text(PersianReportStyle.FormatCurrency(Model.TotalBalance)).Style(PersianReportStyle.TotalHighlightStyle);
                    });
                });
        }

        private void ComposeFooter(IContainer container)
        {
            container.BorderTop(1).BorderColor(PersianReportStyle.ColorBorder).PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Text("سامانه مدیریت مشتریان پیوند").FontSize(8).FontColor(PersianReportStyle.ColorMuted);

                row.RelativeItem().AlignLeft().Text(text =>
                {
                    text.Span("صفحه ").FontSize(8).FontColor(PersianReportStyle.ColorMuted);
                    text.CurrentPageNumber().FontSize(8).FontColor(PersianReportStyle.ColorMuted);
                    text.Span(" از ").FontSize(8).FontColor(PersianReportStyle.ColorMuted);
                    text.TotalPages().FontSize(8).FontColor(PersianReportStyle.ColorMuted);
                });
            });
        }
    }
}
```

- [ ] **Step 8: Run the tests to verify they pass**

Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q --filter FullyQualifiedName~PaymentReportTests`
Expected: PASS, 9 tests.

- [ ] **Step 9: Fill the two new fields in the invoice factory**

In `CRMPeyvand/Reports/Services/InvoiceReportModelFactory.cs`, add to the returned model:

```csharp
                PaidAmount = (double)invoice.Paid,
                RemainingBalance = (double)invoice.Balance,
```

- [ ] **Step 10: Print them on the invoice document**

In `CRMPeyvand/Reports/Documents/InvoiceDocument.cs`, inside `ComposeSummaryAndNotes`, insert this block after the discount row and before the `LineHorizontal`:

```csharp
                        // Paid so far
                        col.Item().Row(r =>
                        {
                            r.AutoItem().Text("پرداخت شده:").Style(PersianReportStyle.SummaryLabelStyle);
                            r.RelativeItem().AlignLeft().Text(PersianReportStyle.FormatCurrency(Model.PaidAmount)).Style(PersianReportStyle.SummaryValueStyle);
                        });

                        // Remaining balance
                        col.Item().Row(r =>
                        {
                            r.AutoItem().Text("مانده حساب:").Style(PersianReportStyle.SummaryLabelStyle);
                            r.RelativeItem().AlignLeft().Text(PersianReportStyle.FormatCurrency(Model.RemainingBalance)).Style(PersianReportStyle.SummaryValueStyle);
                        });
```

- [ ] **Step 11: Add the statement to the reports window**

In `CRMPeyvand/ReportsWindow.xaml`, add one `RadioButton` to the general-report `GroupBox`, after `rbPrintProducts`:

```xml
                            <RadioButton x:Name="rbPrintCustomerBalances" Content="مانده حساب مشتریان" FontFamily="Shabnam FD" FontSize="13" Margin="0,6"/>
```

In `CRMPeyvand/ReportsWindow.xaml.cs`, add the field beside the other BLL fields:

```csharp
        private readonly PaymentBLL _paybll = new PaymentBLL();
```

and add this branch to `btnPrintGeneralReport_Click`, after the `rbPrintProducts` branch:

```csharp
                else if (rbPrintCustomerBalances.IsChecked == true)
                {
                    var debtors = _paybll.ReadCustomerBalances() ?? new List<Customer>();
                    var model = new CustomerBalanceReportModel
                    {
                        ReportTitle = "گزارش مانده حساب مشتریان",
                        GeneratedDatePersian = ToPersianDate(DateTime.Now),
                        Customers = debtors.Select((c, idx) => new CustomerBalanceRowModel
                        {
                            RowIndex = idx + 1,
                            Name = c.Name ?? "",
                            Phone = c.Phone ?? "",
                            InvoiceCount = c.Invoices.Count(i => !i.DeleteStatus),
                            TotalBilled = (double)c.PayableTotal,
                            TotalReceived = (double)c.PaidTotal,
                            Balance = (double)c.Balance,
                        }).ToList()
                    };
                    ReportViewerService.OpenReportPdf(new CustomerBalanceDocument(model), "CustomerBalances");
                }
```

`List<Customer>`, `CustomerBalanceReportModel`, `CustomerBalanceRowModel` and `CustomerBalanceDocument` must all be in scope in `ReportsWindow.xaml.cs`; it already imports `System.Collections.Generic`, `System.Linq`, `BE` and `CRMPeyvand.Reports.Documents`/`Models`, so this compiles.

- [ ] **Step 12: Update the invoice factory tests**

In `CRMPeyvand.Tests/InvoiceReportModelFactoryTests.cs`, add:

```csharp
        [Fact]
        public void Paid_and_remaining_come_from_the_invoices_payments()
        {
            var customer = new BE.Customer { Name = "مشتری", Phone = "09120000000" };
            var invoice = new BE.Invoice { RegDate = new DateTime(2026, 10, 1), DiscountAmount = 0m, Customer = customer };
            invoice.Lines.Add(new BE.InvoiceLine { Quantity = 1, UnitPrice = 10000m });
            invoice.Payments.Add(new BE.Payment { Amount = 4000m, RegDate = new DateTime(2026, 10, 2) });

            var model = InvoiceReportModelFactory.FromInvoice(invoice);

            Assert.Equal(4000d, model.PaidAmount);
            Assert.Equal(6000d, model.RemainingBalance);
        }

        [Fact]
        public void An_invoice_with_no_payments_owes_its_full_total()
        {
            var invoice = new BE.Invoice { RegDate = new DateTime(2026, 10, 1), DiscountAmount = 0m };
            invoice.Lines.Add(new BE.InvoiceLine { Quantity = 1, UnitPrice = 7000m });

            var model = InvoiceReportModelFactory.FromInvoice(invoice);

            Assert.Equal(0d, model.PaidAmount);
            Assert.Equal(7000d, model.RemainingBalance);
        }
```

And add to the null-invoice test in that file:

```csharp
            Assert.Equal(0d, model.PaidAmount);
            Assert.Equal(0d, model.RemainingBalance);
```

- [ ] **Step 13: Build and run everything**

Run: `dotnet build CRMPeyvand.sln`
Expected: succeeds.
Run: `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: PASS.

- [ ] **Step 14: Verify by hand, then commit**

In «دریافت‌ها», save a payment and press «چاپ رسید دریافت»: the PDF states the invoice total, the amount paid, what is left, the instrument and the operator. In «گزارشات» pick «مانده حساب مشتریان» and confirm the PDF lists only customers who owe something, with a grand total. Print an invoice that is partly paid and confirm it now shows پرداخت شده and مانده حساب.

```bash
git add CRMPeyvand/Reports/ CRMPeyvand/ReportsWindow.xaml CRMPeyvand/ReportsWindow.xaml.cs CRMPeyvand.Tests/Reporting/PaymentReportTests.cs CRMPeyvand.Tests/InvoiceReportModelFactoryTests.cs
git commit -m "feat(reports): add the رسید دریافت receipt and the مانده حساب statement"
```

---

### Task 9: The dashboard debtor card, the README, and clearing the fake data

**Files:**
- Modify: `CRMPeyvand/MainWindow.xaml`, `CRMPeyvand/MainWindow.xaml.cs`, `README.md`
- Delete: the local development database file (not in the repository)

**Interfaces:**
- Consumes: `DashboardBLL.DebtorCustomerCount()` (Task 5).
- Produces: no new public types.

- [ ] **Step 1: Understand the layout before changing it**

The dashboard's three cards all sit at `Grid.Row="1" Grid.RowSpan="4"` — rows 1 through 4 — and each spans four columns starting at 0, 3 and 6, so they deliberately overlap by one column and are separated visually by their `Margin` values. The reminders panel occupies `Grid.Row="5" Grid.RowSpan="7"`. **Row 4 therefore belongs to the cards, and no card column is free**, so a fourth card cannot simply be appended beside the other three.

The cheapest correct answer is to shrink the three cards to `RowSpan="3"` and put the debtor figure in a full-width strip on row 4. All rows are star-sized, so this needs no window-height change.

- [ ] **Step 2: Shrink the three existing cards**

In `CRMPeyvand/MainWindow.xaml`, change `Grid.RowSpan="4"` to `Grid.RowSpan="3"` on exactly three `Border` elements — the ones whose markup is `<Border Grid.Column="6" Grid.RowSpan="4" Grid.Row="1" Grid.ColumnSpan="4" Margin="85,10,20,20" …>`, `<Border Grid.Column="3" Grid.RowSpan="4" Grid.Row="1" Grid.ColumnSpan="4" Margin="55,10,55,20" …>` and `<Border Grid.Column="0" Grid.RowSpan="4" Grid.Row="1" Grid.ColumnSpan="4" Margin="20,11,85,19" …>`.

Leave every inner `Image` and `Label` of those three cards untouched: they sit on `Grid.Row="1"` or `Grid.Row="3"`, which are still inside a three-row span.

- [ ] **Step 3: Add the debtor strip**

In `CRMPeyvand/MainWindow.xaml`, insert after the third card's closing `Label` (the one carrying `x:Name="lblCountSellsWeek"`):

```xml
        <!--کارت چهارم: مشتریان بدهکار-->
        <Border Grid.Column="0" Grid.Row="4" Grid.RowSpan="1" Grid.ColumnSpan="10" Margin="20,6,85,8" BorderBrush="#F9FAFB" BorderThickness="5" CornerRadius="8">
            <Rectangle Fill="#F9FAFB"/>
        </Border>
        <Image Source="/Images/Checked.png" Grid.Column="9" Grid.Row="4" Height="28" Width="28" Margin="14,10,21,0" VerticalAlignment="Center" Cursor="Hand" MouseLeftButtonDown="PaymentIcon_MouseLeftButtonDown" ToolTip="رفتن به دریافت ها"/>
        <Label Style="{StaticResource MainWindowLabels}" Content="مشتریان بدهکار" Grid.Column="7" Grid.Row="4" Grid.ColumnSpan="2" HorizontalAlignment="Left" VerticalAlignment="Center" FontSize="13" Foreground="#A6B6FF" FontFamily="Shabnam FD" FontWeight="Bold" Margin="74,0,0,0"/>
        <Label Style="{StaticResource MainWindowLabels}" x:Name="lblDebtorCount" Content="0" Grid.Column="6" Grid.Row="4" HorizontalAlignment="Right" VerticalAlignment="Center" FontSize="22" Foreground="#4A8FE7" FontFamily="Shabnam FD" FontWeight="Bold" Margin="0,0,20,0"/>
        <!--کارت چهارم-->
```

The count is at `FontSize="22"` rather than the other cards' `42` because it shares a row with two labels instead of having a card to itself. The icon is a click target into «دریافت‌ها».

- [ ] **Step 4: Feed it**

In `CRMPeyvand/MainWindow.xaml.cs`, add a line to the existing try block in `RefreshPage()`:

```csharp
                try
                {
                    TodaySells.Content = Dbll.SellsCountToday();
                    lblCustomersCount.Content = Dbll.CustomersCount();
                    lblCountSellsWeek.Content = Dbll.CountSellsWeek();
                    lblReminderCount.Content = Dbll.UserReminderCount(loggedInUser);
                    lblDebtorCount.Content = Dbll.DebtorCustomerCount();
                }
                catch
                {
                }
```

Leave `lblCountSellsWeek` alone. The third card's label is bound to `SellsCountWeek()`, and `lblCountSellsWeek` is its name.

- [ ] **Step 5: Build, run the tests, and eyeball the dashboard**

Run: `dotnet build CRMPeyvand.sln`, then `dotnet test CRMPeyvand.Tests/CRMPeyvand.Tests.csproj --nologo -v q`
Expected: build succeeds, tests PASS.

Launch the app. The three original cards must still read correctly and the new «مشتریان بدهکار» strip must sit below them without overlapping the reminders list. Adjust the `Margin` values if it does not — this is a layout judgement, not something the build can check.

- [ ] **Step 6: Update the README**

In `README.md`:

Add a bullet to `### 🧾 Invoicing & Sales Billing`:

```markdown
- **Payments Against Invoices**: Record what a customer actually paid, in full or in part, against any invoice. The invoice's "paid" state is derived from those payments, so the two can never disagree — see [ADR-0009](docs/adr/0009-payments-authoritative-over-checkout-flag.md).
- **Outstanding Balances**: Every invoice and every customer carries a derived balance. Overpayment is refused rather than held as credit, so a payment never exceeds what the invoice still owes.
- **رسید دریافت (Payment Receipt)**: A printed receipt per payment, carrying the invoice total, what that receipt paid, what is left, the Payment Instrument and who took it.
```

Add a new section after `### 🏷️ Promotions & Discount Codes`:

```markdown
### 💰 Receivables
- **Ledger Screen**: A dedicated «دریافت‌ها» screen lists every payment, searchable by invoice number or customer name. Reachable from the sidebar, from shortcut `V`, or by right-clicking any unpaid invoice and choosing «ثبت وصولی».
- **Payment Instruments**: Cash, card, transfer, cheque or other, with an optional reference number — enough to answer "how much came in cash versus card" without dragging in full cheque tracking.
- **Void, Never Amend**: A payment may already have a printed receipt in the customer's hand, so a correction is a void plus a fresh payment rather than an edit.
- **مانده حساب Statement**: A report of every customer who still owes something, with their invoice count, total billed, total received and balance.
- **Cashier-Safe Access**: Payments are their own permission section, so a cashier can take money without being able to issue invoices — and therefore without being able to reduce stock.
```

Add a bullet to `### 📊 Dashboard & Visual Analytics`:

```markdown
- **Debtor Count**: A «مشتریان بدهکار» card showing how many customers currently owe something, and a click target into the payments screen.
```

Fix the Section list in `### 🔐 Granular Role-Based Access Control (RBAC)`, which currently omits `CatalogItems` and `Discounts`:

```markdown
  - **Sections**: Customers, CatalogItems, Invoices, Activities, Reminders, Users, SmsPanel, Reports, Settings, Discounts, Payments.
```

Add a bullet to the Persian feature list, and note the Arabic-script term in the `#### مدیریت پیشرفته مشتریان` area:

```markdown
- **وصولی و مانده حساب**: ثبت پرداخت مشتری (کامل یا بخشی) بابت هر فاکتور، نگهداری مانده حساب هر مشتری و فاکتور، چاپ <span dir="ltr">رسید دریافت</span> و گزارش <span dir="ltr">مانده حساب</span> مشتریان. وضعیت «پرداخت شده» دیگر یک گزینه دستی نیست و از همین وصولی‌ها محاسبه می‌شود.
```

Add to the ADRs list in the Architecture section:

```markdown
- [ADR-0009](docs/adr/0009-payments-authoritative-over-checkout-flag.md): Payments are the record of what was received; the invoice's checkout flag is derived from them.
```

- [ ] **Step 7: Clear the fake data from your local database**

The invoices in your development database are the pre-payments fiction this change replaces: they were ticked off with a boolean and no money behind them, so under the derived rule they now read as unpaid. You confirmed nobody has installed the released MSI, so there is no real installation to protect — but your own book should not be left wrong.

Locate the file first:

```powershell
$paths = @(
  "$env:ProgramData\CRMPeyvand\CRMPeyvand.db",
  "$env:LOCALAPPDATA\CRMPeyvand\CRMPeyvand.db"
)
$paths | ForEach-Object { if (Test-Path -LiteralPath $_) { Get-Item -LiteralPath $_ | Select-Object FullName, Length, LastWriteTime } }
```

Back it up, then delete it:

```powershell
Copy-Item -LiteralPath "$env:ProgramData\CRMPeyvand\CRMPeyvand.db" -Destination "$env:TEMP\CRMPeyvand-dev-backup.db"
Remove-Item -LiteralPath "$env:ProgramData\CRMPeyvand\CRMPeyvand.db"
```

`SqliteSchema.Ensure` recreates every table on the next connection, and the Payments table is now part of the DDL, so there is nothing else to do. Deleting the file also deletes your users, so the next launch goes through First Run and asks you to create the administrator again — that is expected.

If you would rather keep the accounts, open the file in any SQLite browser and run `DELETE FROM InvoiceLines; DELETE FROM Invoices;` instead of deleting the file.

- [ ] **Step 8: Commit**

```bash
git add CRMPeyvand/MainWindow.xaml CRMPeyvand/MainWindow.xaml.cs README.md
git commit -m "feat(dashboard): show how many customers owe money, and document the feature"
```

---

## Deferred, deliberately

These came out of the design conversation and are **not** in this plan. Do not let them creep in:

- **Advance payments / بیعانه** — money received before an invoice exists. Needs a customer-account model with payment allocation. `Payment.Invoice` becoming nullable is the first step, whenever it is wanted.
- **Automatic SMS for overdue invoices** — there is no due-date concept anywhere in the app, so "overdue" has nothing to be measured against, and `Invoice` has only `RegDate`.
- **Cheque maturity and settlement** — a `Cheque` entity with its own lifecycle, not a field on `Payment`.
- **Invoice due dates and aged receivables** — the statement is per-customer with no aging, which was the deliberate choice.
- **A collections chart** — LiveCharts and the reporting window already make this cheap later.
- **Editing an invoice after it has been paid** — invoices remain immutable, as they were before this change.

## Pre-existing bugs found while designing this, and left alone

Both are real and both are outside this feature's blast radius. Record them rather than fixing them here:

- `DAL/OffCodeDAL.CanUse` counts discount-code usage by joining through `Customer.Phone` and does **not** filter `DeleteStatus`, so soft-deleted invoices still consume a Discount Code's `LimitCount`.
- `DAL/InvoiceDAL.ReadInvoiceLastID` is dead code with no caller, and it dereferences `q.id` without a null check, so it would throw on an empty table.

---

## Deviations from this plan, and why

This plan was written before the code existed and was executed task by task. Six things came out differently on contact with the code. Each is recorded here rather than edited back into the task that diverged, so the reasoning survives.

**1. `BE.Customer` does not carry a balance.** Task 1 added `PayableTotal`, `PaidTotal` and `Balance` to the entity. They are gone. A customer's balance is a sum over that customer's invoices, and loading every invoice's lines *and* payments in one EF6 query means a collection include under a collection include — which makes EF6 emit `APPLY`, and SQLite answers `APPLY joins are not supported`. A property that reads as `0` whenever the collections happen not to be loaded is worse than no property, so the figure is computed where it is loaded: `DAL/BalanceQuery`, from two flat queries, returning a `DAL.CustomerBalance` per customer. `Invoice.Paid` / `.Balance` stayed, because `InvoiceDAL` loads an invoice's lines and payments from the invoice's *own* root, where collection includes are plain left joins.

**2. `PaymentDAL.Create` and `Void` use the injected context.** Task 4 had them open `new DB()` per call, mirroring `InvoiceDAL.Create`. That makes them untestable: `new DB()` resolves to the process-wide `DataSource`, so the test suite wrote 39 junk payment rows into the developer's real `%ProgramData%\CRMPeyvand\CRMPeyvand.db` before the design was corrected. They now run on `db` with a transaction, and a rollback is followed by `DiscardPendingChanges` so a rejected payment cannot be flushed by the next successful `SaveChanges`. As a side effect `Create` and `Void` are now genuinely covered by tests, which no DAL mutator in this codebase was.

**3. `Sum` over a decimal column needs a nullable cast.** `PaidSoFar` sums `p.Amount`; over zero rows SQL returns `NULL`, and EF6 then throws *"The cast to value type 'System.Decimal' failed because the materialized value is null"* rather than yielding zero. The sum is now over `(decimal?)p.Amount` with `?? 0m`.

**4. `AsNoTracking` appears only where it is safe.** The statement and the dashboard read through long-lived contexts, and a tracked entity keeps the collections it was loaded with — so without it the debtor count would freeze at whatever it was when the screen was first opened. But `AsNoTracking` combined with a collection include is what triggers the `APPLY`, so it is applied only to the flat queries in `BalanceQuery`.

**5. `Include("Customer")` is load-bearing in `BalanceQuery`.** Its absence is silent: every `invoice.Customer` comes back null, every balance comes out zero, and the debtor list is simply empty. There is no error to notice.

**6. The migration has no `.resx` and no model snapshot.** Task 2 said to copy the `Designer` and add the entity to `BuildTargetModel`. The existing `Designer` is 29 lines of `IMigrationMetadata` with no model in it; the target model is a `Target` resource in the `.resx`, as an opaque serialised blob. So the new `Designer` returns `Target = null`, there is no `.resx`, and the migration file carries a `KNOWN GAP` note telling the next developer to delete and re-add the migration from Visual Studio if they need to scaffold. `Up`/`Down` are complete, which is all the app needs in order to run.

Two further things were corrected rather than planned: the `CustomerForm` money-column grouping step was missing from Task 4's file list, and Task 4's original test for `Customer.Balance` moved to `BalanceQuery` coverage once that property stopped existing.