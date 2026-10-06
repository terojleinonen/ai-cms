namespace Cms.Application.Ai;

public record SeoSuggestion(string Title, string Description, string Slug, IReadOnlyList<string> Tags);

public interface IAiTextService
{
    /// <summary>Human-readable name of the backing provider, surfaced to the admin UI.</summary>
    string ProviderName { get; }

    Task<string> GenerateAsync(string prompt, CancellationToken ct = default);
    Task<string> RewriteAsync(string text, string instruction, CancellationToken ct = default);
    Task<string> SummarizeAsync(string text, CancellationToken ct = default);
    Task<SeoSuggestion> SuggestSeoAsync(string title, string body, CancellationToken ct = default);
}
