using System.Collections.Generic;
using CRMPeyvand.Reports.Documents;
using CRMPeyvand.Reports.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Xunit;

namespace CRMPeyvand.Tests.Reporting
{
    public class ManagementReportsTests
    {
        public ManagementReportsTests()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        #region CustomerListDocument Tests

        [Fact]
        public void CustomerListDocument_RendersValidPdf_WithSinglePage()
        {
            // Arrange
            var model = new CustomerReportModel
            {
                ReportTitle = "گزارش مشتریان",
                GeneratedDatePersian = "1405/06/05",
                Customers = new List<CustomerRowModel>
                {
                    new CustomerRowModel { RowIndex = 1, Name = "احسان شریفی", Phone = "09120000000", RegDatePersian = "1405/01/01" },
                    new CustomerRowModel { RowIndex = 2, Name = "زهرا رضایی", Phone = "09121111111", RegDatePersian = "1405/02/15" }
                }
            };

            var doc = new CustomerListDocument(model);

            // Act
            byte[] pdfBytes = doc.GeneratePdf();

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 1000, "Generated customer list PDF size should be larger than 1KB.");
            Assert.Equal((byte)'%', pdfBytes[0]);
            Assert.Equal((byte)'P', pdfBytes[1]);
            Assert.Equal((byte)'D', pdfBytes[2]);
            Assert.Equal((byte)'F', pdfBytes[3]);
        }

