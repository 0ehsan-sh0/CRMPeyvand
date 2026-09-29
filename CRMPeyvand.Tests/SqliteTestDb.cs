using System;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using DAL;

namespace CRMPeyvand.Tests
{
    /// <summary>
    /// Creates a throwaway SQLite database using the production schema, so data
    /// layer tests exercise the same DDL the application ships.
    /// </summary>
    public static class SqliteTestDb
    {
        private static string NewPath() => Path.Combine(
            Path.GetTempPath(),
            "crmpeyvand-test-" + Guid.NewGuid().ToString("N") + ".db");

        private static string ConnectionString(string path) =>
            new SQLiteConnectionStringBuilder
            {
                DataSource = path,
                // Mirrors DataSource.DefaultSqlite. The date format is named
                // there because the date range queries depend on it; a test
                // connection built differently would be testing a provider
                // configuration the application never opens. See the note in
                // SqliteSchema.
                DateTimeFormat = SQLiteDateFormats.ISO8601,
            }.ToString()
            + ";providerName=" + SqliteSchema.Invariant;

        public static void WithRaw(Action<SQLiteConnection> body)
        {
            var path = NewPath();
            try
            {
                using (var connection = new SQLiteConnection(ConnectionString(path)))
                {
                    connection.Open();
                    body(connection);
                }
            }
            finally
            {
                TryDelete(path);
            }
        }

        /// <summary>
        /// Best effort, and deliberately not allowed to throw: EF6 opens its own
        /// connection to check the model, and that handle can survive until the
        /// finalizer runs, leaving the file locked for a moment. Leaking a file
        /// in TEMP is harmless; throwing from here would mask whatever the test
        /// body actually did.
        ///
        /// Public because a test that builds its own DbContext has the same
        /// EF6-locking problem and no other way to clean up.
        /// </summary>
        public static void TryDelete(string path)
        {
            for (var attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    if (File.Exists(path)) File.Delete(path);
                    return;
                }
                catch (IOException)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch (UnauthorizedAccessException)
                {
                    return;
                }
            }
        }

        public static void WithDb(Action<DB> body)
        {
            WithRaw(connection =>
            {
                SqliteSchema.Ensure(connection);
                using (var db = new DB(connection))
                    body(db);
            });
        }
    }
}
