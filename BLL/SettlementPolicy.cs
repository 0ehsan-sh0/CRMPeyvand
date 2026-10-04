using System;

namespace BLL
{
    /// <summary>
    /// What makes a payment a legal thing to record. Pure and static like Pricing
    /// and StockPolicy, so it can be tested without a database and so the DAL can
    /// take it as a delegate — the DAL cannot reference this assembly.
    ///
    /// The order of the checks is the order an employee needs them in: fix the
    /// amount before worrying about the date.
    /// </summary>
    public static class SettlementPolicy
    {
        public static InvalidOperationException ValidateRecording(
            decimal amount, decimal balance, DateTime date, DateTime today)
        {
            if (amount <= 0m)
                return new InvalidOperationException("مبلغ وصولی باید بیشتر از صفر باشد");

            if (date.Date > today.Date)
                return new InvalidOperationException("تاریخ وصولی نمیتواند در آینده باشد");

            if (amount > balance)
                return new InvalidOperationException("مبلغ وصولی نمیتواند بیشتر از مانده حساب فاکتور باشد");

            return null;
        }
    }
}