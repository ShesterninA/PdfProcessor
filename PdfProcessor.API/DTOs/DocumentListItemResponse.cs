using PdfProcessor.Shared.Enums;

namespace PdfProcessor.API.DTOs
{
    public class DocumentListItemResponse
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public DocumentStatus Status { get; set; }
    }
}
