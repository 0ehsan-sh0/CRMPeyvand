using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Xunit;

namespace CRMPeyvand.Tests.Reporting
{
    public class QuestPdfSetupTests
    {
        [Fact]
        public void License_ShouldBeConfiguredAsCommunity()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            Assert.Equal(LicenseType.Community, QuestPDF.Settings.License);
        }

        [Fact]
        public void GeneratePdf_ShouldProduceNonEmptyByteArray()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.Header().Text("Hello QuestPDF").FontSize(20).Bold();
                    page.Content().Text("Sample Content for Setup Verification");
                });
            });

            byte[] pdfBytes = document.GeneratePdf();

            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
            Assert.True(pdfBytes.Length > 0);

            // PDF magic bytes verification (%PDF-)
            Assert.True(pdfBytes.Length >= 4);
            Assert.Equal((byte)'%', pdfBytes[0]);
            Assert.Equal((byte)'P', pdfBytes[1]);
            Assert.Equal((byte)'D', pdfBytes[2]);
            Assert.Equal((byte)'F', pdfBytes[3]);
        }
    }
}
