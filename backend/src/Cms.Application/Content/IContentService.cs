namespace Cms.Application.Content;

public interface IContentService
{
    Task<PagedResult<ContentListItemDto>> ListAsync(ContentQuery query, CancellationToken ct = default);
    Task<ContentItemDto> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns a published item by slug, or throws <see cref="Common.NotFoundException"/>.</summary>
    Task<ContentItemDto> GetPublishedBySlugAsync(string slug, CancellationToken ct = default);

    Task<ContentItemDto> CreateAsync(SaveContentRequest request, CancellationToken ct = default);
    Task<ContentItemDto> UpdateAsync(Guid id, SaveContentRequest request, CancellationToken ct = default);
    Task<ContentItemDto> SetPublishedAsync(Guid id, bool published, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
