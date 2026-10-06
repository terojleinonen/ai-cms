using Cms.Application.Common;

namespace Cms.Application.Content;

public static class ContentValidator
{
    public const int MaxTitle = 200;
    public const int MaxBody = 200_000;
    public const int MaxSummary = 600;
    public const int MaxSeoTitle = 120;
    public const int MaxSeoDescription = 320;
    public const int MaxTags = 20;
    public const int MaxTagLength = 40;

    public static void ThrowIfInvalid(SaveContentRequest r)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(r.Title)) errors["title"] = ["Title is required."];
        else if (r.Title.Length > MaxTitle) errors["title"] = [$"Title must be at most {MaxTitle} characters."];

        if ((r.Body?.Length ?? 0) > MaxBody) errors["body"] = [$"Body must be at most {MaxBody} characters."];
        if ((r.Summary?.Length ?? 0) > MaxSummary) errors["summary"] = [$"Summary must be at most {MaxSummary} characters."];
        if ((r.SeoTitle?.Length ?? 0) > MaxSeoTitle) errors["seoTitle"] = [$"SEO title must be at most {MaxSeoTitle} characters."];
        if ((r.SeoDescription?.Length ?? 0) > MaxSeoDescription) errors["seoDescription"] = [$"SEO description must be at most {MaxSeoDescription} characters."];

        if (!string.IsNullOrWhiteSpace(r.Slug) && !Slug.IsValid(r.Slug))
            errors["slug"] = ["Slug may contain only lowercase letters, digits and single hyphens."];

        if (r.Tags is { Count: > MaxTags } || r.Tags?.Any(t => t.Length > MaxTagLength) == true)
            errors["tags"] = [$"At most {MaxTags} tags of up to {MaxTagLength} characters each."];

        if (errors.Count > 0) throw new ValidationFailedException(errors);
    }
}
