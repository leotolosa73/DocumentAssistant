
using DocumentAssistant.Api.DTOs.Documents;
using DocumentAssistant.Api.Models;
using DocumentAssistant.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using DocumentAssistant.Api.Mappings;

namespace DocumentAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DocumentResponse>>> GetDocuments(
        CancellationToken cancellationToken)
    {
        var documents = await _documentService.GetDocumentsAsync(
            cancellationToken);

        var response = documents
            .Select(document => document.ToResponse())
            .ToList();

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<DocumentResponse>> CreateDocument(
        [FromBody] CreateDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var document = await _documentService.CreateDocumentAsync(
            request,
            cancellationToken);

        var response = document.ToResponse();

        return CreatedAtAction(
            nameof(GetDocumentById),
            new { id = response.Id },
            response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DocumentResponse>> GetDocumentById(
        int id,
        CancellationToken cancellationToken)
    {
        var document = await _documentService.GetDocumentByIdAsync(
            id,
            cancellationToken);

        if (document is null)
        {
            return NotFound();
        }

        return Ok(document.ToResponse());
    }
}
