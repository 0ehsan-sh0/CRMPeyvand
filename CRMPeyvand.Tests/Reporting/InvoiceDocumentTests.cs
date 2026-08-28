using System.Collections.Generic;
using CRMPeyvand.Reports.Documents;
using CRMPeyvand.Reports.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Xunit;

namespace CRMPeyvand.Tests.Reporting
{
    public class InvoiceDocumentTests
    {
        public InvoiceDocumentTests()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        [Fact]
        public void InvoiceDocument_RendersValidPdf_WithExpectedByteSize()
        {
            // Arrange
            var model = new InvoiceReportModel
            {
                InvoiceNumber = "INV-1001",
                IssueDatePersian = "1405/06/05",
                CustomerName = "علی رضایی",
                CustomerPhone = "09123456789",
                Items = new List<InvoiceItemRowModel>
                {
                    new InvoiceItemRowModel { RowIndex = 1, ItemName = "نرم‌افزار CRM نسخه سازمانی", Quantity = 1, UnitPrice = 15000000 },
                    new InvoiceItemRowModel { RowIndex = 2, ItemName = "پشتیبانی و آموزش حضوری", Quantity = 2, UnitPrice = 3000000 }
                },
                DiscountAmount = 1000000,
                Note = "تسویه به صورت چک ۳۰ روزه انجام خواهد شد."
            };

            var document = new InvoiceDocument(model);

            // Act
            byte[] pdfBytes = document.GeneratePdf();

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 1000, "Generated invoice PDF size should be larger than 1KB.");

            // Verify PDF header magic bytes (%PDF-)
            Assert.Equal((byte)'%', pdfBytes[0]);
            Assert.Equal((byte)'P', pdfBytes[1]);
            Assert.Equal((byte)'D', pdfBytes[2]);
            Assert.Equal((byte)'F', pdfBytes[3]);
        }

        [Fact]
        public void InvoiceDocument_RendersMultiPageInvoice_WithoutErrors()
        {
            // Arrange: create an invoice with enough items to span across multiple pages
            var items = new List<InvoiceItemRowModel>();
            for (int i = 1; i <= 50; i++)
            {
                items.Add(new InvoiceItemRowModel
                {
                    RowIndex = i,
                    ItemName = $"آیتم تستی شماره {i} با توضیحات تکمیلی کالا یا خدمات",
                    Quantity = i,
                    UnitPrice = 100000 * i
                });
            }

            var model = new InvoiceReportModel
            {
                InvoiceNumber = "INV-MULTI-001",
                IssueDatePersian = "1405/06/05",
                CustomerName = "شرکت توسعه فناوری ایرانیان",
                CustomerPhone = "02188889999",
                Items = items,
                DiscountAmount = 500000,
                Note = "فاکتور تجمیعی پایان دوره اقلام خریداری شده."
            };

            var document = new InvoiceDocument(model);

            // Act
            byte[] pdfBytes = document.GeneratePdf();

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 5000, "Multi-page invoice should produce a substantial PDF file.");
            Assert.Equal((byte)'%', pdfBytes[0]);
            Assert.Equal((byte)'P', pdfBytes[1]);
            Assert.Equal((byte)'D', pdfBytes[2]);
            Assert.Equal((byte)'F', pdfBytes[3]);
        }

        [Fact]
        public void InvoiceDocument_WithEmptyItems_RendersWithoutErrors()
        {
            // Arrange
            var model = new InvoiceReportModel
            {
                InvoiceNumber = "INV-EMPTY",
                IssueDatePersian = "1405/06/05",
                CustomerName = "مشتری جدید",
                CustomerPhone = "09110000000",
                Items = new List<InvoiceItemRowModel>()
            };

            var document = new InvoiceDocument(model);

            // Act
            byte[] pdfBytes = document.GeneratePdf();

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 500);
        }

        [Fact]
        public void InvoiceDocument_WithNullModel_InitializesEmptyModelAndRenders()
        {
            // Arrange
            var document = new InvoiceDocument(null);

            // Assert
            Assert.NotNull(document.Model);
            Assert.Equal(string.Empty, document.Model.InvoiceNumber);

            // Act
            byte[] pdfBytes = document.GeneratePdf();
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
        }

        [Fact]
        public void InvoiceDocument_GetMetadata_ReturnsExpectedMetadata()
        {
            // Arrange
            var model = new InvoiceReportModel
            {
                InvoiceNumber = "INV-777"
            };
            var document = new InvoiceDocument(model);

            // Act
            var metadata = document.GetMetadata();

            // Assert
            Assert.NotNull(metadata);
            Assert.Contains("INV-777", metadata.Title);
            Assert.Equal("CRMPeyvand", metadata.Author);
        }
    }
}
