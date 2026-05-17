using Microsoft.EntityFrameworkCore;
using PdfProcessor.Data.Entities;

namespace PdfProcessor.Data
{
    public class PdfProcessorDbContext : DbContext
    {
        public DbSet<PdfDocument> PdfDocuments { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        public PdfProcessorDbContext(DbContextOptions<PdfProcessorDbContext> options) : base(options)
        {
        }
    }
}
