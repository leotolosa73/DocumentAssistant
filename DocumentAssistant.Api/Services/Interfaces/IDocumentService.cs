
using DocumentAssistant.Api.Models;
using DocumentAssistant.Api.DTOs.Documents;

namespace DocumentAssistant.Api.Services.Interfaces;

public interface IDocumentService
{
    Task<PagedResponse<Document>> GetDocumentsAsync(
        GetDocumentsRequest request,
        CancellationToken cancellationToken = default);

    Task<Document> CreateDocumentAsync(
        CreateDocumentRequest request,
        CancellationToken cancellationToken = default);

    Task<Document?> GetDocumentByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Document?> UpdateDocumentAsync(
        int id,
        UpdateDocumentRequest request,
        CancellationToken cancellationToken = default);
}
