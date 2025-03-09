namespace CardActions.Application.Common.Interfaces;

/// <summary>
///     Interfejs do obsługi lokalizacji w warstwie Application.
/// </summary>
public interface ILocalizationService
{
    string GetString(string key);

    string GetString(string key, params object[] args);
}