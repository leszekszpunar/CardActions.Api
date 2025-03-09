using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CardActions.Application.Services;
using CardActions.Domain.Models;
using CardActions.Domain.Policies;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace CardActions.Infrastructure.Services;

/// <summary>
/// Implementacja dostawcy reguł akcji karty, która wczytuje reguły z pliku CSV.
/// Klasa ta odpowiada za wczytywanie i parsowanie reguł z pliku CSV oraz dostarczanie
/// ich do warstwy domenowej.
/// 
/// Wzorce projektowe:
/// - Repository Pattern: Ta klasa działa jako repozytorium reguł akcji karty, 
///   oddzielając logikę dostępu do danych (z pliku CSV) od logiki biznesowej.
/// - Factory Pattern: Metoda LoadRulesFromCsv działa jako fabryka obiektów CardActionRule,
///   tworząc je na podstawie danych z pliku CSV.
/// - Adapter Pattern: Klasa adaptuje dane z pliku CSV do obiektów domenowych CardActionRule.
/// - Dependency Injection: Logger jest wstrzykiwany przez konstruktor, co ułatwia testowanie.
/// 
/// Zalety:
/// - Odseparowanie dostępu do danych od logiki biznesowej
/// - Elastyczność źródła danych (można łatwo zmienić na inny format pliku lub bazę danych)
/// - Centralne miejsce do wczytywania i zarządzania regułami
/// - Obsługa błędów i logowanie
/// 
/// Zasady SOLID:
/// - Single Responsibility: Klasa ma jedno zadanie - dostarczanie reguł
/// - Open/Closed: Można rozszerzyć funkcjonalność bez modyfikacji istniejącego kodu
/// - Liskov Substitution: Implementuje interfejs ICardActionRulesProvider
/// - Interface Segregation: Interfejs ICardActionRulesProvider jest mały i spójny
/// - Dependency Inversion: Zależność od logowania jest wstrzykiwana
/// </summary>
internal sealed class CardActionRulesProvider : ICardActionRulesProvider
{
    /// <summary>
    /// Kolekcja reguł akcji karty.
    /// </summary>
    private readonly IReadOnlyCollection<CardActions.Domain.Policies.CardActionRule> _rules;

    /// <summary>
    /// Lista wszystkich nazw akcji karty.
    /// </summary>
    private readonly IReadOnlyList<string> _allActionNames;

    /// <summary>
    /// Logger używany do logowania informacji, ostrzeżeń i błędów.
    /// </summary>
    private readonly ILogger<CardActionRulesProvider> _logger;

    /// <summary>
    /// Inicjalizuje nową instancję klasy <see cref="CardActionRulesProvider"/>.
    /// </summary>
    /// <param name="csvPath">Ścieżka do pliku CSV z regułami</param>
    /// <param name="logger">Logger używany do logowania</param>
    /// <exception cref="FileNotFoundException">Rzucany, gdy plik CSV nie istnieje</exception>
    public CardActionRulesProvider(string csvPath, ILogger<CardActionRulesProvider> logger)
    {
        _logger = logger;
        
        var absolutePath = Path.GetFullPath(csvPath);
        _logger.LogInformation("Loading card action rules from path: {CsvPath} (absolute: {AbsolutePath})", csvPath, absolutePath);
        
        if (!File.Exists(absolutePath))
        {
            _logger.LogError("CSV file not found at {CsvPath}", absolutePath);
            throw new FileNotFoundException($"CSV file not found at {absolutePath}", absolutePath);
        }
        
        _rules = LoadRulesFromCsv(csvPath);
        _allActionNames = _rules
            .Select(r => r.ActionName)
            .Distinct()
            .ToList();
        
        PrintLoadedRules();
    }

    /// <summary>
    /// Pobiera wszystkie reguły akcji karty.
    /// </summary>
    /// <returns>Kolekcja reguł akcji karty</returns>
    public IReadOnlyCollection<CardActions.Domain.Policies.CardActionRule> GetAllRules()
    {
        return _rules;
    }

    /// <summary>
    /// Pobiera wszystkie nazwy akcji karty.
    /// </summary>
    /// <returns>Lista nazw akcji karty</returns>
    public IReadOnlyList<string> GetAllActionNames()
    {
        return _allActionNames;
    }

