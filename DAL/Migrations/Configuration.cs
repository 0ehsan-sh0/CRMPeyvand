namespace DAL.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<DAL.DB>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "DAL.DB";
        }

        protected override void Seed(DAL.DB context)
        {
            EnsureReportStoredProcedures(context);
        }

        private static void EnsureReportStoredProcedures(DAL.DB context)
        {
            context.Database.ExecuteSqlCommand(@"
CREATE OR ALTER PROCEDURE dbo.ThisYearInvoices
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        i.RegDate AS [تاریخ ثبت],
        i.CheckoutDate AS [تاریخ پرداخت],
        i.IsCheckedout AS [وضعیت پرداخت],
        i.OffCode AS [کد تخفیف],
        CAST((SELECT ISNULL(SUM(l.Quantity * l.UnitPrice), 0) FROM dbo.InvoiceLines l WHERE l.InvoiceId = i.id) - i.DiscountAmount AS float) AS [قیمت کل],
        (SELECT ISNULL(SUM(l.Quantity), 0) FROM dbo.InvoiceLines l WHERE l.InvoiceId = i.id) AS [تعداد کالاهای فاکتور],
        u.Name AS [نام کاربر],
        c.Name AS [نام مشتری],
        CAST(i.id AS nvarchar(50)) AS [شماره فاکتور]
    FROM dbo.Invoices i
    LEFT JOIN dbo.Users u ON i.User_id = u.id
    LEFT JOIN dbo.Customers c ON i.Customer_id = c.id
    WHERE i.DeleteStatus = 0
      AND i.RegDate >= DATEADD(YEAR, -1, GETDATE())
    ORDER BY i.id DESC;
END;
");

            context.Database.ExecuteSqlCommand(@"
CREATE OR ALTER PROCEDURE dbo.ThisMonthInvoices
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        i.RegDate AS [تاریخ ثبت],
        i.CheckoutDate AS [تاریخ پرداخت],
        i.IsCheckedout AS [وضعیت پرداخت],
        i.OffCode AS [کد تخفیف],
        CAST((SELECT ISNULL(SUM(l.Quantity * l.UnitPrice), 0) FROM dbo.InvoiceLines l WHERE l.InvoiceId = i.id) - i.DiscountAmount AS float) AS [قیمت کل],
        (SELECT ISNULL(SUM(l.Quantity), 0) FROM dbo.InvoiceLines l WHERE l.InvoiceId = i.id) AS [تعداد کالاهای فاکتور],
        u.Name AS [نام کاربر],
        c.Name AS [نام مشتری],
        CAST(i.id AS nvarchar(50)) AS [شماره فاکتور]
    FROM dbo.Invoices i
    LEFT JOIN dbo.Users u ON i.User_id = u.id
    LEFT JOIN dbo.Customers c ON i.Customer_id = c.id
    WHERE i.DeleteStatus = 0
      AND i.RegDate >= DATEADD(MONTH, -1, GETDATE())
    ORDER BY i.id DESC;
END;
");

            context.Database.ExecuteSqlCommand(@"
CREATE OR ALTER PROCEDURE dbo.ThisWeekInvoices
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        i.RegDate AS [تاریخ ثبت],
        i.CheckoutDate AS [تاریخ پرداخت],
        i.IsCheckedout AS [وضعیت پرداخت],
        i.OffCode AS [کد تخفیف],
        CAST((SELECT ISNULL(SUM(l.Quantity * l.UnitPrice), 0) FROM dbo.InvoiceLines l WHERE l.InvoiceId = i.id) - i.DiscountAmount AS float) AS [قیمت کل],
        (SELECT ISNULL(SUM(l.Quantity), 0) FROM dbo.InvoiceLines l WHERE l.InvoiceId = i.id) AS [تعداد کالاهای فاکتور],
        u.Name AS [نام کاربر],
        c.Name AS [نام مشتری],
        CAST(i.id AS nvarchar(50)) AS [شماره فاکتور]
    FROM dbo.Invoices i
    LEFT JOIN dbo.Users u ON i.User_id = u.id
    LEFT JOIN dbo.Customers c ON i.Customer_id = c.id
    WHERE i.DeleteStatus = 0
      AND i.RegDate >= DATEADD(WEEK, -1, GETDATE())
    ORDER BY i.id DESC;
END;
");

            context.Database.ExecuteSqlCommand(@"
CREATE OR ALTER PROCEDURE dbo.ActivitiesView
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        a.RegDate AS [تاریخ ثبت],
        u.Name AS [نام کاربر],
        a.Info AS [توضیحات],
        c.Name AS [نام مشتری],
        ac.CategoryName AS [دسته بندی],
        a.Title AS [موضوع]
    FROM dbo.Activities a
    LEFT JOIN dbo.Users u ON a.User_id = u.id
    LEFT JOIN dbo.Customers c ON a.Customer_id = c.id
    LEFT JOIN dbo.ActivityCategories ac ON a.ActivityCategory_id = ac.id
    WHERE a.DeleteStatus = 0
    ORDER BY a.id DESC;
END;
");

            context.Database.ExecuteSqlCommand(@"
CREATE OR ALTER PROCEDURE dbo.ProductsTotal
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        ci.Stock AS [موجودی انبار],
        CASE WHEN ci.Kind = 1 THEN N'کالا' ELSE N'خدمات' END AS [نوع],
        ci.Name AS [نام]
    FROM dbo.CatalogItems ci
    WHERE ci.DeleteStatus = 0
    ORDER BY ci.id DESC;
END;
");
        }
    }
}
