using System;
using BLL;
using Xunit;

namespace CRMPeyvand.Tests
{
    public class SettlementPolicyTests
    {
        private static readonly DateTime Today = new DateTime(2026, 10, 4);

        private static string Rejection(decimal amount, decimal balance, DateTime date)
        {
            return SettlementPolicy
                .ValidateRecording(amount, balance, date, Today)
                ?.Message;
        }

        [Fact]
        public void A_payment_up_to_the_balance_is_allowed()
        {
            Assert.Null(Rejection(5000m, 5000m, Today));
            Assert.Null(Rejection(4999m, 5000m, Today));
        }

        [Fact]
        public void A_payment_beyond_the_balance_is_refused()
        {
            Assert.Equal(
                "مبلغ وصولی نمیتواند بیشتر از مانده حساب فاکتور باشد",
                Rejection(5001m, 5000m, Today));
        }

        [Fact]
        public void Nothing_is_allowed_against_a_settled_invoice()
        {
            Assert.NotNull(Rejection(1m, 0m, Today));
        }

        [Fact]
        public void A_zero_amount_is_refused()
        {
            Assert.Equal(
                "مبلغ وصولی باید بیشتر از صفر باشد",
                Rejection(0m, 5000m, Today));
        }

        [Fact]
        public void A_negative_amount_is_refused()
        {
            Assert.Equal(
                "مبلغ وصولی باید بیشتر از صفر باشد",
                Rejection(-500m, 5000m, Today));
        }

        [Fact]
        public void A_payment_dated_in_the_future_is_refused()
        {
            Assert.Equal(
                "تاریخ وصولی نمیتواند در آینده باشد",
                Rejection(1000m, 5000m, Today.AddDays(1)));
        }

        [Fact]
        public void Backdating_is_allowed_and_so_is_an_earlier_hour_today()
        {
            Assert.Null(Rejection(1000m, 5000m, Today.AddHours(9)));
            Assert.Null(Rejection(1000m, 5000m, Today.AddDays(-30)));
        }

        [Fact]
        public void The_amount_is_checked_before_the_date_so_a_bad_amount_is_reported_first()
        {
            Assert.Equal(
                "مبلغ وصولی باید بیشتر از صفر باشد",
                Rejection(0m, 5000m, Today.AddDays(5)));
        }
    }
}