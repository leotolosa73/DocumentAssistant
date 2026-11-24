using DocumentAssistant.Api.Data;
using DocumentAssistant.Api.Models;
using DocumentAssistant.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using DocumentAssistant.Api.DTOs.Documents;
using DocumentAssistant.Api.Enums;
using DocumentAssistant.Api.Exceptions;
using Microsoft.Data.SqlClient;

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
        var documentExists = await _context.Documents
            .AnyAsync(
                document => document.Name == request.Name,
                cancellationToken);

        if (documentExists)
        {
            throw new DocumentAlreadyExistsException(request.Name);
        }

        var document = new Document
        {
            Name = request.Name,
            Status = DocumentStatus.Pending
        };

        _context.Documents.Add(document);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        when (IsUniqueConstraintViolation(ex))
        {
            throw new DocumentAlreadyExistsException(request.Name);
        }

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

    internal static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException &&
            (sqlException.Number == 2601 ||
                sqlException.Number == 2627);
    }
}
