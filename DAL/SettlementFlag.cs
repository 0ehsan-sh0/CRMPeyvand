using System;
using BE;

namespace DAL
{
    /// <summary>
    /// The only writer of Invoice.IsCheckedout and Invoice.CheckoutDate.
    ///
    /// The flag is a cache, not a record: payments are the truth and this is
    /// derived from them. See
    /// docs/adr/0009-payments-authoritative-over-checkout-flag.md.
    ///
    /// Callers pass the paid total rather than letting this read Invoice.Paid,
    /// because on the write path the payment being recorded is not yet in the
    /// invoice's loaded collection, and on the void path the payment being voided
    /// still is. Taking the figure as an argument keeps the rule in one place and
    /// makes the caller responsible for getting the number right.
    /// </summary>
    internal static class SettlementFlag
    {
        internal static void Apply(Invoice invoice, decimal paid)
        {
            invoice.IsCheckedout = Math.Max(0m, invoice.Payable - paid) <= 0m;
            invoice.CheckoutDate = invoice.IsCheckedout
                ? (invoice.CheckoutDate ?? invoice.RegDate)
                : null;
        }
    }
}