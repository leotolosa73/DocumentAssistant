
using DocumentAssistant.Api.Data;
using DocumentAssistant.Api.DTOs.Documents;
using DocumentAssistant.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using DocumentAssistant.Api.Models;
using DocumentAssistant.Api.Enums;
using DocumentAssistant.Api.Exceptions;
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


    [Fact]
    public async Task GetDocumentsAsync_ShouldReturnFirstPage()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        context.Documents.AddRange(
            new Document { Name = "Documento A", Status = DocumentStatus.Pending },
            new Document { Name = "Documento B", Status = DocumentStatus.Pending },
            new Document { Name = "Documento C", Status = DocumentStatus.Pending },
            new Document { Name = "Documento D", Status = DocumentStatus.Pending },
            new Document { Name = "Documento E", Status = DocumentStatus.Pending }
        );

        await context.SaveChangesAsync();

        var service = new DocumentService(context);

        var request = new GetDocumentsRequest
        {
            Page = 1,
            PageSize = 2
        };

        // Act
        var result = await service.GetDocumentsAsync(request);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);

        Assert.Equal("Documento A", result.Items[0].Name);
        Assert.Equal("Documento B", result.Items[1].Name);
    }


    [Fact]
    public async Task GetDocumentsAsync_ShouldReturnSecondPage()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        context.Documents.AddRange(
            new Document { Name = "Documento A", Status = DocumentStatus.Pending },
            new Document { Name = "Documento B", Status = DocumentStatus.Pending },
            new Document { Name = "Documento C", Status = DocumentStatus.Pending },
            new Document { Name = "Documento D", Status = DocumentStatus.Pending },
            new Document { Name = "Documento E", Status = DocumentStatus.Pending }
        );

        await context.SaveChangesAsync();

        var service = new DocumentService(context);

        var request = new GetDocumentsRequest
        {
            Page = 2,
            PageSize = 2
        };

        // Act
        var result = await service.GetDocumentsAsync(request);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);

        Assert.Equal("Documento C", result.Items[0].Name);
        Assert.Equal("Documento D", result.Items[1].Name);
    }


    [Fact]
    public async Task GetDocumentsAsync_ShouldReturnEmptyItems_WhenPageIsOutOfRange()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        context.Documents.AddRange(
            new Document { Name = "Documento A", Status = DocumentStatus.Pending },
            new Document { Name = "Documento B", Status = DocumentStatus.Pending },
            new Document { Name = "Documento C", Status = DocumentStatus.Pending }
        );

        await context.SaveChangesAsync();

        var service = new DocumentService(context);

        var request = new GetDocumentsRequest
        {
            Page = 10,
            PageSize = 2
        };

        // Act
        var result = await service.GetDocumentsAsync(request);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(10, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
    }


    [Fact]
    public async Task GetDocumentsAsync_ShouldFilterByName()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        context.Documents.AddRange(
            new Document { Name = "Contrato de alquiler", Status = DocumentStatus.Pending },
            new Document { Name = "Factura de electricidad", Status = DocumentStatus.Pending },
            new Document { Name = "Contrato laboral", Status = DocumentStatus.Completed }
        );

        await context.SaveChangesAsync();

        var service = new DocumentService(context);

        var request = new GetDocumentsRequest
        {
            Search = "Contrato"
        };

        // Act
        var result = await service.GetDocumentsAsync(request);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);
        Assert.Contains(result.Items, document => document.Name == "Contrato de alquiler");
        Assert.Contains(result.Items, document => document.Name == "Contrato laboral");
    }


    [Fact]
    public async Task GetDocumentsAsync_ShouldFilterByStatus()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        context.Documents.AddRange(
            new Document { Name = "Documento A", Status = DocumentStatus.Pending },
            new Document { Name = "Documento B", Status = DocumentStatus.Completed },
            new Document { Name = "Documento C", Status = DocumentStatus.Pending },
            new Document { Name = "Documento D", Status = DocumentStatus.Failed }
        );

        await context.SaveChangesAsync();

        var service = new DocumentService(context);

        var request = new GetDocumentsRequest
        {
            Status = DocumentStatus.Pending
        };

        // Act
        var result = await service.GetDocumentsAsync(request);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);

        Assert.All(
            result.Items,
            document => Assert.Equal(DocumentStatus.Pending, document.Status));
    }


    [Fact]
    public async Task GetDocumentsAsync_ShouldSortByNameAscending()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        context.Documents.AddRange(
            new Document { Name = "Zebra", Status = DocumentStatus.Pending },
            new Document { Name = "Alfa", Status = DocumentStatus.Pending },
            new Document { Name = "Motor", Status = DocumentStatus.Pending }
        );

        await context.SaveChangesAsync();

        var service = new DocumentService(context);

        var request = new GetDocumentsRequest
        {
            SortBy = "name",
            SortDirection = "asc"
        };

        // Act
        var result = await service.GetDocumentsAsync(request);

        // Assert
        Assert.Equal(3, result.Items.Count);

        Assert.Equal("Alfa", result.Items[0].Name);
        Assert.Equal("Motor", result.Items[1].Name);
        Assert.Equal("Zebra", result.Items[2].Name);
    }


    [Fact]
    public async Task GetDocumentsAsync_ShouldSortByNameDescending()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        context.Documents.AddRange(
            new Document { Name = "Zebra", Status = DocumentStatus.Pending },
            new Document { Name = "Alfa", Status = DocumentStatus.Pending },
            new Document { Name = "Motor", Status = DocumentStatus.Pending }
        );

        await context.SaveChangesAsync();

        var service = new DocumentService(context);

        var request = new GetDocumentsRequest
        {
            SortBy = "name",
            SortDirection = "desc"
        };

        // Act
        var result = await service.GetDocumentsAsync(request);

        // Assert
        Assert.Equal(3, result.Items.Count);

        Assert.Equal("Zebra", result.Items[0].Name);
        Assert.Equal("Motor", result.Items[1].Name);
        Assert.Equal("Alfa", result.Items[2].Name);
    }


    [Fact]
    public async Task GetDocumentsAsync_ShouldApplyFiltersSortingAndPaginationTogether()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        context.Documents.AddRange(
            new Document { Name = "Contrato Delta", Status = DocumentStatus.Pending },
            new Document { Name = "Contrato Alfa", Status = DocumentStatus.Pending },
            new Document { Name = "Contrato Charlie", Status = DocumentStatus.Pending },
            new Document { Name = "Contrato Bravo", Status = DocumentStatus.Pending },
            new Document { Name = "Contrato Echo", Status = DocumentStatus.Completed },
            new Document { Name = "Factura Alfa", Status = DocumentStatus.Pending }
        );

        await context.SaveChangesAsync();

        var service = new DocumentService(context);

        var request = new GetDocumentsRequest
        {
            Search = "Contrato",
            Status = DocumentStatus.Pending,
            SortBy = "name",
            SortDirection = "asc",
            Page = 2,
            PageSize = 2
        };

        // Act
        var result = await service.GetDocumentsAsync(request);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(4, result.TotalCount);
        Assert.Equal(2, result.TotalPages);

        Assert.Equal("Contrato Charlie", result.Items[0].Name);
        Assert.Equal("Contrato Delta", result.Items[1].Name);
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldUseIdAsSecondarySortCriteria()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        context.Documents.AddRange(
            new Document { Name = "Documento A", Status = DocumentStatus.Pending },
            new Document { Name = "Documento B", Status = DocumentStatus.Pending },
            new Document { Name = "Documento C", Status = DocumentStatus.Pending }
        );

        await context.SaveChangesAsync();

        var expectedIds = await context.Documents
            .OrderBy(document => document.Id)
            .Select(document => document.Id)
            .ToListAsync();

        var service = new DocumentService(context);

        var request = new GetDocumentsRequest
        {
            SortBy = "status",
            SortDirection = "asc"
        };

        // Act
        var result = await service.GetDocumentsAsync(request);

        // Assert
        Assert.Equal(3, result.Items.Count);

        Assert.Equal(
            expectedIds,
            result.Items.Select(document => document.Id).ToList());
    }

    [Fact]
    public async Task UpdateDocumentAsync_ShouldUpdateName_AndPreserveStatus()
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
            Name = "Contrato original",
            Status = DocumentStatus.Completed
        };

        context.Documents.Add(document);
        await context.SaveChangesAsync();

        var service = new DocumentService(context);

        var request = new UpdateDocumentRequest
        {
            Name = "Contrato actualizado"
        };

        // Act
        var result = await service.UpdateDocumentAsync(document.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Contrato actualizado", result.Name);
        Assert.Equal(DocumentStatus.Completed, result.Status);
        Assert.Equal(document.Id, result.Id);
    }

    [Fact]
    public async Task UpdateDocumentAsync_ShouldReturnNull_WhenDocumentDoesNotExist()
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

        var request = new UpdateDocumentRequest
        {
            Name = "Contrato actualizado"
        };

        // Act
        var result = await service.UpdateDocumentAsync(99999, request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateDocumentAsync_ShouldThrowException_WhenNameAlreadyExists()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var firstDocument = new Document
        {
            Name = "Contrato A",
            Status = DocumentStatus.Pending
        };

        var secondDocument = new Document
        {
            Name = "Contrato B",
            Status = DocumentStatus.Completed
        };

        context.Documents.AddRange(firstDocument, secondDocument);
        await context.SaveChangesAsync();

        var service = new DocumentService(context);

        var request = new UpdateDocumentRequest
        {
            Name = "Contrato A"
        };

        // Act & Assert
        await Assert.ThrowsAsync<DocumentAlreadyExistsException>(
            () => service.UpdateDocumentAsync(secondDocument.Id, request));
    }

    [Fact]
    public async Task UpdateDocumentAsync_ShouldAllowKeepingSameName()
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
            Name = "Contrato original",
            Status = DocumentStatus.Pending
        };

        context.Documents.Add(document);
        await context.SaveChangesAsync();

        var service = new DocumentService(context);

        var request = new UpdateDocumentRequest
        {
            Name = "Contrato original"
        };

        // Act
        var result = await service.UpdateDocumentAsync(document.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Contrato original", result.Name);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldDeleteDocument_WhenDocumentExists()
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
            Name = "Contrato para eliminar",
            Status = DocumentStatus.Pending
        };

        context.Documents.Add(document);
        await context.SaveChangesAsync();

        var service = new DocumentService(context);

        // Act
        var result = await service.DeleteDocumentAsync(document.Id);

        // Assert
        Assert.True(result);

        var deletedDocument = await context.Documents
            .FindAsync(document.Id);

        Assert.Null(deletedDocument);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldReturnFalse_WhenDocumentDoesNotExist()
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
        var result = await service.DeleteDocumentAsync(99999);

        // Assert
        Assert.False(result);
    }
    
}
