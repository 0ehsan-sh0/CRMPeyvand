using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using DAL;
using Xunit;

namespace CRMPeyvand.Tests
{
    /// <summary>
    /// The date-range queries, exercised under the culture the application
    /// actually runs in.
    ///
    /// PublicMethods.ChangeToPersianCulture() puts PersianCulture - fa-IR with
    /// a PersianCalendar - on the thread before anything touches the database,
    /// so the test host's en-US default is not the configuration that ships. The
    /// worry this class exists to answer is that the SQLite provider writes
    /// dates as text and a bound parameter has to be text in the *same* shape,
    /// or the comparison becomes a string comparison between two
    /// representations that do not sort the same way.
    ///
    /// Persian date strings are the specific hazard: 1405/07/09 and 2026-09-30
    /// sort correctly against each other by luck of the year digits, but the
    /// Persian month and the Gregorian month are not the same month, so a
    /// window that happens to open or close on a Persian month boundary is
    /// where a culture-formatted column would first disagree with an invariant
    /// one. The two Date_range_predicate tests stand on that boundary and on a
    /// Gregorian year rollover, on purpose.
    ///
    /// Every test restores the previous culture in a finally, and the class is
    /// in the DisableParallelization collection, so the change cannot reach a
    /// test running beside it or a later run of the suite.
    /// </summary>
    [Collection(DataSourceCollection.Name)]
    public class PersianCultureDateTests
    {
        [Fact]
        public void Dashboard_counts_invoices_registered_today_under_the_persian_culture()
        {
            UnderPersianCulture(() =>
            SqliteTestDb.WithDb(db =>
            {
                db.Invoices.Add(new BE.Invoice
                {
                    RegDate = DateTime.Today.AddHours(9), IsCheckedout = false, DiscountAmount = 0m,
                });
                db.Invoices.Add(new BE.Invoice
                {
                    RegDate = DateTime.Today.AddDays(-3), IsCheckedout = false, DiscountAmount = 0m,
                });
                db.SaveChanges();

                var dal = new DashboardDAL(db);
                Assert.Equal("1", dal.SellsCountToday());
                Assert.Equal("2", dal.SellsCountWeek());
            }));
        }

        [Fact]
        public void Dashboard_count_today_counts_a_row_late_in_the_day_under_the_persian_culture()
        {
            UnderPersianCulture(() =>
            SqliteTestDb.WithDb(db =>
            {
                db.Invoices.Add(new BE.Invoice
                {
                    RegDate = DateTime.Today.AddHours(23).AddMinutes(59),
                    IsCheckedout = false, DiscountAmount = 0m,
                });
                db.SaveChanges();

                Assert.Equal("1", new DashboardDAL(db).SellsCountToday());
            }));
        }

        [Fact]
        public void Dashboard_count_today_counts_a_row_at_midnight_under_the_persian_culture()
        {
            UnderPersianCulture(() =>
            SqliteTestDb.WithDb(db =>
            {
                db.Invoices.Add(new BE.Invoice
                {
                    RegDate = DateTime.Today, IsCheckedout = false, DiscountAmount = 0m,
                });
                db.SaveChanges();

                Assert.Equal("1", new DashboardDAL(db).SellsCountToday());
            }));
        }

        [Fact]
        public void Dashboard_count_today_excludes_yesterday_at_late_evening_under_the_persian_culture()
        {
            // Yesterday at 23:59 is the nearest miss the same-day range can
            // make. Under a calendar whose months do not line up with the
            // Gregorian ones, this pair is also where "same day" is easiest to
            // get wrong, because the hour digits and the day digits are both in
            // play at once.
            UnderPersianCulture(() =>
            SqliteTestDb.WithDb(db =>
            {
                db.Invoices.Add(new BE.Invoice
                {
                    RegDate = DateTime.Today.AddHours(9), IsCheckedout = false, DiscountAmount = 0m,
                });
                db.Invoices.Add(new BE.Invoice
                {
                    RegDate = DateTime.Today.AddDays(-1).AddHours(23).AddMinutes(59),
                    IsCheckedout = false, DiscountAmount = 0m,
                });
                db.SaveChanges();

                Assert.Equal("1", new DashboardDAL(db).SellsCountToday());
            }));
        }

        [Fact]
        public void Dashboard_week_count_spans_exactly_seven_days_under_the_persian_culture()
        {
            // Six days back is in, eight days back is out. The two sit either
            // side of the boundary on purpose: a window that widened by one
            // day in either direction is caught, not just a window that is
            // simply empty.
            UnderPersianCulture(() =>
            SqliteTestDb.WithDb(db =>
            {
                db.Invoices.Add(new BE.Invoice
                {
                    RegDate = DateTime.Now.AddDays(-6), IsCheckedout = false, DiscountAmount = 0m,
                });
                db.Invoices.Add(new BE.Invoice
                {
                    RegDate = DateTime.Now.AddDays(-8), IsCheckedout = false, DiscountAmount = 0m,
                });
                db.SaveChanges();

                Assert.Equal("1", new DashboardDAL(db).SellsCountWeek());
            }));
        }

