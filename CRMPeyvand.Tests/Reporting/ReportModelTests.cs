using System;
using System.Collections.Generic;
using System.Linq;
using CRMPeyvand.Reports.Models;
using Xunit;

namespace CRMPeyvand.Tests.Reporting
{
    public class ReportModelTests
    {
        [Fact]
        public void InvoiceReportModel_CalculatesTotalsCorrectly()
        {
            var model = new InvoiceReportModel
            {
                InvoiceNumber = "INV-1001",
                IssueDatePersian = "1405/06/05",
                CustomerName = "علی رضایی",
                CustomerPhone = "09123456789",
                Items = new List<InvoiceItemRowModel>
                {
                    new InvoiceItemRowModel { RowIndex = 1, ItemName = "نرم‌افزار CRM", Quantity = 2, UnitPrice = 5000000 },
                    new InvoiceItemRowModel { RowIndex = 2, ItemName = "پشتیبانی سالانه", Quantity = 1, UnitPrice = 2000000 }
                },
                DiscountAmount = 1000000,
                Note = "تسویه نقدی"
            };

            Assert.Equal(10000000, model.Items[0].TotalPrice);
            Assert.Equal(2000000, model.Items[1].TotalPrice);
            Assert.Equal(12000000, model.SubTotal);
            Assert.Equal(11000000, model.FinalTotal);
        }

        [Fact]
        public void InvoiceReportModel_DefaultValues_AreInitialized()
        {
            var model = new InvoiceReportModel();

            Assert.NotNull(model.Items);
            Assert.Empty(model.Items);
            Assert.Equal(0, model.SubTotal);
            Assert.Equal(0, model.FinalTotal);
            Assert.Equal(0, model.DiscountAmount);
            Assert.Equal(string.Empty, model.InvoiceNumber);
            Assert.Equal(string.Empty, model.IssueDatePersian);
            Assert.Equal(string.Empty, model.CustomerName);
            Assert.Equal(string.Empty, model.CustomerPhone);
            Assert.Equal(string.Empty, model.Note);
        }

        [Fact]
        public void CustomerReportModel_InitializesAndHoldsData()
        {
            var model = new CustomerReportModel
            {
                ReportTitle = "گزارش مشتریان",
                GeneratedDatePersian = "1405/06/05",
                Customers = new List<CustomerRowModel>
                {
                    new CustomerRowModel { RowIndex = 1, Name = "علی رضایی", Phone = "09123456789", RegDatePersian = "1405/01/01" },
                    new CustomerRowModel { RowIndex = 2, Name = "سارا احمدی", Phone = "09129876543", RegDatePersian = "1405/02/15" }
                }
            };

            Assert.Equal("گزارش مشتریان", model.ReportTitle);
            Assert.Equal("1405/06/05", model.GeneratedDatePersian);
            Assert.Equal(2, model.Customers.Count);
            Assert.Equal(2, model.TotalCount);
            Assert.Equal("علی رضایی", model.Customers[0].Name);
            Assert.Equal("09123456789", model.Customers[0].Phone);
            Assert.Equal("1405/01/01", model.Customers[0].RegDatePersian);
        }

        [Fact]
        public void ActivityReportModel_InitializesAndHoldsData()
        {
            var model = new ActivityReportModel
            {
                ReportTitle = "گزارش فعالیت‌ها",
                GeneratedDatePersian = "1405/06/05",
                StartDatePersian = "1405/06/01",
                EndDatePersian = "1405/06/05",
                Activities = new List<ActivityRowModel>
                {
                    new ActivityRowModel
                    {
                        RowIndex = 1,
                        CustomerName = "شرکت پویان",
                        UserName = "کاربر ارشد",
                        CategoryTitle = "تماس تلفنی",
                        Description = "پیگیری سفارش",
                        DatePersian = "1405/06/02",
                        Status = "انجام شده"
                    }
                }
            };

            Assert.Equal("گزارش فعالیت‌ها", model.ReportTitle);
            Assert.Equal(1, model.TotalCount);
            Assert.Equal("شرکت پویان", model.Activities[0].CustomerName);
            Assert.Equal("کاربر ارشد", model.Activities[0].UserName);
            Assert.Equal("تماس تلفنی", model.Activities[0].CategoryTitle);
            Assert.Equal("پیگیری سفارش", model.Activities[0].Description);
            Assert.Equal("1405/06/02", model.Activities[0].DatePersian);
            Assert.Equal("انجام شده", model.Activities[0].Status);
        }

        [Fact]
        public void SalesSummaryReportModel_CalculatesTotalsCorrectly()
        {
            var model = new SalesSummaryReportModel
            {
                ReportTitle = "گزارش فروش دوره‌ای",
                GeneratedDatePersian = "1405/06/05",
                StartDatePersian = "1405/06/01",
                EndDatePersian = "1405/06/05",
                UserSales = new List<UserSalesRowModel>
                {
                    new UserSalesRowModel { RowIndex = 1, UserName = "علی", InvoicesCount = 5, TotalAmount = 25000000 },
                    new UserSalesRowModel { RowIndex = 2, UserName = "مریم", InvoicesCount = 3, TotalAmount = 15000000 }
                }
            };

            Assert.Equal(8, model.TotalInvoicesCount);
            Assert.Equal(40000000, model.GrandTotalAmount);
        }

        [Fact]
        public void CatalogItemReportModel_InitializesAndCalculatesTotalsCorrectly()
        {
            var model = new CatalogItemReportModel
            {
                ReportTitle = "گزارش انبار و خدمات",
                GeneratedDatePersian = "1405/06/05",
                Items = new List<CatalogItemRowModel>
                {
                    new CatalogItemRowModel { RowIndex = 1, Name = "کالای A", Kind = "فیزیکی", Stock = 10, Price = 150000 },
                    new CatalogItemRowModel { RowIndex = 2, Name = "خدمت B", Kind = "خدمات", Stock = 5, Price = 200000 }
                }
            };

            Assert.Equal(2, model.TotalItemsCount);
            Assert.Equal(15, model.TotalStock);
            Assert.Equal(2500000, model.TotalValue); // (10 * 150000) + (5 * 200000) = 1500000 + 1000000 = 2500000
            Assert.Equal("کالای A", model.Items[0].Name);
            Assert.Equal("فیزیکی", model.Items[0].Kind);
            Assert.Equal(10, model.Items[0].Stock);
            Assert.Equal(150000, model.Items[0].Price);
        }
    }
}
