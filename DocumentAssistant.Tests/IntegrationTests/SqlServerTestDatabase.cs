
using DocumentAssistant.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using DocumentAssistant.Tests.Infrastructure;

namespace DocumentAssistant.Tests.Infrastructure;

public class SqlServerTestDatabase
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public SqlServerTestDatabase(
        DbCommandInterceptor? interceptor = null)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(
                    "Server=localhost;Database=DocumentAssistantTestDb;Integrated Security=True;TrustServerCertificate=True;");

        if (interceptor is not null)
        {
            optionsBuilder.AddInterceptors(interceptor);
        }

        _options = optionsBuilder.Options;
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
