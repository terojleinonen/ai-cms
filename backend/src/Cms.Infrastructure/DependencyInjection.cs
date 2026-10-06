using Cms.Application.Content;
using Cms.Infrastructure.Persistence;
using Cms.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers persistence. <c>Database:Provider</c> selects "Sqlite" (default) or "Postgres";
    /// the connection string is read from <c>ConnectionStrings:Cms</c>.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var provider = config["Database:Provider"] ?? "Sqlite";
        var connection = config.GetConnectionString("Cms") ?? "Data Source=cms.db";

        services.AddDbContext<CmsDbContext>(options =>
        {
            switch (provider.ToLowerInvariant())
            {
                case "sqlite": options.UseSqlite(connection); break;
                case "postgres" or "postgresql": options.UseNpgsql(connection); break;
                default: throw new InvalidOperationException($"Unsupported Database:Provider '{provider}'. Use Sqlite or Postgres.");
            }
        });

        services.AddScoped<IContentService, ContentService>();
        return services;
    }
}
