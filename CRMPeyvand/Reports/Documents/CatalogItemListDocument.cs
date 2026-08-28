using System;
using CRMPeyvand.Reports.Common;
using CRMPeyvand.Reports.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CRMPeyvand.Reports.Documents
{
    /// <summary>
    /// QuestPDF Document implementation for Persian Products / Services / Catalog Items report.
    /// </summary>
    public class CatalogItemListDocument : IDocument
    {
        public CatalogItemReportModel Model { get; }

        public CatalogItemListDocument(CatalogItemReportModel model)
        {
            Model = model ?? new CatalogItemReportModel();
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = string.IsNullOrWhiteSpace(Model.ReportTitle) ? "گزارش محصولات و خدمات" : Model.ReportTitle,
            Author = "CRMPeyvand",
            Subject = "گزارش محصولات و خدمات",
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
                // Title and Subtitle
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(string.IsNullOrWhiteSpace(Model.ReportTitle) ? "گزارش محصولات و خدمات" : Model.ReportTitle)
                        .Style(PersianReportStyle.TitleTextStyle);
                    col.Item().Text("سامانه مدیریت مشتریان پیوند").Style(PersianReportStyle.SubtitleTextStyle);
                });

                // Generation Date
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

                // Items Table
                column.Item().Element(ComposeTable);

                // Summary Cards
                column.Item().Element(ComposeSummaryCards);
            });
        }

        private void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);   // ردیف
                    columns.RelativeColumn(4);    // عنوان کالا / خدمات
                    columns.RelativeColumn(2.5f); // نوع
                    columns.RelativeColumn(2.5f); // موجودی انبار
                    columns.RelativeColumn(3f);   // قیمت واحد (ریال)
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("ردیف").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("عنوان کالا / خدمات").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("نوع").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("موجودی انبار").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("قیمت واحد (ریال)").Style(PersianReportStyle.TableHeaderTextStyle);
                });

                // Rows
                if (Model.Items != null && Model.Items.Count > 0)
                {
                    for (int i = 0; i < Model.Items.Count; i++)
                    {
                        var item = Model.Items[i];
                        bool isZebra = i % 2 == 1;
                        int rowIndex = item.RowIndex > 0 ? item.RowIndex : i + 1;

                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(rowIndex.ToString());
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).PaddingRight(6).Text(item.Name ?? string.Empty);
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(item.Kind ?? string.Empty);
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(PersianReportStyle.FormatNumber(item.Stock));
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(PersianReportStyle.FormatCurrency(item.Price));
                    }
                }
                else
                {
                    table.Cell().ColumnSpan(5).Element(c => PersianReportStyle.TableBodyCell(c, false))
                        .AlignCenter().Padding(12)
                        .Text("هیچ رکوردی برای نمایش وجود ندارد.")
                        .Italic().FontColor(PersianReportStyle.ColorMuted);
                }
            });
        }

        private void ComposeSummaryCards(IContainer container)
        {
            container.Border(1)
                .BorderColor(PersianReportStyle.ColorBorder)
                .Background(PersianReportStyle.ColorZebraBg)
                .Padding(10)
                .Row(row =>
                {
                    row.RelativeItem().Row(r =>
                    {
                        r.AutoItem().Text("تعداد اقلام: ").Style(PersianReportStyle.SummaryLabelStyle);
                        r.RelativeItem().Text(PersianReportStyle.FormatNumber(Model.TotalItemsCount)).Style(PersianReportStyle.SummaryValueStyle);
                    });

                    row.RelativeItem().Row(r =>
                    {
                        r.AutoItem().Text("مجموع موجودی: ").Style(PersianReportStyle.SummaryLabelStyle);
                        r.RelativeItem().Text(PersianReportStyle.FormatNumber(Model.TotalStock)).Style(PersianReportStyle.SummaryValueStyle);
                    });

                    row.RelativeItem().Row(r =>
                    {
                        r.AutoItem().Text("ارزش کل انبار: ").Style(PersianReportStyle.SummaryLabelStyle);
                        r.RelativeItem().Text(PersianReportStyle.FormatCurrency(Model.TotalValue)).Style(PersianReportStyle.TotalHighlightStyle);
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
