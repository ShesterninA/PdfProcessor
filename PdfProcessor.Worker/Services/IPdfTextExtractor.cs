namespace PdfProcessor.Worker.Services
{
    public interface IPdfTextExtractor
    {
        string ExtractText(string filePath);
    }
}