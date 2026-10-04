using System.Data.Entity.Migrations;

namespace DAL.Migrations
{
    /// <summary>
    /// Adds the Payments table on the SQL Server path.
    ///
    /// SQLite needs nothing here: DAL/SqliteSchema already creates the table in
    /// its forward-only DDL batch, which is applied on every connection.
    ///
    /// This migration exists because DAL/Migrations/Configuration sets
    /// AutomaticMigrationsEnabled = false. Without a migration for the new DbSet,
    /// MigrateDatabaseToLatestVersion throws at DbContext construction for anyone
    /// who has chosen SQL Server, so the app would not start at all.
    ///
    /// KNOWN GAP - read before adding the next migration.
    /// The Designer file's Target model snapshot is null rather than a serialized
    /// EF model. EF6 normally embeds that snapshot as a "Target" resource in the
    /// migration's .resx, and it is what the next Add-Migration diffs the current
    /// model against. A null snapshot means the next Add-Migration would believe
    /// no tables exist and try to create all fourteen again.
    ///
    /// So: if you are in Visual Studio with a SQL Server instance holding a
    /// database at this migration, delete this migration and re-add it with
    /// Add-Migration so a real snapshot is generated. Do not hand-maintain the
    /// blob. Up and Down below are correct and are all that is needed for this
    /// app to run; only future scaffolding is affected.
    /// </summary>
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
            // Foreign keys, then indexes, then the table - the order the
            // InitialCreate migration drops in.
            DropForeignKey("dbo.Payments", "Invoice_id", "dbo.Invoices");
            DropForeignKey("dbo.Payments", "User_id", "dbo.Users");
            DropIndex("dbo.Payments", new[] { "Invoice_id" });
            DropIndex("dbo.Payments", new[] { "User_id" });
            DropTable("dbo.Payments");
        }
    }
}