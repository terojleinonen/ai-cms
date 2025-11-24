using Cms.Ai.Services;
using Cms.Application.Interfaces;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Cms.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext (in-memory by default for template)
builder.Services.AddDbContext<CmsDbContext>(options =>
    options.UseInMemoryDatabase("cms-template"));

// Services
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddSingleton<IAiTextService, DummyAiTextService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new { status = "ok", message = "CMS API template running" }));

// Content endpoints (very basic)
app.MapGet("/api/content", async (IContentService contentService, CancellationToken ct) =>
{
    var items = await contentService.GetAllAsync(ct);
    return Results.Ok(items);
});

app.MapGet("/api/content/{id:guid}", async (Guid id, IContentService contentService, CancellationToken ct) =>
{
    var item = await contentService.GetByIdAsync(id, ct);
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.MapPost("/api/content", async (ContentItem item, IContentService contentService, CancellationToken ct) =>
{
    var created = await contentService.CreateAsync(item, ct);
    return Results.Created($"/api/content/{created.Id}", created);
});

// AI endpoints (dummy)
app.MapPost("/api/ai/generate-text", async (IAiTextService ai, AiGenerateRequest req, CancellationToken ct) =>
{
    var result = await ai.GenerateAsync(req.Prompt, ct);
    return Results.Ok(new { text = result });
});

app.MapPost("/api/ai/rewrite", async (IAiTextService ai, AiRewriteRequest req, CancellationToken ct) =>
{
    var result = await ai.RewriteAsync(req.Text, req.Instruction, ct);
    return Results.Ok(new { text = result });
});

app.MapPost("/api/ai/summarize", async (IAiTextService ai, AiSummarizeRequest req, CancellationToken ct) =>
{
    var result = await ai.SummarizeAsync(req.Text, ct);
    return Results.Ok(new { text = result });
});

app.Run();

public record AiGenerateRequest(string Prompt);
public record AiRewriteRequest(string Text, string Instruction);
public record AiSummarizeRequest(string Text);
