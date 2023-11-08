namespace Services.FileTypeBasedTextExtraction
{
    using System.Text;
    using iText.Kernel.Pdf;
    using iText.Kernel.Pdf.Canvas.Parser;
    using iText.Kernel.Pdf.Canvas.Parser.Listener;

    public class PdfToTextService : ITextExtraction
    {
        public string ExtractText(string filePath)
        {
            return this.ExtractTextFromPDF(filePath);
        }

        private string ExtractTextFromPDF(string filePath)
        {
            using var pdfReader = new PdfReader(filePath);
            using var pdfDoc = new PdfDocument(pdfReader);
            var sb = new StringBuilder();

            for (var page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
            {
                sb.AppendLine(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), new SimpleTextExtractionStrategy()));
            }

            return sb.ToString();
        }
    }
}
