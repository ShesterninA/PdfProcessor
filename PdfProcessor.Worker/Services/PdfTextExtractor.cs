using UglyToad.PdfPig;

namespace PdfProcessor.Worker.Services
{
    public class PdfTextExtractor : IPdfTextExtractor
    {
        public string ExtractText(string filePath)
        {
            using var document = PdfDocument.Open(filePath);

            var pages = document.GetPages();

            return string.Join(Environment.NewLine, pages.Select(p => p.Text));
        }
    }
}
