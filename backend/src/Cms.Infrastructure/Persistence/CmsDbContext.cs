using Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cms.Infrastructure.Persistence;

public class CmsDbContext : DbContext
{
    public CmsDbContext(DbContextOptions<CmsDbContext> options) : base(options) { }

    public DbSet<ContentType> ContentTypes => Set<ContentType>();
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
}
