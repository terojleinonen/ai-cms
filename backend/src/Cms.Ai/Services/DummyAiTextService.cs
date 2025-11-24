using Cms.Application.Interfaces;

namespace Cms.Ai.Services;

/// <summary>
/// Placeholder AI service. Replace with a real OpenAI / Azure OpenAI implementation.
/// </summary>
public class DummyAiTextService : IAiTextService
{
    public Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
        => Task.FromResult($"[AI draft based on prompt]: {prompt}");

    public Task<string> RewriteAsync(string text, string instruction, CancellationToken cancellationToken = default)
        => Task.FromResult($"[AI rewrite ({instruction})]: {text}");

    public Task<string> SummarizeAsync(string text, CancellationToken cancellationToken = default)
        => Task.FromResult($"[AI summary]: {text[..Math.Min(text.Length, 120)]}...");
}
