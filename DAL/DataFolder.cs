using System;
using System.IO;

namespace DAL
{
    /// <summary>
    /// Resolves a writable directory for the database and the provider settings.
    ///
    /// Prefers %ProgramData%\CRMPeyvand so that everyone signing in to the same
    /// machine shares one database, which is what SQL Server mode gives them. The
    /// folder is created by whoever runs first, so writability is probed rather
    /// than assumed; a later user falls back to %LocalAppData% and gets their own
    /// database. Same pattern, and the same trade-off, as
    /// PublicMethods.UserPicturesDirectory.
    ///
    /// The answer is cached, because the brief's version created and deleted a
    /// probe file on every call and Resolve is read by Current, by Use and by
    /// FilePath.
    /// </summary>
    public static class DataFolder
    {
        public const string Name = "CRMPeyvand";

        private static readonly object Gate = new object();
        private static string _forced;
        private static string _resolved;

        public static string Resolve()
        {
            // Read outside the lock, and honoured over the cache, so a test can
            // redirect the folder without disturbing the production answer.
            var forced = _forced;
            if (forced != null) return forced;

            lock (Gate)
            {
                if (_resolved == null) _resolved = ResolveDefault();
                return _resolved;
            }
        }

        internal static void UseForTests(string path)
        {
            lock (Gate) { _forced = path; }
        }

        internal static void ResetForTests()
        {
            lock (Gate) { _forced = null; }
        }

        private static string ResolveDefault()
        {
            var shared = TryUnder(Environment.SpecialFolder.CommonApplicationData);
            if (shared != null) return shared;

            var own = TryUnder(Environment.SpecialFolder.LocalApplicationData);
            if (own != null) return own;

            // Nothing else to try. Reported here rather than returned as null,
            // because every caller immediately does Path.Combine on the result.
            throw new InvalidOperationException(
                "پوشه داده برنامه ساخته نشد و قابل نوشتن نیست.");
        }

        private static string TryUnder(Environment.SpecialFolder root)
        {
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(root), Name);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                var probe = Path.Combine(path, ".write-test-" + Guid.NewGuid().ToString("N"));
                try
                {
                    using (File.Create(probe)) { }
                }
                finally
                {
                    File.Delete(probe);
                }

                return path;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
