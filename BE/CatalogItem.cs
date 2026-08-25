namespace BE
{
    public class CatalogItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ItemKind Kind { get; set; }
        public decimal SalePrice { get; set; }
        public int Stock { get; set; }              // meaningful only for Good
        public bool DeleteStatus { get; set; }
    }
}
