using Microsoft.AspNetCore.Mvc;
using PdfProcessor.API.DTOs;
using PdfProcessor.API.Services;

namespace PdfProcessor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
        => _documentService = documentService;

    [HttpGet("{id}/content")]
    public async Task<ActionResult<DocumentContentResponse>> GetDocumentContent(Guid id)
    {
        var result = await _documentService.GetPdfDocumentContentAsync(id);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<DocumentListItemResponse>>> GetDocuments(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _documentService.GetPdfDocumentsAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UploadPdfResult>> Upload([FromForm] IFormFile file)
    {
        var result = await _documentService.UploadPdfAsync(file);
        return CreatedAtAction(nameof(GetDocumentContent), new { id = result.Id }, result);
    }
}