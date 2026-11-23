using DocumentAssistant.Api.Enums;
namespace DocumentAssistant.Api.DTOs.Documents;
public class DocumentResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
}