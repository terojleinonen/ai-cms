using Cms.Application.Common;
using Cms.Application.Content;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cms.Infrastructure.Services;

public class ContentService(CmsDbContext db) : IContentService
{
    public async Task<PagedResult<ContentListItemDto>> ListAsync(ContentQuery query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var q = db.ContentItems.AsNoTracking().AsQueryable();
        if (query.Status is { } status) q = q.Where(x => x.Status == status);
        if (query.Kind is { } kind) q = q.Where(x => x.Kind == kind);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(x => x.Title.ToLower().Contains(term) || x.Slug.Contains(term));
        }

        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.UpdatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<ContentListItemDto>(rows.Select(ContentListItemDto.From).ToList(), page, pageSize, total);
    }

    public async Task<ContentItemDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        => ContentItemDto.From(await FindAsync(id, ct, track: false));

    public async Task<ContentItemDto> GetPublishedBySlugAsync(string slug, CancellationToken ct = default)
    {
        var item = await db.ContentItems.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == slug && x.Status == ContentStatus.Published, ct);
        return item is null ? throw new NotFoundException($"No published content with slug '{slug}'.") : ContentItemDto.From(item);
    }

    public async Task<ContentItemDto> CreateAsync(SaveContentRequest request, CancellationToken ct = default)
    {
        ContentValidator.ThrowIfInvalid(request);
        var slug = ResolveSlug(request);
        await EnsureSlugFreeAsync(slug, exceptId: null, ct);

        var item = new ContentItem();
        Apply(item, request, slug);
        db.ContentItems.Add(item);
        await SaveAsync(ct);
        return ContentItemDto.From(item);
    }

    public async Task<ContentItemDto> UpdateAsync(Guid id, SaveContentRequest request, CancellationToken ct = default)
    {
        ContentValidator.ThrowIfInvalid(request);
        var item = await FindAsync(id, ct, track: true);
        var slug = ResolveSlug(request);
        await EnsureSlugFreeAsync(slug, exceptId: id, ct);

        Apply(item, request, slug);
        item.UpdatedAt = DateTime.UtcNow;
        await SaveAsync(ct);
        return ContentItemDto.From(item);
    }

    public async Task<ContentItemDto> SetPublishedAsync(Guid id, bool published, CancellationToken ct = default)
    {
        var item = await FindAsync(id, ct, track: true);
        if (published && item.Status != ContentStatus.Published)
        {
            item.Status = ContentStatus.Published;
            item.PublishedAt = DateTime.UtcNow;
        }
        else if (!published)
        {
            item.Status = ContentStatus.Draft;
        }
        item.UpdatedAt = DateTime.UtcNow;
        await SaveAsync(ct);
        return ContentItemDto.From(item);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var item = await FindAsync(id, ct, track: true);
        db.ContentItems.Remove(item);
        await db.SaveChangesAsync(ct);
    }

    private async Task<ContentItem> FindAsync(Guid id, CancellationToken ct, bool track)
    {
        var q = track ? db.ContentItems : db.ContentItems.AsNoTracking();
        return await q.FirstOrDefaultAsync(x => x.Id == id, ct)
               ?? throw new NotFoundException($"Content item {id} was not found.");
    }

    private static string ResolveSlug(SaveContentRequest r)
    {
        var slug = string.IsNullOrWhiteSpace(r.Slug) ? Slug.From(r.Title) : r.Slug!;
        if (!Slug.IsValid(slug))
            throw new ValidationFailedException(new Dictionary<string, string[]>
            {
                ["slug"] = ["Could not derive a valid slug; please provide one."]
            });
        return slug;
    }

    private async Task EnsureSlugFreeAsync(string slug, Guid? exceptId, CancellationToken ct)
    {
        var taken = await db.ContentItems.AnyAsync(x => x.Slug == slug && x.Id != exceptId, ct);
        if (taken) throw new ConflictException($"Slug '{slug}' is already in use.");
    }

    // The unique index is the source of truth; this turns a lost race into a 409 instead of a 500.
    private async Task SaveAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("slug", StringComparison.OrdinalIgnoreCase) == true
                                           || ex.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true)
        {
            throw new ConflictException("Slug is already in use.");
        }
    }

    private static void Apply(ContentItem item, SaveContentRequest r, string slug)
    {
        item.Kind = r.Kind;
        item.Title = r.Title.Trim();
        item.Slug = slug;
        item.Body = r.Body ?? string.Empty;
        item.Summary = NullIfBlank(r.Summary);
        item.SeoTitle = NullIfBlank(r.SeoTitle);
        item.SeoDescription = NullIfBlank(r.SeoDescription);
        item.Tags = (r.Tags ?? new List<string>())
            .Select(t => t.Trim()).Where(t => t.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string? NullIfBlank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
