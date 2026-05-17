using PdfProcessor.API.Services;
using PdfProcessor.Shared.Enums;

namespace PdfProcessor.API.DTOs
{
    public class UploadPdfResult
    {
        public Guid Id { get; set; }
        public DocumentStatus Status { get; set; }
    }
}
