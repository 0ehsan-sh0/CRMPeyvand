using BE;
using Xunit;

namespace CRMPeyvand.Tests
{
    public class InvoiceBalanceTests
    {
        private static Invoice InvoiceWithPayable(decimal payable)
        {
            var invoice = new Invoice { DiscountAmount = 0m };
            invoice.Lines.Add(new InvoiceLine { Quantity = 1, UnitPrice = payable });
            return invoice;
        }

        [Fact]
        public void An_invoice_with_no_payments_owes_everything()
        {
            var invoice = InvoiceWithPayable(5000m);

            Assert.Equal(0m, invoice.Paid);
            Assert.Equal(5000m, invoice.Balance);
            Assert.False(invoice.IsSettled);
        }

        [Fact]
        public void A_partial_payment_leaves_the_remainder_owed()
        {
            var invoice = InvoiceWithPayable(5000m);
            invoice.Payments.Add(new Payment { Amount = 2000m });

            Assert.Equal(2000m, invoice.Paid);
            Assert.Equal(3000m, invoice.Balance);
            Assert.False(invoice.IsSettled);
        }

        [Fact]
        public void Payments_covering_the_payable_settle_the_invoice()
        {
            var invoice = InvoiceWithPayable(5000m);
            invoice.Payments.Add(new Payment { Amount = 2000m });
            invoice.Payments.Add(new Payment { Amount = 3000m });

            Assert.Equal(5000m, invoice.Paid);
            Assert.Equal(0m, invoice.Balance);
            Assert.True(invoice.IsSettled);
        }

        [Fact]
        public void A_voided_payment_stops_counting_towards_the_paid_total()
        {
            var invoice = InvoiceWithPayable(5000m);
            invoice.Payments.Add(new Payment { Amount = 2000m });
            invoice.Payments.Add(new Payment { Amount = 3000m, DeleteStatus = true });

            Assert.Equal(2000m, invoice.Paid);
            Assert.Equal(3000m, invoice.Balance);
        }

        [Fact]
        public void Balance_never_goes_negative_even_if_payments_overshoot()
        {
            var invoice = InvoiceWithPayable(5000m);
            invoice.Payments.Add(new Payment { Amount = 9000m });

            Assert.Equal(9000m, invoice.Paid);
            Assert.Equal(0m, invoice.Balance);
            Assert.True(invoice.IsSettled);
        }

        [Fact]
        public void Balance_uses_the_discounted_payable_not_the_subtotal()
        {
            var invoice = InvoiceWithPayable(10000m);
            invoice.DiscountAmount = 4000m;

            Assert.Equal(6000m, invoice.Balance);
        }

        [Fact]
        public void Customer_balance_sums_only_their_live_invoices()
        {
            var customer = new Customer();

            var live = new Invoice { DiscountAmount = 0m };
            live.Lines.Add(new InvoiceLine { Quantity = 1, UnitPrice = 8000m });
            live.Payments.Add(new Payment { Amount = 3000m });

            var deleted = new Invoice { DeleteStatus = true, DiscountAmount = 0m };
            deleted.Lines.Add(new InvoiceLine { Quantity = 1, UnitPrice = 100000m });

            customer.Invoices.Add(live);
            customer.Invoices.Add(deleted);

            Assert.Equal(8000m, customer.PayableTotal);
            Assert.Equal(3000m, customer.PaidTotal);
            Assert.Equal(5000m, customer.Balance);
        }

        [Fact]
        public void Instrument_captions_are_the_words_the_grid_and_the_receipt_share()
        {
            Assert.Equal("نقد", PaymentInstrumentTitles.Of(PaymentInstrument.Cash));
            Assert.Equal("کارت", PaymentInstrumentTitles.Of(PaymentInstrument.Card));
            Assert.Equal("حواله", PaymentInstrumentTitles.Of(PaymentInstrument.Transfer));
            Assert.Equal("چک", PaymentInstrumentTitles.Of(PaymentInstrument.Cheque));
            Assert.Equal("سایر", PaymentInstrumentTitles.Of(PaymentInstrument.Other));
        }

        [Fact]
        public void An_unknown_instrument_number_reads_as_unspecified_rather_than_throwing()
        {
            Assert.Equal("نامشخص", PaymentInstrumentTitles.Of((PaymentInstrument)99));
        }

        [Fact]
        public void A_new_payment_starts_uncashed_and_current()
        {
            var payment = new Payment();

            Assert.Equal(PaymentInstrument.Cash, payment.Instrument);
            Assert.False(payment.DeleteStatus);
        }
    }
}