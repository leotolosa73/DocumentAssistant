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
    
    public async Task<PagedResponse<Document>> GetDocumentsAsync(
        GetDocumentsRequest request,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Document> query = _context.Documents.AsNoTracking();

        // 1. Filter by name
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(document =>
                document.Name.Contains(search));
        }

        // 2. Filter by status
        if (request.Status.HasValue)
        {
            query = query.Where(document =>
                document.Status == request.Status.Value);
        }

        // 3. Get total count after filtering
        var totalCount = await query.CountAsync(cancellationToken);

        // 4. Apply sorting
        query = (request.SortBy.ToLowerInvariant(),
                request.SortDirection.ToLowerInvariant()) switch
        {
            ("name", "asc") => query
                .OrderBy(document => document.Name)
                .ThenBy(document => document.Id),

            ("name", "desc") => query
                .OrderByDescending(document => document.Name)
                .ThenBy(document => document.Id),

            ("status", "asc") => query
                .OrderBy(document => document.Status)
                .ThenBy(document => document.Id),

            ("status", "desc") => query
                .OrderByDescending(document => document.Status)
                .ThenBy(document => document.Id),

            ("id", "desc") => query.OrderByDescending(document => document.Id),

            _ => query.OrderBy(document => document.Id)
        };


        // 5. Apply pagination
        var documents = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<Document>(
            documents,
            request.Page,
            request.PageSize,
            totalCount);
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

    public async Task<Document?> UpdateDocumentAsync(
    int id,
    UpdateDocumentRequest request,
    CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents
            .FirstOrDefaultAsync(
                document => document.Id == id,
                cancellationToken);

        if (document is null)
            return null;

        var documentExists = await _context.Documents
            .AnyAsync(
                existingDocument =>
                    existingDocument.Id != id &&
                    existingDocument.Name == request.Name,
                cancellationToken);

        if (documentExists)
            throw new DocumentAlreadyExistsException(request.Name);

        document.Name = request.Name;

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
}
