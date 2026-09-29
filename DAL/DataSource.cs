using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DAL
{
    public enum DbProviderKind
    {
        Sqlite,
        SqlServer,
    }

    /// <summary>
    /// Which database the application is talking to, and how to reach it.
    ///
    /// The default is SQLite so that a fresh install works with nothing else
    /// installed. SQL Server is opt-in: the settings screen writes a
    /// SqlServer DataSource and it is used from then on.
    ///
    /// Resolution happens once per process behind a lock. That matters: the EF6
    /// initialiser runs inside DbContext construction and MigrateDatabaseToLatestVersion
    /// is not thread-safe, and thirteen DAL classes construct a DB in a field
    /// initialiser.
    /// </summary>
    public sealed class DataSource
    {
        public const string SqliteFileName = "CRMPeyvand.db";
        private const string SettingsFileName = "provider.json";
        private static readonly string[] ProviderNameToken = { ";providerName=" };

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DbProviderKind Kind { get; set; } = DbProviderKind.Sqlite;

        public string ConnectionString { get; set; } = string.Empty;

        private static readonly object Gate = new object();
        private static DataSource _current;

        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        public static string FilePath => Path.Combine(DataFolder.Resolve(), SettingsFileName);

        public static DataSource Current
        {
            get
            {
                lock (Gate)
                {
                    return _current ?? (_current = Load());
                }
            }
        }

        public static DataSource DefaultSqlite() => new DataSource
        {
            Kind = DbProviderKind.Sqlite,
            ConnectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = Path.Combine(DataFolder.Resolve(), SqliteFileName),
            }.ToString() + ";providerName=" + SqliteSchema.Invariant,
        };

        /// <summary>
        /// The connection string that shipped in App.config, kept as the
        /// SQL Server default so an existing install keeps working untouched.
        ///
        /// DB.ConnectionString is null-tolerant on purpose, because a static
        /// field initialiser that throws takes down every use of DB and the test
        /// host reads no App.config at all. In the application the value is still
        /// read from CRMPeyvand\App.config, exactly as the DB() constructor has
        /// always read it, because ConfigurationManager resolves against the
        /// entry assembly. A blank value is reported here rather than handed back
        /// as a DataSource: Use and Test would both fail on it later, and Test
        /// would report "cannot connect" for what is really a broken build.
        /// </summary>
        public static DataSource DefaultSqlServer()
        {
            var connectionString = DB.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "رشته اتصال SQL Server در پیکربندی برنامه یافت نشد.");

            return new DataSource
            {
                Kind = DbProviderKind.SqlServer,
                ConnectionString = connectionString,
            };
        }

        public static void Use(DataSource dataSource)
        {
            if (dataSource == null) throw new ArgumentNullException(nameof(dataSource));
            if (string.IsNullOrWhiteSpace(dataSource.ConnectionString))
                throw new ArgumentException("رشته اتصال نمی تواند خالی باشد", nameof(dataSource));

            lock (Gate)
            {
                var json = JsonSerializer.Serialize(dataSource, Options);
                File.WriteAllText(FilePath, json);
                _current = dataSource;
            }
        }

        /// <summary>Sets the in-memory choice only. Tests only.</summary>
        public static void UseForTests(DataSource dataSource)
        {
            lock (Gate) { _current = dataSource; }
        }

        /// <summary>
        /// Opens the connection and, for SQLite, applies the schema. Returns
        /// null on success, or a Persian sentence describing the problem.
        ///
        /// The connection is disposed before returning, deliberately: SQLite
        /// pooling is off in 2.0.3, so the .db file stays locked for the whole
        /// life of the connection and until this disposes.
        /// </summary>
        public static string Test(DataSource dataSource)
        {
            if (dataSource == null) return "پیکربندی پایگاه داده نامعتبر است";

            try
            {
                // CreateConnection has already opened the SQLite connection and
                // applied the schema. SQLiteConnection.Open() throws rather than
                // returning quietly when the connection is already open, so the
                // state is checked instead of assumed; the SQL Server connection
                // comes back closed and is opened here. The second Ensure is
                // idempotent and keeps this method honest on its own terms: it is
                // what tells the operator whether their settings work.
                using (var connection = CreateConnection(dataSource))
                {
                    if (connection.State != ConnectionState.Open) connection.Open();
                    if (dataSource.Kind == DbProviderKind.Sqlite)
                        SqliteSchema.Ensure(connection);
                }
                return null;
            }
            catch (Exception e)
            {
                return "به پایگاه داده متصل نشد:\n" + e.Message;
            }
        }

        /// <summary>
        /// Opens a connection to the given data source, applying the SQLite
        /// schema on the way through.
        ///
        /// The SQLite connection comes back open because that is the point where
        /// a fresh install gets its tables: a connection handed back closed
        /// would let the first query arrive before the schema exists. SQL Server
        /// is left closed, because EF6 has to open it to run the migration
        /// first.
        /// </summary>
        internal static DbConnection CreateConnection(DataSource dataSource)
        {
            if (dataSource == null) throw new ArgumentNullException(nameof(dataSource));

            // Microsoft.Data.SqlClient for SQL Server, raw ADO.NET for SQLite.
            // The SQL Server string is handed over untouched; running it
            // through SQLiteConnectionStringBuilder first would be at best
            // pointless and at worst a second way to fail.
            if (dataSource.Kind == DbProviderKind.SqlServer)
                return new Microsoft.Data.SqlClient.SqlConnection(dataSource.ConnectionString);

            var connection = new SQLiteConnection(SqliteConnectionString(dataSource.ConnectionString));
            connection.Open();
            SqliteSchema.Ensure(connection);
            return connection;
        }

        /// <summary>
        /// The connection string without its "providerName=" tail. That token is
        /// how EF6 finds the provider; it is not an ADO.NET keyword, and
        /// SQLiteConnection refuses a connection string that still carries it.
        /// </summary>
        internal static string SqliteConnectionString(string connectionString) =>
            new SQLiteConnectionStringBuilder(
                (connectionString ?? string.Empty)
                    .Split(ProviderNameToken, StringSplitOptions.None)[0])
                .ToString();

        private static DataSource Load()
        {
            try
            {
                var path = FilePath;
                if (File.Exists(path))
                {
                    var loaded = JsonSerializer.Deserialize<DataSource>(
                        File.ReadAllText(path), Options);
                    if (loaded != null && !string.IsNullOrWhiteSpace(loaded.ConnectionString))
                        return loaded;
                }
            }
            catch (Exception)
            {
                // A corrupt or unreadable settings file must not stop the app
                // from starting; fall through to the default.
            }

            return DefaultSqlite();
        }
    }
}
