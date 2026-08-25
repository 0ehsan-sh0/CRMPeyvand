using System;
using BE;

namespace BLL
{
    public static class StockPolicy
    {
        private const string InsufficientMessage = "موجودی انبار کافی نمیباشد";

        public static InvalidOperationException Validate(CatalogItem item, int requestedQuantity)
        {
            if (item.Kind == ItemKind.Service) return null;
            if (requestedQuantity <= 0)
                return new InvalidOperationException("تعداد باید بیشتر از صفر باشد");
            if (item.Stock < requestedQuantity)
                return new InvalidOperationException(InsufficientMessage + " : " + item.Name);
            return null;
        }
    }
}
