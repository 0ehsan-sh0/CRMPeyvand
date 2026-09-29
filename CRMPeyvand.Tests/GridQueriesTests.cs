using System;
using System.Data;
using System.Linq;
using DAL;
using Xunit;

namespace CRMPeyvand.Tests
{
    /// <summary>
    /// Same collection as DataSourceTests: the DAL classes under test take a DB,
    /// but a test that forgets to pass one falls back to the parameterless
    /// constructor, which opens the process-wide DataSource and would create
    /// C:\ProgramData\CRMPeyvand\CRMPeyvand.db on the developer's machine.
    ///
    /// The column names asserted here are the grid headers, because
    /// PublicMethods.dgvFiller binds with AutoGenerateColumns = true. They are
    /// copied from the T-SQL each query replaced, spelling included.
    /// </summary>
    [Collection(DataSourceCollection.Name)]
    public class GridQueriesTests
    {
        [Fact]
        public void Build_preserves_column_names_and_order()
        {
            var table = GridTable.Build(
                new[] { "نام", "شماره تماس" },
                new[] { new object[] { "علی", "09120000000" } });

            Assert.Equal(new[] { "نام", "شماره تماس" },
                         table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            Assert.Single(table.Rows);
            Assert.Equal("علی", table.Rows[0]["نام"]);
        }

        [Fact]
        public void Build_with_no_rows_still_has_the_columns()
        {
            // The grid binds AutoGenerateColumns, so an empty result must still
            // produce the headers or the grid renders blank.
            var table = GridTable.Build(new[] { "نام", "شماره تماس" }, Enumerable.Empty<object[]>());
            Assert.Equal(2, table.Columns.Count);
            Assert.Equal(0, table.Rows.Count);
        }

        [Fact]
        public void Build_rejects_a_row_whose_arity_does_not_match_the_columns()
        {
            // A silently truncated row would misalign every column after it,
            // which is far harder to spot in a grid than an exception.
            var columns = new[] { "نام", "شماره تماس" };
            var rows = new[] { new object[] { "علی", "09120000000", "سرریز" } };

            Assert.Throws<ArgumentException>(() => GridTable.Build(columns, rows));
        }

        [Fact]
        public void Matches_with_an_empty_filter_matches_everything()
        {
            Assert.True(GridTable.Matches(null, "هرچیزی"));
            Assert.True(GridTable.Matches("", "هرچیزی"));
        }

        [Fact]
        public void Matches_ignores_ascii_case()
        {
            Assert.True(GridTable.Matches("ali", "Ali"));
            Assert.True(GridTable.Matches("ALI", "ali"));
            Assert.False(GridTable.Matches("ali", "زندگی"));
        }

        [Fact]
        public void Customer_Read_returns_the_persian_headers()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.Customers.Add(new BE.Customer
                {
                    Name = "رضا", Phone = "09121110000", RegDate = DateTime.Now,
                });
                db.SaveChanges();

                var table = new CustomerDAL(db).Read();
                Assert.Equal(new[] { "نام", "شماره تماس", "تاریخ ثبت" },
                             table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                Assert.Single(table.Rows);
            });
        }

        [Fact]
        public void Customer_Read_excludes_soft_deleted_rows()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.Customers.Add(new BE.Customer
                {
                    Name = "زنده", Phone = "09120000001", RegDate = DateTime.Now,
                });
                db.Customers.Add(new BE.Customer
                {
                    Name = "حذف‌شده", Phone = "09120000002",
                    RegDate = DateTime.Now, DeleteStatus = true,
                });
                db.SaveChanges();

