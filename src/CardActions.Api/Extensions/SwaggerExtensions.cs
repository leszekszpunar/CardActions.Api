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
                config.PostProcess = document =>
                {
                    document.Info.Title = "API Akcji Kart Płatniczych";
                    document.Info.Version = "v1";
                    document.Info.Description = @"API do zarządzania akcjami na kartach płatniczych.

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
            app.UseOpenApi(config =>
            {
                config.Path = "/swagger/{documentName}/swagger.json";
                config.DocumentName = "v1";
            });

            app.UseSwaggerUi(config =>
            {
                config.DocumentPath = "/swagger/{documentName}/swagger.json";
                config.DocumentTitle = "API Akcji Kart Płatniczych";
            });

            app.UseReDoc(config =>
            {
                config.Path = "/docs";
                config.DocumentPath = "/swagger/v1/swagger.json";
                config.DocumentTitle = "Dokumentacja API Akcji Kart Płatniczych";
            });
        }

        return app;
    }
}