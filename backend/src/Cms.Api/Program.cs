using System.Threading.RateLimiting;
using Cms.Ai;
using Cms.Api.Endpoints;
using Cms.Api.Security;
using Cms.Application.Common;
using Cms.Infrastructure;
using Cms.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAi(builder.Configuration);

builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection(ApiKeyOptions.Section));
builder.Services.AddSingleton<ApiKeyFilter>();

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddDbContextCheck<CmsDbContext>("database");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
{
    if (origins.Length > 0) p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
}));

// AI calls cost money: cap them per client.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy(AiEndpoints.RateLimitPolicy, ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();

// Schema is created on first start; see README for the migrations note.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<CmsDbContext>().Database.EnsureCreated();
}

app.UseExceptionHandler(errorApp => errorApp.Run(async ctx =>
{
    var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
    var logger = ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Cms.Api");

    (int status, string title, IDictionary<string, string[]>? errors) = ex switch
    {
        ValidationFailedException v => (StatusCodes.Status400BadRequest, "Validation failed", v.Errors),
        NotFoundException n => (StatusCodes.Status404NotFound, n.Message, null),
        ConflictException c => (StatusCodes.Status409Conflict, c.Message, null),
        AiProviderException a => (StatusCodes.Status502BadGateway, a.Message, null),
        BadHttpRequestException => (StatusCodes.Status400BadRequest, "Malformed request.", null),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", null)
    };
    if (status >= 500 && status != StatusCodes.Status502BadGateway) logger.LogError(ex, "Unhandled exception");

    ctx.Response.StatusCode = status;
    ctx.Response.ContentType = "application/problem+json";
    await Results.Json(errors is null
            ? new ProblemDetails { Status = status, Title = title }
            : new ValidationProblemDetails(errors) { Status = status, Title = title },
        contentType: "application/problem+json").ExecuteAsync(ctx);
}));

app.UseCors();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok(new { name = "AI CMS API", docs = "/swagger" }));
app.MapContentEndpoints();
app.MapAiEndpoints();

app.Run();

public partial class Program;
