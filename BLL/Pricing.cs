using System;
using BE;

namespace BLL
{
    public static class Pricing
    {
        public static decimal ComputeDiscount(OffCode off, decimal subTotal)
        {
            if (off == null || subTotal <= 0) return 0m;
            if (off.IsPrice) return Math.Min(off.Price ?? 0m, subTotal);
            var percent = off.Percent ?? 0;
            return decimal.Floor(subTotal * percent / 100m);
        }

        public static decimal ComputePayable(decimal subTotal, decimal discountAmount)
        {
            var payable = subTotal - discountAmount;
            return payable < 0m ? 0m : payable;
        }
    }
}
