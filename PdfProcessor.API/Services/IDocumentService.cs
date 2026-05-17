using PdfProcessor.API.DTOs;

namespace PdfProcessor.API.Services
{
    public interface IDocumentService
    {
        Task<DocumentContentResponse> GetPdfDocumentContentAsync(Guid documentId);
        Task<List<DocumentListItemResponse>> GetPdfDocumentsAsync(int pageNumber, int pageSize);
        Task<UploadPdfResult> UploadPdfAsync(IFormFile file);
    }
}
