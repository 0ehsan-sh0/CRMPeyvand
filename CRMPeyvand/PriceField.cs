using System.Windows.Controls;
using System.Windows.Input;

namespace CRMPeyvand
{
    /// <summary>
    /// Keeps a price field grouped while it is being typed into.
    ///
    /// This cannot reuse <see cref="PublicMethods.FilterNumber"/>, which deletes
    /// any character that is not a digit: it would strip the very separator it
    /// has just been asked to show. Both are wired to the same TextChanged event
    /// on the same form, so which one a field gets is decided by the XAML.
    /// </summary>
    public static class PriceField
    {
        /// <summary>
        /// Call from TextChanged. Leaves the field holding nothing but its digits
        /// and the separators between them, with the caret on the digit the user
        /// was last editing rather than wherever the rewrite dropped it.
        /// </summary>
        public static void GroupAsTyped(TextBox field)
        {
            if (field == null)
            {
                return;
            }

            string before = field.Text;
            string digits = Money.Digits(before);
            if (digits.Length == 0)
            {
                if (before.Length != 0)
                {
                    field.Text = string.Empty;
                }
                return;
            }

            string grouped = Money.Group(digits);
            if (grouped == before)
            {
                return;
            }

            // Where the caret was, counted in digits rather than characters, so
            // that the separators rewritten around it do not count as movement.
            int caret = field.SelectionStart;
            int digitsBeforeCaret = Money.Digits(
                caret > before.Length ? before : before.Substring(0, caret)).Length;

            field.Text = grouped;
            field.SelectionStart = CaretAfter(grouped, digitsBeforeCaret);
            field.SelectionLength = 0;
        }

        /// <summary>
        /// Call from PreviewKeyDown; mark the event handled when this returns
        /// true.
        ///
        /// Backspace immediately after a separator would delete the separator and
        /// nothing else, because the regrouping in <see cref="GroupAsTyped"/> puts
        /// it straight back, leaving the key looking dead. So it takes the digit
        /// in front of the separator as well, which is what the user meant.
        /// </summary>
        public static bool Backspace(TextBox field, Key key)
        {
            if (key != Key.Back || field == null || field.SelectionLength > 0)
            {
                return false;
            }

            int caret = field.SelectionStart;
            if (caret < 2 || !Money.IsGroupSeparator(field.Text[caret - 1]))
            {
                return false;
            }

            field.Text = field.Text.Remove(caret - 2, 1);
            field.SelectionStart = caret - 2;
            field.SelectionLength = 0;
            return true;
        }

        /// <summary>
        /// The index just past the nth digit of the grouped text, or the end of
        /// the text when there are fewer digits than that.
        /// </summary>
        private static int CaretAfter(string grouped, int digits)
        {
            if (digits <= 0)
            {
                return 0;
            }

            int seen = 0;
            for (int i = 0; i < grouped.Length; i++)
            {
                if (!char.IsDigit(grouped[i]))
                {
                    continue;
                }

                if (++seen == digits)
                {
                    return i + 1;
                }
            }
            return grouped.Length;
        }
    }
}
