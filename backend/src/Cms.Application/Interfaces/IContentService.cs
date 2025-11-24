using Cms.Domain.Entities;

namespace Cms.Application.Interfaces;

public interface IContentService
{
    Task<IEnumerable<ContentItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ContentItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContentItem> CreateAsync(ContentItem item, CancellationToken cancellationToken = default);
    Task<ContentItem> UpdateAsync(ContentItem item, CancellationToken cancellationToken = default);
}
