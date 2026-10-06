using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Cms.Tests;

public class CmsFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"cms-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Cms", $"Data Source={_dbPath}");
        builder.UseSetting("Auth:AdminApiKey", "test-key");
        builder.UseSetting("Anthropic:ApiKey", "");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (var f in new[] { _dbPath, _dbPath + "-shm", _dbPath + "-wal" }) if (File.Exists(f)) File.Delete(f);
    }
}

public class ApiTests(CmsFactory factory) : IClassFixture<CmsFactory>
{
    private HttpClient Admin()
    {
        var c = factory.CreateClient();
        c.DefaultRequestHeaders.Add("X-Api-Key", "test-key");
        return c;
    }

    private static object Post(string title, string? slug = null) =>
        new { kind = "Post", title, slug, body = "Body text. Second sentence.", tags = new[] { "x" } };

    [Fact]
    public async Task Health_is_ok()
    {
        var res = await factory.CreateClient().GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Theory]
    [InlineData("/api/admin/content")]
    [InlineData("/api/admin/ai/status")]
    public async Task Admin_routes_reject_missing_or_wrong_key(string path)
    {
        var anon = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anon.GetAsync(path)).StatusCode);

        anon.DefaultRequestHeaders.Add("X-Api-Key", "wrong");
        Assert.Equal(HttpStatusCode.Unauthorized, (await anon.GetAsync(path)).StatusCode);
    }

    [Fact]
    public async Task Full_content_lifecycle()
    {
        var admin = Admin();

        var created = await admin.PostAsJsonAsync("/api/admin/content", Post("Lifecycle Post"));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var item = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = item.GetProperty("id").GetString();
        Assert.Equal("lifecycle-post", item.GetProperty("slug").GetString());
        Assert.Equal("Draft", item.GetProperty("status").GetString());

        // Drafts are invisible publicly.
        var pub = factory.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound, (await pub.GetAsync("/api/public/content/lifecycle-post")).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"/api/admin/content/{id}/publish", null)).StatusCode);
        var visible = await pub.GetAsync("/api/public/content/lifecycle-post");
        Assert.Equal(HttpStatusCode.OK, visible.StatusCode);

        var update = await admin.PutAsJsonAsync($"/api/admin/content/{id}", Post("Lifecycle Renamed", "lifecycle-post"));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = await update.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Lifecycle Renamed", updated.GetProperty("title").GetString());
        Assert.Equal("Published", updated.GetProperty("status").GetString());

        var list = await pub.GetFromJsonAsync<JsonElement>("/api/public/content?search=renamed");
        Assert.Equal(1, list.GetProperty("totalCount").GetInt32());

        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/admin/content/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync($"/api/admin/content/{id}")).StatusCode);
    }

    [Fact]
    public async Task Duplicate_slug_returns_conflict()
    {
        var admin = Admin();
        Assert.Equal(HttpStatusCode.Created, (await admin.PostAsJsonAsync("/api/admin/content", Post("Dup", "dup-slug"))).StatusCode);
        var second = await admin.PostAsJsonAsync("/api/admin/content", Post("Dup Two", "dup-slug"));
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Invalid_input_returns_validation_problem()
    {
        var res = await Admin().PostAsJsonAsync("/api/admin/content", Post("", "Bad Slug"));
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var problem = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty("title", out _));
        Assert.True(problem.GetProperty("errors").TryGetProperty("slug", out _));
    }

    [Fact]
    public async Task Ai_endpoints_work_with_offline_provider()
    {
        var admin = Admin();
        var status = await admin.GetFromJsonAsync<JsonElement>("/api/admin/ai/status");
        Assert.Equal("offline", status.GetProperty("provider").GetString());

        var seo = await admin.PostAsJsonAsync("/api/admin/ai/seo", new { title = "Hello AI", body = "Some body text here." });
        Assert.Equal(HttpStatusCode.OK, seo.StatusCode);
        Assert.Equal("hello-ai", (await seo.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("slug").GetString());

        var bad = await admin.PostAsJsonAsync("/api/admin/ai/generate", new { prompt = "" });
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
    }
}
