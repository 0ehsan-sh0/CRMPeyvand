using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;
using System.IO;

namespace DAL
{
    public class SettingDAL
    {
        DB db;

        public SettingDAL()
        {
            db = new DB();
        }

        /// <summary>
        /// Takes the context instead of opening one of its own. BackUp works on
        /// the database file and never reads through EF, so a test can hand in a
        /// context it does not care about rather than have the parameterless
        /// constructor open - and create - the very file it is about to look for.
        /// </summary>
        public SettingDAL(DB db)
        {
            this.db = db;
        }

        /// <summary>
        /// Writes a restorable copy of the database to Path and answers with a
        /// Persian sentence saying whether it worked. DataBaseForm shows whatever
        /// comes back in a message box, so this never throws and the wording is
        /// part of the interface.
        /// </summary>
        public string BackUp(string Path)
        {
            try
            {
                if (DataSource.Current.Kind == DbProviderKind.Sqlite)
                    return BackUpSqlite(Path);

                return BackUpSqlServer(Path);
            }
            catch (Exception e)
            {
                return "ذخیره پشتیبان با مشکلی مواجه شد:\n" + e.Message;
            }
        }

        private string BackUpSqlServer(string path)
        {
            using (var connection = new SqlConnection(DB.ConnectionString))
            using (var command = new SqlCommand())
            {
                // BACKUP DATABASE cannot be parameterized for the db name; it is taken from
                // the live connection (not user input). The file path stays a parameter.
                command.CommandText = "BACKUP DATABASE [" + connection.Database + "] TO DISK = @path WITH INIT";
                command.Parameters.AddWithValue("@path", path);
                command.Connection = connection;
                connection.Open();
                command.ExecuteNonQuery();
            }

            return "ذخیره فایل با موفقیت انجام شد لطفا پوشه مورد نظر را بررسی کنید";
        }

        /// <summary>
        /// SQLite has no BACKUP DATABASE, and File.Copy is not a substitute: a
        /// plain copy of a single-file database can catch it mid-write and
        /// produce a file that will not open. The online backup API instead reads
        /// the pages under SQLite's own locks and returns a consistent snapshot,
        /// with the source left open and usable throughout - an employee can keep
        /// working while a backup is being taken.
        ///
        /// A connection of our own, rather than DataSource.CreateConnection:
        /// that one applies the schema, which would create the very file this
        /// method reports as missing.
        /// </summary>
        private string BackUpSqlite(string path)
        {
            var connectionString = DataSource.SqliteConnectionString(DataSource.Current.ConnectionString);
            var source = new SQLiteConnectionStringBuilder(connectionString).DataSource;

            if (string.IsNullOrEmpty(source) || !File.Exists(source))
                return "فایل پایگاه داده یافت نشد";

            using (var sourceConnection = new SQLiteConnection(connectionString))
            {
                sourceConnection.Open();
                using (var destination = new SQLiteConnection(
                           new SQLiteConnectionStringBuilder
                           {
                               DataSource = path,
                               DateTimeFormat = SQLiteDateFormats.ISO8601,
                           }.ToString()))
                {
                    destination.Open();
                    // A page count of -1 copies the whole database in one step and
                    // comes back finished, so there is no progress callback to
                    // pump and no busy retry loop to write. The two names are the
                    // source and destination schemas; both are "main" here.
                    // Note the argument order: destination first, then source.
                    sourceConnection.BackupDatabase(destination, "main", "main", -1, null, 0);
                }
            }

            return "ذخیره فایل با موفقیت انجام شد لطفا پوشه مورد نظر را بررسی کنید";
        }

        public string Recovery()
        {
            try
            {
                var q = db.Activities.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q)
                {
                    item.DeleteStatus = false;
                }
                var q1 = db.ActivityCategories.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q1)
                {
                    item.DeleteStatus = false;
                }
                var q2 = db.Customers.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q2)
                {
                    item.DeleteStatus = false;
                }
                var q3 = db.Invoices.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q3)
                {
                    item.DeleteStatus = false;
                }
                var q4 = db.Messages.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q4)
                {
                    item.DeleteStatus = false;
                }
                var q5 = db.OffCodes.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q5)
                {
                    item.DeleteStatus = false;
                }
                var q6 = db.CatalogItems.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q6)
                {
                    item.DeleteStatus = false;
                }
                var q7 = db.Reminders.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q7)
                {
                    item.DeleteStatus = false;
                }
                var q8 = db.Users.Where(i => i.DeleteStatus == true).ToList();
                foreach (var item in q8)
                {
                    item.DeleteStatus = false;
                }
                db.SaveChanges();
                return "اطلاعات با موفقیت بازگردانی شد";
            }
            catch (Exception e)
            {
                return "بازگردانی اطلاعات حذف شده با مشکلی روبرو شد:\n" + e.Message;
            }
        }
    }
}
