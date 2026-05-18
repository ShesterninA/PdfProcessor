using Microsoft.EntityFrameworkCore;
using PdfProcessor.Data.Entities;

namespace PdfProcessor.Data;

public class PdfProcessorDbContext : DbContext
{
    public DbSet<PdfDocument> PdfDocuments => Set<PdfDocument>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public PdfProcessorDbContext(
        DbContextOptions<PdfProcessorDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PdfDocument>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(x => x.Status)
                .IsRequired();
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Payload)
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.ProcessedAt,
                x.CreatedAt
            });
        });
    }
}