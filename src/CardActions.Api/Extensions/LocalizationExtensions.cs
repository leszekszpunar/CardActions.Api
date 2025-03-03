using System.Globalization;
using CardActions.Api.Services;
using CardActions.Application.Common.Interfaces;
using Microsoft.AspNetCore.Localization;

namespace CardActions.Api.Extensions;

/// <summary>
/// Configuration extensions for localization
/// </summary>
public static class LocalizationExtensions
{
    /// <summary>
    /// Adds localization configuration to services
    /// </summary>
    public static IServiceCollection AddLocalizationConfiguration(this IServiceCollection services)
    {
        // Rejestracja StringLocalizerWrapper jako ILocalizationService
        services.AddSingleton<ILocalizationService, StringLocalizerWrapper>();

        // Konfiguracja obsługiwanych kultur
        var supportedCultures = new[]
        {
            new CultureInfo("pl"),
            new CultureInfo("en"),
        };

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("pl");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        return services;
    }
}