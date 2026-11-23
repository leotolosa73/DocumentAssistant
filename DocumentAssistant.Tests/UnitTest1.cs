
using DocumentAssistant.Api.Data;
using DocumentAssistant.Api.DTOs.Documents;
using DocumentAssistant.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using DocumentAssistant.Api.Models;
using DocumentAssistant.Api.Enums;
using Xunit;

namespace DocumentAssistant.Tests;

public class DocumentServiceTests
{
    [Fact]
    public async Task CreateDocumentAsync_ShouldSaveDocumentWithPendingStatus()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var service = new DocumentService(context);

        var request = new CreateDocumentRequest
        {
            Name = "Contrato de prueba"
        };

        // Act
        var result = await service.CreateDocumentAsync(request);

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal("Contrato de prueba", result.Name);
        Assert.Equal(DocumentStatus.Pending, result.Status);
        var savedDocument = await context.Documents
            .FirstOrDefaultAsync(document => document.Id == result.Id);

        Assert.NotNull(savedDocument);
        Assert.Equal("Contrato de prueba", savedDocument.Name);
    }


[Fact]
public async Task GetDocumentByIdAsync_ShouldReturnNull_WhenDocumentDoesNotExist()
{
    // Arrange
    await using var connection = new SqliteConnection("Data Source=:memory:");
    await connection.OpenAsync();

    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlite(connection)
        .Options;

    await using var context = new ApplicationDbContext(options);
    await context.Database.EnsureCreatedAsync();

    var service = new DocumentService(context);

    // Act
    var result = await service.GetDocumentByIdAsync(99999);

    // Assert
    Assert.Null(result);
}


[Fact]
public async Task GetDocumentByIdAsync_ShouldReturnDocument_WhenDocumentExists()
{
    // Arrange
    await using var connection = new SqliteConnection("Data Source=:memory:");
    await connection.OpenAsync();

    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlite(connection)
        .Options;

    await using var context = new ApplicationDbContext(options);
    await context.Database.EnsureCreatedAsync();

    var document = new Document
    {
        Name = "Contrato existente",
        Status = DocumentStatus.Pending
    };

    context.Documents.Add(document);
    await context.SaveChangesAsync();

    var service = new DocumentService(context);

    // Act
    var result = await service.GetDocumentByIdAsync(document.Id);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(document.Id, result.Id);
    Assert.Equal("Contrato existente", result.Name);
    Assert.Equal(DocumentStatus.Pending, result.Status);
    }


}
