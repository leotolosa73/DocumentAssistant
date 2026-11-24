using DocumentAssistant.Api.Enums;

namespace DocumentAssistant.Api.DTOs.Documents;

public record DocumentResponse(
    int Id,
    string Name,
    DocumentStatus Status);