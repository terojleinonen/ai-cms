using Cms.Api.Security;
using Cms.Application.Content;
using Cms.Domain.Entities;

namespace Cms.Api.Endpoints;

public static class ContentEndpoints
{
    public static void MapContentEndpoints(this IEndpointRouteBuilder app)
    {
        var pub = app.MapGroup("/api/public/content").WithTags("Public");
        pub.MapGet("/", async (IContentService svc, ContentKind? kind, string? search, int? page, int? pageSize, CancellationToken ct) =>
            Results.Ok(await svc.ListAsync(
                new ContentQuery(ContentStatus.Published, kind, search, page ?? 1, pageSize ?? 20), ct)));
        pub.MapGet("/{slug}", async (string slug, IContentService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetPublishedBySlugAsync(slug, ct)));

        var admin = app.MapGroup("/api/admin/content").WithTags("Admin")
            .AddEndpointFilter<ApiKeyFilter>();

        admin.MapGet("/", async (IContentService svc, ContentStatus? status, ContentKind? kind, string? search, int? page, int? pageSize, CancellationToken ct) =>
            Results.Ok(await svc.ListAsync(new ContentQuery(status, kind, search, page ?? 1, pageSize ?? 20), ct)));
        admin.MapGet("/{id:guid}", async (Guid id, IContentService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetByIdAsync(id, ct)));
        admin.MapPost("/", async (SaveContentRequest req, IContentService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateAsync(req, ct);
            return Results.Created($"/api/admin/content/{created.Id}", created);
        });
        admin.MapPut("/{id:guid}", async (Guid id, SaveContentRequest req, IContentService svc, CancellationToken ct) =>
            Results.Ok(await svc.UpdateAsync(id, req, ct)));
        admin.MapPost("/{id:guid}/publish", async (Guid id, IContentService svc, CancellationToken ct) =>
            Results.Ok(await svc.SetPublishedAsync(id, true, ct)));
        admin.MapPost("/{id:guid}/unpublish", async (Guid id, IContentService svc, CancellationToken ct) =>
            Results.Ok(await svc.SetPublishedAsync(id, false, ct)));
        admin.MapDelete("/{id:guid}", async (Guid id, IContentService svc, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return Results.NoContent();
        });
    }
}
