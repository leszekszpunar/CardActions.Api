using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using CardActions.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CardActions.Api.Services;

/// <summary>
/// Custom JSON-based localization service with caching.
/// </summary>
internal class StringLocalizerWrapper : ILocalizationService
{
    private readonly string _resourcesPath;
    private readonly ILogger<StringLocalizerWrapper> _logger;
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _translations = new();

    public StringLocalizerWrapper(IMemoryCache cache, ILogger<StringLocalizerWrapper> logger)
    {
        _resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/Languages");
        _cache = cache;
        _logger = logger;
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
        {
            return args.Length > 0 ? string.Format(value, args) : value;
        }

        _logger.LogWarning($"Translation key '{key}' not found in {culture}.json");
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
                _logger.LogWarning($"Localization file not found: {filePath}");
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
                _logger.LogError($"Error parsing localization file {filePath}: {ex.Message}");
                return new Dictionary<string, string>(); 
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error reading localization file {filePath}: {ex.Message}");
                return new Dictionary<string, string>(); 
            }
        }) ?? new Dictionary<string, string>();
    }
}