using System.Text.Json;
using Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cms.Infrastructure.Persistence;

public class CmsDbContext(DbContextOptions<CmsDbContext> options) : DbContext(options)
{
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContentItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(120).IsRequired();
            e.Property(x => x.Summary).HasMaxLength(600);
            e.Property(x => x.SeoTitle).HasMaxLength(120);
            e.Property(x => x.SeoDescription).HasMaxLength(320);
            e.Property(x => x.Kind).HasConversion<string>().HasMaxLength(16);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
            ConfigureTags(e.Property(x => x.Tags));
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasIndex(x => new { x.Status, x.PublishedAt });
        });
    }

    // Tags are stored as a JSON array in a text column so the same model works on SQLite and PostgreSQL.
    private static void ConfigureTags(PropertyBuilder<List<string>> property)
    {
        property.HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>(),
            new ValueComparer<List<string>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                v => v.ToList()));
        property.HasColumnType("text");
    }
}
