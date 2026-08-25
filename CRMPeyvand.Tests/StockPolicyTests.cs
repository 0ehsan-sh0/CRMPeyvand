using System;
using BLL;
using Xunit;

namespace CRMPeyvand.Tests
{
    public class StockPolicyTests
    {
        [Fact]
        public void Service_is_always_sellable()
        {
            var service = new BE.CatalogItem { Kind = BE.ItemKind.Service, Stock = 0 };
            Assert.Null(StockPolicy.Validate(service, 99));
        }

        [Fact]
        public void Good_within_stock_passes()
        {
            var good = new BE.CatalogItem { Kind = BE.ItemKind.Good, Stock = 5 };
            Assert.Null(StockPolicy.Validate(good, 5));
        }

        [Fact]
        public void Good_beyond_stock_is_rejected_with_persian_message()
        {
            var good = new BE.CatalogItem { Kind = BE.ItemKind.Good, Stock = 2, Name = "کابل" };
            var ex = StockPolicy.Validate(good, 3);
            Assert.NotNull(ex);
            Assert.Contains("موجودی", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Non_positive_quantity_is_rejected_even_for_stocky_goods(int qty)
        {
            var good = new BE.CatalogItem { Kind = BE.ItemKind.Good, Stock = 10 };
            Assert.NotNull(StockPolicy.Validate(good, qty));
        }
    }
}
