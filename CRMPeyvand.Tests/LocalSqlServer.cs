using System;

namespace CRMPeyvand.Tests
{
    /// <summary>
    /// The SQL Server this machine happens to have, and a way to find out
    /// whether it is there.
    ///
    /// A test that needs SQL Server has to be able to run where there is none
    /// (a build agent, another developer's laptop) and still be worth running
    /// where there is. IsAvailable is the whole of that: it asks the server a
    /// question that only a working connection and a real CRMPeyvand database
    /// can answer, and reports the answer. A test that skips on it skips
    /// because the database is not there, never because the application
    /// misbehaved.
    /// </summary>
    internal static class LocalSqlServer
    {
        // Matches CRMPeyvand\App.config, which is what the shipped SQL Server
        // default comes from. Windows authentication, because that is what the
        // local MSSQLSERVER instance accepts.
        public const string ConnectionString =
            "Data Source=.;Initial Catalog=CRMPeyvand;Integrated Security=true;"
            + "TrustServerCertificate=True;MultipleActiveResultSets=True;Connect Timeout=5";

        /// <summary>
        /// How many rows dbo.Users holds, or null when SQL Server is not there
        /// to ask. Raw ADO.NET on purpose: this runs before, and independently
        /// of, anything the test is about to exercise, so it cannot be fooled by
        /// the same fault it is meant to detect.
        /// </summary>
        public static int? TryReadUserCount()
        {
            try
            {
                using (var connection =
                           new System.Data.SqlClient.SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM dbo.Users";
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