                var table = new CustomerDAL(db).Read();
                Assert.Single(table.Rows);
                Assert.Equal("زنده", table.Rows[0]["نام"]);
            });
        }

        [Fact]
        public void Customer_Search_matches_persian_text_case_insensitively()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.Customers.Add(new BE.Customer
                {
                    Name = "مشتری الف", Phone = "09120000003", RegDate = DateTime.Now,
                });
                db.Customers.Add(new BE.Customer
                {
                    Name = "مشتری ب", Phone = "09120000004", RegDate = DateTime.Now,
                });
                db.SaveChanges();

                var table = new CustomerDAL(db).Search("الف");
                Assert.Single(table.Rows);
                Assert.Equal("مشتری الف", table.Rows[0]["نام"]);
            });
        }

        [Fact]
        public void Customer_Search_matches_the_phone_number()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.Customers.Add(new BE.Customer
                {
                    Name = "الف", Phone = "09121234567", RegDate = DateTime.Now,
                });
                db.SaveChanges();

                var table = new CustomerDAL(db).Search("1234");
                Assert.Single(table.Rows);
            });
        }

        [Fact]
        public void ActivityCategory_Read_returns_the_persian_headers()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.ActivityCategories.Add(new BE.ActivityCategory { CategoryName = "پیگیری" });
                db.SaveChanges();

                var table = new ActivityCategoryDAL(db).Read();
                Assert.Equal(new[] { "ردیف", "نام دسته بندی" },
                             table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                Assert.Single(table.Rows);
                Assert.Equal("پیگیری", table.Rows[0]["نام دسته بندی"]);
            });
        }

        [Fact]
        public void ActivityCategory_Search_matches_the_name()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.ActivityCategories.Add(new BE.ActivityCategory { CategoryName = "تماس" });
                db.ActivityCategories.Add(new BE.ActivityCategory { CategoryName = "سایر" });
                db.SaveChanges();

                var table = new ActivityCategoryDAL(db).Search("تماس");
                Assert.Single(table.Rows);
                Assert.Equal("تماس", table.Rows[0]["نام دسته بندی"]);
            });
        }

        [Fact]
        public void Activity_Read_returns_the_persian_headers()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedOneActivity(db);

                var table = new ActivityDAL(db).Read();
                // "توضبحات" has no ی in it. The old SQL said it that way and the
                // grid has always shown it that way, so it stays.
                Assert.Equal(
                    new[] { "ردیف", "عنوان", "توضبحات", "دسته بندی", "نام کاربر", "تاریخ ثبت" },
                    table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                Assert.Single(table.Rows);
            });
        }

        [Fact]
        public void Activity_Read_shows_the_user_name_and_never_the_display_name()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedOneActivity(db);

                // The old SQL joined dbo.Users and took UserName, not Name, and
                // put it in the "نام کاربر" column. The two values are seeded
                // differently on purpose: swapping one for the other changes
                // what the employee reads and breaks nothing else.
                var table = new ActivityDAL(db).Read();
                Assert.Equal("m.karimi", table.Rows[0]["نام کاربر"]);
                Assert.DoesNotContain("نام", table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            });
        }

        [Fact]
        public void Activity_Read_fills_the_joined_category()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedOneActivity(db);

                // Reaching ActivityCategory without Include comes back null
                // here, because EF6 does not lazy load.
                var table = new ActivityDAL(db).Read();
                Assert.Equal("پیگیری", table.Rows[0]["دسته بندی"]);
            });
        }

        [Fact]
        public void Activity_Search_matches_the_title()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoActivities(db);

                var table = new ActivityDAL(db).Search("تماس اول");
                Assert.Single(table.Rows);
                Assert.Equal("تماس اول", table.Rows[0]["عنوان"]);
            });
        }

        [Fact]
        public void Activity_Search_matches_the_info()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoActivities(db);

                var table = new ActivityDAL(db).Search("یادداشت دوم");
                Assert.Single(table.Rows);
            });
        }

        [Fact]
        public void Activity_Search_matches_the_category_name()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoActivities(db);

                var table = new ActivityDAL(db).Search("پیگیری");
                Assert.Single(table.Rows);
                Assert.Equal("تماس اول", table.Rows[0]["عنوان"]);
            });
        }

        [Fact]
        public void Activity_Search_matches_the_user_name()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoActivities(db);

                var table = new ActivityDAL(db).Search("m.karimi");
                Assert.Single(table.Rows);
            });
        }

        [Fact]
        public void Activity_Search_ignores_the_user_display_name()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoActivities(db);

                // "کارشناس" is in both users' Name and in neither UserName. The
                // old query searched Users.UserName only, so it must still not
                // match here.
                var table = new ActivityDAL(db).Search("کارشناس");
                Assert.Equal(0, table.Rows.Count);
            });
        }

        [Fact]
        public void User_Read_returns_the_persian_headers()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoUsers(db);

                var table = new UserDAL(db).Read();
                Assert.Equal(
                    new[] { "نام", "نام کاربری", "گروه کاربری", "تاریخ ثبت" },
                    table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            });
        }

        [Fact]
        public void User_Read_hides_the_built_in_administrator_group()
        {
            SqliteTestDb.WithDb(db =>
            {
                var builtIn = new BE.UserGroup { Title = "مدیریت", IsBuiltIn = true };
                var normal = new BE.UserGroup { Title = "کارشناس", IsBuiltIn = false };
                db.UserGroups.Add(builtIn);
                db.UserGroups.Add(normal);
                db.Users.Add(new BE.User
                {
                    Name = "مدیر", UserName = "admin", Password = "x",
                    RegDate = DateTime.Now, UserGroup = builtIn,
                });
                db.Users.Add(new BE.User
                {
                    Name = "کارمند", UserName = "staff", Password = "x",
                    RegDate = DateTime.Now, UserGroup = normal,
                });
                db.SaveChanges();

                var table = new UserDAL(db).Read();
                Assert.Single(table.Rows);
                Assert.Equal("کارمند", table.Rows[0]["نام"]);
                Assert.Equal("کارشناس", table.Rows[0]["گروه کاربری"]);
            });
        }

        [Fact]
        public void User_Read_excludes_soft_deleted_rows()
        {
            SqliteTestDb.WithDb(db =>
            {
                var group = new BE.UserGroup { Title = "کارشناس" };
                db.UserGroups.Add(group);
                db.Users.Add(new BE.User
                {
                    Name = "زنده", UserName = "alive", Password = "x",
                    RegDate = DateTime.Now, UserGroup = group,
                });
                db.Users.Add(new BE.User
                {
                    Name = "حذف‌شده", UserName = "gone", Password = "x",
                    RegDate = DateTime.Now, UserGroup = group, DeleteStatus = true,
                });
                db.SaveChanges();

                var table = new UserDAL(db).Read();
                Assert.Single(table.Rows);
                Assert.Equal("زنده", table.Rows[0]["نام"]);
            });
        }

        [Fact]
        public void User_Search_matches_the_name_the_user_name_and_the_group()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoUsers(db);

                // The old query was one statement with no filter, so there is no
                // LIKE clause to mirror here; these three are the columns the
                // grid shows and the ones search offers.
                Assert.Single(new UserDAL(db).Search("کارمند").Rows);
                Assert.Single(new UserDAL(db).Search("s.ahmadi").Rows);
                Assert.Single(new UserDAL(db).Search("کارشناس فروش").Rows);
                Assert.Equal(0, new UserDAL(db).Search("پشتیبانی").Rows.Count);
            });
        }

        [Fact]
        public void UserGroup_Read_hides_the_built_in_group()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.UserGroups.Add(new BE.UserGroup { Title = "مدیریت", IsBuiltIn = true });
                db.UserGroups.Add(new BE.UserGroup { Title = "کارشناس", IsBuiltIn = false });
                db.SaveChanges();

                var table = new UserGroupDAL(db).Read();
                Assert.Equal(new[] { "نام گروه کاربری" },
                             table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                Assert.Single(table.Rows);
                Assert.Equal("کارشناس", table.Rows[0]["نام گروه کاربری"]);
            });
        }

        [Fact]
        public void UserGroup_Search_matches_the_title()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.UserGroups.Add(new BE.UserGroup { Title = "مدیریت", IsBuiltIn = true });
                db.UserGroups.Add(new BE.UserGroup { Title = "کارشناس فروش" });
                db.UserGroups.Add(new BE.UserGroup { Title = "کارشناس پشتیبانی" });
                db.SaveChanges();

                var table = new UserGroupDAL(db).Search("فروش");
                Assert.Single(table.Rows);
                Assert.Equal("کارشناس فروش", table.Rows[0]["نام گروه کاربری"]);
            });
        }

        [Fact]
        public void Reminder_Read_returns_the_seven_columns_including_the_unaliased_one()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoReminders(db);

                var table = new ReminderDAL(db).Read();
                // "RegDate" is the seventh column's header because the original
                // SELECT listed dbo.Reminders.RegDate with no AS clause. It is
                // ugly and it is what the grid has always shown, so it stands
                // until someone deliberately renames it.
                Assert.Equal(
                    new[] { "ردیف", "موضوع", "توضیحات", "تاریخ یادآوری", "وضعیت یادآور", "نام کاربری", "RegDate" },
                    table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            });
        }

        [Fact]
        public void Reminder_Read_includes_the_owner_name()
        {
            SqliteTestDb.WithDb(db =>
            {
                var group = new BE.UserGroup { Title = "کارشناس" };
                db.UserGroups.Add(group);
                db.Users.Add(new BE.User
                {
                    Name = "کارمند", UserName = "staff", Password = "x",
                    RegDate = DateTime.Now, UserGroup = group,
                });
                db.Reminders.Add(new BE.Reminder
                {
                    Title = "تماس", Info = "یادآوری", RegDate = DateTime.Now,
                    RemindDate = DateTime.Now, User = db.Users.Local.First(),
                });
                db.SaveChanges();

                // Reaching User without Include comes back null here, because
                // EF6 does not lazy load.
                var table = new ReminderDAL(db).Read();
                Assert.Single(table.Rows);
                Assert.Equal("کارمند", table.Rows[0]["نام کاربری"]);
            });
        }

        [Fact]
        public void Reminder_Read_shows_the_owner_display_name_not_the_login_name()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoReminders(db);

                // The column is called "نام کاربری" but the old query took
                // dbo.Users.Name, not UserName. The two are seeded differently on
                // purpose: swapping one for the other changes what the employee
                // reads and breaks nothing else. The row is picked by its عنوان
                // because the grid is ordered by id descending.
                var table = new ReminderDAL(db).Read();
                var row = table.Rows.Cast<DataRow>()
                    .Single(r => (string)r["موضوع"] == "تماس اول");
                Assert.Equal("کارشناس فروش", row["نام کاربری"]);
            });
        }

        [Fact]
        public void Reminder_Search_matches_the_title_and_the_info()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoReminders(db);

                Assert.Single(new ReminderDAL(db).Search("تماس اول").Rows);
                Assert.Single(new ReminderDAL(db).Search("یادداشت دوم").Rows);
            });
        }

        [Fact]
        public void Reminder_Search_ignores_the_owner_name()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoReminders(db);

                // "کارشناس" is in both owners' Name and in neither reminder's
                // Title or Info. The old LIKE clauses covered Title and Info
                // only, so it must still not match here.
                var table = new ReminderDAL(db).Search("کارشناس");
                Assert.Equal(0, table.Rows.Count);
            });
        }

        [Fact]
        public void Reminder_Search_excludes_soft_deleted_rows()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoReminders(db);
                db.Reminders.Local.First().DeleteStatus = true;
                db.SaveChanges();

                var table = new ReminderDAL(db).Search(string.Empty);
                Assert.Single(table.Rows);
            });
        }

        /// <summary>
        /// One user in the built-in administrator group and one in an ordinary
        /// group, so a Read assertion cannot be satisfied by accident.
        /// </summary>
        private static void SeedTwoUsers(DB db)
        {
            db.UserGroups.Add(new BE.UserGroup { Title = "مدیریت", IsBuiltIn = true });
            db.UserGroups.Add(new BE.UserGroup { Title = "کارشناس فروش" });
            db.SaveChanges();

            db.Users.Add(new BE.User
            {
                Name = "مدیر سامانه", UserName = "admin", Password = "x",
                RegDate = DateTime.Now,
                UserGroup = db.UserGroups.Single(i => i.Title == "مدیریت"),
            });
            db.Users.Add(new BE.User
            {
                Name = "کارمند", UserName = "s.ahmadi", Password = "x",
                RegDate = DateTime.Now,
                UserGroup = db.UserGroups.Single(i => i.Title == "کارشناس فروش"),
            });
            db.SaveChanges();
        }

        /// <summary>
        /// Two reminders whose title, info and owner differ, so each search
        /// column selects exactly one row. Both owners share a display-name
        /// stem, which is what makes the "search ignores the owner" assertion
        /// meaningful.
        /// </summary>
        private static void SeedTwoReminders(DB db)
        {
            db.UserGroups.Add(new BE.UserGroup { Title = "کارشناس فروش" });
            db.UserGroups.Add(new BE.UserGroup { Title = "کارشناس پشتیبانی" });
            db.Users.Add(new BE.User
            {
                Name = "کارشناس فروش", UserName = "m.karimi", Password = "x",
                RegDate = DateTime.Now,
            });
            db.Users.Add(new BE.User
            {
                Name = "کارشناس پشتیبانی", UserName = "s.ahmadi", Password = "x",
                RegDate = DateTime.Now,
            });
            db.SaveChanges();

            db.Reminders.Add(new BE.Reminder
            {
                Title = "تماس اول", Info = "یادداشت اول", RegDate = DateTime.Now,
                RemindDate = DateTime.Now,
                User = db.Users.Single(i => i.UserName == "m.karimi"),
            });
            db.Reminders.Add(new BE.Reminder
            {
                Title = "بازدید میدانی", Info = "یادداشت دوم", RegDate = DateTime.Now,
                RemindDate = DateTime.Now,
                User = db.Users.Single(i => i.UserName == "s.ahmadi"),
            });
            db.SaveChanges();
        }

        /// <summary>
        /// One activity, so a Read assertion cannot be satisfied by accident.
        /// The user has a display name and a login name on purpose.
        /// </summary>
        private static void SeedOneActivity(DB db)
        {
            db.ActivityCategories.Add(new BE.ActivityCategory { CategoryName = "پیگیری" });
            db.Users.Add(new BE.User
            {
                Name = "کارشناس فروش",
                UserName = "m.karimi",
                RegDate = DateTime.Now,
            });
            db.SaveChanges();

            db.Activities.Add(new BE.Activity
            {
                Title = "تماس اول",
                Info = "یادداشت اول",
                RegDate = DateTime.Now,
                ActivityCategory = db.ActivityCategories.Single(i => i.CategoryName == "پیگیری"),
                User = db.Users.Single(i => i.UserName == "m.karimi"),
            });
            db.SaveChanges();
        }

        /// <summary>
        /// Two activities whose title, info, category and user name each differ,
        /// so any one of the four search columns selects exactly one row. Both
        /// users share a display-name stem, which is what makes the
        /// UserName-only assertion meaningful.
        /// </summary>
        private static void SeedTwoActivities(DB db)
        {
            SeedOneActivity(db);

            db.ActivityCategories.Add(new BE.ActivityCategory { CategoryName = "بازدید" });
            db.Users.Add(new BE.User
            {
                Name = "کارشناس پشتیبانی",
                UserName = "s.ahmadi",
                RegDate = DateTime.Now,
            });
            db.SaveChanges();

            db.Activities.Add(new BE.Activity
            {
                Title = "بازدید میدانی",
                Info = "یادداشت دوم",
                RegDate = DateTime.Now,
                ActivityCategory = db.ActivityCategories.Single(i => i.CategoryName == "بازدید"),
                User = db.Users.Single(i => i.UserName == "s.ahmadi"),
            });
            db.SaveChanges();
        }
    }
}
