
using DocumentAssistant.Api.Models;
using DocumentAssistant.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using DocumentAssistant.Api.Services;
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
}
