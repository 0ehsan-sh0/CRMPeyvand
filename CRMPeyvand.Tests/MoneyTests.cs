using System;
using System.Globalization;
using System.Linq;
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
        public void Display_matches_the_N0_format_the_rest_of_the_app_already_uses()
        {
            WithCulture(CultureInfo.GetCultureInfo("en-US"), () =>
            {
                Assert.Equal(1234567m.ToString("N0"), Money.Display(1234567m));
                Assert.Equal(1000m.ToString("N0"), Money.Display(1000m));
                Assert.Equal(0m.ToString("N0"), Money.Display(0m));
            });
        }

        /// <summary>
        /// The whole point: the number a user types into the price field and the
        /// number the grid and the labels show are formatted by the same rule.
        /// </summary>
        [Fact]
        public void What_the_field_shows_and_what_the_label_shows_agree()
        {
            WithCulture(CultureInfo.GetCultureInfo("fa-IR"), () =>
            {
                Assert.Equal(Money.Display(1234567m), Money.Group("1234567"));
                Assert.Equal(Money.Display(100000m), Money.Group("100000"));
            });
        }

        [Fact]
        public void DisplayWithCurrency_names_the_unit()
        {
            WithCulture(CultureInfo.GetCultureInfo("en-US"), () =>
            {
                Assert.Equal("1,000 تومان", Money.DisplayWithCurrency(1000m));
                Assert.Equal(Money.Display(1000m) + " " + Money.Currency, Money.DisplayWithCurrency(1000m));
            });
        }

        [Theory]
        [InlineData("1,234,567", 1234567)]
        [InlineData("1234", 1234)]
        [InlineData(" 12,345 ", 12345)]
        [InlineData("0", 0)]
        public void ParseWhole_reads_a_grouped_amount(string typed, int expected)
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
            Assert.Null(Money.ParseWhole("999999999999"));
        }

        [Theory]
        [InlineData("0")]
        [InlineData("7")]
        [InlineData("100")]
        [InlineData("1000")]
        [InlineData("1234567")]
        [InlineData("1234567890")]
        public void Parsing_back_what_Group_wrote_gives_the_original_amount(string digits)
        {
            WithCulture(CultureInfo.GetCultureInfo("fa-IR"), () =>
                Assert.Equal(int.Parse(digits, CultureInfo.InvariantCulture), Money.ParseWhole(Money.Group(digits))));
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
