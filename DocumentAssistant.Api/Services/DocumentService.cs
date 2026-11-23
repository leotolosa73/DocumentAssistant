using DocumentAssistant.Api.Data;
using DocumentAssistant.Api.Models;
using DocumentAssistant.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using DocumentAssistant.Api.DTOs.Documents;
using DocumentAssistant.Api.Enums;

namespace DocumentAssistant.Api.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;

    public DocumentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Document>> GetDocumentsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Document> CreateDocumentAsync(
        CreateDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        var document = new Document
        {
            Name = request.Name,
            Status = DocumentStatus.Pending
        };

        _context.Documents.Add(document);

        await _context.SaveChangesAsync(cancellationToken);

        return document;
    }


public async Task<Document?> GetDocumentByIdAsync(
    int id,
    CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(
                document => document.Id == id,
                cancellationToken);
    }

}