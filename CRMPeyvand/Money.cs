using System;
using System.Globalization;

namespace CRMPeyvand
{
    /// <summary>
    /// Money as the user reads and writes it: whole Toman, grouped so the digits
    /// can be counted, and with the unit named.
    ///
    /// Grouping goes through the current culture rather than a hard-coded comma
    /// because the invoice labels were already using "N0" and the currency this
    /// app runs under is fa-IR. One rule, so a price cannot look one way in the
    /// field it was typed into and another way in the grid that lists it.
    ///
    /// The two units, and where each one applies:
    ///
    ///   Rial is what is STORED and what the REPORTS print. A printed document
    ///   leaves the building and is read by an accountant, so it says «ریال» and
    ///   shows the stored figure unaltered.
    ///
    ///   Toman is what the FORMS and GRIDS show and what the user types. Ten
    ///   Toman make a Rial, so a price on screen is a tenth of the number in the
    ///   database. Every conversion goes through ToToman or FromToman here rather
    ///   than being written as a division at each call site, so there is one place
    ///   that knows the ratio and one place to change if it ever does.
    /// </summary>
    public static class Money
    {
        public const string Currency = "تومان";

        /// <summary>Ten Toman make a Rial. Rial is the stored unit.</summary>
        public const decimal RialPerToman = 10m;

        /// <summary>
        /// Stored Rial as Toman, for showing.
        ///
        /// Floors rather than rounds, because Rial do not divide into Toman evenly
        /// and a price on screen must never be worth more than the money behind it.
        /// </summary>
        public static decimal ToToman(decimal rial)
        {
            return Math.Floor(rial / RialPerToman);
        }

        /// <summary>
        /// Toman as stored Rial, for saving. Exact: multiplying cannot lose a
        /// fraction the way the division above does.
        /// </summary>
        public static decimal FromToman(decimal toman)
        {
            return toman * RialPerToman;
        }

        /// <summary>
        /// The character fa-IR groups with. A culture is allowed to have none,
        /// which would print a price as one unreadable run of digits, so a comma
        /// stands in.
        /// </summary>
        public static string GroupSeparator
        {
            get
            {
                string separator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
                return string.IsNullOrEmpty(separator) ? "," : separator;
            }
        }

        /// <summary>
        /// Just the digits, in the order they were typed: separators, spaces and
        /// anything else dropped. An empty or absent field reads as no digits
        /// rather than as a zero, so a price that was never filled in is not
        /// mistaken for a free item.
        /// </summary>
        public static string Digits(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var digits = new System.Text.StringBuilder(text.Length);
            foreach (char c in text)
            {
                if (c >= '0' && c <= '9')
                {
                    digits.Append(c);
                }
            }
            return digits.ToString();
        }

        /// <summary>
        /// "1234567" becomes "1,234,567". Counted from the right, because that is
        /// the end a number grows at.
        ///
        /// Anything already grouped, empty, or far too long for a long to hold
        /// comes through as its digits; a price that does not fit is the save
        /// button's problem to report, not this method's to throw over.
        /// </summary>
        public static string Group(string digits)
        {
            if (string.IsNullOrEmpty(digits))
            {
                return string.Empty;
            }

            digits = Digits(digits);
            if (digits.Length <= 3)
            {
                return digits;
            }

            string separator = GroupSeparator;
            var grouped = new System.Text.StringBuilder(digits.Length + digits.Length / 3);
            int lead = digits.Length % 3;
            if (lead == 0)
            {
                lead = 3;
            }

            grouped.Append(digits, 0, lead);
            for (int i = lead; i < digits.Length; i += 3)
            {
                grouped.Append(separator);
                grouped.Append(digits, i, 3);
            }
            return grouped.ToString();
        }

        /// <summary>
        /// What a grid cell or a label shows: the stored Rial converted to Toman,
        /// grouped, with no unit named.
        /// </summary>
        public static string Display(decimal rial)
        {
            return ToToman(rial).ToString("N0");
        }

        /// <summary>What the price field shows once it holds something.</summary>
        public static string DisplayWithCurrency(decimal rial)
        {
            return Display(rial) + " " + Currency;
        }

        /// <summary>
        /// The number behind whatever the field is showing, or null when it is
        /// not a number this app can sell at. Null rather than an exception
        /// because the field is mid-edit most of the time and an amount too big
        /// for an int is a thing to tell the user, not to crash on.
        /// </summary>
        public static int? ParseWhole(string text)
        {
            string digits = Digits(text);
            if (digits.Length == 0)
            {
                return null;
            }

            return int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out int amount)
                ? amount
                : (int?)null;
        }

        /// <summary>
        /// True for the separators a grouped field puts in, and only those: the
        /// backspace handling needs to tell a separator from a digit.
        /// </summary>
        public static bool IsGroupSeparator(char c)
        {
            if (c == ',' || c == '٬')
            {
                return true;
            }

            string separator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
            return separator.Length == 1 && separator[0] == c;
        }
    }
}
