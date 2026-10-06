namespace Cms.Domain.Entities;

public enum ContentKind
{
    Post = 0,
    Page = 1
}

public enum ContentStatus
{
    Draft = 0,
    Published = 1
}

public class ContentItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ContentKind Kind { get; set; } = ContentKind.Post;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public List<string> Tags { get; set; } = new();
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
}
