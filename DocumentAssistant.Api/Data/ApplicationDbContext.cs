
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (Database.IsSqlServer())
        {
            modelBuilder.Entity<Document>()
                .Property(document => document.Name)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");
        }
        else if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            modelBuilder.Entity<Document>()
                .Property(document => document.Name)
                .UseCollation("NOCASE");
        }

        // Garantiza que no existan documentos con nombres duplicados.
        modelBuilder.Entity<Document>()
            .HasIndex(document => document.Name)
            .IsUnique();
    }
}
