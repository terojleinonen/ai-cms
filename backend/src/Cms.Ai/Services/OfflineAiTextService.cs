using System.Text.RegularExpressions;
using Cms.Application.Ai;
using Cms.Application.Common;

namespace Cms.Ai.Services;

/// <summary>
/// Deterministic, dependency-free fallback used when no Anthropic API key is configured.
/// It keeps the whole app usable in demos, tests and CI. Output quality is basic by design.
/// </summary>
public partial class OfflineAiTextService : IAiTextService
{
    public string ProviderName => "offline";

    public Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
    {
        var topic = prompt.Trim().TrimEnd('.', '!', '?');
        var text = $"# {Capitalize(topic)}\n\n" +
                   $"*Offline draft — configure an Anthropic API key for real AI generation.*\n\n" +
                   $"## Overview\n\nWrite an introduction to {topic} here.\n\n" +
                   "## Key points\n\n- Point one\n- Point two\n- Point three\n\n## Conclusion\n\nSummarize the takeaways.";
        return Task.FromResult(text);
    }

    public Task<string> RewriteAsync(string text, string instruction, CancellationToken ct = default)
    {
        var lower = instruction.ToLowerInvariant();
        var result = text;
        if (lower.Contains("short") || lower.Contains("concise")) result = FirstSentences(text, 2);
        else if (lower.Contains("upper")) result = text.ToUpperInvariant();
        else result = WhitespaceRun().Replace(text, " ").Trim();
        return Task.FromResult(result);
    }

    public Task<string> SummarizeAsync(string text, CancellationToken ct = default)
    {
        var summary = FirstSentences(StripMarkdown(text), 2);
        if (summary.Length > 300) summary = summary[..297].TrimEnd() + "...";
        return Task.FromResult(summary);
    }

    public Task<SeoSuggestion> SuggestSeoAsync(string title, string body, CancellationToken ct = default)
    {
        var plain = StripMarkdown(body);
        var description = FirstSentences(plain, 2);
        if (description.Length > 155) description = description[..152].TrimEnd() + "...";

        var words = WordPattern().Matches(plain.ToLowerInvariant()).Select(m => m.Value)
            .Where(w => w.Length >= 4 && !Stop.Contains(w))
            .GroupBy(w => w).OrderByDescending(g => g.Count()).ThenBy(g => g.Key)
            .Take(5).Select(g => g.Key).ToList();

        var seoTitle = title.Length <= 60 ? title : title[..57].TrimEnd() + "...";
        return Task.FromResult(new SeoSuggestion(seoTitle, description, Slug.From(title), words));
    }

    private static readonly HashSet<string> Stop =
        ["about", "which", "their", "there", "would", "could", "should", "these", "those", "other", "where", "while"];

    private static string FirstSentences(string text, int count)
    {
        var sentences = SentenceEnd().Split(text.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).Take(count);
        return string.Join(" ", sentences).Trim();
    }

    private static string StripMarkdown(string text) =>
        WhitespaceRun().Replace(MarkdownSyntax().Replace(text, " "), " ").Trim();

    private static string Capitalize(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];

    [GeneratedRegex(@"\s+")] private static partial Regex WhitespaceRun();
    [GeneratedRegex(@"(?<=[.!?])\s+")] private static partial Regex SentenceEnd();
    [GeneratedRegex(@"[#*_`>\[\]\(\)-]+")] private static partial Regex MarkdownSyntax();
    [GeneratedRegex(@"[a-z]{3,}")] private static partial Regex WordPattern();
}
