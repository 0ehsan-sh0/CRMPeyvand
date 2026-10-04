using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Migrations;
using System.Data.SQLite;
using BE;

namespace DAL
{
    public class DB : DbContext
    {
        /// <summary>
        /// The SQL Server connection string, kept for DataSource.DefaultSqlServer
        /// and for the existing settings screen. DataSource decides which
        /// provider is actually in use.
        ///
        /// Null-tolerant on purpose: a static field initialiser that throws takes
        /// down every use of the type, and the test host has no App.config, so a
        /// hard dereference here would break the SQLite tests that never touch
        /// SQL Server.
        /// </summary>
        public static string ConnectionString =
            ConfigurationManager.ConnectionStrings["conStr"]?.ConnectionString;

        static DB()
        {
            // Registers the SQLite ADO.NET factory and EF6 provider services in
            // code. App.config registration does not work on .NET 10: the
            // system.data section is not recognised because System.Data.Common
            // is a separate assembly, and even once declared the factory cannot
            // be resolved to a provider invariant. This does not displace the
            // App.config <providers> entry, so SQL Server still resolves.
            DbConfiguration.SetConfiguration(new SqliteConfiguration());

            // Migrations only for SQL Server. The SQLite schema is created by
            // SqliteSchema.Ensure; EF6's SQLite provider cannot generate tables,
            // and the migration hardcodes dbo. table names. Without this line
            // SQL Server silently falls back to CreateDatabaseIfNotExists and
            // existing databases stop being migrated.
            //
            // Reading DataSource.Current here is not an initialisation cycle. The
            // SQLite path reaches DefaultSqlite, which never touches DB; Load
            // returns a deserialised DataSource when a settings file exists and
            // otherwise falls back to DefaultSqlite. DefaultSqlServer is the only
            // member that reads DB.ConnectionString and nothing reachable from
            // Current calls it.
            if (DataSource.Current.Kind == DbProviderKind.SqlServer)
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<DB, Migrations.Configuration>());
        }

        private sealed class SqliteConfiguration : DbConfiguration
        {
            public SqliteConfiguration()
            {
                SetProviderFactory(
                    SqliteSchema.Invariant,
                    SQLiteFactory.Instance);
                SetProviderServices(
                    SqliteSchema.Invariant,
                    (DbProviderServices)Activator.CreateInstance(Type.GetType(
                        "System.Data.SQLite.EF6.SQLiteProviderServices, System.Data.SQLite.EF6",
                        throwOnError: true)));
            }
        }

        /// <summary>
        /// Opens the configured provider, whatever type of connection it needs.
        /// DataSource.CreateConnection decides that: SQLite is applied on the
        /// way through so the schema is in place before the first query, and
        /// SQL Server is handed to EF6 already in the connection type its
        /// provider services require, closed, for EF6 to open and migrate.
        /// </summary>
        public DB() : base(DataSource.CreateConnection(DataSource.Current), contextOwnsConnection: true)
        {
        }

        /// <summary>
        /// Used by the SQLite path and by tests, which supply their own
        /// connection (a temporary file, in the test case).
        /// </summary>
        public DB(DbConnection connection) : base(connection, contextOwnsConnection: true)
        {
        }

        public DbSet<Activity> Activities { get; set; }
        public DbSet<ActivityCategory> ActivityCategories { get; set; }
        public DbSet<CatalogItem> CatalogItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<AccessGrant> AccessGrants { get; set; }
        public DbSet<OffCode> OffCodes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessagePanel> MessagePanels { get; set; }
        public DbSet<RememberMe> RememberMe { get; set; }
        public DbSet<Payment> Payments { get; set; }
    }
}
