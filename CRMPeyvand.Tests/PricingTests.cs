using BLL;
using Xunit;

namespace CRMPeyvand.Tests
{
    public class PricingTests
    {
        [Fact]
        public void Null_off_code_gives_zero_discount()
        {
            Assert.Equal(0m, Pricing.ComputeDiscount(null, 1000m));
        }

        [Fact]
        public void Fixed_price_discount_is_capped_at_subtotal()
        {
            var off = new BE.OffCode { IsPrice = true, Price = 5000m };
            Assert.Equal(3000m, Pricing.ComputeDiscount(off, 3000m));
            Assert.Equal(5000m, Pricing.ComputeDiscount(off, 20000m));
        }

        [Fact]
        public void Percent_discount_floors_fractional_toman()
        {
            var off = new BE.OffCode { IsPrice = false, Percent = 15 };
            Assert.Equal(150m, Pricing.ComputeDiscount(off, 1003m));
        }

        [Fact]
        public void Payable_never_goes_negative()
        {
            Assert.Equal(0m, Pricing.ComputePayable(100m, 500m));
            Assert.Equal(600m, Pricing.ComputePayable(1000m, 400m));
        }
    }
}
