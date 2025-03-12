using System.Reflection;
using CardActions.Application.Features.Version.Queries.GetVersionInfo;
using CardActions.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CardActions.Infrastructure.Services;

/// <summary>
///     Implementacja serwisu do pobierania informacji o wersji aplikacji
/// </summary>
internal class VersionService : IVersionService
{
    private readonly ILogger<VersionService> _logger;

    /// <summary>
    ///     Inicjalizuje nową instancję klasy <see cref="VersionService" />
    /// </summary>
    /// <param name="logger">Logger</param>
    public VersionService(ILogger<VersionService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public VersionInfoDto GetVersionInfo(Assembly assembly, bool includeDetailedInfo = true)
    {
        try
        {
            // Podstawowe informacje o wersji
            var version = assembly.GetName().Version?.ToString() ?? "1.0.0";
            var fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? version;
            var infoVersion =
                assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? version;

            // Hash commita z InformationalVersion
            var commitHash = string.Empty;
            if (infoVersion.Contains('+'))
            {
                var parts = infoVersion.Split('+');
                if (parts.Length > 1)
                {
                    commitHash = parts[1];
                    infoVersion = parts[0]; // Wersja bez hasha
                }
            }

            // Dodatkowe metadane
            var sourceRevision = string.Empty;
            var buildDate = string.Empty;
            var releaseChannel = string.Empty;

            try
            {
                // W .NET 6 i nowszych możemy użyć GetCustomAttributes z nazwą metadanej
                var sourceRevAttr = assembly.GetCustomAttributes().OfType<AssemblyMetadataAttribute>()
                    .FirstOrDefault(a => a.Key == "SourceRevisionId");
                var buildDateAttr = assembly.GetCustomAttributes().OfType<AssemblyMetadataAttribute>()
                    .FirstOrDefault(a => a.Key == "BuildDate");
                var channelAttr = assembly.GetCustomAttributes().OfType<AssemblyMetadataAttribute>()
                    .FirstOrDefault(a => a.Key == "ReleaseChannel");

                sourceRevision = sourceRevAttr?.Value ?? commitHash;
                buildDate = buildDateAttr?.Value ?? DateTime.UtcNow.ToString("o");

                // Jeśli nie ma informacji o kanale wydania, spróbuj wykryć na podstawie środowiska
                if (string.IsNullOrEmpty(channelAttr?.Value))
                {
                    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";
                    releaseChannel = string.IsNullOrEmpty(environment)
                        ? "unknown"
                        : environment.ToLower().Contains("prod")
                            ? "production"
                            : environment.ToLower().Contains("dev") || environment.ToLower() == "development"
                                ? "development"
                                : environment;

                    _logger.LogInformation(
                        "Kanał wydania nie został znaleziony w metadanych assembly. Wykryto środowisko: {Environment}, ustawiono kanał: {Channel}",
                        environment, releaseChannel);
                }
                else
                {
                    releaseChannel = channelAttr.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nie udało się pobrać metadanych assembly. Używam wartości domyślnych.");
                // W przypadku błędu, użyj wartości domyślnych
                sourceRevision = commitHash;
                buildDate = DateTime.UtcNow.ToString("o");
                releaseChannel = "unknown";
            }

            // Utwórz obiekt odpowiedzi
            var versionInfo = new VersionInfoDto
            {
                Version = infoVersion,
                FileVersion = fileVersion,
                AssemblyVersion = version,
                FullVersion = string.IsNullOrEmpty(commitHash) ? infoVersion : $"{infoVersion}+{commitHash}",
                Product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "CardActions.Api",
                Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ??
                              "API do zarządzania akcjami dla kart płatniczych"
            };

            // Dodaj szczegółowe informacje tylko jeśli wymagane
            if (includeDetailedInfo)
            {
                versionInfo.CommitHash = sourceRevision;
                versionInfo.BuildDate = buildDate;
                versionInfo.ReleaseChannel = releaseChannel;
            }
            else
            {
                // W przeciwnym razie pokazujemy ograniczone informacje
                versionInfo.CommitHash = sourceRevision.Length > 7 ? sourceRevision.Substring(0, 7) : sourceRevision;
                versionInfo.BuildDate = "Niedostępne";
                versionInfo.ReleaseChannel = releaseChannel; // Kanał wydania jest bezpieczny
            }

            return versionInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas pobierania informacji o wersji");
            throw new ApplicationException("Nie udało się pobrać informacji o wersji.", ex);
        }
    }
}