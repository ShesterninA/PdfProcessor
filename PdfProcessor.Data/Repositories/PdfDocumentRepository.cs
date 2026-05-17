using PdfProcessor.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace PdfProcessor.Data.Repositories
{
    public class PdfDocumentRepository : IPdfDocumentRepository
    {
        private readonly PdfProcessorDbContext _dbContext;
        public PdfDocumentRepository(PdfProcessorDbContext dbContext) => _dbContext = dbContext;

        public async Task CreateAsync(PdfDocument document)
        {
            await _dbContext.AddAsync(document);
        }

        public async Task<string?> GetContentByIdAsync(Guid fileId)
        {
            return await _dbContext.PdfDocuments
                .AsNoTracking()
                .Where(d => d.Id == fileId)
                .Select(d => d.Content)
                .FirstOrDefaultAsync();
        }

        public async Task<List<PdfDocument>> GetPdfDocumentsAsync(int pageNumber, int pageSize)
        {
            return await _dbContext.PdfDocuments
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(d => d.Id)
                .ToListAsync();
        }
    }
}
