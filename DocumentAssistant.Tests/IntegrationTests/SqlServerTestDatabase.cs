
using DocumentAssistant.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace DocumentAssistant.Tests.Infrastructure;

public class SqlServerTestDatabase
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public SqlServerTestDatabase()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=DocumentAssistantTestDb;Integrated Security=True;TrustServerCertificate=True;")
            .Options;
    }

    public async Task InitializeAsync()
    {
        await using var context = new ApplicationDbContext(_options);
        await context.Database.MigrateAsync();
    }

    public ApplicationDbContext CreateContext()
    {
        return new ApplicationDbContext(_options);
    }
}
