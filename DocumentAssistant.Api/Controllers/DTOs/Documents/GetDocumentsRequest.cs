
using System.ComponentModel.DataAnnotations;
using DocumentAssistant.Api.Enums;

namespace DocumentAssistant.Api.DTOs.Documents;

public record GetDocumentsRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

    [StringLength(200)]
    public string? Search { get; init; }

    public DocumentStatus? Status { get; init; }

    [RegularExpression("^(?i:id|name|status)$",
    ErrorMessage = "SortBy must be id, name or status.")]
    public string SortBy { get; init; } = "id";

    [RegularExpression("^(?i:asc|desc)$",
        ErrorMessage = "SortDirection must be asc or desc.")]
    public string SortDirection { get; init; } = "asc";
}
