namespace Cms.Application.Interfaces;

public interface IAiTextService
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
    Task<string> RewriteAsync(string text, string instruction, CancellationToken cancellationToken = default);
    Task<string> SummarizeAsync(string text, CancellationToken cancellationToken = default);
}
