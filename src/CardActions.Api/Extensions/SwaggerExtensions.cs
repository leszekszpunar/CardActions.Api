using System.Reflection;
using NSwag;

namespace CardActions.Api.Extensions;

/// <summary>
///     Rozszerzenia konfiguracyjne dla Swagger/OpenAPI
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    ///     Dodaje konfigurację Swagger/OpenAPI do serwisów
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services,
        IConfiguration configuration)
    {
        var disableSwagger = configuration["DisableSwagger"] == "true";
        if (!disableSwagger)
        {
            services.AddEndpointsApiExplorer();
            services.AddOpenApiDocument(config =>
            {
                // Pobierz wersję z assembly
                var version = GetAssemblyVersion();
                
                config.PostProcess = document =>
                {
                    document.Info.Title = "API Akcji Kart Płatniczych";
                    document.Info.Version = version;
                    document.Info.Description = $@"API do zarządzania akcjami na kartach płatniczych.

## Wersja: {version}

## Linki
- [Dokumentacja Swagger](/swagger)
- [Stan API](/health)
- [Dokumentacja ReDoc](/docs)

## Opis biznesowy
API umożliwia sprawdzenie dozwolonych akcji dla karty płatniczej na podstawie:
- Rodzaju karty (PREPAID/DEBIT/CREDIT)
- Statusu karty
- Stanu PIN

## Dozwolone akcje

| Akcja | PREPAID | DEBIT | CREDIT | ORDERED | INACTIVE | ACTIVE | RESTRICTED | BLOCKED | EXPIRED | CLOSED |
|-------|---------|-------|--------|---------|----------|--------|------------|---------|---------|--------|
| ACTION1 | TAK | TAK | TAK | NIE | NIE | TAK | NIE | NIE | NIE | NIE |
| ACTION2 | TAK | TAK | TAK | NIE | TAK | NIE | NIE | NIE | NIE | NIE |
| ACTION3 | TAK | TAK | TAK | TAK | TAK | TAK | TAK | TAK | TAK | TAK |
| ACTION4 | TAK | TAK | TAK | TAK | TAK | TAK | TAK | TAK | TAK | TAK |
| ACTION5 | NIE | NIE | TAK | TAK | TAK | TAK | TAK | TAK | TAK | TAK |
| ACTION6 | TAK | TAK | TAK | TAK* | TAK* | TAK* | NIE | TAK** | NIE | NIE |
| ACTION7 | TAK | TAK | TAK | TAK** | TAK** | TAK** | NIE | TAK** | NIE | NIE |
| ACTION8 | TAK | TAK | TAK | TAK | TAK | TAK | NIE | TAK | NIE | NIE |
| ACTION9 | TAK | TAK | TAK | TAK | TAK | TAK | TAK | TAK | TAK | TAK |
| ACTION10 | TAK | TAK | TAK | TAK | TAK | TAK | NIE | NIE | NIE | NIE |
| ACTION11 | TAK | TAK | TAK | NIE | TAK | TAK | NIE | NIE | NIE | NIE |
| ACTION12 | TAK | TAK | TAK | TAK | TAK | TAK | NIE | NIE | NIE | NIE |
| ACTION13 | TAK | TAK | TAK | TAK | TAK | TAK | NIE | NIE | NIE | NIE |

\* - ale jak nie ma pin to NIE

\** - jeżeli pin nadany";

                    document.Info.Contact = new OpenApiContact
                    {
                        Name = "Zespół API",
                        Email = "api@bank.pl"
                    };
                };

                config.SchemaSettings.GenerateXmlObjects = true;

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            });
        }

        return services;
    }

    /// <summary>
    ///     Konfiguruje middleware Swagger/OpenAPI
    /// </summary>
    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var disableSwagger = configuration["DisableSwagger"] == "true";
        if (!disableSwagger)
        {
            // Pobierz wersję z assembly
            var version = GetAssemblyVersion();
            
            app.UseOpenApi(config =>
            {
                config.Path = "/swagger/{documentName}/swagger.json";
                config.DocumentName = "v1";
            });

            app.UseSwaggerUi(config =>
            {
                config.DocumentPath = "/swagger/{documentName}/swagger.json";
                config.DocumentTitle = $"API Akcji Kart Płatniczych (v{version})";
            });

            app.UseReDoc(config =>
            {
                config.Path = "/docs";
                config.DocumentPath = "/swagger/v1/swagger.json";
                config.DocumentTitle = $"Dokumentacja API Akcji Kart Płatniczych (v{version})";
            });
        }

        return app;
    }
    
    /// <summary>
    /// Pobiera wersję aplikacji z assembly
    /// </summary>
    private static string GetAssemblyVersion()
    {
        try
        {
            // Pobierz wersję z assembly
            var assembly = Assembly.GetExecutingAssembly();
            
            // Najpierw spróbuj pobrać wersję z atrybutu informacyjnego
            var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (!string.IsNullOrEmpty(infoVersion))
            {
                // Usuń suffiks z informationalVersion (np. +sha.xxx)
                var plusIndex = infoVersion.IndexOf('+');
                if (plusIndex > 0)
                {
                    infoVersion = infoVersion.Substring(0, plusIndex);
                }
                return infoVersion;
            }
            
            // Następnie spróbuj pobrać wersję z atrybutu file version
            var fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            if (!string.IsNullOrEmpty(fileVersion))
            {
                return fileVersion;
            }
            
            // Na końcu użyj standardowej wersji assembly
            return assembly.GetName().Version?.ToString() ?? "1.0.0";
        }
        catch (Exception)
        {
            // W przypadku jakichkolwiek błędów, zwróć domyślną wersję
            return "1.0.0";
        }
    }
}