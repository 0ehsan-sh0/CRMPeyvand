using System;
using System.Globalization;
using Xunit;

namespace CRMPeyvand.Tests
{
    public class SmokeTests
    {
        [Fact]
        public void TestProjectRuns()
        {
            Assert.True(true);
        }

        [Fact]
        public void TestPersianCalendarOnCultureInfo()
        {
            var culture = new CultureInfo("fa-IR");
            culture.DateTimeFormat.Calendar = new PersianCalendar();
            culture.DateTimeFormat.ShortDatePattern = "yyyy/MM/dd";

            var date = new DateTime(2026, 3, 21); // Farvardin 1, 1405
            var formatted = date.ToString("yyyy/MM/dd", culture);
            Assert.Equal("1405/01/01", formatted);
        }

        [Fact]
        public void TestPersianCultureCustomClassInstantiation()
        {
            var culture = new PersianCulture();
            Assert.IsType<PersianCalendar>(culture.Calendar);
            Assert.Contains(culture.OptionalCalendars, c => c is PersianCalendar);

            var date = new DateTime(2026, 8, 27);
            var formatted = date.ToString("yyyy/MM/dd", culture);
            Assert.Matches(@"^\d{4}/\d{2}/\d{2}$", formatted);
        }

        public class TestInvoiceItem
        {
            public string Name { get; set; }
            public double Price { get; set; }
            public int Count { get; set; }
        }

        [Fact]
        public void TestInvoiceReportRendering()
        {
            Stimulsoft.Report.StiOptions.Engine.ForceInterpretationMode = true;

            var sti = new Stimulsoft.Report.StiReport();
            string path = System.IO.Path.Combine(AppContext.BaseDirectory, "Reports", "InvoicePrint.mrt");
            if (!System.IO.File.Exists(path))
            {
                path = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\CRMPeyvand\Reports\InvoicePrint.mrt"));
            }
            Assert.True(System.IO.File.Exists(path), $"Report file not found at: {path}");

            sti.Load(path);
            sti.CalculationMode = Stimulsoft.Report.StiCalculationMode.Interpretation;

            sti["InvoiceNum"] = "101";
            sti["Date"] = "1405/06/05";
            sti["CustomerName"] = "احسان";
            sti["CustomerPhone"] = "09123456789";
            sti["TotalPrice"] = "86,000";
            sti["FinalPrice"] = "86,000";

            var items = new System.Collections.Generic.List<TestInvoiceItem>
            {
                new TestInvoiceItem { Name = "تست", Price = 12000, Count = 1 },
                new TestInvoiceItem { Name = "تست آزمون", Price = 25000, Count = 2 }
            };

            sti.RegBusinessObject("", "Product", items);
            sti.RegBusinessObject("Product", items);
            sti.Dictionary.Synchronize();
            sti.Render(false);
            Assert.NotNull(sti.RenderedPages);
        }

        [Fact]
        public void TestUsersSellsReportRendering()
        {
            Stimulsoft.Report.StiOptions.Engine.ForceInterpretationMode = true;
            var sti = new Stimulsoft.Report.StiReport();
            string path = System.IO.Path.Combine(AppContext.BaseDirectory, "Reports", "UsersSells.mrt");
            if (!System.IO.File.Exists(path))
            {
                path = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\CRMPeyvand\Reports\UsersSells.mrt"));
            }
            Assert.True(System.IO.File.Exists(path));

            sti.Load(path);
            sti.CalculationMode = Stimulsoft.Report.StiCalculationMode.Interpretation;

            sti["Date"] = "1405/06/05";
            sti["Start"] = "1405/06/01";
            sti["End"] = "1405/06/05";

            var items = new System.Collections.Generic.List<TestInvoiceItem>
            {
                new TestInvoiceItem { Name = "کاربر ۱", Count = 5 }
            };

            sti.RegBusinessObject("", "UsersSells", items);
            sti.RegBusinessObject("UsersSells", items);
            sti.Dictionary.Synchronize();
            sti.Render(false);
            Assert.NotNull(sti.RenderedPages);
        }

        [Fact]
        public void TestBlankReportRendering()
        {
            Stimulsoft.Report.StiOptions.Engine.ForceInterpretationMode = true;
            var sti = new Stimulsoft.Report.StiReport();
            sti.CalculationMode = Stimulsoft.Report.StiCalculationMode.Interpretation;
            sti.Render(false);
            Assert.NotNull(sti.RenderedPages);
            Assert.True(sti.RenderedPages.Count > 0);
        }
    }
}
