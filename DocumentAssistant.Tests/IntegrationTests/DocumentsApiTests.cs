
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

    public DocumentsApiTests(
        CustomWebApplicationFactory factory)
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

        var createdDocument = await response.Content.ReadFromJsonAsync<DocumentResponse>(
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

}
