using Cms.Api.Security;
using Cms.Application.Ai;
using Cms.Application.Common;

namespace Cms.Api.Endpoints;

public record AiGenerateRequest(string Prompt);
public record AiRewriteRequest(string Text, string Instruction);
public record AiSummarizeRequest(string Text);
public record AiSeoRequest(string Title, string Body);

public static class AiEndpoints
{
    public const string RateLimitPolicy = "ai";
    private const int MaxInput = 50_000;

    public static void MapAiEndpoints(this IEndpointRouteBuilder app)
    {
        var ai = app.MapGroup("/api/admin/ai").WithTags("AI")
            .AddEndpointFilter<ApiKeyFilter>()
            .RequireRateLimiting(RateLimitPolicy);

        ai.MapGet("/status", (IAiTextService svc) => Results.Ok(new { provider = svc.ProviderName }));

        ai.MapPost("/generate", async (AiGenerateRequest r, IAiTextService svc, CancellationToken ct) =>
        {
            Require(("prompt", r.Prompt, 2_000));
            return Results.Ok(new { text = await svc.GenerateAsync(r.Prompt, ct) });
        });
        ai.MapPost("/rewrite", async (AiRewriteRequest r, IAiTextService svc, CancellationToken ct) =>
        {
            Require(("text", r.Text, MaxInput), ("instruction", r.Instruction, 500));
            return Results.Ok(new { text = await svc.RewriteAsync(r.Text, r.Instruction, ct) });
        });
        ai.MapPost("/summarize", async (AiSummarizeRequest r, IAiTextService svc, CancellationToken ct) =>
        {
            Require(("text", r.Text, MaxInput));
            return Results.Ok(new { text = await svc.SummarizeAsync(r.Text, ct) });
        });
        ai.MapPost("/seo", async (AiSeoRequest r, IAiTextService svc, CancellationToken ct) =>
        {
            Require(("title", r.Title, 200), ("body", r.Body, MaxInput));
            return Results.Ok(await svc.SuggestSeoAsync(r.Title, r.Body, ct));
        });
    }

    private static void Require(params (string Name, string? Value, int Max)[] fields)
    {
        var errors = new Dictionary<string, string[]>();
        foreach (var (name, value, max) in fields)
        {
            if (string.IsNullOrWhiteSpace(value)) errors[name] = [$"{name} is required."];
            else if (value.Length > max) errors[name] = [$"{name} must be at most {max} characters."];
        }
        if (errors.Count > 0) throw new ValidationFailedException(errors);
    }
}
