
using DocumentAssistant.Tests.Infrastructure;
using DocumentAssistant.Api.DTOs.Documents;
using DocumentAssistant.Api.Enums;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace DocumentAssistant.Tests.IntegrationTests;

public class DocumentsApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DocumentsApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDocuments_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/documents");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateDocument_ShouldReturnCreated()
    {
        // Arrange
        var request = new
        {
            Name = "Contrato de integración"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/documents",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

        var createdDocument =
            await response.Content.ReadFromJsonAsync<DocumentResponse>(
                jsonOptions);

        Assert.NotNull(createdDocument);
        Assert.NotEqual(0, createdDocument.Id);
        Assert.Equal("Contrato de integración", createdDocument.Name);
        Assert.Equal(DocumentStatus.Pending, createdDocument.Status);

        var getResponse = await _client.GetAsync(
            $"/api/documents/{createdDocument.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateDocument_ShouldReturnBadRequest_WhenNameIsTooShort()
    {
        // Arrange
        var request = new
        {
            Name = "AB"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/documents",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDocument_ShouldReturnConflict_WhenNameAlreadyExistsWithDifferentCasing()
    {
        // Arrange
        var firstRequest = new
        {
            Name = "Contrato duplicado"
        };

        var secondRequest = new
        {
            Name = "CONTRATO DUPLICADO"
        };

        // Act
        var firstResponse = await _client.PostAsJsonAsync(
            "/api/documents",
            firstRequest);

        var secondResponse = await _client.PostAsJsonAsync(
            "/api/documents",
            secondRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task GetDocuments_ShouldReturnBadRequest_WhenPageIsZero()
    {
        // Act
        var response = await _client.GetAsync(
            "/api/documents?page=0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDocuments_ShouldReturnBadRequest_WhenPageSizeExceedsMaximum()
    {
        // Act
        var response = await _client.GetAsync(
            "/api/documents?pageSize=500");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDocuments_ShouldReturnBadRequest_WhenSortByIsInvalid()
    {
        // Act
        var response = await _client.GetAsync(
            "/api/documents?sortBy=invalid");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDocuments_ShouldReturnBadRequest_WhenSortDirectionIsInvalid()
    {
        // Act
        var response = await _client.GetAsync(
            "/api/documents?sortDirection=invalid");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDocuments_ShouldReturnOk_WhenSortByIsUppercase()
    {
        var response = await _client.GetAsync("/api/documents?sortBy=NAME");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDocuments_ShouldReturnOk_WhenSortDirectionIsUppercase()
    {
        var response = await _client.GetAsync("/api/documents?sortDirection=DESC");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDocument_ShouldReturnOk_WhenDocumentExists()
    {
        // Arrange
        var createRequest = new
        {
            Name = "Contrato para actualizar"
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/documents",
            createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        var createdDocument = await createResponse.Content
            .ReadFromJsonAsync<DocumentResponse>(jsonOptions);

        Assert.NotNull(createdDocument);

        var updateRequest = new
        {
            Name = "Contrato actualizado"
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/documents/{createdDocument.Id}",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedDocument = await response.Content
            .ReadFromJsonAsync<DocumentResponse>(jsonOptions);

        Assert.NotNull(updatedDocument);
        Assert.Equal(createdDocument.Id, updatedDocument.Id);
        Assert.Equal("Contrato actualizado", updatedDocument.Name);
        Assert.Equal(DocumentStatus.Pending, updatedDocument.Status);
    }

    [Fact]
    public async Task UpdateDocument_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        var updateRequest = new
        {
            Name = "Contrato inexistente"
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            "/api/documents/99999",
            updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDocument_ShouldReturnConflict_WhenNameAlreadyExists()
    {
        // Arrange
        var firstResponse = await _client.PostAsJsonAsync(
            "/api/documents",
            new { Name = "Contrato original A" });

        var secondResponse = await _client.PostAsJsonAsync(
            "/api/documents",
            new { Name = "Contrato original B" });

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);

        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        var firstDocument = await firstResponse.Content
            .ReadFromJsonAsync<DocumentResponse>(jsonOptions);

        var secondDocument = await secondResponse.Content
            .ReadFromJsonAsync<DocumentResponse>(jsonOptions);

        Assert.NotNull(firstDocument);
        Assert.NotNull(secondDocument);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/documents/{secondDocument.Id}",
            new { Name = firstDocument.Name });

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDocument_ShouldReturnBadRequest_WhenNameIsTooShort()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(
            "/api/documents",
            new { Name = "Contrato válido" });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        var document = await createResponse.Content
            .ReadFromJsonAsync<DocumentResponse>(jsonOptions);

        Assert.NotNull(document);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/documents/{document.Id}",
            new { Name = "AB" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteDocument_ShouldReturnNoContent_WhenDocumentExists()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(
            "/api/documents",
            new { Name = "Contrato para eliminar mediante API" });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        var document = await createResponse.Content
            .ReadFromJsonAsync<DocumentResponse>(jsonOptions);

        Assert.NotNull(document);

        // Act
        var deleteResponse = await _client.DeleteAsync(
            $"/api/documents/{document.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify that the document no longer exists
        var getResponse = await _client.GetAsync(
            $"/api/documents/{document.Id}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteDocument_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        const int nonExistingId = 99999;

        // Act
        var response = await _client.DeleteAsync(
            $"/api/documents/{nonExistingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
