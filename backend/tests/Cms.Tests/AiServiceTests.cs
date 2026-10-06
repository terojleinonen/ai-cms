using System.Net;
using System.Text;
using Cms.Ai;
using Cms.Ai.Services;
using Cms.Application.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Cms.Tests;

public class AiServiceTests
{
    private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Request = request;
            RequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
            return new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        }
    }

    private static (AnthropicAiTextService svc, StubHandler handler) Create(HttpStatusCode status, string body)
    {
        var handler = new StubHandler(status, body);
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        var opts = Options.Create(new AnthropicOptions { ApiKey = "k", Model = "test-model" });
        return (new AnthropicAiTextService(http, opts, NullLogger<AnthropicAiTextService>.Instance), handler);
    }

    [Fact]
    public async Task Generate_returns_text_blocks_and_sends_expected_payload()
    {
        var (svc, handler) = Create(HttpStatusCode.OK, """{"content":[{"type":"text","text":" Hello "}]}""");

        var text = await svc.GenerateAsync("write hello");

        Assert.Equal("Hello", text);
        Assert.Equal("/v1/messages", handler.Request!.RequestUri!.AbsolutePath);
        Assert.Contains("\"max_tokens\":2048", handler.RequestBody);
        Assert.Contains("test-model", handler.RequestBody);
    }

    [Fact]
    public async Task Provider_error_becomes_AiProviderException_without_leaking_body()
    {
        var (svc, _) = Create(HttpStatusCode.Unauthorized, """{"error":"secret-detail"}""");

        var ex = await Assert.ThrowsAsync<AiProviderException>(() => svc.SummarizeAsync("x"));
        Assert.DoesNotContain("secret-detail", ex.Message);
    }

    [Fact]
    public async Task Seo_parses_json_wrapped_in_prose_and_normalizes_slug()
    {
        var json = """
            {"content":[{"type":"text","text":"Sure!\n```json\n{\"title\":\"T\",\"description\":\"D\",\"slug\":\"My Slug!\",\"tags\":[\"A\",\" b \",\"a\"]}\n```"}]}
            """;
        var (svc, _) = Create(HttpStatusCode.OK, json);

        var seo = await svc.SuggestSeoAsync("title", "body");

        Assert.Equal("my-slug", seo.Slug);
        Assert.Equal(new[] { "a", "b" }, seo.Tags);
    }

    [Fact]
    public async Task Seo_with_garbage_output_throws_AiProviderException()
    {
        var (svc, _) = Create(HttpStatusCode.OK, """{"content":[{"type":"text","text":"no json here"}]}""");
        await Assert.ThrowsAsync<AiProviderException>(() => svc.SuggestSeoAsync("t", "b"));
    }

    [Fact]
    public async Task Offline_provider_is_deterministic_and_bounded()
    {
        var svc = new OfflineAiTextService();
        var body = "# Title\n\nCats are great pets. Cats purr loudly. Cats sleep a lot. More text follows here.";

        var summary = await svc.SummarizeAsync(body);
        var seo = await svc.SuggestSeoAsync("A very good article about cats", body);

        Assert.True(summary.Length <= 300);
        Assert.Contains("Cats are great pets.", summary);
        Assert.Equal("a-very-good-article-about-cats", seo.Slug);
        Assert.Contains("cats", seo.Tags);
        Assert.True(seo.Description.Length <= 155);
    }
}
