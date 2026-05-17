using PdfProcessor.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace PdfProcessor.Data.Repositories
{
    public class PdfDocumentRepository : IPdfDocumentRepository
    {
        private readonly PdfProcessorDbContext _dbContext;
        public PdfDocumentRepository(PdfProcessorDbContext dbContext) => _dbContext = dbContext;

        public void Add(PdfDocument document) => _dbContext.PdfDocuments.Add(document);

        public async Task<PdfDocument?> GetByIdAsync(Guid fileId)
        {
            return await _dbContext.PdfDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == fileId);
        }

        public async Task<List<PdfDocument>> GetPdfDocumentsAsync(int pageNumber, int pageSize)
        {
            return await _dbContext.PdfDocuments
                .AsNoTracking()
                .OrderByDescending(d => d.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
