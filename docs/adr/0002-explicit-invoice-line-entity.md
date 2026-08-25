# Explicit Invoice Line entity replaces the implicit product↔invoice link

Invoices previously linked Products through a bare many-to-many join table (`ProductInvoices`, two FKs only), leaving nowhere to record Quantity or the unit price at sale time; quantity was smuggled through transient state on the catalog entity and stock was decremented inside the DAL. We replace it with an explicit **Invoice Line** entity owned by the Invoice — ProductId, Quantity, UnitPrice (sale time), LineTotal — while the Invoice header carries computed SubTotal / Discount / Payable. Stock decrement becomes explicit domain logic on the Invoice creation path, applied only to Goods.

## Consequences

- The join table is dropped; an invoice may list the same catalog item twice as distinct lines.
- Existing database content is discarded (dev-only data); migrations restart from a clean baseline with automatic migrations disabled.
