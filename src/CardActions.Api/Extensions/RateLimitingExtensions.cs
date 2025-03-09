using System.Threading.RateLimiting;

namespace CardActions.Api.Extensions;

/// <summary>
///     Rozszerzenia konfiguracyjne dla limitowania żądań
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    ///     Dodaje konfigurację limitowania żądań do usług
    /// </summary>
    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
                    partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            options.AddPolicy("api", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
                    partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        QueueLimit = 0,
                        Window = TimeSpan.FromSeconds(10)
                    }));

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsync(
                    """{"title":"Too Many Requests","status":429,"detail":"Przekroczono limit żądań. Spróbuj ponownie później."}""",
                    token);
            };
        });

        return services;
    }
}