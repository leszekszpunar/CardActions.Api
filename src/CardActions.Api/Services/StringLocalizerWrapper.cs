using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using CardActions.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CardActions.Api.Services;

/// <summary>
///     Custom JSON-based localization service with caching.
/// </summary>
internal class StringLocalizerWrapper : ILocalizationService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<StringLocalizerWrapper> _logger;
    private readonly string _resourcesPath;
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _translations = new();

    public StringLocalizerWrapper(IMemoryCache cache, ILogger<StringLocalizerWrapper> logger)
    {
        _resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/Languages");
        _cache = cache;
        _logger = logger;
        
        // Log dostępnych plików lokalizacji
        if (Directory.Exists(_resourcesPath))
        {
            var files = Directory.GetFiles(_resourcesPath, "*.json");
            _logger.LogInformation("Available localization files: {Files}", string.Join(", ", files));
        }
        else
        {
            _logger.LogWarning("Localization directory does not exist: {ResourcePath}", _resourcesPath);
        }
    }

    public string GetString(string key)
    {
        return GetString(key, Array.Empty<object>());
    }

    public string GetString(string key, params object[] args)
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        var translations = LoadTranslations(culture);

        if (translations.TryGetValue(key, out var value)) 
            return args.Length > 0 ? string.Format(value, args) : value;

        // Jeśli nie znaleziono, próbujemy użyć kodu języka bez regionu (pl-PL -> pl)
        var languageCode = culture.Split('-')[0];
        if (languageCode != culture)
        {
            translations = LoadTranslations(languageCode);
            if (translations.TryGetValue(key, out value))
                return args.Length > 0 ? string.Format(value, args) : value;
        }

        // Jeśli nadal nie znaleziono, próbujemy użyć angielskiego jako fallback
        if (culture != "en" && languageCode != "en")
        {
            translations = LoadTranslations("en");
            if (translations.TryGetValue(key, out value))
                return args.Length > 0 ? string.Format(value, args) : value;
        }

        _logger.LogWarning("Translation key '{Key}' not found in {Culture}.json", key, culture);
        return $"[{key}]"; // Fallback jeśli klucz nie istnieje
    }

    private Dictionary<string, string> LoadTranslations(string culture)
    {
        return _cache.GetOrCreate($"translations_{culture}", entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(30); // Buforowanie na 30 minut

            var filePath = Path.Combine(_resourcesPath, $"{culture}.json");
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Localization file not found: {FilePath}", filePath);
                return new Dictionary<string, string>();
            }

            try
            {
                var json = File.ReadAllText(filePath);
                var parsedData = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                return parsedData ?? new Dictionary<string, string>();
            }
            catch (JsonException ex)
            {
                _logger.LogError("Error parsing localization file {FilePath}: {Message}", filePath, ex.Message);
                return new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error reading localization file {FilePath}: {Message}", filePath, ex.Message);
                return new Dictionary<string, string>();
            }
        }) ?? new Dictionary<string, string>();
    }
}