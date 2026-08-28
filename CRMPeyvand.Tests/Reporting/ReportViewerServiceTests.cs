using System;
using CRMPeyvand.Reports.Documents;
using CRMPeyvand.Reports.Models;
using CRMPeyvand.Reports.Services;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Xunit;

namespace CRMPeyvand.Tests.Reporting
{
    public class ReportViewerServiceTests
    {
        public ReportViewerServiceTests()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        [Fact]
        public void OpenReportPdf_WithNullDocument_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ReportViewerService.OpenReportPdf(null));
        }

        [Fact]
        public void ExportReportPdf_WithNullDocument_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ReportViewerService.ExportReportPdf(null));
        }

        [Fact]
        public void ReportDocument_GeneratePdf_ProducesValidPdfBytes()
        {
            // Arrange
            var model = new InvoiceReportModel
            {
                InvoiceNumber = "TEST-100",
                IssueDatePersian = "1405/01/01",
                CustomerName = "مشتری آزمایشی"
            };
            var doc = new InvoiceDocument(model);

            // Act
            byte[] bytes = doc.GeneratePdf();

            // Assert
            Assert.NotNull(bytes);
            Assert.NotEmpty(bytes);
            Assert.Equal((byte)'%', bytes[0]);
            Assert.Equal((byte)'P', bytes[1]);
            Assert.Equal((byte)'D', bytes[2]);
            Assert.Equal((byte)'F', bytes[3]);
        }
    }
}
