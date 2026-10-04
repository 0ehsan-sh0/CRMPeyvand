using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DAL;
using Xunit;

namespace CRMPeyvand.Tests
{
    /// <summary>
    /// Prices are whole Toman shown with a group separator. These pin the two
    /// halves of that round trip - what the field writes and what the grids and
    /// labels read back - because they are only useful if they agree.
    /// </summary>
    public class MoneyTests
    {
        /// <summary>
        /// Money reads the current culture for its separator, and the test host's
        /// culture is whatever the machine happens to be. Culture is per thread,
        /// so setting it here cannot disturb a test running on another one, but it
        /// does outlive the test on a pooled thread and has to be put back.
        /// </summary>
        private static void WithCulture(CultureInfo culture, Action body)
        {
            CultureInfo original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = culture;
                body();
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        [Theory]
        [InlineData("1234567", "1234567")]
        [InlineData("1,234,567", "1234567")]
        [InlineData("12a3", "123")]
        [InlineData("  12  ", "12")]
        [InlineData("-1,234", "1234")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void Digits_keeps_only_what_can_be_a_number(string typed, string expected)
        {
            Assert.Equal(expected, Money.Digits(typed));
        }

        [Theory]
        [InlineData("1", "1")]
        [InlineData("12", "12")]
        [InlineData("123", "123")]
        [InlineData("1234", "1,234")]
        [InlineData("12345", "12,345")]
        [InlineData("123456", "123,456")]
        [InlineData("1234567", "1,234,567")]
        [InlineData("1234567890", "1,234,567,890")]
        public void Group_puts_a_separator_every_three_digits_from_the_right(string digits, string expected)
        {
            WithCulture(CultureInfo.GetCultureInfo("en-US"), () => Assert.Equal(expected, Money.Group(digits)));
        }

        [Theory]
        [InlineData("")]
        [InlineData("0")]
        [InlineData("7")]
        public void Group_leaves_a_short_amount_alone(string digits)
        {
            WithCulture(CultureInfo.GetCultureInfo("en-US"), () => Assert.Equal(digits, Money.Group(digits)));
        }

        /// <summary>
        /// The app runs under PersianCulture, which is fa-IR, and the invoice
        /// labels already went through "N0". If fa-IR turned out to group with
        /// nothing then every price in the app would read as one long run of
        /// digits, which is the thing being fixed.
        /// </summary>
        [Fact]
        public void Group_uses_the_cultures_own_separator()
        {
            WithCulture(CultureInfo.GetCultureInfo("fa-IR"), () =>
            {
                string separator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
                Assert.NotEmpty(separator);
                Assert.Equal("1" + separator + "234" + separator + "567", Money.Group("1234567"));
            });
        }

        [Fact]
        public void A_culture_with_no_group_separator_falls_back_to_a_comma()
        {
            var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            culture.NumberFormat.NumberGroupSeparator = string.Empty;

            WithCulture(culture, () => Assert.Equal("1,234,567", Money.Group("1234567")));
        }

        /// <summary>
        /// A long enough run of digits does not fit a long, and the field must
        /// not throw on the way to telling the user the amount is too big.
        /// </summary>
        [Fact]
        public void Group_handles_a_run_longer_than_a_long_can_hold()
        {
            WithCulture(CultureInfo.GetCultureInfo("en-US"), () =>
            {
                string typed = new string('9', 40);
                string grouped = Money.Group(typed);

                Assert.Equal(typed, Money.Digits(grouped));
                // 40 digits is a leading group of one, then thirteen groups of three.
                Assert.Equal(13, grouped.Count(c => c == ','));
            });
        }

        [Fact]
        public void Display_converts_stored_rial_into_toman_before_formatting()
        {
            WithCulture(CultureInfo.GetCultureInfo("en-US"), () =>
            {
                // 1,234,570 Rial is 123,457 Toman - the figure anyone says out loud
                // is a tenth of what the database holds.
                Assert.Equal(123457m.ToString("N0"), Money.Display(1234570m));
                Assert.Equal(100m.ToString("N0"), Money.Display(1000m));
                Assert.Equal(0m.ToString("N0"), Money.Display(0m));
            });
        }

        /// <summary>
        /// The whole point: the number a user types into the price field and the
        /// number the grid and the labels show are formatted by the same rule.
        /// Both sides are Toman now, so the field's text and Display agree.
        /// </summary>
        [Fact]
        public void What_the_field_shows_and_what_the_label_shows_agree()
        {
            WithCulture(CultureInfo.GetCultureInfo("fa-IR"), () =>
            {
                Assert.Equal(Money.Display(12345670m), Money.Group("1234567"));
                Assert.Equal(Money.Display(1000000m), Money.Group("100000"));
            });
        }

        [Fact]
        public void DisplayWithCurrency_names_the_unit()
        {
            WithCulture(CultureInfo.GetCultureInfo("en-US"), () =>
            {
                Assert.Equal("100 تومان", Money.DisplayWithCurrency(1000m));
                Assert.Equal(Money.Display(1000m) + " " + Money.Currency, Money.DisplayWithCurrency(1000m));
            });
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1000, 100)]
        [InlineData(300000, 30000)]
        [InlineData(1234570, 123457)]
        public void ToToman_drops_the_rial_a_ten_times_over(decimal rial, decimal expected)
        {
            Assert.Equal(expected, Money.ToToman(rial));
        }

        /// <summary>
        /// Rial do not divide into Toman evenly - 1,555 Rial is 155.5 Toman - and the
        /// fields only take whole numbers. Flooring keeps a shown price from ever
        /// being worth more than the money behind it.
        /// </summary>
        [Theory]
        [InlineData(1555, 155)]
        [InlineData(1559, 155)]
        [InlineData(1500, 150)]
        [InlineData(9, 0)]
        public void ToToman_floors_rather_than_rounding_up(decimal rial, decimal expected)
        {
            Assert.Equal(expected, Money.ToToman(rial));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(100, 1000)]
        [InlineData(30000, 300000)]
        [InlineData(155, 1550)]
        public void FromToman_adds_the_rial_back_on(decimal toman, decimal expected)
        {
            Assert.Equal(expected, Money.FromToman(toman));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(999)]
        [InlineData(123456)]
        public void A_whole_toman_amount_survives_the_round_trip(decimal toman)
        {
            Assert.Equal(toman, Money.ToToman(Money.FromToman(toman)));
        }

        [Fact]
        public void Ten_toman_make_a_rial()
        {
            Assert.Equal(10m, Money.RialPerToman);
        }

        [Theory]
        [InlineData("1,234,567", 1234567L)]
        [InlineData("1234", 1234L)]
        [InlineData(" 12,345 ", 12345L)]
        [InlineData("0", 0L)]
        [InlineData("30,000,000,000", 30000000000L)]
        public void ParseWhole_reads_a_grouped_amount(string typed, long expected)
        {
            Assert.Equal(expected, Money.ParseWhole(typed));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        [InlineData("abc")]
        public void ParseWhole_has_nothing_to_read(string typed)
        {
            Assert.Null(Money.ParseWhole(typed));
        }

        /// <summary>
        /// Convert.ToInt32 used to be called on this and threw, taking the window
        /// down with it.
        /// </summary>
        [Fact]
        public void ParseWhole_refuses_an_amount_past_the_range_rather_than_overflowing()
        {
            Assert.Null(Money.ParseWhole("99999999999999999999999999"));
        }

        /// <summary>
        /// Fields were once read into an int, so a price of 30,000,000,000 - a
        /// figure that fits the database column without trouble - came back null and
        /// was reported as an empty field. long is what the stored column can
        /// actually be asked for.
        /// </summary>
        [Theory]
        [InlineData("2147483648")]
        [InlineData("30000000000")]
        [InlineData("999999999999")]
        [InlineData("9223372036854775807")]
        public void A_price_beyond_the_old_int_range_is_read_normally(string typed)
        {
            Assert.Equal(long.Parse(typed), Money.ParseWhole(typed));
            Assert.False(Money.IsTooLarge(typed));
        }

        [Theory]
        [InlineData("99999999999999999999999999")]
        [InlineData("9223372036854775808")]
        public void A_number_wider_than_a_long_is_recognised_as_such(string typed)
        {
            Assert.True(Money.IsTooLarge(typed));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        [InlineData("abc")]
        [InlineData("0")]
        [InlineData("1234")]
        [InlineData("12,345")]
        public void A_field_that_is_blank_or_within_range_is_not_too_large(string typed)
        {
            Assert.False(Money.IsTooLarge(typed));
        }

        /// <summary>
        /// Too large and blank used to be the same null, so a form asking only "did
        /// it parse?" told someone whose field was full that they had filled
        /// nothing in.
        /// </summary>
        [Fact]
        public void Too_large_and_blank_are_different_answers()
        {
            Assert.Null(Money.ParseWhole("99999999999999999999999999"));
            Assert.True(Money.IsTooLarge("99999999999999999999999999"));

            Assert.Null(Money.ParseWhole(""));
            Assert.False(Money.IsTooLarge(""));
        }

        /// <summary>
        /// The widest amount a field accepts, once Toman has been converted to the
        /// Rial that gets stored, still has to be storable - so the money columns,
        /// not the field, have to be the wall. Reading the declared type out of the
        /// SQLite DDL rather than repeating the number means widening one and not the
        /// other cannot pass unnoticed.
        /// </summary>
        [Theory]
        [InlineData("OffCodes", "Price")]
        [InlineData("Invoices", "DiscountAmount")]
        [InlineData("CatalogItems", "SalePrice")]
        [InlineData("InvoiceLines", "UnitPrice")]
        [InlineData("Payments", "Amount")]
        public void Anything_a_field_accepts_fits_the_column_behind_it(string table, string column)
        {
            string declared = DeclaredColumnType(table, column);
            int precision = int.Parse(declared.Substring("DECIMAL(".Length, declared.IndexOf(',') - "DECIMAL(".Length),
                CultureInfo.InvariantCulture);

            // DECIMAL(p,2) spends two digits after the point, so p - 2 before it.
            int integerDigits = precision - 2;
            decimal widestColumnHolds = (decimal)Math.Pow(10, integerDigits) - 0.01m;
            decimal widestAFieldCanStore = Money.FromToman(Money.MaxFieldAmount);

            Assert.True(widestAFieldCanStore <= widestColumnHolds,
                $"{table}.{column} is DECIMAL({precision},2) and holds {integerDigits} integer digits, "
                + $"but a field can ask to store {widestAFieldCanStore} - the field must never be the wider of the two");
        }

        /// <summary>
        /// The int the fields used to be read into held 2,147,483,647 - about 21
        /// billion Rial - and that, not the schema, is what refused a price of
        /// 30,000,000,000.
        /// </summary>
        [Fact]
        public void The_field_range_is_wider_than_int()
        {
            Assert.True(Money.MaxFieldAmount > int.MaxValue);
            Assert.Equal(long.MaxValue, Money.MaxFieldAmount);
        }

        /// <summary>
        /// Pulls "DECIMAL(28,2)" out of the hand-written DDL for one column, so this
        /// test reads the same text the database is created from.
        /// </summary>
        private static string DeclaredColumnType(string table, string column)
        {
            Match tableBlock = Regex.Match(SqliteSchema.Ddl,
                $@"CREATE TABLE IF NOT EXISTS {table} \((?<body>.*?)\n\);",
                RegexOptions.Singleline);

            Assert.True(tableBlock.Success, $"{table} is not in the SQLite schema");

            Match columnLine = Regex.Match(tableBlock.Groups["body"].Value,
                $@"^\s*{column}\s+(?<type>\w+\([\d,]+\)|\w+)",
                RegexOptions.Multiline);

            Assert.True(columnLine.Success, $"{table}.{column} is not in the SQLite schema");
            return columnLine.Groups["type"].Value;
        }

        [Theory]
        [InlineData("0")]
        [InlineData("7")]
        [InlineData("100")]
        [InlineData("1000")]
        [InlineData("1234567")]
        [InlineData("1234567890")]
        [InlineData("30000000000")]
        public void Parsing_back_what_Group_wrote_gives_the_original_amount(string digits)
        {
            WithCulture(CultureInfo.GetCultureInfo("fa-IR"), () =>
                Assert.Equal(long.Parse(digits, CultureInfo.InvariantCulture), Money.ParseWhole(Money.Group(digits))));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void The_separator_the_field_looks_for_is_recognised(bool persianSeparator)
        {
            char separator = persianSeparator ? '٬' : ',';
            Assert.True(Money.IsGroupSeparator(separator));
        }

        [Theory]
        [InlineData('1')]
        [InlineData('a')]
        [InlineData(' ')]
        public void An_ordinary_character_is_not_a_separator(char typed)
        {
            Assert.False(Money.IsGroupSeparator(typed));
        }
    }
}
