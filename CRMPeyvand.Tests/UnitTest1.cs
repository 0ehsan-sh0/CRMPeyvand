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
    }
}
