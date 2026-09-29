namespace DAL.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// EF6 migration configuration. Migrations are used for SQL Server only -
    /// the SQLite schema is created by DAL.SqliteSchema, because the EF6 SQLite
    /// provider does not generate tables (verified: CreateIfNotExists creates
    /// nothing).
    ///
    /// The Seed() override that used to live here created five stored procedures
    /// for the retired Stimulsoft reporting engine. Nothing called them; they
    /// were removed because their T-SQL made a SQLite first run impossible.
    /// </summary>
    internal sealed class Configuration : DbMigrationsConfiguration<DAL.DB>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "DAL.DB";
        }
    }
}
