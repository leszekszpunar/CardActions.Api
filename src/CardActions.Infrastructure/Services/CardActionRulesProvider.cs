using CardActions.Application.Services;
using CardActions.Domain.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace CardActions.Infrastructure.Services;

/// <summary>
/// Provider odpowiedzialny za ładowanie reguł akcji kart z pliku CSV i dostarczanie dozwolonych akcji.
/// </summary>
internal sealed class CardActionRulesProvider : ICardActionRulesProvider
{
    private readonly IReadOnlyList<CardActionRule> _rules;
    private readonly ILogger<CardActionRulesProvider> _logger;

    public CardActionRulesProvider(string csvPath, ILogger<CardActionRulesProvider> logger)
    {
        _logger = logger;
        _rules = LoadRulesFromCsv(csvPath);
        PrintLoadedRules();
    }

    /// <summary>
    /// Pobiera listę dozwolonych akcji na podstawie typu karty, statusu i ustawienia PIN.
    /// </summary>
    public IReadOnlyList<string> GetAllowedActions(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        var filteredActions = _rules
            .Where(r => r.CardType == cardType
                    && r.CardStatus == cardStatus
                    && r.IsAllowed // Upewniamy się, że tylko dozwolone akcje przechodzą
                    && (!r.RequiresPinSet.HasValue || r.RequiresPinSet.Value == isPinSet))
            .Select(r => r.ActionName)
            .Distinct()
            .ToList();

        _logger.LogDebug("Returning actions: [{Actions}]", string.Join(", ", filteredActions));

        return filteredActions;
    }

    /// <summary>
    /// Ładuje reguły akcji z pliku CSV, rozpoznając dynamicznie CARD KIND i CARD STATUS.
    /// </summary>
    private IReadOnlyList<CardActionRule> LoadRulesFromCsv(string csvPath)
    {
        try
        {
            var absolutePath = Path.GetFullPath(csvPath);
            _logger.LogInformation("Loading card action rules from path: {CsvPath} (absolute: {AbsolutePath})", csvPath, absolutePath);

            if (!File.Exists(csvPath))
            {
                _logger.LogError("CSV file not found at {CsvPath}", absolutePath);
                throw new FileNotFoundException($"CSV file not found at {absolutePath}");
            }

            var rules = new List<CardActionRule>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                Encoding = Encoding.UTF8,
                HasHeaderRecord = true,
                MissingFieldFound = null,
                IgnoreBlankLines = true
            };

            using (var reader = new StreamReader(csvPath))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<dynamic>().ToList();

                // Odczytujemy nagłówki, aby poprawnie zmapować `CARD KIND` i `CARD STATUS`
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                reader.DiscardBufferedData();
                var headers = reader.ReadLine()?.Split(',');

                if (headers == null || headers.Length < 3)
                {
                    throw new InvalidOperationException("Invalid CSV structure: Missing headers.");
                }

                int cardKindStartIndex = 1; // Po "ALLOWED ACTION"
                int cardStatusStartIndex = Array.IndexOf(headers, "ORDERED"); // Znajdujemy pierwszy status

                if (cardStatusStartIndex < 0)
                {
                    throw new InvalidOperationException("Invalid CSV structure: Missing CARD STATUS section.");
                }

                foreach (var record in records)
                {
                    var recordDict = (IDictionary<string, object>)record;
                    string actionName = recordDict["ALLOWED ACTION"]?.ToString() ?? string.Empty;

                    // Pobieramy typy kart (PREPAID, DEBIT, CREDIT)
                    foreach (var cardType in Enum.GetValues<CardType>())
                    {
                        var cardTypeIndex = cardKindStartIndex + (int)cardType;

                        if (cardTypeIndex >= headers.Length)
                            continue;

                        string cardTypeValue = recordDict[headers[cardTypeIndex]]?.ToString() ?? string.Empty;
                        bool isCardTypeAllowed = cardTypeValue.Equals("TAK", StringComparison.OrdinalIgnoreCase);

                        if (!isCardTypeAllowed)
                            continue;

                        // Pobieramy statusy kart (ORDERED, INACTIVE, etc.)
                        foreach (var cardStatus in Enum.GetValues<CardStatus>())
                        {
                            var cardStatusIndex = cardStatusStartIndex + (int)cardStatus;

                            if (cardStatusIndex >= headers.Length)
                                continue;

                            string value = recordDict[headers[cardStatusIndex]]?.ToString() ?? string.Empty;
                            var (isAllowed, requiresPinSet) = ParseRuleValue(value);

                            rules.Add(new CardActionRule
                            {
                                ActionName = actionName,
                                CardType = cardType,
                                CardStatus = cardStatus,
                                IsAllowed = isAllowed,
                                RequiresPinSet = requiresPinSet
                            });

                            _logger.LogDebug("Imported rule: {ActionName} -> Type: {CardType}, Status: {CardStatus}, Allowed: {IsAllowed}, RequiresPin: {RequiresPinSet}",
                                actionName, cardType, cardStatus, isAllowed, requiresPinSet);
                        }
                    }
                }
            }

            _logger.LogInformation("Successfully loaded {RuleCount} card action rules", rules.Count);
            return rules;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading card action rules from {CsvPath}", csvPath);
            throw;
        }
    }

    /// <summary>
    /// Parsuje wartość reguły z pliku CSV do logicznych flag.
    /// </summary>
    private static (bool isAllowed, bool? requiresPinSet) ParseRuleValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("NIE", StringComparison.OrdinalIgnoreCase))
            return (false, null);

        if (value.Contains("pin nadany", StringComparison.OrdinalIgnoreCase))
            return (true, true);

        if (value.Contains("brak pin", StringComparison.OrdinalIgnoreCase))
            return (true, false);

        if (value.Contains("ale jak nie ma pin to NIE", StringComparison.OrdinalIgnoreCase))
            return (false, false); // Akcja dostępna tylko jeśli PIN jest ustawiony

        return (value.StartsWith("TAK", StringComparison.OrdinalIgnoreCase), null);
    }

    
    /// <summary>
    /// Wyświetla wczytane reguły akcji w konsoli.
    /// </summary>
    private void PrintLoadedRules()
    {
        foreach (var rule in _rules)
        {
            Console.WriteLine($"Action: {rule.ActionName}, CardType: {rule.CardType}, CardStatus: {rule.CardStatus}, " +
                              $"Allowed: {rule.IsAllowed}, RequiresPin: {rule.RequiresPinSet}");
        }
    }
}



/// <summary>
/// Reprezentuje pojedynczą regułę akcji karty.
/// </summary>
internal sealed record CardActionRule
{
    /// <summary>
    /// Nazwa akcji (np. ACTION1, ACTION2, ACTION6).
    /// </summary>
    public required string ActionName { get; init; }

    /// <summary>
    /// Typ karty (np. Prepaid, Debit, Credit).
    /// </summary>
    public required CardType CardType { get; init; }

    /// <summary>
    /// Status karty (np. Ordered, Active, Blocked).
    /// </summary>
    public required CardStatus CardStatus { get; init; }

    /// <summary>
    /// Określa, czy akcja jest dozwolona dla danej kombinacji karty i statusu.
    /// </summary>
    public required bool IsAllowed { get; init; }

    /// <summary>
    /// Określa, czy akcja wymaga ustawionego PIN-u.
    /// - `true` → wymagany PIN,
    /// - `false` → akcja dostępna tylko jeśli PIN nie jest ustawiony,
    /// - `null` → brak wymogu PIN.
    /// </summary>
    public bool? RequiresPinSet { get; init; }
}
