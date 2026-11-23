
using DocumentAssistant.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentAssistant.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Document> Documents => Set<Document>();
}
