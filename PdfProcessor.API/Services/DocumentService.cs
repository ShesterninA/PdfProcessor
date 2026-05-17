using Microsoft.Extensions.Options;
using PdfProcessor.API.DTOs;
using PdfProcessor.API.Exceptions;
using PdfProcessor.API.Settings;
using PdfProcessor.Data;
using PdfProcessor.Data.Entities;
using PdfProcessor.Data.Repositories;
using PdfProcessor.Shared.Constants;
using PdfProcessor.Shared.Enums;
using System.Text.Json;

namespace PdfProcessor.API.Services;

public class DocumentService : IDocumentService
{
    private readonly IOptions<StorageSettings> _storageSettings;
    private readonly IPdfDocumentRepository _pdfDocumentRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly PdfProcessorDbContext _context;

    public DocumentService(
        IOptions<StorageSettings> storageSettings,
        IPdfDocumentRepository pdfDocumentRepository,
        IOutboxRepository outboxRepository,
        PdfProcessorDbContext context)
    {
        _storageSettings = storageSettings;
        _pdfDocumentRepository = pdfDocumentRepository;
        _outboxRepository = outboxRepository;
        _context = context;
    }

    public async Task<UploadPdfResult> UploadPdfAsync(IFormFile file)
    {
        ValidateFile(file);

        var documentId = Guid.NewGuid();
        var uploadFilePath = Path.Combine(
            _storageSettings.Value.UploadPdfPath,
            $"{documentId}.pdf");

        using (var fileStream = new FileStream(uploadFilePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        try
        {
            var pdfDocument = new PdfDocument(documentId, file.FileName);
            var outbox = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = OutboxMessageTypes.DocumentUploaded,
                Payload = JsonSerializer.Serialize(new { pdfDocument.Id }),
                CreatedAt = DateTime.UtcNow
            };

            _pdfDocumentRepository.Add(pdfDocument);
            _outboxRepository.Add(outbox);
            await _context.SaveChangesAsync();
        }
        catch
        {
            if (File.Exists(uploadFilePath))
                File.Delete(uploadFilePath);
            throw;
        }

        return new UploadPdfResult
        {
            Id = documentId,
            Status = DocumentStatus.Pending
        };
    }

    public async Task<DocumentContentResponse> GetPdfDocumentContentAsync(Guid documentId)
    {
        var document = await _pdfDocumentRepository.GetByIdAsync(documentId);

        if (document is null)
        {
            throw new NotFoundException($"Pdf document id={documentId} not found.");
        }

        if (string.IsNullOrWhiteSpace(document.Content))
        {
            throw new ValidationException("Document content is not ready yet.");
        }

        return new DocumentContentResponse
        {
            Content = document.Content
        };
    }

    public async Task<List<DocumentListItemResponse>> GetPdfDocumentsAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ValidationException("pageNumber must be >= 1");

        if (pageSize <= 0 || pageSize > 100)
            throw new ValidationException("pageSize must be between 1 and 100");

        var documents = await _pdfDocumentRepository.GetPdfDocumentsAsync(pageNumber, pageSize);

        return documents.Select(d => new DocumentListItemResponse
        {
            Id = d.Id,
            FileName = d.FileName,
            Status = d.Status
        }).ToList();
    }

    private void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ValidationException("File is empty");

        if (!string.Equals(Path.GetExtension(file.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
            throw new ValidationException("File is not a valid PDF");

        var maxSize = _storageSettings.Value.MaxFileSizeMb * 1024 * 1024;
        if (file.Length > maxSize)
            throw new ValidationException($"File exceeds maximum size of {_storageSettings.Value.MaxFileSizeMb}MB");
    }
}