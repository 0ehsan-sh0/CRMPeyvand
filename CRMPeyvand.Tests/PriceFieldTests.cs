using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace CRMPeyvand.Tests
{
    /// <summary>
    /// The price field rewrites its own text under the caret on every keystroke
    /// to keep the separators in place. Two things have to survive that: the
    /// caret, and the user's ability to backspace. Both are checked here rather
    /// than in a window, because a window cannot be opened from a test.
    ///
    /// WPF needs an STA thread and the test host supplies an MTA one, so each
    /// case borrows a thread of its own.
    /// </summary>
    public class PriceFieldTests
    {
        private const string Grouped = "1,234,567";

        private static T OnStaThread<T>(Func<T> body)
        {
            T result = default;
            Exception failure = null;

            var thread = new Thread(() =>
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
                try
                {
                    result = body();
                }
                catch (Exception e)
                {
                    failure = e;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (failure != null)
            {
                ExceptionDispatchInfo.Capture(failure).Throw();
            }
            return result;
        }

        /// <summary>A field wired up the way the product form wires it.</summary>
        private static TextBox NewPriceField()
        {
            var field = new TextBox();
            field.TextChanged += (s, e) => PriceField.GroupAsTyped((TextBox)s);
            return field;
        }

        [Fact]
        public void Typed_digits_come_out_grouped()
        {
            string text = OnStaThread(() =>
            {
                TextBox field = NewPriceField();
                field.Text = "1234567";
                return field.Text;
            });

            Assert.Equal(Grouped, text);
        }

        [Fact]
        public void Letters_never_reach_the_field()
        {
            string text = OnStaThread(() =>
            {
                TextBox field = NewPriceField();
                field.Text = "12a3456";
                return field.Text;
            });

            Assert.Equal("123,456", text);
        }

        [Fact]
        public void An_empty_field_stays_empty_rather_than_becoming_a_zero()
        {
            string text = OnStaThread(() =>
            {
                TextBox field = NewPriceField();
                field.Text = "1234";
                field.Clear();
                return field.Text;
            });

            Assert.Equal(string.Empty, text);
        }

        /// <summary>
        /// Typing into the middle of a grouped number shifts every group to its
        /// right. The caret has to land on the digit just typed, not at the end.
        /// </summary>
        [Fact]
        public void The_caret_stays_on_the_digit_being_edited()
        {
            (string text, int caret) = OnStaThread(() =>
            {
                TextBox field = NewPriceField();
                field.Text = "123,456";
                // The user types a 9 after the third digit, so the caret sits at 4.
                field.Text = "1239,456";
                field.SelectionStart = 4;
                PriceField.GroupAsTyped(field);
                return (field.Text, field.SelectionStart);
            });

            Assert.Equal("1,239,456", text);
            Assert.Equal(4, caret);
        }

        [Fact]
        public void The_caret_at_the_very_start_stays_at_the_very_start()
        {
            int caret = OnStaThread(() =>
            {
                TextBox field = NewPriceField();
                field.Text = "1234";
                field.SelectionStart = 0;
                PriceField.GroupAsTyped(field);
                return field.SelectionStart;
            });

            Assert.Equal(0, caret);
        }

        [Fact]
        public void The_caret_at_the_end_moves_with_the_regrouping()
        {
            (string text, int caret) = OnStaThread(() =>
            {
                // No handler, so the text is still ungrouped when the caret is set
                // and GroupAsTyped has regrouping left to do.
                var field = new TextBox();
                field.Text = "12345678";
                field.SelectionStart = field.Text.Length;
                PriceField.GroupAsTyped(field);
                return (field.Text, field.SelectionStart);
            });

            Assert.Equal("12,345,678", text);
            Assert.Equal("12,345,678".Length, caret);
        }

        /// <summary>
        /// Backspace straight after a separator would delete the separator and
        /// nothing else, because regrouping puts it straight back - the key
        /// would look broken. It takes the digit in front of it instead.
        /// </summary>
        [Fact]
        public void Backspace_after_a_separator_takes_the_digit_with_it()
        {
            (string text, int caret, bool handled) = OnStaThread(() =>
            {
                TextBox field = NewPriceField();
                field.Text = "1,234";
                // Between the separator and the first digit of the second group.
                field.SelectionStart = 2;
                field.SelectionLength = 0;

                bool tookIt = PriceField.Backspace(field, Key.Back);
                return (field.Text, field.SelectionStart, tookIt);
            });

            Assert.True(handled);
            Assert.Equal("234", text);
            Assert.Equal(0, caret);
        }

        /// <summary>
        /// Backspacing a grouped number has to give up one digit per press. It is
        /// easy to lose a press to a separator that the regrouping puts straight
        /// back, and that reads to the user as a dead key rather than as a bug.
        /// </summary>
        [Fact]
        public void Backspacing_from_the_end_takes_one_digit_at_a_time()
        {
            string[] steps = OnStaThread(() =>
            {
                TextBox field = NewPriceField();
                field.Text = "1234";

                var steps = new List<string>();
                for (int i = 0; i < 6; i++)
                {
                    steps.Add(field.Text);

                    field.SelectionStart = field.Text.Length;
                    if (field.Text.Length > 0 && !PriceField.Backspace(field, Key.Back))
                    {
                        // Not ours: this is what the text box does on its own.
                        field.Text = field.Text.Remove(field.Text.Length - 1);
                    }
                }
                return steps.ToArray();
            });

            Assert.Equal(new[] { "1,234", "123", "12", "1", "", "" }, steps);
        }

        /// <summary>
        /// A field can be left holding a separator that was never typed - pasted
        /// in, or set from code - and no amount of typing would ever move it,
        /// because it is not a digit.
        /// </summary>
        [Theory]
        [InlineData("1,", "1")]
        [InlineData(",", "")]
        [InlineData(" ", "")]
        public void Separators_left_on_their_own_are_dropped(string typed, string expected)
        {
            string text = OnStaThread(() =>
            {
                // No handler, so the text is still as it was set.
                var field = new TextBox();
                field.Text = typed;
                PriceField.GroupAsTyped(field);
                return field.Text;
            });

            Assert.Equal(expected, text);
        }

        [Fact]
        public void Backspace_that_has_a_selection_is_left_to_the_text_box()
        {
            bool handled = OnStaThread(() =>
            {
                TextBox field = NewPriceField();
                field.Text = "1,234";
                field.SelectionStart = 1;
                field.SelectionLength = 2;
                return PriceField.Backspace(field, Key.Back);
            });

            Assert.False(handled);
        }

        [Fact]
        public void Backspace_in_front_of_a_digit_is_left_to_the_text_box()
        {
            bool handled = OnStaThread(() =>
            {
                TextBox field = NewPriceField();
                field.Text = "1,234";
                field.SelectionStart = 4;
                field.SelectionLength = 0;
                return PriceField.Backspace(field, Key.Back);
            });

            Assert.False(handled);
        }

        [Theory]
        [InlineData(Key.Delete)]
        [InlineData(Key.Enter)]
        [InlineData(Key.Tab)]
        public void Any_other_key_is_not_ours(Key key)
        {
            bool handled = OnStaThread(() =>
            {
                TextBox field = NewPriceField();
                field.Text = "1,234";
                field.SelectionStart = 3;
                return PriceField.Backspace(field, key);
            });

            Assert.False(handled);
        }

        [Fact]
        public void Backspace_at_the_start_of_the_field_is_left_to_the_text_box()
        {
            bool handled = OnStaThread(() =>
            {
                TextBox field = NewPriceField();
                field.Text = "123";
                field.SelectionStart = 0;
                return PriceField.Backspace(field, Key.Back);
            });

            Assert.False(handled);
        }

        /// <summary>
        /// The amount the field ends up holding has to be the amount that gets
        /// saved, which means the separators have to come back off.
        /// </summary>
        [Fact]
        public void What_the_field_shows_is_what_gets_saved()
        {
            long? saved = OnStaThread(() =>
            {
                TextBox field = NewPriceField();
                field.Text = "1234567";
                return Money.ParseWhole(field.Text);
            });

            Assert.Equal(1234567L, saved);
        }

        /// <summary>
        /// The field used to hand its text to an int, so typing eleven digits came
        /// back null and the form assumed nothing had been typed at all.
        /// </summary>
        [Fact]
        public void A_wide_amount_survives_the_round_trip_out_of_the_field()
        {
            long? saved = OnStaThread(() =>
            {
                TextBox field = NewPriceField();
                field.Text = "30000000000";
                Assert.False(Money.IsTooLarge(field.Text));
                return Money.ParseWhole(field.Text);
            });

            Assert.Equal(30000000000L, saved);
        }
    }
}
