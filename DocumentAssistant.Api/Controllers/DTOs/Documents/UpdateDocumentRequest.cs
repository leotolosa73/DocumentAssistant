using System.ComponentModel.DataAnnotations;

namespace DocumentAssistant.Api.DTOs.Documents;

public record UpdateDocumentRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Name { get; init; } = string.Empty;
}