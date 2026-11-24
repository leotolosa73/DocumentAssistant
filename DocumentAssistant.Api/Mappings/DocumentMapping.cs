using DocumentAssistant.Api.DTOs.Documents;
using DocumentAssistant.Api.Models;

namespace DocumentAssistant.Api.Mappings;

public static class DocumentMapping
{
    public static DocumentResponse ToResponse(this Document document)
    {
        return new DocumentResponse(
            document.Id,
            document.Name,
            document.Status);
    }
}