        [Fact]
        public void Dashboard_reminders_use_todays_date_under_the_persian_culture()
        {
            UnderPersianCulture(() =>
            SqliteTestDb.WithDb(db =>
            {
                var owner = SeedOwner(db);

                var dal = new DashboardDAL(db);
                Assert.Equal("1", dal.UserReminderCount(owner));

                // The list the dashboard renders, not just the counter on it.
                var reminders = dal.GetUserReminder(owner);
                Assert.Equal(new[] { "تماس امروز" }, reminders.Select(r => r.Title));
            }));
        }

        [Fact]
        public void Date_range_predicate_survives_a_gregorian_year_boundary_under_the_persian_culture()
        {
            // 2026-12-31 to 2027-01-02 crosses the Gregorian new year, so the
            // year prefix of every stored value changes inside the window. If
            // the column were written in the thread's format this window would
            // have to survive a string comparison across a year rollover, and
            // under the Persian calendar the year digits are not the Gregorian
            // ones at all.
            UnderPersianCulture(() =>
            SqliteTestDb.WithDb(db =>
            {
                var from = new DateTime(2026, 12, 31, 8, 0, 0);
                var to = new DateTime(2027, 1, 2, 8, 0, 0);
                SeedInvoices(db, from.AddHours(-1), from, from.AddHours(12), to, to.AddHours(1));

                var inWindow = db.Invoices.Count(i =>
                    i.DeleteStatus == false && i.RegDate >= from && i.RegDate < to);

                Assert.Equal(2, inWindow);
            }));
        }

        [Fact]
        public void Date_range_predicate_survives_a_persian_month_boundary_under_the_persian_culture()
        {
            // The Persian calendar's months are offset from the Gregorian ones:
            // Shahrivar 1405 begins on 2026-08-23 and Mehr on 2026-09-23. A
            // window spanning one of those starts on the 23rd of a Gregorian
            // month, which is not a boundary the Gregorian format has anywhere
            // near. A column stored as 1405/07/xx against a bound parameter of
            // 2026-09-23 is the exact shape that compares as text and gets the
            // wrong answer.
            UnderPersianCulture(() =>
            SqliteTestDb.WithDb(db =>
            {
                var from = new DateTime(2026, 9, 23, 0, 0, 0);
                var to = from.AddDays(1);
                SeedInvoices(db, from.AddHours(-1), from, from.AddHours(12), to, to.AddHours(1));

                var inWindow = db.Invoices.Count(i =>
                    i.DeleteStatus == false && i.RegDate >= from && i.RegDate < to);

                Assert.Equal(2, inWindow);
            }));
        }

        [Fact]
        public void Ordering_by_date_is_chronological_under_the_persian_culture()
        {
            // Every grid the application opens sorts by a date column, and in
            // SQLite that ORDER BY is a sort of text. The dates below are
            // deliberately unsorted in the insert order and span both a month
            // and a year rollover.
            UnderPersianCulture(() =>
            SqliteTestDb.WithDb(db =>
            {
                var dates = new[]
                {
                    new DateTime(2027, 1, 1, 0, 0, 0),
                    new DateTime(2026, 9, 23, 12, 0, 0),
                    new DateTime(2026, 12, 31, 23, 59, 59),
                    new DateTime(2026, 1, 1, 0, 0, 0),
                    new DateTime(2026, 9, 23, 0, 0, 0),
                    new DateTime(2026, 10, 1, 6, 30, 0),
                };
                foreach (var date in dates) SeedInvoice(db, date);

                var read = db.Invoices.AsNoTracking()
                              .OrderBy(i => i.RegDate)
                              .Select(i => i.RegDate)
                              .ToList();

                Assert.Equal(dates.OrderBy(d => d), read);
            }));
        }

        [Fact]
        public void Dates_round_trip_unchanged_under_the_persian_culture()
        {
            // Read back as well as counted: a column that stored the Persian
            // date text would still answer the range questions above if the
            // provider parsed the text back with the Persian calendar, and the
            // employee would be shown 2026-09-01 as 1405/06/10.
            UnderPersianCulture(() =>
            SqliteTestDb.WithDb(db =>
            {
                var wanted = new DateTime(2026, 9, 23, 8, 30, 45);
                SeedInvoice(db, wanted);

                var read = db.Invoices.AsNoTracking().Single().RegDate;
                Assert.Equal(wanted, read);
            }));
        }

