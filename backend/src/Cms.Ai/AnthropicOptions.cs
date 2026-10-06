namespace Cms.Ai;

public class AnthropicOptions
{
    public const string Section = "Anthropic";

    /// <summary>API key. When empty the offline heuristic provider is used instead.</summary>
    public string? ApiKey { get; set; }
    public string Model { get; set; } = "claude-sonnet-5-5";
    public string BaseUrl { get; set; } = "https://api.anthropic.com/";
    public int MaxTokens { get; set; } = 2048;
    public int TimeoutSeconds { get; set; } = 60;
}
