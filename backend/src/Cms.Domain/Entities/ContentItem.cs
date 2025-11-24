using System.Text.Json;

namespace Cms.Domain.Entities;

public class ContentItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ContentTypeId { get; set; }
    public ContentType? ContentType { get; set; }
    public string Slug { get; set; } = string.Empty;
    public JsonDocument Data { get; set; } = JsonDocument.Parse("{}");
    public bool IsDraft { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
