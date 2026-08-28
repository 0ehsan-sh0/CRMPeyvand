using System;
using CRMPeyvand.Reports.Common;
using CRMPeyvand.Reports.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CRMPeyvand.Reports.Documents
{
    /// <summary>
    /// QuestPDF Document implementation for Persian Activity List report.
    /// </summary>
    public class ActivityListDocument : IDocument
    {
        public ActivityReportModel Model { get; }

        public ActivityListDocument(ActivityReportModel model)
        {
            Model = model ?? new ActivityReportModel();
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = string.IsNullOrWhiteSpace(Model.ReportTitle) ? "گزارش فعالیت‌ها" : Model.ReportTitle,
            Author = "CRMPeyvand",
            Subject = "گزارش فعالیت‌ها",
            CreationDate = DateTime.Now
        };

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.2f, Unit.Centimetre);
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
                    col.Item().Text(string.IsNullOrWhiteSpace(Model.ReportTitle) ? "گزارش فعالیت‌ها" : Model.ReportTitle)
                        .Style(PersianReportStyle.TitleTextStyle);
                    col.Item().Text("سامانه مدیریت مشتریان پیوند").Style(PersianReportStyle.SubtitleTextStyle);
                });

                // Date Filter & Generation Date
                row.ConstantItem(250).AlignLeft().Column(col =>
                {
                    if (!string.IsNullOrWhiteSpace(Model.StartDatePersian) || !string.IsNullOrWhiteSpace(Model.EndDatePersian))
                    {
                        col.Item().Row(r =>
                        {
                            r.AutoItem().Text("بازه زمانی: ").Style(PersianReportStyle.SummaryLabelStyle);
                            r.RelativeItem().Text($"از {Model.StartDatePersian} تا {Model.EndDatePersian}").SemiBold();
                        });
                    }

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

                // Activity Table
                column.Item().Element(ComposeTable);

                // Summary Card
                column.Item().Element(ComposeSummaryCard);
            });
        }

        private void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);   // ردیف
                    columns.RelativeColumn(2.5f); // مشتری
                    columns.RelativeColumn(2.5f); // ثبت‌کننده
                    columns.RelativeColumn(2.5f); // دسته‌بندی
                    columns.RelativeColumn(4.5f); // توضیحات
                    columns.RelativeColumn(2f);   // تاریخ
                    columns.RelativeColumn(2f);   // وضعیت
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("ردیف").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("مشتری").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("ثبت‌کننده").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("دسته‌بندی").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("توضیحات").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("تاریخ").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("وضعیت").Style(PersianReportStyle.TableHeaderTextStyle);
                });

                // Rows
                if (Model.Activities != null && Model.Activities.Count > 0)
                {
                    for (int i = 0; i < Model.Activities.Count; i++)
                    {
                        var item = Model.Activities[i];
                        bool isZebra = i % 2 == 1;
                        int rowIndex = item.RowIndex > 0 ? item.RowIndex : i + 1;

                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(rowIndex.ToString());
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).PaddingRight(4).Text(item.CustomerName ?? string.Empty);
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).PaddingRight(4).Text(item.UserName ?? string.Empty);
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).PaddingRight(4).Text(item.CategoryTitle ?? string.Empty);
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).PaddingRight(4).Text(item.Description ?? string.Empty);
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(item.DatePersian ?? string.Empty);
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(item.Status ?? string.Empty);
                    }
                }
                else
                {
                    table.Cell().ColumnSpan(7).Element(c => PersianReportStyle.TableBodyCell(c, false))
                        .AlignCenter().Padding(12)
                        .Text("هیچ رکوردی برای نمایش وجود ندارد.")
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
                .Row(row =>
                {
                    row.RelativeItem().Row(r =>
                    {
                        r.AutoItem().Text("تعداد کل فعالیت‌ها: ").Style(PersianReportStyle.SummaryLabelStyle);
                        r.RelativeItem().Text(PersianReportStyle.FormatNumber(Model.TotalCount)).Style(PersianReportStyle.TotalHighlightStyle);
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
