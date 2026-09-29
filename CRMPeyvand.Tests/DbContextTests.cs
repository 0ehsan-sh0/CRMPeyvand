using System;
using System.IO;
using System.Linq;
using DAL;
using Xunit;

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
        private readonly DbProviderKind _originalKind = DataSource.Current.Kind;
        private readonly string _originalConnectionString = DataSource.Current.ConnectionString;

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
    }
}
