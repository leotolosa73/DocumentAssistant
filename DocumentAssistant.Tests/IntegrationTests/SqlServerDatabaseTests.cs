
using DocumentAssistant.Api.DTOs.Documents;
using DocumentAssistant.Api.Exceptions;
using DocumentAssistant.Api.Services;
using Microsoft.EntityFrameworkCore;
using DocumentAssistant.Api.Models;
using DocumentAssistant.Tests.Infrastructure;
using Microsoft.Data.SqlClient;

namespace DocumentAssistant.Tests.IntegrationTests;

public class SqlServerDatabaseTests
{
    [Fact]
    public async Task InitializeAsync_ShouldCreateDatabaseSchema()
    {
        // Arrange
        var database = new SqlServerTestDatabase();

        // Act
        await database.InitializeAsync();

        // Assert
        await using var context = database.CreateContext();

        var canConnect = await context.Database.CanConnectAsync();

        Assert.True(canConnect);
    }

    [Fact]
    public async Task CreateDocument_ShouldRejectDuplicateName_WhenUsingSqlServer()
    {
        // Arrange
        var database = new SqlServerTestDatabase();
        await database.InitializeAsync();

        var documentName = $"Documento-{Guid.NewGuid()}";

        // Act
        await using (var context = database.CreateContext())
        {
            context.Documents.Add(new Document
            {
                Name = documentName,
                Status = DocumentAssistant.Api.Enums.DocumentStatus.Pending
            });

            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = database.CreateContext())
        {
            context.Documents.Add(new Document
            {
                Name = documentName,
                Status = DocumentAssistant.Api.Enums.DocumentStatus.Pending
            });

            var exception = await Assert.ThrowsAsync<DbUpdateException>(
                () => context.SaveChangesAsync());

            var sqlException = Assert.IsType<SqlException>(
                exception.InnerException);

            Assert.True(
                sqlException.Number == 2601 || sqlException.Number == 2627);

            Assert.True(
                DocumentService.IsUniqueConstraintViolation(exception));
        }
    }


    
    [Fact]
    public async Task CreateDocument_ShouldAllowOnlyOneInsert_WhenRequestsAreConcurrent()
    {
        var interceptor = new ConcurrencyCommandInterceptor();
        var database = new SqlServerTestDatabase(interceptor);

        await database.InitializeAsync();

        var documentName = $"Concurrent-{Guid.NewGuid()}";

        async Task<bool> TryCreateDocumentAsync()
        {
            await using var context = database.CreateContext();
            var service = new DocumentService(context);

            try
            {
                await service.CreateDocumentAsync(
                    new CreateDocumentRequest { Name = documentName });

                return true;
            }
            catch (DocumentAlreadyExistsException)
            {
                return false;
            }
        }

        var firstTask = TryCreateDocumentAsync();
        var secondTask = TryCreateDocumentAsync();

        await interceptor.WaitForFirstQueryAsync();

        var results = await Task.WhenAll(firstTask, secondTask);

        Assert.Single(results, created => created);
        Assert.Equal(1, results.Count(created => !created));

        await using var verificationContext = database.CreateContext();

        var documentCount = await verificationContext.Documents
            .CountAsync(document => document.Name == documentName);

        Assert.Equal(1, documentCount);
    }


}
