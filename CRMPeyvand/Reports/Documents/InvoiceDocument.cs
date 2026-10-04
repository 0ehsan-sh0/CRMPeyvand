using System;
using System.Linq;
using CRMPeyvand.Reports.Common;
using CRMPeyvand.Reports.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CRMPeyvand.Reports.Documents
{
    /// <summary>
    /// QuestPDF Document implementation for Persian sales invoices.
    /// </summary>
    public class InvoiceDocument : IDocument
    {
        public InvoiceReportModel Model { get; }

        public InvoiceDocument(InvoiceReportModel model)
        {
            Model = model ?? new InvoiceReportModel();
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = $"فاکتور فروش {Model.InvoiceNumber}".Trim(),
            Author = "CRMPeyvand",
            Subject = "فاکتور فروش",
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
                // Right side: Document Title and Subtitle
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("فاکتور فروش").Style(PersianReportStyle.TitleTextStyle);
                    col.Item().Text("سامانه مدیریت مشتریان پیوند").Style(PersianReportStyle.SubtitleTextStyle);
                });

                // Left side: Invoice Number and Date
                row.ConstantItem(200).AlignLeft().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.AutoItem().Text("شماره فاکتور: ").Style(PersianReportStyle.SummaryLabelStyle);
                        r.RelativeItem().Text(string.IsNullOrWhiteSpace(Model.InvoiceNumber) ? "---" : Model.InvoiceNumber).Bold();
                    });

                    col.Item().Row(r =>
                    {
                        r.AutoItem().Text("تاریخ: ").Style(PersianReportStyle.SummaryLabelStyle);
                        r.RelativeItem().Text(string.IsNullOrWhiteSpace(Model.IssueDatePersian) ? "---" : Model.IssueDatePersian).Bold();
                    });
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(10).Column(column =>
            {
                column.Spacing(12);

                // 1. Customer Information Card
                column.Item().Element(ComposeCustomerInfo);

                // 2. Invoice Items Table
                column.Item().Element(ComposeTable);

                // 3. Totals and Notes Summary Section
                column.Item().Element(ComposeSummaryAndNotes);

                // 4. Signatures Box
                column.Item().PaddingTop(16).Element(ComposeSignatures);
            });
        }

        private void ComposeCustomerInfo(IContainer container)
        {
            container.Border(1)
                .BorderColor(PersianReportStyle.ColorBorder)
                .Background(PersianReportStyle.ColorZebraBg)
                .Padding(10)
                .Column(col =>
                {
                    col.Item().PaddingBottom(4).Text("مشخصات خریدار").SemiBold().FontColor(PersianReportStyle.ColorPrimary);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Row(r =>
                        {
                            r.AutoItem().Text("نام مشتری: ").Style(PersianReportStyle.SummaryLabelStyle);
                            r.RelativeItem().Text(string.IsNullOrWhiteSpace(Model.CustomerName) ? "نامشخص" : Model.CustomerName).Bold();
                        });

                        row.RelativeItem().Row(r =>
                        {
                            r.AutoItem().Text("شماره تماس: ").Style(PersianReportStyle.SummaryLabelStyle);
                            r.RelativeItem().Text(string.IsNullOrWhiteSpace(Model.CustomerPhone) ? "---" : Model.CustomerPhone).Bold();
                        });
                    });
                });
        }

        private void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35); // #
                    columns.RelativeColumn(5);  // Description
                    columns.RelativeColumn(2);  // Qty
                    columns.RelativeColumn(3);  // Unit Price
                    columns.RelativeColumn(3);  // Total Price
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("ردیف").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("شرح کالا / خدمات").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("تعداد").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("قیمت واحد (ریال)").Style(PersianReportStyle.TableHeaderTextStyle);
                    header.Cell().Element(PersianReportStyle.TableHeaderCell).Text("مبلغ کل (ریال)").Style(PersianReportStyle.TableHeaderTextStyle);
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
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).Text(item.ItemName ?? string.Empty);
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(PersianReportStyle.FormatNumber(item.Quantity));
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(PersianReportStyle.FormatNumber(item.UnitPrice));
                        table.Cell().Element(c => PersianReportStyle.TableBodyCell(c, isZebra)).AlignCenter().Text(PersianReportStyle.FormatNumber(item.TotalPrice));
                    }
                }
                else
                {
                    table.Cell().ColumnSpan(5).Element(c => PersianReportStyle.TableBodyCell(c, false)).AlignCenter().Padding(12).Text("هیچ ردیف کالایی در این فاکتور ثبت نشده است.").Italic().FontColor(PersianReportStyle.ColorMuted);
                }
            });
        }

        private void ComposeSummaryAndNotes(IContainer container)
        {
            container.Row(row =>
            {
                // Notes Column (Right side in RTL)
                row.RelativeItem(3).Column(col =>
                {
                    if (!string.IsNullOrWhiteSpace(Model.Note))
                    {
                        col.Item().Border(1)
                            .BorderColor(PersianReportStyle.ColorBorder)
                            .Background(PersianReportStyle.ColorWhite)
                            .Padding(8)
                            .Column(noteCol =>
                            {
                                noteCol.Item().Text("توضیحات:").SemiBold().FontColor(PersianReportStyle.ColorMuted);
                                noteCol.Item().Text(Model.Note);
                            });
                    }
                });

                row.ConstantItem(20); // Spacing

                // Totals Summary Box (Left side in RTL)
                row.RelativeItem(2).Border(1)
                    .BorderColor(PersianReportStyle.ColorBorder)
                    .Background(PersianReportStyle.ColorWhite)
                    .Padding(8)
                    .Column(col =>
                    {
                        col.Spacing(4);

                        // Subtotal
                        col.Item().Row(r =>
                        {
                            r.AutoItem().Text("جمع کل:").Style(PersianReportStyle.SummaryLabelStyle);
                            r.RelativeItem().AlignLeft().Text(PersianReportStyle.FormatCurrency(Model.SubTotal)).Style(PersianReportStyle.SummaryValueStyle);
                        });

                        // Discount
                        col.Item().Row(r =>
                        {
                            r.AutoItem().Text("تخفیف:").Style(PersianReportStyle.SummaryLabelStyle);
                            r.RelativeItem().AlignLeft().Text(PersianReportStyle.FormatCurrency(Model.DiscountAmount)).Style(PersianReportStyle.SummaryValueStyle);
                        });

                        // Paid so far
                        col.Item().Row(r =>
                        {
                            r.AutoItem().Text("پرداخت شده:").Style(PersianReportStyle.SummaryLabelStyle);
                            r.RelativeItem().AlignLeft().Text(PersianReportStyle.FormatCurrency(Model.PaidAmount)).Style(PersianReportStyle.SummaryValueStyle);
                        });

                        // What is still owed
                        col.Item().Row(r =>
                        {
                            r.AutoItem().Text("مانده حساب:").Style(PersianReportStyle.SummaryLabelStyle);
                            r.RelativeItem().AlignLeft().Text(PersianReportStyle.FormatCurrency(Model.RemainingBalance)).Style(PersianReportStyle.SummaryValueStyle);
                        });

                        col.Item().LineHorizontal(0.5f).LineColor(PersianReportStyle.ColorBorder);

                        // Final Total
                        col.Item().Background(PersianReportStyle.ColorZebraBg).Padding(4).Row(r =>
                        {
                            r.AutoItem().Text("مبلغ نهایی:").SemiBold().FontColor(PersianReportStyle.ColorAccent);
                            r.RelativeItem().AlignLeft().Text(PersianReportStyle.FormatCurrency(Model.FinalTotal)).Style(PersianReportStyle.TotalHighlightStyle);
                        });
                    });
            });
        }

        private void ComposeSignatures(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Border(0.5f)
                    .BorderColor(PersianReportStyle.ColorBorder)
                    .Padding(10)
                    .Column(col =>
                    {
                        col.Item().AlignCenter().Text("مهر و امضاء خریدار").SemiBold().FontColor(PersianReportStyle.ColorMuted);
                        col.Item().Height(40);
                    });

                row.ConstantItem(20);

                row.RelativeItem().Border(0.5f)
                    .BorderColor(PersianReportStyle.ColorBorder)
                    .Padding(10)
                    .Column(col =>
                    {
                        col.Item().AlignCenter().Text("مهر و امضاء فروشنده").SemiBold().FontColor(PersianReportStyle.ColorMuted);
                        col.Item().Height(40);
                    });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.BorderTop(1).BorderColor(PersianReportStyle.ColorBorder).PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Text("از خرید شما متشکریم.").FontSize(8).FontColor(PersianReportStyle.ColorMuted);

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