        [Fact]
        public void The_same_date_is_stored_identically_whatever_the_thread_culture()
        {
            // The invariant that makes the whole thing safe: the text in the
            // column does not depend on who wrote it. This is the write side
            // of the range query's parameter side, and a provider configured to
            // format dates with the current culture would fail here even though
            // every range query above passed - both sides of the comparison
            // would be Persian text and would still agree with each other,
            // while every row written before the culture was set would not.
            var wanted = new DateTime(2026, 9, 23, 8, 30, 45);

            string storedUnderPersian = null;
            UnderPersianCulture(() =>
            {
                SqliteTestDb.WithDb(db =>
                {
                    SeedInvoice(db, wanted);
                    storedUnderPersian = ReadRawRegDate(db);
                });
            });

            string storedUnderEnglish = null;
            UnderEnglishCulture(() =>
            {
                SqliteTestDb.WithDb(db =>
                {
                    SeedInvoice(db, wanted);
                    storedUnderEnglish = ReadRawRegDate(db);
                });
            });

            Assert.Equal(storedUnderEnglish, storedUnderPersian);
            Assert.Equal("2026-09-23 08:30:45", storedUnderPersian);
        }

        [Fact]
        public void The_shipped_connection_string_pins_the_date_format()
        {
            // Every assertion above is about a column whose text sorts as a
            // date. That only holds because the connection string names the
            // format, so the property is pinned where the connection is built
            // rather than left to the provider's default.
            //
            // DefaultSqlite resolves the real data folder to build its path, so
            // it is redirected for the duration: the assertion is about the
            // connection string, not about the machine's ProgramData.
            var folder = Path.Combine(Path.GetTempPath(),
                                     "crmpeyvand-culture-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            DataFolder.UseForTests(folder);
            try
            {
                var connectionString = DataSource.DefaultSqlite().ConnectionString;

                Assert.Contains(
                    "DateTimeFormat=" + System.Data.SQLite.SQLiteDateFormats.ISO8601,
                    connectionString,
                    StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                DataFolder.ResetForTests();
                // Best effort, like SqliteTestDb: a throw from here would replace
                // whatever the assertion was already reporting.
                try
                {
                    if (Directory.Exists(folder)) Directory.Delete(folder, true);
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }

        /// <summary>
        /// The text in the column, not a value the provider has already
        /// converted. Every other helper here would hide a culture-formatted
        /// column behind the reader's own parse.
        /// </summary>
        private static string ReadRawRegDate(DB db)
        {
            var connection = (System.Data.SQLite.SQLiteConnection)db.Database.Connection;
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT RegDate FROM Invoices";
                using (var reader = command.ExecuteReader())
                {
                    reader.Read();
                    return reader.GetString(0);
                }
            }
        }

        private static void UnderPersianCulture(Action body)
        {
            var persian = new PersianCulture();
            // A test that quietly ran under en-US would pass for the wrong
            // reason and be worse than no test at all.
            Assert.Equal("fa-IR", persian.Name);
            Assert.IsType<PersianCalendar>(persian.Calendar);

            UnderCulture(persian, body);
        }

        private static void UnderEnglishCulture(Action body)
        {
            UnderCulture(CultureInfo.GetCultureInfo("en-US"), body);
        }

        /// <summary>
        /// Sets both cultures for the whole body and puts back whatever was
        /// there, in a finally, because an exception out of the body must not
        /// leave fa-IR on a thread the rest of the suite is going to reuse.
        /// </summary>
        private static void UnderCulture(CultureInfo culture, Action body)
        {
            var previousCulture = CultureInfo.CurrentCulture;
            var previousUiCulture = CultureInfo.CurrentUICulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                body();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previousCulture;
                Thread.CurrentThread.CurrentUICulture = previousUiCulture;
            }
        }

        private static void SeedInvoice(DB db, DateTime regDate)
        {
            db.Invoices.Add(new BE.Invoice
            {
                RegDate = regDate, IsCheckedout = false, DiscountAmount = 0m,
            });
            db.SaveChanges();
        }

        private static void SeedInvoices(DB db, params DateTime[] dates)
        {
            foreach (var date in dates) SeedInvoice(db, date);
        }

        /// <summary>
        /// One employee and three reminders, of which only the first is due
        /// today: the other two are yesterday's, so the counter cannot be
        /// satisfied by a query that has stopped filtering on the date at all.
        /// </summary>
        private static BE.User SeedOwner(DB db)
        {
            var today = DateTime.Today;

            db.UserGroups.Add(new BE.UserGroup { Title = "کارشناس فروش" });
            db.SaveChanges();
            var group = db.UserGroups.Single();

            db.Users.Add(new BE.User
            {
                Name = "کارشناس فروش", UserName = "m.karimi", Password = "x",
                RegDate = DateTime.Now, UserGroup = group,
            });
            db.SaveChanges();

            var owner = db.Users.Single(i => i.UserName == "m.karimi");

            db.Reminders.Add(new BE.Reminder
            {
                Title = "تماس امروز", Info = "یادآوری", RegDate = DateTime.Now,
                RemindDate = today.AddHours(9), User = owner,
            });
            db.Reminders.Add(new BE.Reminder
            {
                Title = "تماس دیروز", Info = "یادآوری", RegDate = DateTime.Now,
                RemindDate = today.AddDays(-1), User = owner,
            });
            db.Reminders.Add(new BE.Reminder
            {
                Title = "تماس فردا", Info = "یادآوری", RegDate = DateTime.Now,
                RemindDate = today.AddDays(1), User = owner,
            });
            db.SaveChanges();

            return owner;
        }
    }
}