    /// <summary>
    /// Wczytuje reguły akcji karty z pliku CSV.
    /// </summary>
    /// <param name="csvPath">Ścieżka do pliku CSV</param>
    /// <returns>Kolekcja reguł akcji karty</returns>
    /// <exception cref="InvalidOperationException">Rzucany, gdy nie udało się wczytać reguł z pliku CSV</exception>
    private IReadOnlyCollection<CardActions.Domain.Policies.CardActionRule> LoadRulesFromCsv(string csvPath)
    {
        try
        {
            var rules = new List<CardActions.Domain.Policies.CardActionRule>();
            
            using (var reader = new StreamReader(csvPath, Encoding.UTF8))
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
                
                // Znajdź indeksy kolumn
                var cardKindStartIndex = Array.FindIndex(headers, h => h == "PREPAID");
                var cardStatusStartIndex = Array.FindIndex(headers, h => h == "ORDERED");
                
                if (cardKindStartIndex == -1 || cardStatusStartIndex == -1)
                {
                    _logger.LogError("CSV file does not contain required headers: PREPAID or ORDERED");
                    throw new InvalidOperationException("CSV file does not contain required headers: PREPAID or ORDERED");
                }
                
                // Wczytaj rekordy
                while (csv.Read())
                {
                    var record = csv.GetRecord<dynamic>();
                    var recordDict = (IDictionary<string, object>)record;
                    
                    if (!recordDict.ContainsKey("ALLOWED ACTION"))
                    {
                        _logger.LogWarning("Record does not contain 'ALLOWED ACTION' column, skipping");
                        continue;
                    }
                    
                    string actionName = recordDict["ALLOWED ACTION"]?.ToString() ?? string.Empty;
                    
                    if (string.IsNullOrEmpty(actionName))
                    {
                        _logger.LogWarning("Record has empty action name, skipping");
                        continue;
                    }

                    foreach (var cardType in Enum.GetValues<CardType>())
                    {
                        var cardTypeIndex = cardKindStartIndex + (int)cardType;
                        if (cardTypeIndex >= headers.Length) continue;
                        
                        var cardTypeHeader = headers[cardTypeIndex];
                        if (!recordDict.ContainsKey(cardTypeHeader))
                        {
                            _logger.LogWarning("Record does not contain '{CardTypeHeader}' column, skipping", cardTypeHeader);
                            continue;
                        }

                        string cardTypeValue = recordDict[cardTypeHeader]?.ToString() ?? string.Empty;
                        bool isCardTypeAllowed = cardTypeValue.Equals("TAK", StringComparison.OrdinalIgnoreCase);
                        if (!isCardTypeAllowed) continue;

                        foreach (var cardStatus in Enum.GetValues<CardStatus>())
                        {
                            var cardStatusIndex = cardStatusStartIndex + (int)cardStatus;
                            if (cardStatusIndex >= headers.Length) continue;
                            
                            var cardStatusHeader = headers[cardStatusIndex];
                            if (!recordDict.ContainsKey(cardStatusHeader))
                            {
                                _logger.LogWarning("Record does not contain '{CardStatusHeader}' column, skipping", cardStatusHeader);
                                continue;
                            }

                            string value = recordDict[cardStatusHeader]?.ToString() ?? string.Empty;
                            var (isAllowed, requiresPinSet) = ParseRuleValue(value);

                            rules.Add(new CardActions.Domain.Policies.CardActionRule(
                                actionName,
                                cardType,
                                cardStatus,
                                isAllowed,
                                requiresPinSet));

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
            throw new InvalidOperationException($"Failed to load card action rules from {csvPath}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parsuje wartość z pliku CSV i określa, czy akcja jest dozwolona i czy wymaga ustawionego PIN-u.
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

    /// <summary>
    /// Wyświetla załadowane reguły w logach (tylko w trybie Debug).
    /// </summary>
    private void PrintLoadedRules()
    {
        foreach (var rule in _rules)
        {
            _logger.LogDebug($"Action: {rule.ActionName}, CardType: {rule.CardType}, CardStatus: {rule.CardStatus}, Allowed: {rule.IsAllowed}, RequiresPin: {rule.RequiresPinSet}");
        }
    }
}
