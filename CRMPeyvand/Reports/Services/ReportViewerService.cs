using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace CRMPeyvand.Reports.Services
{
    /// <summary>
    /// Service responsible for rendering QuestPDF documents to temporary files or user-specified locations,
    /// and launching the system PDF viewer.
    /// </summary>
    public static class ReportViewerService
    {
        /// <summary>
        /// Generates a PDF file in %TEMP% and opens it with the default system PDF viewer.
        /// </summary>
        /// <param name="document">QuestPDF document instance</param>
        /// <param name="filePrefix">Prefix for the temporary PDF file name</param>
        /// <returns>Path to the generated temporary PDF file</returns>
        public static string OpenReportPdf(IDocument document, string filePrefix = "Report")
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            string sanitizedPrefix = string.IsNullOrWhiteSpace(filePrefix) ? "Report" : filePrefix;
            string fileName = $"{sanitizedPrefix}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.pdf";
            string tempPath = Path.Combine(Path.GetTempPath(), fileName);

            document.GeneratePdf(tempPath);

            var psi = new ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true
            };
            Process.Start(psi);

            return tempPath;
        }

        /// <summary>
        /// Opens a SaveFileDialog for exporting the document to a user-selected PDF location.
        /// </summary>
        /// <param name="document">QuestPDF document instance</param>
        /// <param name="suggestedFileName">Default file name suggested in the save dialog</param>
        /// <returns>Destination file path if saved, or null if cancelled</returns>
        public static string ExportReportPdf(IDocument document, string suggestedFileName = "Report.pdf")
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = suggestedFileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                document.GeneratePdf(saveFileDialog.FileName);
                return saveFileDialog.FileName;
            }

            return null;
        }
    }
}
