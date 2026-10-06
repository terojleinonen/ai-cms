using Cms.Ai.Services;
using Cms.Application.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Ai;

public static class DependencyInjection
{
    /// <summary>Uses Anthropic when <c>Anthropic:ApiKey</c> is set, otherwise the offline fallback.</summary>
    public static IServiceCollection AddAi(this IServiceCollection services, IConfiguration config)
    {
        var section = config.GetSection(AnthropicOptions.Section);
        services.Configure<AnthropicOptions>(section);
        var options = section.Get<AnthropicOptions>() ?? new AnthropicOptions();

        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            services.AddSingleton<IAiTextService, OfflineAiTextService>();
            return services;
        }

        services.AddHttpClient<IAiTextService, AnthropicAiTextService>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        });
        return services;
    }
}
