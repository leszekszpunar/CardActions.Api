using System.Globalization;
using System.Text;
using CardActions.Domain.Enums;
using CardActions.Domain.Models;
using CardActions.Domain.Policies;
using CardActions.Infrastructure.Services.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace CardActions.Infrastructure.Services;

/// <summary>
///     Implementacja wczytywaczy reguł akcji karty z pliku CSV.
///     Ta klasa odpowiada za wczytywanie i parsowanie reguł z pliku CSV.
/// </summary>
internal sealed class CsvCardActionRulesLoader : ICardActionRulesLoader
{
    private readonly string _csvPath;
    private readonly ILogger<CsvCardActionRulesLoader> _logger;

    /// <summary>
    ///     Inicjalizuje nową instancję klasy <see cref="CsvCardActionRulesLoader" />.
    /// </summary>
    /// <param name="csvPath">Ścieżka do pliku CSV z regułami</param>
    /// <param name="logger">Logger używany do logowania</param>
    /// <exception cref="FileNotFoundException">Rzucany, gdy plik CSV nie istnieje</exception>
    public CsvCardActionRulesLoader(string csvPath, ILogger<CsvCardActionRulesLoader> logger)
    {
        _logger = logger;

        var absolutePath = Path.GetFullPath(csvPath);
        _logger.LogInformation("CSV rules path: {CsvPath} (absolute: {AbsolutePath})", csvPath, absolutePath);

        if (!File.Exists(absolutePath))
        {
            _logger.LogError("CSV file not found at {CsvPath}", absolutePath);
            throw new FileNotFoundException($"CSV file not found at {absolutePath}", absolutePath);
        }

        _csvPath = absolutePath;
    }

