using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using DAL;
using Xunit;
using Xunit.Abstractions;

namespace CRMPeyvand.Tests
{
    /// <summary>
    /// Same collection as DataSourceTests: this class points the process-wide
    /// DataSource at a throwaway file, and the DB() constructor under test reads
    /// it back.
    /// </summary>
    [Collection(DataSourceCollection.Name)]
    public class DbContextTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly DbProviderKind _originalKind = DataSource.Current.Kind;
        private readonly string _originalConnectionString = DataSource.Current.ConnectionString;

        public DbContextTests(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// Puts the process-wide DataSource back, so a later class does not find
        /// itself pointed at a file this test deleted.
        /// </summary>
        public void Dispose()
        {
            DataSource.UseForTests(new DataSource
            {
                Kind = _originalKind,
                ConnectionString = _originalConnectionString,
            });
        }

        [Fact]
        public void A_fresh_sqlite_database_is_usable_through_the_default_constructor()
        {
            var path = Path.Combine(Path.GetTempPath(),
                                   "ctx-" + Guid.NewGuid().ToString("N") + ".db");
            DataSource.UseForTests(new DataSource
            {
                Kind = DbProviderKind.Sqlite,
                ConnectionString =
                    "Data Source=" + path + ";providerName=" + SqliteSchema.Invariant,
            });

            try
            {
                // The whole point: no schema step, no SQL Server, just open it.
                using (var db = new DAL.DB())
                {
                    db.Customers.Add(new BE.Customer
                    {
                        Name = "بدون اسکیما",
                        Phone = "09121111111",
                        RegDate = DateTime.Now,
                    });
                    db.SaveChanges();

                    Assert.Equal(1, db.Customers.Count());
                }
            }
            finally
            {
                // Best effort, for the reason given on SqliteTestDb.TryDelete.
                SqliteTestDb.TryDelete(path);
            }
        }

        /// <summary>
        /// EF6 has to be able to run a query against SQL Server, which is the
        /// one thing the SQLite test above cannot show. A Microsoft.Data.SqlClient
        /// connection opens perfectly and is still refused by EF6's SQL Server
        /// provider services, so the assertion is not "it connects" but "the
        /// rows EF6 reads back are the rows the server reports".
        ///
        /// Read-only. The context is given a null initialiser, so opening it can
        /// neither create the database nor migrate it, and nothing here saves.
        /// </summary>
        [Fact]
        public void A_sql_server_database_answers_a_query_through_EF()
        {
            var expected = LocalSqlServer.TryReadUserCount();
            if (expected == null)
            {
                _output.WriteLine(
                    "Skipped: no SQL Server instance with the CRMPeyvand database was reachable.");
                return;
            }

            var warmUp = NewSqlitePath();

            // DB registers its provider services in a static constructor that
            // runs once per process and reads DataSource.Current at that moment.
            // Touching the type while the choice is still SQLite keeps that
            // one-time step from being taken with SQL Server settings, which
            // would point the context at this machine's real database and its
            // migrations. The initialiser is then replaced so that opening the
            // SQL Server context below cannot write to it either.
            DataSource.UseForTests(new DataSource
            {
                Kind = DbProviderKind.Sqlite,
                ConnectionString =
                    "Data Source=" + warmUp + ";providerName=" + SqliteSchema.Invariant,
            });
            try
            {
                using (new DAL.DB()) { }
            }
            finally
            {
                SqliteTestDb.TryDelete(warmUp);
            }
            Database.SetInitializer<DAL.DB>(new NullDatabaseInitializer<DAL.DB>());

            DataSource.UseForTests(new DataSource
            {
                Kind = DbProviderKind.SqlServer,
                ConnectionString = LocalSqlServer.ConnectionString,
            });

            using (var db = new DAL.DB())
            {
                var userNames = db.Users.Select(u => u.UserName).ToList();
                _output.WriteLine(
                    "SELECT UserName FROM dbo.Users over EF6 returned "
                    + userNames.Count + " row(s): " + string.Join(", ", userNames));
                Assert.Equal(expected, userNames.Count);

                // The grid every screen fills, so this covers the shape the
                // application actually asks for rather than a bare list.
                var grid = new CatalogItemDAL(db).Read();
                Assert.Contains("نام", grid.Columns.Cast<System.Data.DataColumn>()
                                        .Select(c => c.ColumnName));
            }
        }

        private static string NewSqlitePath() => Path.Combine(
            Path.GetTempPath(), "ctx-" + Guid.NewGuid().ToString("N") + ".db");
    }
}
