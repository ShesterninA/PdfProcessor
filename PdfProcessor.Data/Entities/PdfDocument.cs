using PdfProcessor.Shared.Enums;

namespace PdfProcessor.Data.Entities
{
    public class PdfDocument
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = null!;
        public DocumentStatus Status { get; set; }
        public string? Content { get; set; }

        private PdfDocument() { }

        public PdfDocument(Guid id, string fileName)
        {
            Id = id;
            FileName = fileName;
            Status = DocumentStatus.Pending;
        }
    }
}
