using System.Globalization;
using CardActions.Api.Services;
using CardActions.Application.Common.Interfaces;
using Microsoft.AspNetCore.Localization;

namespace CardActions.Api.Extensions;

/// <summary>
///     Rozszerzenia konfiguracyjne dla lokalizacji
/// </summary>
public static class LocalizationExtensions
{
    /// <summary>
    ///     Dodaje konfigurację lokalizacji do usług
    /// </summary>
    public static IServiceCollection AddLocalizationConfiguration(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[]
            {
                new CultureInfo("pl"),
                new CultureInfo("pl-PL"),
                new CultureInfo("en"),
                new CultureInfo("en-US")
            };

            options.DefaultRequestCulture = new RequestCulture("pl-PL");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        services.AddSingleton<ILocalizationService, StringLocalizerWrapper>();

        return services;
    }
}