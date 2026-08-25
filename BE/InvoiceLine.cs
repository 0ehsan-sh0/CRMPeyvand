namespace BE
{
    public class InvoiceLine
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }
        public int CatalogItemId { get; set; }
        public CatalogItem CatalogItem { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }      // sale-time snapshot
        public decimal LineTotal { get { return Quantity * UnitPrice; } }
    }
}
