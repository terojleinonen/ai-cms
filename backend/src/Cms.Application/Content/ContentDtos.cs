using Cms.Domain.Entities;

namespace Cms.Application.Content;

public record ContentItemDto(
    Guid Id,
    ContentKind Kind,
    string Title,
    string Slug,
    string Body,
    string? Summary,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<string> Tags,
    ContentStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt)
{
    public static ContentItemDto From(ContentItem i) => new(
        i.Id, i.Kind, i.Title, i.Slug, i.Body, i.Summary, i.SeoTitle, i.SeoDescription,
        i.Tags.ToList(), i.Status, i.CreatedAt, i.UpdatedAt, i.PublishedAt);
}

/// <summary>Body-less projection used for list views.</summary>
public record ContentListItemDto(
    Guid Id,
    ContentKind Kind,
    string Title,
    string Slug,
    string? Summary,
    IReadOnlyList<string> Tags,
    ContentStatus Status,
    DateTime UpdatedAt,
    DateTime? PublishedAt)
{
    public static ContentListItemDto From(ContentItem i) => new(
        i.Id, i.Kind, i.Title, i.Slug, i.Summary, i.Tags.ToList(), i.Status, i.UpdatedAt, i.PublishedAt);
}

public record SaveContentRequest(
    ContentKind Kind,
    string Title,
    string? Slug,
    string Body,
    string? Summary,
    string? SeoTitle,
    string? SeoDescription,
    List<string>? Tags);

public record ContentQuery(
    ContentStatus? Status = null,
    ContentKind? Kind = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
