using System;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace CRMPeyvand.Reports.Common
{
    /// <summary>
    /// Cohesive styling constants and reusable layout helpers for Persian QuestPDF reports.
    /// </summary>
    public static class PersianReportStyle
    {
        // Colors
        public const string PrimaryColorHex = "#1E293B";   // Slate 800
        public const string AccentColorHex = "#2563EB";    // Blue 600
        public const string MutedColorHex = "#64748B";     // Slate 500
        public const string BorderColorHex = "#E2E8F0";    // Slate 200
        public const string ZebraBgHex = "#F8FAFC";        // Slate 50
        public const string DarkBgHex = "#0F172A";         // Slate 900
        public const string SuccessColorHex = "#16A34A";   // Green 600
        public const string WhiteHex = "#FFFFFF";

        public static readonly Color ColorPrimary = Color.FromHex(PrimaryColorHex);
        public static readonly Color ColorAccent = Color.FromHex(AccentColorHex);
        public static readonly Color ColorMuted = Color.FromHex(MutedColorHex);
        public static readonly Color ColorBorder = Color.FromHex(BorderColorHex);
        public static readonly Color ColorZebraBg = Color.FromHex(ZebraBgHex);
        public static readonly Color ColorDarkBg = Color.FromHex(DarkBgHex);
        public static readonly Color ColorSuccess = Color.FromHex(SuccessColorHex);
        public static readonly Color ColorWhite = Color.FromHex(WhiteHex);

        // Font Families
        public const string PrimaryFontFamily = "Tahoma";
        public const string SecondaryFontFamily = "Segoe UI";
        public static readonly string[] FallbackFontFamilies = new[] { "Tahoma", "Segoe UI", "Vazirmatn", "Arial" };

        // Typography Styles
        public static TextStyle DefaultTextStyle => TextStyle.Default
            .FontFamily(PrimaryFontFamily)
            .FontSize(9)
            .FontColor(ColorPrimary);

        public static TextStyle TitleTextStyle => TextStyle.Default
            .FontFamily(PrimaryFontFamily)
            .FontSize(16)
            .Bold()
            .FontColor(ColorPrimary);

        public static TextStyle SubtitleTextStyle => TextStyle.Default
            .FontFamily(PrimaryFontFamily)
            .FontSize(10)
            .SemiBold()
            .FontColor(ColorMuted);

        public static TextStyle TableHeaderTextStyle => TextStyle.Default
            .FontFamily(PrimaryFontFamily)
            .FontSize(9)
            .Bold()
            .FontColor(ColorWhite);

        public static TextStyle TableBodyTextStyle => TextStyle.Default
            .FontFamily(PrimaryFontFamily)
            .FontSize(9)
            .FontColor(ColorPrimary);

        public static TextStyle SummaryLabelStyle => TextStyle.Default
            .FontFamily(PrimaryFontFamily)
            .FontSize(9)
            .SemiBold()
            .FontColor(ColorMuted);

        public static TextStyle SummaryValueStyle => TextStyle.Default
            .FontFamily(PrimaryFontFamily)
            .FontSize(9)
            .Bold()
            .FontColor(ColorPrimary);

        public static TextStyle TotalHighlightStyle => TextStyle.Default
            .FontFamily(PrimaryFontFamily)
            .FontSize(11)
            .Bold()
            .FontColor(ColorAccent);

        public static TextStyle MutedTextStyle => TextStyle.Default
            .FontFamily(PrimaryFontFamily)
            .FontSize(8)
            .FontColor(ColorMuted);

        // Reusable Container Extension Helpers
        public static IContainer TableHeaderCell(this IContainer container)
        {
            return container
                .Background(ColorPrimary)
                .Border(0.5f)
                .BorderColor(ColorPrimary)
                .PaddingVertical(6)
                .PaddingHorizontal(4)
                .AlignCenter()
                .AlignMiddle();
        }

        public static IContainer TableBodyCell(this IContainer container, bool isZebra = false)
        {
            return container
                .Background(isZebra ? ColorZebraBg : ColorWhite)
                .BorderBottom(0.5f)
                .BorderColor(ColorBorder)
                .BorderLeft(0.5f)
                .BorderRight(0.5f)
                .PaddingVertical(5)
                .PaddingHorizontal(4)
                .AlignMiddle();
        }

        public static IContainer Card(this IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(ColorBorder)
                .Background(ColorWhite)
                .Padding(8);
        }

        // Formatting Helpers
        public static string FormatCurrency(double amount, string unit = "ریال")
        {
            return $"{amount:N0} {unit}".Trim();
        }

        public static string FormatNumber(double number)
        {
            return number.ToString("N0");
        }

        public static string FormatNumber(int number)
        {
            return number.ToString("N0");
        }

        public static string FormatPersianDate(DateTime date)
        {
            var calendar = new PersianCalendar();
            return $"{calendar.GetYear(date):0000}/{calendar.GetMonth(date):00}/{calendar.GetDayOfMonth(date):00}";
        }
    }
}