    /// <summary>
    ///     Wczytuje reguły akcji karty z pliku CSV.
    /// </summary>
    /// <returns>Kolekcja reguł akcji karty</returns>
    /// <exception cref="InvalidOperationException">Rzucany, gdy nie udało się wczytać reguł z pliku CSV</exception>
    public IReadOnlyCollection<CardActionRule> LoadRules()
    {
        try
        {
            var rules = new List<CardActionRule>();

            using (var reader = new StreamReader(_csvPath, Encoding.UTF8))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                // Wczytaj nagłówki
                csv.Read();
                csv.ReadHeader();
                var headers = csv.HeaderRecord;

                if (headers == null)
                {
                    _logger.LogError("CSV file does not contain headers");
                    throw new InvalidOperationException("CSV file does not contain headers");
                }

                ValidateHeaders(headers);

                // Znajdź indeksy kolumn
                var cardKindStartIndex = Array.FindIndex(headers, h => h == "PREPAID");
                var cardStatusStartIndex = Array.FindIndex(headers, h => h == "ORDERED");

                // Wczytaj rekordy
                while (csv.Read()) ProcessCsvRecord(csv, headers, cardKindStartIndex, cardStatusStartIndex, rules);
            }

            _logger.LogInformation("Successfully loaded {RuleCount} card action rules", rules.Count);
            return rules;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading card action rules from {CsvPath}", _csvPath);
            throw new InvalidOperationException($"Failed to load card action rules from {_csvPath}: {ex.Message}", ex);
        }
    }

    /// <summary>
    ///     Waliduje nagłówki pliku CSV.
    /// </summary>
    /// <param name="headers">Nagłówki pliku CSV</param>
    /// <exception cref="InvalidOperationException">Rzucany, gdy nagłówki są nieprawidłowe</exception>
    private void ValidateHeaders(string[] headers)
    {
        var cardKindStartIndex = Array.FindIndex(headers, h => h == "PREPAID");
        var cardStatusStartIndex = Array.FindIndex(headers, h => h == "ORDERED");

        if (cardKindStartIndex == -1 || cardStatusStartIndex == -1)
        {
            _logger.LogError("CSV file does not contain required headers: PREPAID or ORDERED");
            throw new InvalidOperationException("CSV file does not contain required headers: PREPAID or ORDERED");
        }
    }

    /// <summary>
    ///     Przetwarza rekord CSV i tworzy reguły akcji karty.
    /// </summary>
    /// <param name="csv">Czytnik CSV</param>
    /// <param name="headers">Nagłówki pliku CSV</param>
    /// <param name="cardKindStartIndex">Indeks startowy dla typów kart</param>
    /// <param name="cardStatusStartIndex">Indeks startowy dla statusów kart</param>
    /// <param name="rules">Lista reguł do uzupełnienia</param>
    private void ProcessCsvRecord(CsvReader csv, string[] headers, int cardKindStartIndex, int cardStatusStartIndex,
        List<CardActionRule> rules)
    {
        var record = csv.GetRecord<dynamic>();
        var recordDict = (IDictionary<string, object>)record;

        if (!recordDict.ContainsKey("ALLOWED ACTION"))
        {
            _logger.LogWarning("Record does not contain 'ALLOWED ACTION' column, skipping");
            return;
        }

        var actionName = recordDict["ALLOWED ACTION"]?.ToString() ?? string.Empty;

        if (string.IsNullOrEmpty(actionName))
        {
            _logger.LogWarning("Record has empty action name, skipping");
            return;
        }

        foreach (var cardType in Enum.GetValues<CardType>())
            ProcessCardType(headers, recordDict, actionName, cardType, cardKindStartIndex, cardStatusStartIndex, rules);
    }

    /// <summary>
    ///     Przetwarza pojedynczy typ karty i tworzy reguły dla wszystkich statusów.
    /// </summary>
    private void ProcessCardType(
        string[] headers,
        IDictionary<string, object> recordDict,
        string actionName,
        CardType cardType,
        int cardKindStartIndex,
        int cardStatusStartIndex,
        List<CardActionRule> rules)
    {
        var cardTypeIndex = cardKindStartIndex + (int)cardType;
        if (cardTypeIndex >= headers.Length) return;

        var cardTypeHeader = headers[cardTypeIndex];
        if (!recordDict.ContainsKey(cardTypeHeader))
        {
            _logger.LogWarning("Record does not contain '{CardTypeHeader}' column, skipping", cardTypeHeader);
            return;
        }

        var cardTypeValue = recordDict[cardTypeHeader]?.ToString() ?? string.Empty;
        var isCardTypeAllowed = cardTypeValue.Equals("TAK", StringComparison.OrdinalIgnoreCase);
        if (!isCardTypeAllowed) return;

        foreach (var cardStatus in Enum.GetValues<CardStatus>())
            ProcessCardStatus(headers, recordDict, actionName, cardType, cardStatus, cardStatusStartIndex, rules);
    }

    /// <summary>
    ///     Przetwarza pojedynczy status karty i tworzy regułę.
    /// </summary>
    private void ProcessCardStatus(
        string[] headers,
        IDictionary<string, object> recordDict,
        string actionName,
        CardType cardType,
        CardStatus cardStatus,
        int cardStatusStartIndex,
        List<CardActionRule> rules)
    {
        var cardStatusIndex = cardStatusStartIndex + (int)cardStatus;
        if (cardStatusIndex >= headers.Length) return;

        var cardStatusHeader = headers[cardStatusIndex];
        if (!recordDict.ContainsKey(cardStatusHeader))
        {
            _logger.LogWarning("Record does not contain '{CardStatusHeader}' column, skipping", cardStatusHeader);
            return;
        }

        var value = recordDict[cardStatusHeader]?.ToString() ?? string.Empty;
        var (isAllowed, requiresPinSet) = ParseRuleValue(value);

        rules.Add(new CardActionRule(
            actionName,
            cardType,
            cardStatus,
            isAllowed,
            requiresPinSet));

        _logger.LogDebug(
            "Loaded rule: {ActionName} -> Type: {CardType}, Status: {CardStatus}, Allowed: {IsAllowed}, RequiresPin: {RequiresPinSet}",
            actionName, cardType, cardStatus, isAllowed, requiresPinSet);
    }

    /// <summary>
    ///     Parsuje wartość z pliku CSV i określa, czy akcja jest dozwolona i czy wymaga ustawionego PIN-u.
    /// </summary>
    /// <param name="value">Wartość z pliku CSV</param>
    /// <returns>Tuple zawierający informację, czy akcja jest dozwolona i czy wymaga ustawionego PIN-u</returns>
    public static (bool isAllowed, bool? requiresPinSet) ParseRuleValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("NIE", StringComparison.OrdinalIgnoreCase))
            return (false, null);

        if (value.Equals("TAK", StringComparison.OrdinalIgnoreCase))
            return (true, null);

        if (value.Contains("ale jak nie ma pin to NIE", StringComparison.OrdinalIgnoreCase))
            return (true, true);

        if (value.Contains("jeżeli pin nadany", StringComparison.OrdinalIgnoreCase))
            return (true, true);

        if (value.Contains("jeżeli brak pin", StringComparison.OrdinalIgnoreCase))
            return (true, false);

        return (true, null);
    }
}