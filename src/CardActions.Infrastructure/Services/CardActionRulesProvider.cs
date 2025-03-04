using CardActions.Application.Services;
using CardActions.Domain.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using CardActions.Infrastructure.Data.Entities;

namespace CardActions.Infrastructure.Services;

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

    public IReadOnlyList<string> GetAllowedActions(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        var filteredActions = _rules
            .Where(r => r.CardType == cardType
                    && r.CardStatus == cardStatus
                    && r.IsAllowed
                    && (!r.RequiresPinSet.HasValue || r.RequiresPinSet.Value == isPinSet))
            .Select(r => r.ActionName)
            .Distinct()
            .ToList();

        _logger.LogDebug("Final allowed actions for CardType: {CardType}, CardStatus: {CardStatus}, IsPinSet: {IsPinSet}: [{Actions}]",
            cardType, cardStatus, isPinSet, string.Join(", ", filteredActions));

        return filteredActions;
    }

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

                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                reader.DiscardBufferedData();
                var headers = reader.ReadLine()?.Split(',');

                if (headers == null || headers.Length < 3)
                {
                    throw new InvalidOperationException("Invalid CSV structure: Missing headers.");
                }

                int cardKindStartIndex = 1;
                int cardStatusStartIndex = Array.IndexOf(headers, "ORDERED");

                if (cardStatusStartIndex < 0)
                {
                    throw new InvalidOperationException("Invalid CSV structure: Missing CARD STATUS section.");
                }

                foreach (var record in records)
                {
                    var recordDict = (IDictionary<string, object>)record;
                    string actionName = recordDict["ALLOWED ACTION"]?.ToString() ?? string.Empty;

                    foreach (var cardType in Enum.GetValues<CardType>())
                    {
                        var cardTypeIndex = cardKindStartIndex + (int)cardType;
                        if (cardTypeIndex >= headers.Length) continue;

                        string cardTypeValue = recordDict[headers[cardTypeIndex]]?.ToString() ?? string.Empty;
                        bool isCardTypeAllowed = cardTypeValue.Equals("TAK", StringComparison.OrdinalIgnoreCase);
                        if (!isCardTypeAllowed) continue;

                        foreach (var cardStatus in Enum.GetValues<CardStatus>())
                        {
                            var cardStatusIndex = cardStatusStartIndex + (int)cardStatus;
                            if (cardStatusIndex >= headers.Length) continue;

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

                            _logger.LogDebug("Loaded rule: {ActionName} -> Type: {CardType}, Status: {CardStatus}, Allowed: {IsAllowed}, RequiresPin: {RequiresPinSet}",
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

    internal static (bool isAllowed, bool? requiresPinSet) ParseRuleValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("NIE", StringComparison.OrdinalIgnoreCase))
            return (false, null);

        if (value.Contains("pin nadany", StringComparison.OrdinalIgnoreCase))
            return (true, true);

        if (value.Contains("brak pin", StringComparison.OrdinalIgnoreCase))
            return (true, false);

        if (value.Contains("ale jak nie ma pin to NIE", StringComparison.OrdinalIgnoreCase))
            return (false, false);

        return (value.StartsWith("TAK", StringComparison.OrdinalIgnoreCase), null);
    }

    private void PrintLoadedRules()
    {
        foreach (var rule in _rules)
        {
            _logger.LogDebug($"Action: {rule.ActionName}, CardType: {rule.CardType}, CardStatus: {rule.CardStatus}, Allowed: {rule.IsAllowed}, RequiresPin: {rule.RequiresPinSet}");
        }
    }
}
