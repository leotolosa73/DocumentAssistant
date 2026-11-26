
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
    public async Task<ActionResult<PagedResponse<DocumentResponse>>> GetDocuments(
        [FromQuery] GetDocumentsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _documentService.GetDocumentsAsync(
            request,
            cancellationToken);

        var response = new PagedResponse<DocumentResponse>(
            result.Items
                .Select(document => document.ToResponse())
                .ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);

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

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DocumentResponse>> UpdateDocument(
        int id,
        [FromBody] UpdateDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var document = await _documentService.UpdateDocumentAsync(
            id,
            request,
            cancellationToken);

        if (document is null)
            return NotFound();

        return Ok(document.ToResponse());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDocument(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _documentService.DeleteDocumentAsync(
            id,
            cancellationToken);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
