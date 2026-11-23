
namespace DocumentAssistant.Api.Exceptions;

public class DocumentAlreadyExistsException : Exception
{
    public DocumentAlreadyExistsException(string documentName)
        : base($"A document with the name '{documentName}' already exists.")
    {
    }
}
