using System.Data.Entity.Migrations;

namespace DAL.Migrations
{
    /// <summary>
    /// Widens the five money columns from DECIMAL(18,2) to DECIMAL(28,2) on the SQL
    /// Server path, and nothing at all on SQLite.
    ///
    /// Why this was needed: the price fields used to be read into an int, which caps
    /// a whole-Toman entry at 2,147,483,647 - about 21 billion Rial. That was the
    /// real bottleneck and it was in the code, not the schema: 30,000,000,000 Toman
    /// is 3x10^11 Rial and DECIMAL(18,2) already had sixteen integer digits for it.
    /// Money.ParseWhole now returns long, so a field can hold 19 digits, and after
    /// converting Toman to the Rial that gets stored it needs 20. DECIMAL(18,2) can
    /// only take 16, so on SQL Server a field could hold a figure the column would
    /// reject with an arithmetic overflow. This migration closes that gap by giving
    /// the columns 26 integer digits.
    ///
    /// Scale stays at 2: the stored unit is Rial, so there is nothing below it to
    /// record, and leaving scale alone means no existing figure is reinterpreted.
    ///
    /// SQLite needs nothing here and will not get it. Its CREATE TABLE statements are
    /// IF NOT EXISTS and applied on every connection, so an existing SQLite file
    /// keeps the DECIMAL(18,2) it was created with - which costs nothing, because
    /// SQLite gives DECIMAL a NUMERIC affinity and does not enforce precision or
    /// scale at all. It stores whatever it is handed. Only new SQLite files pick up
    /// the widened text in DAL/SqliteSchema.cs.
    ///
    /// Safe to run on an existing database: these are widening alters, so no figure
    /// is truncated or lost, and none of the five columns is indexed or computed
    /// (checked against InitialCreate - the only indexes are on the foreign keys).
    /// MigrateDatabaseToLatestVersion is wired up in DAL/DB.cs, so this runs by
    /// itself the next time an existing SQL Server install opens the database.
    ///
    /// KNOWN GAP - see the same note in 202610041200000_AddPayments.cs. The Designer
    /// file's Target snapshot is null rather than a serialized EF model, which is
    /// what the next Add-Migration diffs against. Up and Down are what this app runs
    /// on; only future scaffolding is affected.
    /// </summary>
    public partial class WidenMoneyColumns : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.OffCodes", "Price", c => c.Decimal(nullable: true, precision: 28, scale: 2));
            AlterColumn("dbo.Invoices", "DiscountAmount", c => c.Decimal(precision: 28, scale: 2));
            AlterColumn("dbo.CatalogItems", "SalePrice", c => c.Decimal(precision: 28, scale: 2));
            AlterColumn("dbo.InvoiceLines", "UnitPrice", c => c.Decimal(precision: 28, scale: 2));
            AlterColumn("dbo.Payments", "Amount", c => c.Decimal(precision: 28, scale: 2));
        }

        public override void Down()
        {
            // Narrowing, so unlike Up this one can lose figures: anything above
            // 9999999999999999.99 would be rounded or refused. That is why it is the
            // Down and not the Up.
            AlterColumn("dbo.OffCodes", "Price", c => c.Decimal(nullable: true, precision: 18, scale: 2));
            AlterColumn("dbo.Invoices", "DiscountAmount", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.CatalogItems", "SalePrice", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.InvoiceLines", "UnitPrice", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.Payments", "Amount", c => c.Decimal(precision: 18, scale: 2));
        }
    }
}