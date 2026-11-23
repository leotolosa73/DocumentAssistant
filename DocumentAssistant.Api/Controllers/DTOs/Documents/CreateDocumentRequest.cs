
using System.ComponentModel.DataAnnotations;

namespace DocumentAssistant.Api.DTOs.Documents;

public class CreateDocumentRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;
}
