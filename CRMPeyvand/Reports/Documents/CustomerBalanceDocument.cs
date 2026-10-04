using System;
using CRMPeyvand.Reports.Common;
using CRMPeyvand.Reports.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CRMPeyvand.Reports.Documents
{
    /// <summary>
    /// QuestPDF document for the مانده حساب statement.
    /// </summary>
    public class CustomerBalanceDocument : IDocument
    {
        public CustomerBalanceReportModel Model { get; }

        public CustomerBalanceDocument(CustomerBalanceReportModel model)
        {
            Model = model ?? new CustomerBalanceReportModel();
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = string.IsNullOrWhiteSpace(Model.ReportTitle) ? "گزارش مانده حساب مشتریان" : Model.ReportTitle,
            Author = "CRMPeyvand",
            Subject = "گزارش مانده حساب مشتریان",
            CreationDate = DateTime.Now
        };

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
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
            container.PaddingBottom(12).BorderBottom(1).BorderColor(PersianReportStyle.ColorBorder).PaddingBottom(8).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(string.IsNullOrWhiteSpace(Model.ReportTitle) ? "گزارش مانده حساب مشتریان" : Model.ReportTitle)
                        .Style(PersianReportStyle.TitleTextStyle);
                    col.Item().Text("سامانه مدیریت مشتریان پیوند").Style(PersianReportStyle.SubtitleTextStyle);
                });

                row.ConstantItem(180).AlignLeft().Column(col =>
                {
                    if (!string.IsNullOrWhiteSpace(Model.GeneratedDatePersian))
                    {
                        col.Item().Row(r =>
                        {
                            r.AutoItem().Text("تاریخ گزارش: ").Style(PersianReportStyle.SummaryLabelStyle);
                            r.RelativeItem().Text(Model.GeneratedDatePersian).Bold();
                        });
                    }
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(10).Column(column =>
            {
                column.Spacing(12);
                column.Item().Element(ComposeTable);
                column.Item().Element(ComposeSummaryCard);
            });
        }

        private void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40); // ردیف
                    columns.RelativeColumn(3);  // نام مشتری
                    columns.RelativeColumn(2);  // شماره تماس
                    columns.ConstantColumn(60); // تعداد فاکتور
                    columns.RelativeColumn(2);  // مبلغ کل
                    columns.RelativeColumn(2);  // مبلغ وصولی
                    columns.RelativeColumn(2);  // مانده حساب
                });

                table.Header(header =>
                {
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("ردیف").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("نام مشتری").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("شماره تماس").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("تعداد فاکتور").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("مبلغ کل").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("مبلغ وصولی").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("مانده حساب").Style(PersianReportStyle.TableHeaderTextStyle);
                });

                if (Model.Customers != null && Model.Customers.Count > 0)
                {
                    for (int i = 0; i < Model.Customers.Count; i++)
                    {
                        var item = Model.Customers[i];
                        bool isZebra = i % 2 == 1;
                        int rowIndex = item.RowIndex > 0 ? item.RowIndex : i + 1;

                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(rowIndex.ToString());
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).PaddingRight(6).Text(item.Name ?? string.Empty);
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(item.Phone ?? string.Empty);
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(PersianReportStyle.FormatNumber(item.InvoiceCount));
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(PersianReportStyle.FormatCurrency(item.TotalBilled));
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(PersianReportStyle.FormatCurrency(item.TotalReceived));
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(PersianReportStyle.FormatCurrency(item.Balance));
                    }
                }
                else
                {
                    table.Cell().ColumnSpan(7).Element(c => PersianReportStyle.TableBodyCell(c, false))
                        .AlignCenter().Padding(12)
                        .Text("هیچ مشتری بدهکاری وجود ندارد.")
                        .Italic().FontColor(PersianReportStyle.ColorMuted);
                }
            });
        }

        private void ComposeSummaryCard(IContainer container)
        {
            container.Border(1)
                .BorderColor(PersianReportStyle.ColorBorder)
                .Background(PersianReportStyle.ColorZebraBg)
                .Padding(8)
                .Column(col =>
                {
                    col.Spacing(4);
                    col.Item().Row(r =>
                    {
                        r.AutoItem().Text("تعداد مشتریان بدهکار: ").Style(PersianReportStyle.SummaryLabelStyle);
                        r.RelativeItem().Text(PersianReportStyle.FormatNumber(Model.DebtorCount)).Style(PersianReportStyle.TotalHighlightStyle);
                    });
                    col.Item().Row(r =>
                    {
                        r.AutoItem().Text("جمع مانده حساب: ").Style(PersianReportStyle.SummaryLabelStyle);
                        r.RelativeItem().Text(PersianReportStyle.FormatCurrency(Model.TotalBalance)).Style(PersianReportStyle.TotalHighlightStyle);
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