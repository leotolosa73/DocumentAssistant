using DocumentAssistant.Api.Enums;

namespace DocumentAssistant.Api.Models;

public class Document
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DocumentStatus Status { get; set; }
}