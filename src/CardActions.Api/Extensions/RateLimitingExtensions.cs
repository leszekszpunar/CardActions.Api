using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CardActions.Api.Extensions;

/// <summary>
/// Configuration extensions for rate limiting
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Adds rate limiting configuration to services
    /// </summary>
    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("api", options =>
            {
                options.PermitLimit = 30;
                options.Window = TimeSpan.FromSeconds(10);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 2;
            });

            options.OnRejected = async (context, _) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/problem+json";

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too Many Requests",
                    Type = "https://tools.ietf.org/html/rfc6585#section-4",
                    Detail = "API rate limit has been exceeded. Please try again later.",
                    Instance = context.HttpContext.Request.Path
                };

                await context.HttpContext.Response.WriteAsJsonAsync(problem);
            };
        });

        return services;
    }
} 