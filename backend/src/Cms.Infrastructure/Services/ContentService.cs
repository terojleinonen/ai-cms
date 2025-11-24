using Cms.Application.Interfaces;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cms.Infrastructure.Services;

public class ContentService : IContentService
{
    private readonly CmsDbContext _db;

    public ContentService(CmsDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ContentItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.ContentItems.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<ContentItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.ContentItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<ContentItem> CreateAsync(ContentItem item, CancellationToken cancellationToken = default)
    {
        _db.ContentItems.Add(item);
        await _db.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<ContentItem> UpdateAsync(ContentItem item, CancellationToken cancellationToken = default)
    {
        _db.ContentItems.Update(item);
        await _db.SaveChangesAsync(cancellationToken);
        return item;
    }
}
