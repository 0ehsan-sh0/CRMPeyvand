using System;
using CRMPeyvand.Reports.Common;
using CRMPeyvand.Reports.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CRMPeyvand.Reports.Documents
{
    /// <summary>
    /// QuestPDF document for a single رسید دریافت. The customer keeps this, so it
    /// states what the invoice came to, what this receipt pays, and what is left.
    /// </summary>
    public class PaymentReceiptDocument : IDocument
    {
        public PaymentReportModel Model { get; }

        public PaymentReceiptDocument(PaymentReportModel model)
        {
            Model = model ?? new PaymentReportModel();
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = $"رسید دریافت {Model.ReceiptNumber}".Trim(),
            Author = "CRMPeyvand",
            Subject = "رسید دریافت",
            CreationDate = DateTime.Now
        };

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2f, Unit.Centimetre);
                page.PageColor(PersianReportStyle.ColorWhite);
                page.DefaultTextStyle(PersianReportStyle.DefaultTextStyle);
                page.ContentFromRightToLeft();

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.PaddingBottom(12).BorderBottom(1).BorderColor(PersianReportStyle.ColorBorder).PaddingBottom(8).Column(col =>
            {
                col.Item().Text(string.IsNullOrWhiteSpace(Model.ReceiptTitle) ? "رسید دریافت" : Model.ReceiptTitle)
                    .Style(PersianReportStyle.TitleTextStyle);
                col.Item().Text("سامانه مدیریت مشتریان پیوند").Style(PersianReportStyle.SubtitleTextStyle);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(16).Column(column =>
            {
                column.Spacing(14);
                column.Item().Element(ComposeDetailsCard);
                column.Item().Element(ComposeAmountsCard);
                column.Item().Element(ComposeSignatures);
            });
        }

        private void ComposeDetailsCard(IContainer container)
        {
            container.Border(1)
                .BorderColor(PersianReportStyle.ColorBorder)
                .Background(PersianReportStyle.ColorWhite)
                .Padding(10)
                .Column(col =>
                {
                    col.Item().Text("مشخصات وصولی").SemiBold().FontColor(PersianReportStyle.ColorPrimary);
                    col.Spacing(4);

                    Detail(col, "شماره رسید", Model.ReceiptNumber);
                    Detail(col, "تاریخ وصول", Model.PaymentDatePersian);
                    Detail(col, "نام مشتری", Model.CustomerName);
                    Detail(col, "شماره تماس", Model.CustomerPhone);
                    Detail(col, "شماره فاکتور", Model.InvoiceNumber);
                    Detail(col, "تاریخ فاکتور", Model.InvoiceDatePersian);
                    Detail(col, "روش پرداخت", Model.InstrumentTitle);
                    Detail(col, "شماره پیگیری", Model.Reference);
                    Detail(col, "دریافت توسط", Model.ReceivedBy);
                });
        }

        private static void Detail(ColumnDescriptor col, string label, string value)
        {
            col.Item().Row(r =>
            {
                r.AutoItem().Text(label + ": ").Style(PersianReportStyle.SummaryLabelStyle);
                r.RelativeItem().Text(string.IsNullOrWhiteSpace(value) ? "-" : value);
            });
        }

        private void ComposeAmountsCard(IContainer container)
        {
            container.Border(1)
                .BorderColor(PersianReportStyle.ColorBorder)
                .Background(PersianReportStyle.ColorZebraBg)
                .Padding(10)
                .Column(col =>
                {
                    col.Spacing(4);

                    col.Item().Row(r =>
                    {
                        r.AutoItem().Text("مبلغ کل فاکتور:").Style(PersianReportStyle.SummaryLabelStyle);
                        r.RelativeItem().AlignLeft().Text(PersianReportStyle.FormatCurrency(Model.InvoiceTotal)).Style(PersianReportStyle.SummaryValueStyle);
                    });

                    col.Item().Row(r =>
                    {
                        r.AutoItem().Text("مبلغ وصولی:").SemiBold().FontColor(PersianReportStyle.ColorAccent);
                        r.RelativeItem().AlignLeft().Text(PersianReportStyle.FormatCurrency(Model.Amount)).Style(PersianReportStyle.TotalHighlightStyle);
                    });

                    col.Item().LineHorizontal(0.5f).LineColor(PersianReportStyle.ColorBorder);

                    col.Item().Row(r =>
                    {
                        r.AutoItem().Text("مانده حساب فاکتور:").Style(PersianReportStyle.SummaryLabelStyle);
                        r.RelativeItem().AlignLeft().Text(PersianReportStyle.FormatCurrency(Model.RemainingBalance)).Style(PersianReportStyle.SummaryValueStyle);
                    });
                });
        }

        private void ComposeSignatures(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor(PersianReportStyle.ColorBorder).Padding(8).Column(col =>
                {
                    col.Item().Text("مهر و امضاء فروشنده").Style(PersianReportStyle.SummaryLabelStyle);
                    col.Item().Height(40);
                });

                row.ConstantItem(20);

                row.RelativeItem().Border(1).BorderColor(PersianReportStyle.ColorBorder).Padding(8).Column(col =>
                {
                    col.Item().Text("مهر و امضاء دریافت کننده").Style(PersianReportStyle.SummaryLabelStyle);
                    col.Item().Height(40);
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.BorderTop(1).BorderColor(PersianReportStyle.ColorBorder).PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Text("سامانه مدیریت مشتریان پیوند").FontSize(8).FontColor(PersianReportStyle.ColorMuted);

                row.RelativeItem().AlignLeft().Text(text =>
                {
                    text.Span("صفحه ").FontSize(8).FontColor(PersianReportStyle.ColorMuted);
                    text.CurrentPageNumber().FontSize(8).FontColor(PersianReportStyle.ColorMuted);
                    text.Span(" از ").FontSize(8).FontColor(PersianReportStyle.ColorMuted);
                    text.TotalPages().FontSize(8).FontColor(PersianReportStyle.ColorMuted);
                });
            });
        }
    }
}