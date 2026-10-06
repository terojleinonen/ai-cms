using System.Security.Cryptography;
using System.Text;

namespace Cms.Api.Security;

public class ApiKeyOptions
{
    public const string Section = "Auth";
    public const string HeaderName = "X-Api-Key";

    /// <summary>Shared secret required on every admin and AI request.</summary>
    public string? AdminApiKey { get; set; }
}

/// <summary>Endpoint filter guarding admin routes with a constant-time API key check.</summary>
public class ApiKeyFilter(Microsoft.Extensions.Options.IOptions<ApiKeyOptions> options) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var expected = options.Value.AdminApiKey;
        if (string.IsNullOrEmpty(expected))
            return Results.Problem("Admin API key is not configured on the server.", statusCode: StatusCodes.Status503ServiceUnavailable);

        var provided = context.HttpContext.Request.Headers[ApiKeyOptions.HeaderName].ToString();
        var a = SHA256.HashData(Encoding.UTF8.GetBytes(provided));
        var b = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        if (!CryptographicOperations.FixedTimeEquals(a, b))
            return Results.Problem("Missing or invalid API key.", statusCode: StatusCodes.Status401Unauthorized);

        return await next(context);
    }
}
