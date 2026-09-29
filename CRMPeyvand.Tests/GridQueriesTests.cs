using System;
using System.Collections.Generic;
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

        [Fact]
        public void Catalog_Read_returns_the_persian_headers()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.CatalogItems.Add(new BE.CatalogItem
                {
                    Name = "کالای اول", Kind = BE.ItemKind.Good,
                    SalePrice = 100m, Stock = 5,
                });
                db.SaveChanges();

                var table = new CatalogItemDAL(db).Read();
                Assert.Equal(new[] { "نام", "قیمت", "نوع", "موجودی" },
                             table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            });
        }

        [Fact]
        public void Catalog_Read_translates_the_Kind_code_to_a_persian_label()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.CatalogItems.Add(new BE.CatalogItem
                {
                    Name = "کالای اول", Kind = BE.ItemKind.Good,
                    SalePrice = 100m, Stock = 5,
                });
                db.CatalogItems.Add(new BE.CatalogItem
                {
                    Name = "خدمت اول", Kind = BE.ItemKind.Service,
                    SalePrice = 200m, Stock = 0,
                });
                db.SaveChanges();

                var table = new CatalogItemDAL(db).Read();
                var labels = table.Rows.Cast<DataRow>()
                                   .Select(r => (string)r["نوع"]).ToList();
                Assert.Contains("محصول", labels);
                Assert.Contains("خدمات", labels);
            });
        }

        [Fact]
        public void Catalog_Read_fills_every_column_in_order()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.CatalogItems.Add(new BE.CatalogItem
                {
                    Name = "کالای اول", Kind = BE.ItemKind.Good,
                    SalePrice = 100m, Stock = 5,
                });
                db.SaveChanges();

                // The values are asserted by column name as well as by position,
                // so a projection that silently reorders the row fails here.
                var row = new CatalogItemDAL(db).Read().Rows[0];
                Assert.Equal("کالای اول", row["نام"]);
                Assert.Equal(100m, Convert.ToDecimal(row["قیمت"]));
                Assert.Equal("محصول", row["نوع"]);
                Assert.Equal(5, Convert.ToInt32(row["موجودی"]));
            });
        }

        [Fact]
        public void Catalog_Read_leaves_the_label_empty_for_a_Kind_outside_the_enum()
        {
            SqliteTestDb.WithDb(db =>
            {
                // The old SQL was CASE Kind WHEN 1 ... WHEN 2 ... END with no
                // ELSE, so anything other than 1 or 2 came back as NULL. A
                // two-armed ternary would instead report it as "خدمات", which
                // is a different - and wrong - thing to show an employee.
                db.CatalogItems.Add(new BE.CatalogItem
                {
                    Name = "کالای ناشناخته", Kind = (BE.ItemKind)7,
                    SalePrice = 50m, Stock = 1,
                });
                db.SaveChanges();

                var row = new CatalogItemDAL(db).Read().Rows[0];
                Assert.Null(row["نوع"] as string);
            });
        }

        [Fact]
        public void Catalog_Read_excludes_soft_deleted_rows()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.CatalogItems.Add(new BE.CatalogItem
                {
                    Name = "کالای اول", Kind = BE.ItemKind.Good,
                    SalePrice = 100m, Stock = 5,
                });
                db.CatalogItems.Add(new BE.CatalogItem
                {
                    Name = "کالای حذف‌شده", Kind = BE.ItemKind.Good,
                    SalePrice = 100m, Stock = 5, DeleteStatus = true,
                });
                db.SaveChanges();

                var table = new CatalogItemDAL(db).Read();
                Assert.Single(table.Rows);
                Assert.Equal("کالای اول", table.Rows[0]["نام"]);
            });
        }

        [Fact]
        public void Catalog_Search_matches_the_name()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoCatalogItems(db);

                var table = new CatalogItemDAL(db).Search("کالای اول");
                Assert.Single(table.Rows);
                Assert.Equal("کالای اول", table.Rows[0]["نام"]);
            });
        }

        [Fact]
        public void Catalog_Search_matches_the_kind_label()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoCatalogItems(db);

                // Searching the raw enum value finds nothing: the grid shows
                // the translated label, and that is what search offers.
                Assert.Equal(0, new CatalogItemDAL(db).Search("1").Rows.Count);

                var table = new CatalogItemDAL(db).Search("محصول");
                Assert.Single(table.Rows);
                Assert.Equal("کالای اول", table.Rows[0]["نام"]);

                Assert.Single(new CatalogItemDAL(db).Search("خدمات").Rows);
            });
        }

        [Fact]
        public void Catalog_Read_by_type_filters_on_the_label()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoCatalogItems(db);
                var dal = new CatalogItemDAL(db);

                var goods = dal.Read("محصول");
                Assert.Equal(new[] { "نام", "قیمت", "نوع", "موجودی" },
                             goods.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                Assert.Single(goods.Rows);
                Assert.Equal("کالای اول", goods.Rows[0]["نام"]);

                var services = dal.Read("خدمات");
                Assert.Single(services.Rows);
                Assert.Equal("خدمت اول", services.Rows[0]["نام"]);

                Assert.Equal(0, dal.Read("ناموجود").Rows.Count);
            });
        }

        [Fact]
        public void Catalog_Read_by_type_ignores_the_name()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoCatalogItems(db);

                // The filter is on the label column only. A name that happens
                // to contain the label text must not drag the row in.
                Assert.Equal(0, new CatalogItemDAL(db).Read("کالای").Rows.Count);
            });
        }

        [Fact]
        public void Catalog_Read_by_type_excludes_soft_deleted_rows()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.CatalogItems.Add(new BE.CatalogItem
                {
                    Name = "کالای حذف‌شده", Kind = BE.ItemKind.Good,
                    SalePrice = 100m, Stock = 5, DeleteStatus = true,
                });
                db.SaveChanges();

                Assert.Equal(0, new CatalogItemDAL(db).Read("محصول").Rows.Count);
            });
        }

        [Fact]
        public void OffCode_Read_returns_all_discount_columns()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.OffCodes.Add(new BE.OffCode
                {
                    Code = "OFF10", Percent = 10, IsPrice = false,
                    RegDate = DateTime.Now, Price = 0m,
                });
                db.SaveChanges();

                var table = new OffCodeDAL(db).Read();
                Assert.Equal(
                    new[] { "کد تخفیف", "مبلغ تخفیف", "درصد تخفیف", "محدودیت مصرف", "تاریخ انقضا", "تاریخ ثبت" },
                    table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                Assert.Equal(10m, Convert.ToDecimal(table.Rows[0]["درصد تخفیف"]));
            });
        }

        [Fact]
        public void OffCode_Read_fills_every_column_in_order()
        {
            SqliteTestDb.WithDb(db =>
            {
                var regDate = new DateTime(2026, 3, 4, 5, 6, 7);
                var expire = new DateTime(2027, 1, 2);
                db.OffCodes.Add(new BE.OffCode
                {
                    Code = "OFF20", IsPrice = true, Price = 5000m,
                    LimitCount = 3, ExpireDate = expire, RegDate = regDate,
                });
                db.SaveChanges();

                var row = new OffCodeDAL(db).Read().Rows[0];
                Assert.Equal("OFF20", row["کد تخفیف"]);
                Assert.Equal(5000m, Convert.ToDecimal(row["مبلغ تخفیف"]));
                // A price discount carries no percentage; the old query listed
                // the column unconditionally, so it must still be there and be
                // empty rather than zero. DataTable hands back DBNull, not null.
                Assert.Equal(DBNull.Value, row["درصد تخفیف"]);
                Assert.Equal(3, Convert.ToInt32(row["محدودیت مصرف"]));
                Assert.Equal(expire, Convert.ToDateTime(row["تاریخ انقضا"]));
                Assert.Equal(regDate, Convert.ToDateTime(row["تاریخ ثبت"]));
            });
        }

        [Fact]
        public void OffCode_Read_excludes_soft_deleted_rows()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.OffCodes.Add(new BE.OffCode
                {
                    Code = "ALIVE", Percent = 5, RegDate = DateTime.Now,
                });
                db.OffCodes.Add(new BE.OffCode
                {
                    Code = "GONE", Percent = 5, RegDate = DateTime.Now,
                    DeleteStatus = true,
                });
                db.SaveChanges();

                var table = new OffCodeDAL(db).Read();
                Assert.Single(table.Rows);
                Assert.Equal("ALIVE", table.Rows[0]["کد تخفیف"]);
            });
        }

        [Fact]
        public void OffCode_Search_matches_the_code_only()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.OffCodes.Add(new BE.OffCode
                {
                    Code = "SUMMER", Percent = 20, RegDate = DateTime.Now,
                });
                db.OffCodes.Add(new BE.OffCode
                {
                    Code = "WINTER", Percent = 30, RegDate = DateTime.Now,
                });
                db.SaveChanges();

                var dal = new OffCodeDAL(db);
                var table = dal.Search("SUMMER");
                Assert.Single(table.Rows);
                Assert.Equal("SUMMER", table.Rows[0]["کد تخفیف"]);

                // The percentage is shown in the grid but the old LIKE clause
                // covered Code only, and no code here contains a digit, so it
                // must still not match.
                Assert.Equal(0, dal.Search("30").Rows.Count);
            });
        }

        [Fact]
        public void Message_Read_excludes_soft_deleted()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.Messages.Add(new BE.Message
                {
                    Content = "پیام زنده", RegDate = DateTime.Now,
                });
                db.Messages.Add(new BE.Message
                {
                    Content = "پیام حذف‌شده", RegDate = DateTime.Now, DeleteStatus = true,
                });
                db.SaveChanges();

                var table = new MessageDAL(db).Read();
                Assert.Single(table.Rows);
                Assert.Equal("پیام زنده", table.Rows[0]["متن پیام"]);
            });
        }

        [Fact]
        public void Message_Read_returns_the_persian_headers()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.Messages.Add(new BE.Message
                {
                    Content = "پیام اول", RegDate = DateTime.Now,
                });
                db.SaveChanges();

                var table = new MessageDAL(db).Read();
                Assert.Equal(new[] { "متن پیام" },
                             table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            });
        }

        [Fact]
        public void Message_Read_orders_newest_first()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoMessages(db);

                var table = new MessageDAL(db).Read();
                Assert.Equal(new[] { "پیام دوم", "پیام اول" },
                             table.Rows.Cast<DataRow>().Select(r => (string)r["متن پیام"]));
            });
        }

        [Fact]
        public void Message_Search_matches_the_content()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedTwoMessages(db);

                var dal = new MessageDAL(db);
                var table = dal.Search("اول");
                Assert.Single(table.Rows);
                Assert.Equal("پیام اول", table.Rows[0]["متن پیام"]);

                // Both seeded messages share the پیام stem, so that one matches
                // two rows; the distinguishing part of each is the word after it.
                Assert.Equal(2, dal.Search("پیام").Rows.Count);
                Assert.Single(dal.Search("دوم").Rows);
                Assert.Equal(0, dal.Search("سوم").Rows.Count);
            });
        }

        [Fact]
        public void Invoice_Read_returns_the_seven_persian_headers()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedInvoice(db);

                var table = new InvoiceDAL(db).Read();
                // Copied from the AS aliases of the T-SQL this replaces, in its
                // order: the two computed columns come last, not in the middle.
                Assert.Equal(
                    new[] { "شماره فاکتور", "وضعیت پرداخت", "تاریخ پرداخت", "کد تخفیف", "تاریخ ثبت",
                            "تعداد کالاهای فاکتور", "هزینه پرداختی" },
                    table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));

                // "قیمت کل" existed only as an alias inside the stored procedure
                // deleted in the first task. The live query had no joins at all,
                // so there is no customer name and no user name either.
                Assert.DoesNotContain("قیمت کل",
                                      table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            });
        }

        [Fact]
        public void Invoice_Read_sums_line_quantities_per_invoice()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db);
                invoice.Lines.Add(new BE.InvoiceLine
                {
                    Quantity = 2, UnitPrice = 1000m, CatalogItem = db.CatalogItems.Local.First(),
                });
                invoice.Lines.Add(new BE.InvoiceLine
                {
                    Quantity = 3, UnitPrice = 1000m, CatalogItem = db.CatalogItems.Local.First(),
                });
                db.SaveChanges();

                var table = new InvoiceDAL(db).Read();
                Assert.Single(table.Rows);
                Assert.Equal(5, Convert.ToInt32(table.Rows[0]["تعداد کالاهای فاکتور"]));
                Assert.Equal(5000m, Convert.ToDecimal(table.Rows[0]["هزینه پرداختی"]));
            });
        }

        [Fact]
        public void Invoice_Read_subtracts_the_discount_from_the_payable_total()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, discountAmount: 750m);
                invoice.Lines.Add(new BE.InvoiceLine
                {
                    Quantity = 5, UnitPrice = 1000m, CatalogItem = db.CatalogItems.Local.First(),
                });
                db.SaveChanges();

                var table = new InvoiceDAL(db).Read();
                Assert.Equal(4250m, Convert.ToDecimal(table.Rows[0]["هزینه پرداختی"]));
            });
        }

        [Fact]
        public void Invoice_Read_shows_zero_not_null_for_an_invoice_with_no_lines()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedInvoice(db);

                // ISNULL(SUM(...), 0) in the old SQL. Sum's empty-sequence
                // behaviour is the same thing in the client-side projection,
                // which is why the grid must show 0 rather than a blank.
                var table = new InvoiceDAL(db).Read();
                Assert.Equal(0, Convert.ToInt32(table.Rows[0]["تعداد کالاهای فاکتور"]));
                Assert.Equal(0m, Convert.ToDecimal(table.Rows[0]["هزینه پرداختی"]));
                Assert.NotEqual(DBNull.Value, table.Rows[0]["هزینه پرداختی"]);
            });
        }

        [Fact]
        public void Invoice_Read_keeps_the_payable_total_in_decimal()
        {
            SqliteTestDb.WithDb(db =>
            {
                var invoice = SeedInvoice(db, discountAmount: 0.03m);
                invoice.Lines.Add(new BE.InvoiceLine
                {
                    Quantity = 3, UnitPrice = 1234.56m, CatalogItem = db.CatalogItems.Local.First(),
                });
                db.SaveChanges();

                // 3 * 1234.56 = 3703.68, less 0.03. The old SQL rounded this
                // through float, which would report 3703.6499023437504 here.
                var row = new InvoiceDAL(db).Read().Rows[0];
                Assert.Equal(3703.65m, Convert.ToDecimal(row["هزینه پرداختی"]));
            });
        }

        [Fact]
        public void Invoice_Read_excludes_soft_deleted_rows()
        {
            SqliteTestDb.WithDb(db =>
            {
                var alive = SeedInvoice(db);
                var removed = SeedInvoice(db);
                removed.DeleteStatus = true;
                db.SaveChanges();

                var table = new InvoiceDAL(db).Read();
                Assert.Single(table.Rows);
                Assert.Equal(alive.id, Convert.ToInt32(table.Rows[0]["شماره فاکتور"]));
            });
        }

        [Fact]
        public void Invoice_Read_orders_newest_first()
        {
            SqliteTestDb.WithDb(db =>
            {
                var first = SeedInvoice(db);
                var second = SeedInvoice(db);

                var table = new InvoiceDAL(db).Read();
                Assert.Equal(new[] { second.id, first.id },
                             table.Rows.Cast<DataRow>().Select(r => Convert.ToInt32(r["شماره فاکتور"])));
            });
        }

        [Fact]
        public void Invoice_Search_matches_the_invoice_number()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedInvoice(db);
                var wanted = SeedInvoice(db);

                var dal = new InvoiceDAL(db);
                Assert.Equal(2, dal.Read().Rows.Count);

                // The old clause was CONVERT(nvarchar(max), i.id) LIKE
                // N'%' + @search + N'%', so a substring of the number matched.
                // The two invoices are numbered 1 and 2, so only the first has
                // a "1" in it and the filter has something to exclude.
                var table = dal.Search(wanted.id.ToString());
                Assert.Single(table.Rows);
                Assert.Equal(wanted.id, Convert.ToInt32(table.Rows[0]["شماره فاکتور"]));
            });
        }

        [Fact]
        public void Invoice_Search_ignores_everything_but_the_invoice_number()
        {
            SqliteTestDb.WithDb(db =>
            {
                SeedInvoice(db, offCode: "OFF-SUMMER");
                var second = SeedInvoice(db);
                second.IsCheckedout = true;
                db.SaveChanges();

                var dal = new InvoiceDAL(db);

                // The discount code is a visible column but the old LIKE clause
                // covered the converted id only, so it must still not match.
                Assert.Equal(0, dal.Search("OFF-SUMMER").Rows.Count);

                // Neither does the payment status, and an empty filter still
                // returns the whole grid.
                Assert.Equal(0, dal.Search("True").Rows.Count);
                Assert.Equal(2, dal.Search(string.Empty).Rows.Count);
            });
        }

        [Fact]
        public void Invoice_Read_does_not_issue_one_query_per_invoice()
        {
            SqliteTestDb.WithDb(db =>
            {
                var log = new List<string>();
                var first = SeedInvoice(db);
                first.Lines.Add(new BE.InvoiceLine
                {
                    Quantity = 1, UnitPrice = 1000m, CatalogItem = db.CatalogItems.Local.First(),
                });
                db.SaveChanges();

                db.Database.Log = log.Add;
                new InvoiceDAL(db).Read();
                var forOneInvoice = Selects(log);
                log.Clear();

                for (var extra = 0; extra < 2; extra++)
                {
                    var invoice = SeedInvoice(db);
                    invoice.Lines.Add(new BE.InvoiceLine
                    {
                        Quantity = 1, UnitPrice = 1000m, CatalogItem = db.CatalogItems.Local.First(),
                    });
                    db.SaveChanges();
                }
                // The seeding statements are dropped: System.Data.SQLite asks
                // for last_insert_rowid() with a SELECT after every insert,
                // and those are the test's own writes, not the grid's reads.
                log.Clear();

                new InvoiceDAL(db).Read();
                var forThreeInvoices = Selects(log);

                // The two computed columns come from InvoiceLines, so this is
                // the one grid that has to pull a second table in. If the
                // Include were dropped EF6 would not lazy load and Lines would
                // be empty; if the sums ran as SQL per invoice the count would
                // grow with the number of rows. EF6 folds a collection Include
                // into a single LEFT JOIN and the projection runs in memory
                // over the materialised list, so the whole grid is one
                // statement however many invoices are on it.
                Assert.Equal(forOneInvoice, forThreeInvoices);
                Assert.Equal(1, forThreeInvoices);
            });
        }

        [Fact]
        public void Dashboard_counts_invoices_registered_today()
        {
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
            });
        }

        [Fact]
        public void Dashboard_count_today_is_not_confused_by_the_time_of_day()
        {
            // DbFunctions.TruncateTime is unavailable on SQLite, so a range
            // predicate must replace it. Late-evening rows must still count.
            SqliteTestDb.WithDb(db =>
            {
                db.Invoices.Add(new BE.Invoice
                {
                    RegDate = DateTime.Today.AddHours(23).AddMinutes(59),
                    IsCheckedout = false, DiscountAmount = 0m,
                });
                db.SaveChanges();

                Assert.Equal("1", new DashboardDAL(db).SellsCountToday());
            });
        }

        [Fact]
        public void Dashboard_count_today_includes_an_invoice_registered_at_midnight()
        {
            // The lower bound has to be inclusive or an invoice filed in the
            // first second of the day is lost. This is the row a
            // "RegDate > midnight" rewrite would drop.
            SqliteTestDb.WithDb(db =>
            {
                db.Invoices.Add(new BE.Invoice
                {
                    RegDate = DateTime.Today, IsCheckedout = false, DiscountAmount = 0m,
                });
                db.SaveChanges();

                Assert.Equal("1", new DashboardDAL(db).SellsCountToday());
            });
        }

        [Fact]
        public void Dashboard_count_today_excludes_yesterday_even_at_late_evening()
        {
            // Yesterday at 23:59 is the nearest miss the same-day check can
            // make, and it is exactly what a range that stopped at "the start
            // of the week" or a date-only comparison on text would swallow.
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
            });
        }

        [Fact]
        public void Dashboard_counts_exclude_soft_deleted_invoices()
        {
            SqliteTestDb.WithDb(db =>
            {
                db.Invoices.Add(new BE.Invoice
                {
                    RegDate = DateTime.Now, IsCheckedout = false, DiscountAmount = 0m,
                });
                db.Invoices.Add(new BE.Invoice
                {
                    RegDate = DateTime.Now, IsCheckedout = false, DiscountAmount = 0m,
                    DeleteStatus = true,
                });
                db.SaveChanges();

                var dal = new DashboardDAL(db);
                Assert.Equal("1", dal.SellsCountToday());
                Assert.Equal("1", dal.SellsCountWeek());
            });
        }

        [Fact]
        public void Dashboard_week_count_ignores_invoices_older_than_seven_days()
        {
            // DATEADD(WEEK, -1, GETDATE()) is a rolling seven days, not the
            // current calendar week and not the last seven calendar dates, so
            // eight days back is out and six days back is in.
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
            });
        }

        [Fact]
        public void Dashboard_reminder_count_ignores_other_days_done_removed_and_other_owners()
        {
            SqliteTestDb.WithDb(db =>
            {
                var owner = SeedDashboardReminders(db);
                var colleague = db.Users.Single(i => i.UserName == "s.ahmadi");

                var dal = new DashboardDAL(db);
                // Five reminders are seeded; only one of them is due today,
                // still open, not removed and addressed to this employee.
                Assert.Equal("1", dal.UserReminderCount(owner));
                Assert.Equal("1", dal.UserReminderCount(colleague));
            });
        }

        [Fact]
        public void Dashboard_get_user_reminder_returns_only_todays_open_reminders()
        {
            SqliteTestDb.WithDb(db =>
            {
                var owner = SeedDashboardReminders(db);

                var reminders = new DashboardDAL(db).GetUserReminder(owner);
                Assert.Equal(new[] { "تماس امروز" }, reminders.Select(r => r.Title));
            });
        }

        [Fact]
        public void Dashboard_get_user_reminder_fills_the_owner()
        {
            SqliteTestDb.WithDb(db =>
            {
                var owner = SeedDashboardReminders(db);

                // The reminder list is shown with the owner's name, and EF6
                // does not lazy load, so a dropped Include("User") would render
                // a blank line here rather than fail.
                var reminder = new DashboardDAL(db).GetUserReminder(owner).Single();
                Assert.NotNull(reminder.User);
                Assert.Equal("m.karimi", reminder.User.UserName);
            });
        }

        [Fact]
        public void Dashboard_counters_render_zero_rather_than_throwing_when_the_database_fails()
        {
            // The dashboard is the first screen the app opens; a database
            // failure has to show a zero on it, not take the window down.
            // A null context fails every query inside the try block, which is
            // the same place a provider error would land.
            var dal = new DashboardDAL(null);
            Assert.Equal("0", dal.SellsCountToday());
            Assert.Equal("0", dal.SellsCountWeek());
            Assert.Equal("0", dal.UserReminderCount(new BE.User { id = 1 }));
            Assert.Equal("0", dal.CustomersCount());
            Assert.False(dal.PanelIsActive());
            Assert.Empty(dal.GetUserReminder(new BE.User { id = 1 }));
        }

        [Fact]
        public void Dashboard_reminder_counters_ignore_a_null_user()
        {
            // The BLL passes whatever it holds, and "no employee is signed in
            // yet" is a state the window reaches on first run.
            var dal = new DashboardDAL(null);
            Assert.Equal("0", dal.UserReminderCount(null));
            Assert.Empty(dal.GetUserReminder(null));
        }

        [Fact]
        public void Dashboard_queries_use_neither_truncate_nor_getdate()
        {
            SqliteTestDb.WithDb(db =>
            {
                var owner = SeedDashboardReminders(db);
                db.Invoices.Add(new BE.Invoice
                {
                    RegDate = DateTime.Now, IsCheckedout = false, DiscountAmount = 0m,
                });
                db.SaveChanges();

                var log = new List<string>();
                db.Database.Log = log.Add;

                var dal = new DashboardDAL(db);
                dal.CustomersCount();
                dal.SellsCountToday();
                dal.SellsCountWeek();
                dal.UserReminderCount(owner);
                dal.GetUserReminder(owner);
                dal.PanelIsActive();

                // TruncateTime and DATEADD(GETDATE()) are the two constructs
                // SQLite has no implementation of; the failure is a runtime
                // "no such function", not a compile error, so it is pinned
                // here against the statements that actually reach the provider.
                foreach (var forbidden in new[] { "TRUNCATE", "GETDATE", "DATEADD" })
                {
                    Assert.DoesNotContain(log, s =>
                        s.IndexOf(forbidden, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                // Six calls, six statements: the counters are queries, and the
                // Include on the reminder list is folded into the same one
                // rather than costing a second round trip per reminder.
                Assert.Equal(6, Selects(log));
            });
        }

        private static int Selects(IEnumerable<string> log)
        {
            return log.Count(s => s.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// Five reminders and two employees, so a reminder counter assertion
        /// cannot be satisfied by accident. Only "تماس امروز" is due today,
        /// still open, not removed and addressed to m.karimi: the other four
        /// each fail on exactly one of those four conditions.
        /// </summary>
        private static BE.User SeedDashboardReminders(DB db)
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
            db.Users.Add(new BE.User
            {
                Name = "کارشناس پشتیبانی", UserName = "s.ahmadi", Password = "x",
                RegDate = DateTime.Now, UserGroup = group,
            });
            db.SaveChanges();

            var owner = db.Users.Single(i => i.UserName == "m.karimi");
            var colleague = db.Users.Single(i => i.UserName == "s.ahmadi");

            db.Reminders.Add(new BE.Reminder
            {
                Title = "تماس امروز", Info = "یادداشت اول", RegDate = DateTime.Now,
                RemindDate = today.AddHours(9), User = owner,
            });
            db.Reminders.Add(new BE.Reminder
            {
                Title = "تماس دیروز", Info = "یادداشت دوم", RegDate = DateTime.Now,
                RemindDate = today.AddDays(-1), User = owner,
            });
            db.Reminders.Add(new BE.Reminder
            {
                Title = "تماس انجام‌شده", Info = "یادداشت سوم", RegDate = DateTime.Now,
                RemindDate = today.AddHours(9), IsReminded = true, User = owner,
            });
            db.Reminders.Add(new BE.Reminder
            {
                Title = "تماس حذف‌شده", Info = "یادداشت چهارم", RegDate = DateTime.Now,
                RemindDate = today.AddHours(9), DeleteStatus = true, User = owner,
            });
            db.Reminders.Add(new BE.Reminder
            {
                Title = "یادآوری همکار", Info = "یادداشت پنجم", RegDate = DateTime.Now,
                RemindDate = today.AddHours(9), User = colleague,
            });
            db.SaveChanges();

            return owner;
        }

        /// <summary>
        /// An invoice with no lines and no customer or user: both foreign keys
        /// are nullable and the grid shows neither, so an empty test invoice is
        /// enough to pin the columns that are about more than sums.
        /// </summary>
        private static BE.Invoice SeedInvoice(DB db, string offCode = null, decimal discountAmount = 0m)
        {
            if (db.CatalogItems.Local.Count() == 0)
            {
                db.CatalogItems.Add(new BE.CatalogItem
                {
                    Name = "کالا", Kind = BE.ItemKind.Good, SalePrice = 1000m, Stock = 100,
                });
                db.SaveChanges();
            }

            var invoice = new BE.Invoice
            {
                RegDate = DateTime.Now, IsCheckedout = false,
                OffCode = offCode, DiscountAmount = discountAmount,
            };
            db.Invoices.Add(invoice);
            db.SaveChanges();
            return invoice;
        }

        /// <summary>
        /// One good and one service whose names are unrelated to their labels,
        /// so a label search and a name search cannot satisfy each other.
        /// </summary>
        private static void SeedTwoCatalogItems(DB db)
        {
            db.CatalogItems.Add(new BE.CatalogItem
            {
                Name = "کالای اول", Kind = BE.ItemKind.Good,
                SalePrice = 100m, Stock = 5,
            });
            db.CatalogItems.Add(new BE.CatalogItem
            {
                Name = "خدمت اول", Kind = BE.ItemKind.Service,
                SalePrice = 200m, Stock = 0,
            });
            db.SaveChanges();
        }

        private static void SeedTwoMessages(DB db)
        {
            db.Messages.Add(new BE.Message
            {
                Content = "پیام اول", RegDate = DateTime.Now,
            });
            db.Messages.Add(new BE.Message
            {
                Content = "پیام دوم", RegDate = DateTime.Now,
            });
            db.SaveChanges();
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
