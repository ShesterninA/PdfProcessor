using PdfProcessor.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfProcessor.Data.Repositories
{
    public interface IPdfDocumentRepository
    {
        void Add(PdfDocument document);
        Task<PdfDocument?> GetByIdAsync(Guid fileId);
        Task<List<PdfDocument>> GetPdfDocumentsAsync(int pageNumber, int pageSize);
        Task<PdfDocument?> GetTrackedByIdAsync(Guid id);
    }
}
