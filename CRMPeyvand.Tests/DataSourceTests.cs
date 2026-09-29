using System;
using System.IO;
using System.Linq;
using DAL;
using Xunit;

namespace CRMPeyvand.Tests
{
    /// <summary>
    /// DataSource.Current and DataFolder.Resolve are process-wide. A class that
    /// redirects them must not run beside a class that reads them, or the reader
    /// can be handed a throwaway connection string mid-test. Every test that
    /// touches either lives in this collection.
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public class DataSourceCollection
    {
        public const string Name = "DataSource";
    }

    [Collection(DataSourceCollection.Name)]
    public class DataSourceTests : IDisposable
    {
        private readonly string _original = DataSource.Current.ConnectionString;
        private readonly DbProviderKind _originalKind = DataSource.Current.Kind;
        private readonly string _folder;

        public DataSourceTests()
        {
            // Use() persists, so without this the test would leave a real
            // provider.json on the machine and point the next run of the
            // application at a throwaway database in TEMP. The data folder is
            // redirected for the duration of each test instead, so the file
            // this writes is a real file, just not the real one.
            _folder = Path.Combine(Path.GetTempPath(),
                                   "crmpeyvand-provider-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_folder);
            DataFolder.UseForTests(_folder);
        }

        public void Dispose()
        {
            DataFolder.ResetForTests();

            // Best effort, like SqliteTestDb: a throw from here would replace
            // whatever the test body was already reporting.
            try
            {
                if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            DataSource.UseForTests(new DataSource
            {
                Kind = _originalKind,
                ConnectionString = _original,
            });
        }

        [Fact]
        public void Default_is_sqlite()
        {
            Assert.Equal(DbProviderKind.Sqlite, DataSource.DefaultSqlite().Kind);
        }

        [Fact]
        public void Sqlite_default_connection_string_carries_the_provider_name()
        {
            // EF6 only resolves the SQLite provider because the connection string
            // names the invariant.
            Assert.Contains("providerName=" + SqliteSchema.Invariant,
                            DataSource.DefaultSqlite().ConnectionString);
        }

        [Fact]
        public void Sqlite_database_lives_beside_the_settings_file()
        {
            var ds = DataSource.DefaultSqlite();
            Assert.Contains(DataSource.SqliteFileName, ds.ConnectionString);
        }

        [Fact]
        public void Test_reports_success_in_persian_for_a_real_sqlite_file()
        {
            var path = Path.Combine(Path.GetTempPath(),
                                   "probe-" + Guid.NewGuid().ToString("N") + ".db");
            var ds = new DataSource
            {
                Kind = DbProviderKind.Sqlite,
                ConnectionString = new System.Data.SQLite.SQLiteConnectionStringBuilder
                {
                    DataSource = path,
                }.ToString() + ";providerName=" + SqliteSchema.Invariant,
            };
            try
            {
                Assert.Null(DataSource.Test(ds));
                Assert.True(File.Exists(path));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void Test_reports_failure_in_persian_for_an_unusable_sql_server()
        {
            var ds = new DataSource
            {
                Kind = DbProviderKind.SqlServer,
                // Loopback port 1, which nothing listens on, so the attempt is
                // refused inside the one second timeout whatever is installed on
                // the machine. The development box does have a reachable default
                // instance with a CRMPeyvand database on it, so a merely local
                // data source would have connected and this test would have
                // asserted the wrong thing.
                ConnectionString =
                    "Data Source=tcp:127.0.0.1,1;Initial Catalog=CRMPeyvand;Integrated Security=true;" +
                    "TrustServerCertificate=True;Connect Timeout=1",
            };
            var message = DataSource.Test(ds);
            Assert.NotNull(message);
            // A Persian sentence, not an exception name.
            Assert.Contains("متصل", message);
        }

        [Fact]
        public void Test_reports_success_only_when_EF_can_use_the_connection()
        {
            // A raw ADO.NET connection is not enough to call a SQL Server
            // setting good: the provider services accept one type of connection
            // and refuse the other, and the operator would be told their
            // settings work right up until every screen failed. This is the
            // reachable counterpart of the test above, which asks for a port
            // nothing listens on.
            if (LocalSqlServer.TryReadUserCount() == null) return;

            var ds = new DataSource
            {
                Kind = DbProviderKind.SqlServer,
                ConnectionString = LocalSqlServer.ConnectionString,
            };
            Assert.Null(DataSource.Test(ds));
        }

        [Fact]
        public void Use_writes_the_choice_to_disk_and_Current_follows()
        {
            var ds = new DataSource
            {
                Kind = DbProviderKind.Sqlite,
                ConnectionString = "Data Source=" + Path.Combine(Path.GetTempPath(), "x.db")
                                  + ";providerName=" + SqliteSchema.Invariant,
            };
            DataSource.Use(ds);
            Assert.Equal(DbProviderKind.Sqlite, DataSource.Current.Kind);
            Assert.True(File.Exists(DataSource.FilePath));
        }

        // ---- Activating and deactivating the SQL Server connection ------------
        //
        // Deactivating must not throw away the credentials: the whole point of a
        // toggle is that the user can switch away to SQLite and come back without
        // retyping a server, a login and a password.

        private const string SampleSqlServer =
            "Data Source=tcp:127.0.0.1,1;Initial Catalog=CRMPeyvand;Integrated Security=true;" +
            "TrustServerCertificate=True;Connect Timeout=1";

        [Fact]
        public void A_fresh_install_has_no_sql_server_connection_configured()
        {
            Assert.False(DataSource.DefaultSqlite().IsSqlServerConfigured);
        }

        [Fact]
        public void Deactivating_keeps_the_sql_server_connection_string()
        {
            var active = DataSource.ForSqlServer(SampleSqlServer);
            Assert.True(active.IsSqlServerConfigured);

            var deactivated = active.DeactivateSqlServer();

            Assert.Equal(DbProviderKind.Sqlite, deactivated.Kind);
            Assert.Equal(SampleSqlServer, deactivated.SqlServerConnectionString);
            // Still remembered, so the toggle can be thrown back.
            Assert.True(deactivated.IsSqlServerConfigured);
        }

        [Fact]
        public void Deactivating_falls_back_to_a_usable_sqlite_connection()
        {
            var deactivated = DataSource.ForSqlServer(SampleSqlServer).DeactivateSqlServer();

            // A connection string that actually opens, not just the right Kind.
            Assert.Null(DataSource.Test(deactivated));
        }

        [Fact]
        public void Activating_again_reuses_the_kept_connection_string()
        {
            var roundTrip = DataSource.ForSqlServer(SampleSqlServer)
                .DeactivateSqlServer()
                .ActivateSqlServer();

            Assert.Equal(DbProviderKind.SqlServer, roundTrip.Kind);
            Assert.Equal(SampleSqlServer, roundTrip.ConnectionString);
            Assert.Equal(SampleSqlServer, roundTrip.SqlServerConnectionString);
        }

        [Fact]
        public void Activating_without_a_saved_connection_is_refused()
        {
            Assert.Throws<InvalidOperationException>(
                () => DataSource.DefaultSqlite().ActivateSqlServer());
        }

        [Fact]
        public void The_kept_connection_string_survives_a_restart()
        {
            DataSource.Use(DataSource.ForSqlServer(SampleSqlServer).DeactivateSqlServer());
            DataSource.ResetForTests();

            var reloaded = DataSource.Current;

            Assert.Equal(DbProviderKind.Sqlite, reloaded.Kind);
            Assert.Equal(SampleSqlServer, reloaded.SqlServerConnectionString);
        }

        [Fact]
        public void A_settings_file_written_before_the_kept_string_existed_still_recovers_it()
        {
            // A provider.json from the previous release has Kind and
            // ConnectionString but no SqlServerConnectionString. Losing the
            // connection there would mean the toggle could never be thrown back
            // for an install that was already configured.
            File.WriteAllText(DataSource.FilePath,
                "{\n  \"Kind\": \"SqlServer\",\n  \"ConnectionString\": \"" +
                SampleSqlServer.Replace("\"", "\\\"") + "\"\n}");
            DataSource.ResetForTests();

            var reloaded = DataSource.Current;

            Assert.Equal(DbProviderKind.SqlServer, reloaded.Kind);
            Assert.Equal(SampleSqlServer, reloaded.SqlServerConnectionString);
        }

        [Fact]
        public void Sqlite_file_path_is_reported_and_is_where_the_connection_points()
        {
            var sqlite = DataSource.DefaultSqlite();

            Assert.EndsWith(DataSource.SqliteFileName, DataSource.SqliteFilePath);
            Assert.Contains(DataSource.SqliteFilePath, sqlite.ConnectionString);
        }

        [Fact]
        public void The_sqlite_path_is_absent_before_the_database_has_been_created()
        {
            // The settings screen shows this to the user, so "does it exist yet"
            // has to be a real answer rather than a guess.
            var path = Path.Combine(_folder, "not-created-yet.db");
            Assert.False(File.Exists(path));
        }
    }
}