        [Fact]
        public void CustomerListDocument_RendersMultiPage_WithoutErrors()
        {
            // Arrange
            var customers = new List<CustomerRowModel>();
            for (int i = 1; i <= 60; i++)
            {
                customers.Add(new CustomerRowModel
                {
                    RowIndex = i,
                    Name = $"مشتری سازمانی شماره {i}",
                    Phone = $"0912{i:D7}",
                    RegDatePersian = "1405/05/10"
                });
            }

            var model = new CustomerReportModel
            {
                ReportTitle = "گزارش جامع مشتریان",
                GeneratedDatePersian = "1405/06/05",
                Customers = customers
            };

            var doc = new CustomerListDocument(model);

            // Act
            byte[] pdfBytes = doc.GeneratePdf();

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 5000, "Multi-page customer list should produce a substantial PDF file.");
        }

        [Fact]
        public void CustomerListDocument_WithEmptyCustomers_RendersWithoutErrors()
        {
            // Arrange
            var model = new CustomerReportModel
            {
                ReportTitle = "گزارش مشتریان",
                GeneratedDatePersian = "1405/06/05",
                Customers = new List<CustomerRowModel>()
            };

            var doc = new CustomerListDocument(model);

            // Act
            byte[] pdfBytes = doc.GeneratePdf();

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 500);
        }

        [Fact]
        public void CustomerListDocument_WithNullModel_InitializesEmptyModelAndRenders()
        {
            // Arrange
            var doc = new CustomerListDocument(null);

            // Assert
            Assert.NotNull(doc.Model);
            Assert.Equal(0, doc.Model.TotalCount);

            // Act
            byte[] pdfBytes = doc.GeneratePdf();
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
        }

        [Fact]
        public void CustomerListDocument_GetMetadata_ReturnsExpectedMetadata()
        {
            // Arrange
            var model = new CustomerReportModel { ReportTitle = "فهرست مشتریان فعال" };
            var doc = new CustomerListDocument(model);

            // Act
            var metadata = doc.GetMetadata();

            // Assert
            Assert.NotNull(metadata);
            Assert.Equal("فهرست مشتریان فعال", metadata.Title);
            Assert.Equal("CRMPeyvand", metadata.Author);
        }

        #endregion

        #region ActivityListDocument Tests

        [Fact]
        public void ActivityListDocument_RendersValidPdf_WithSinglePage()
        {
            // Arrange
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
                        CustomerName = "علی رضایی",
                        UserName = "کارشناس فروش",
                        CategoryTitle = "تماس تلفنی",
                        Description = "هماهنگی جلسه دمو نرم‌افزار",
                        DatePersian = "1405/06/02",
                        Status = "انجام شد"
                    },
                    new ActivityRowModel
                    {
                        RowIndex = 2,
                        CustomerName = "مریم محمدی",
                        UserName = "مدیر سیستم",
                        CategoryTitle = "جلسه حضوری",
                        Description = "ارائه قرارداد و امضا نهایی",
                        DatePersian = "1405/06/04",
                        Status = "در حال پیگیری"
                    }
                }
            };

            var doc = new ActivityListDocument(model);

            // Act
            byte[] pdfBytes = doc.GeneratePdf();

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 1000);
            Assert.Equal((byte)'%', pdfBytes[0]);
            Assert.Equal((byte)'P', pdfBytes[1]);
            Assert.Equal((byte)'D', pdfBytes[2]);
            Assert.Equal((byte)'F', pdfBytes[3]);
        }

        [Fact]
        public void ActivityListDocument_RendersMultiPage_WithoutErrors()
        {
            // Arrange
            var activities = new List<ActivityRowModel>();
            for (int i = 1; i <= 50; i++)
            {
                activities.Add(new ActivityRowModel
                {
                    RowIndex = i,
                    CustomerName = $"مشتری تستی {i}",
                    UserName = $"کاربر {i % 5 + 1}",
                    CategoryTitle = "پیگیری فروش",
                    Description = $"شرح کامل فعالیت انجام شده برای ردیف شماره {i} به همراه جزئیات",
                    DatePersian = "1405/06/03",
                    Status = i % 2 == 0 ? "انجام شد" : "در انتظار"
                });
            }

            var model = new ActivityReportModel
            {
                ReportTitle = "گزارش کامل فعالیت‌های دوره‌ای",
                GeneratedDatePersian = "1405/06/05",
                StartDatePersian = "1405/05/01",
                EndDatePersian = "1405/06/05",
                Activities = activities
            };

            var doc = new ActivityListDocument(model);

            // Act
            byte[] pdfBytes = doc.GeneratePdf();

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 5000);
        }

        [Fact]
        public void ActivityListDocument_WithEmptyActivities_RendersWithoutErrors()
        {
            // Arrange
            var model = new ActivityReportModel
            {
                ReportTitle = "گزارش فعالیت‌ها",
                GeneratedDatePersian = "1405/06/05",
                Activities = new List<ActivityRowModel>()
            };

            var doc = new ActivityListDocument(model);

            // Act
            byte[] pdfBytes = doc.GeneratePdf();

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 500);
        }

        [Fact]
        public void ActivityListDocument_WithNullModel_InitializesEmptyModelAndRenders()
        {
            // Arrange
            var doc = new ActivityListDocument(null);

            // Assert
            Assert.NotNull(doc.Model);
            Assert.Equal(0, doc.Model.TotalCount);

            // Act
            byte[] pdfBytes = doc.GeneratePdf();
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
        }

        #endregion

        #region SalesSummaryDocument Tests

        [Fact]
        public void SalesSummaryDocument_RendersValidPdf_WithSinglePage()
        {
            // Arrange
            var model = new SalesSummaryReportModel
            {
                ReportTitle = "گزارش فروش دوره‌ای",
                GeneratedDatePersian = "1405/06/05",
                StartDatePersian = "1405/06/01",
                EndDatePersian = "1405/06/05",
                UserSales = new List<UserSalesRowModel>
                {
                    new UserSalesRowModel { RowIndex = 1, UserName = "کاربر ارشد", InvoicesCount = 12, TotalAmount = 45000000 },
                    new UserSalesRowModel { RowIndex = 2, UserName = "کارشناس فروش ۲", InvoicesCount = 8, TotalAmount = 28000000 }
                }
            };

            var doc = new SalesSummaryDocument(model);

            // Act
            byte[] pdfBytes = doc.GeneratePdf();

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 1000);
            Assert.Equal((byte)'%', pdfBytes[0]);
            Assert.Equal((byte)'P', pdfBytes[1]);
            Assert.Equal((byte)'D', pdfBytes[2]);
            Assert.Equal((byte)'F', pdfBytes[3]);
        }

        [Fact]
        public void SalesSummaryDocument_RendersMultiPage_WithoutErrors()
        {
            // Arrange
            var userSales = new List<UserSalesRowModel>();
            for (int i = 1; i <= 50; i++)
            {
                userSales.Add(new UserSalesRowModel
                {
                    RowIndex = i,
                    UserName = $"فروشنده سازمانی شماره {i}",
                    InvoicesCount = i * 3,
                    TotalAmount = i * 5000000
                });
            }

            var model = new SalesSummaryReportModel
            {
                ReportTitle = "گزارش تجمیعی فروش ماهانه",
                GeneratedDatePersian = "1405/06/05",
                StartDatePersian = "1405/05/01",
                EndDatePersian = "1405/06/05",
                UserSales = userSales
            };

            var doc = new SalesSummaryDocument(model);

            // Act
            byte[] pdfBytes = doc.GeneratePdf();

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 5000);
        }

        [Fact]
        public void SalesSummaryDocument_WithEmptyUserSales_RendersWithoutErrors()
        {
            // Arrange
            var model = new SalesSummaryReportModel
            {
                ReportTitle = "گزارش فروش دوره‌ای",
                GeneratedDatePersian = "1405/06/05",
                UserSales = new List<UserSalesRowModel>()
            };

            var doc = new SalesSummaryDocument(model);

            // Act
            byte[] pdfBytes = doc.GeneratePdf();

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 500);
        }

        [Fact]
        public void SalesSummaryDocument_WithNullModel_InitializesEmptyModelAndRenders()
        {
            // Arrange
            var doc = new SalesSummaryDocument(null);

            // Assert
            Assert.NotNull(doc.Model);
            Assert.Equal(0, doc.Model.TotalInvoicesCount);
            Assert.Equal(0, doc.Model.GrandTotalAmount);

            // Act
            byte[] pdfBytes = doc.GeneratePdf();
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
        }

        #endregion

        #region CatalogItemListDocument Tests

        [Fact]
        public void CatalogItemListDocument_RendersValidPdf_WithSinglePage()
        {
            // Arrange
            var model = new CatalogItemReportModel
            {
                ReportTitle = "گزارش محصولات و خدمات",
                GeneratedDatePersian = "1405/06/05",
                Items = new List<CatalogItemRowModel>
                {
                    new CatalogItemRowModel { RowIndex = 1, Name = "لایسنس نرم‌افزار CRM", Kind = "محصول دانلودی", Stock = 100, Price = 12000000 },
                    new CatalogItemRowModel { RowIndex = 2, Name = "آموزش و استقرار سازمانی", Kind = "خدمات", Stock = 0, Price = 5000000 }
                }
            };

            var doc = new CatalogItemListDocument(model);

            // Act
            byte[] pdfBytes = doc.GeneratePdf();

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 1000);
            Assert.Equal((byte)'%', pdfBytes[0]);
            Assert.Equal((byte)'P', pdfBytes[1]);
            Assert.Equal((byte)'D', pdfBytes[2]);
            Assert.Equal((byte)'F', pdfBytes[3]);
        }

        [Fact]
        public void CatalogItemListDocument_RendersMultiPage_WithoutErrors()
        {
            // Arrange
            var items = new List<CatalogItemRowModel>();
            for (int i = 1; i <= 60; i++)
            {
                items.Add(new CatalogItemRowModel
                {
                    RowIndex = i,
                    Name = $"کالا / پکیج خدماتی شماره {i}",
                    Kind = i % 2 == 0 ? "کالای فیزیکی" : "خدمات",
                    Stock = i * 5,
                    Price = i * 250000
                });
            }

            var model = new CatalogItemReportModel
            {
                ReportTitle = "گزارش جامع انبار و خدمات",
                GeneratedDatePersian = "1405/06/05",
                Items = items
            };

            var doc = new CatalogItemListDocument(model);

            // Act
            byte[] pdfBytes = doc.GeneratePdf();

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 5000);
        }

        [Fact]
        public void CatalogItemListDocument_WithEmptyItems_RendersWithoutErrors()
        {
            // Arrange
            var model = new CatalogItemReportModel
            {
                ReportTitle = "گزارش محصولات و خدمات",
                GeneratedDatePersian = "1405/06/05",
                Items = new List<CatalogItemRowModel>()
            };

            var doc = new CatalogItemListDocument(model);

            // Act
            byte[] pdfBytes = doc.GeneratePdf();

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 500);
        }

        [Fact]
        public void CatalogItemListDocument_WithNullModel_InitializesEmptyModelAndRenders()
        {
            // Arrange
            var doc = new CatalogItemListDocument(null);

            // Assert
            Assert.NotNull(doc.Model);
            Assert.Equal(0, doc.Model.TotalItemsCount);
            Assert.Equal(0, doc.Model.TotalStock);
            Assert.Equal(0, doc.Model.TotalValue);

            // Act
            byte[] pdfBytes = doc.GeneratePdf();
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
        }

        #endregion
    }
}
