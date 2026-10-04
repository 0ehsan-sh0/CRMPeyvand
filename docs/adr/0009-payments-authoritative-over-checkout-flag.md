# Payments are the authority; IsCheckedout is derived from them

`Invoice.IsCheckedout` used to be the only record that an invoice had been
settled: a boolean written in one place, never cleared, carrying no amount. It
is now a cache of `Payments` — `Balance <= 0` — rewritten by the DAL inside the
same transaction that inserts or voids a Payment, and it is never read as
input to any decision.

We kept the column rather than dropping it because three things read it and
none of them should have to change: the invoice grid's «وضعیت پرداخت» column,
`MessageDAL.IsNotCheckedOut()` (the audience list for SMS), and the
`InvoiceDetailsForm` status label. Deriving it at write time rather than at
read time is what makes the two representations incapable of disagreeing: there
is no window between them, because there is only one write path.

The alternative was to delete the columns and derive everything at read time.
That is cleaner in principle and was rejected for a concrete reason:
`SqliteSchema` can only express `CREATE TABLE IF NOT EXISTS` and has no
`ALTER TABLE` at all, so dropping a column from an existing SQLite database
would mean a table rebuild in a class whose whole job is a forward-only DDL
batch. Keeping the column costs one assignment and preserves three readers.

**No backfill ships with this change.** An invoice that was marked paid before
Payments existed has no Payment row, so under the new rule it reads as unpaid.
We verified that nobody has installed the released MSI, so there is no
installation whose ledger this could silently corrupt, and the local
development database is cleared of its fake invoices instead. If the app is
ever distributed to an installation with real data, a backfill must be added
*before* this ships to anyone else: insert one Payment per invoice where
`IsCheckedout` is true and no Payment exists, with `Amount` = the invoice's
`Payable` (exactly recoverable, because `InvoiceLine.UnitPrice` is a sale-time
snapshot) and `RegDate` = `CheckoutDate` falling back to `RegDate`. The
`Payment` would have to be marked as an unknown instrument, since the real one
was never recorded.

The invoice form's «وضعیت پرداخت» checkbox keeps its position in the flow but
no longer means what it used to. Ticking it while composing an invoice now
records a real Payment of the full `Payable`, inside `InvoiceDAL.Create`'s
existing transaction, rather than flipping a boolean. It previously called
`InvoiceDAL.Done` in a second context after the invoice was saved, so the two
writes were never atomic. This also means `InvoiceDAL.Done` is gone: nothing
settles an invoice any more except recording money against it.
