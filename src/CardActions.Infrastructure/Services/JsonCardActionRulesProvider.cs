using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CardActions.Application.Services;
using CardActions.Domain.Models;
using CardActions.Domain.Policies;
using Microsoft.Extensions.Logging;

namespace CardActions.Infrastructure.Services;

/// <summary>
/// Implementacja ICardActionRulesProvider, która wczytuje reguły z pliku JSON.
/// </summary>
internal sealed class JsonCardActionRulesProvider : ICardActionRulesProvider
{
    private readonly IReadOnlyCollection<CardActions.Domain.Policies.CardActionRule> _rules;
    private readonly IReadOnlyList<string> _allActionNames;
    private readonly ILogger<JsonCardActionRulesProvider> _logger;

    public JsonCardActionRulesProvider(string jsonPath, ILogger<JsonCardActionRulesProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        if (string.IsNullOrEmpty(jsonPath))
            throw new ArgumentException("Ścieżka do pliku JSON nie może być pusta", nameof(jsonPath));
        
        if (!File.Exists(jsonPath))
            throw new FileNotFoundException($"Nie znaleziono pliku JSON pod ścieżką: {jsonPath}", jsonPath);
        
        _rules = LoadRulesFromJson(jsonPath);
        _allActionNames = _rules
            .Select(r => r.ActionName)
            .Distinct()
            .ToList();
        
        _logger.LogInformation("Loaded {RuleCount} card action rules from JSON file", _rules.Count);
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
    /// Wczytuje reguły akcji karty z pliku JSON.
    /// </summary>
    /// <param name="jsonPath">Ścieżka do pliku JSON</param>
    /// <returns>Kolekcja reguł akcji karty</returns>
    /// <exception cref="InvalidOperationException">Rzucany, gdy nie udało się wczytać reguł z pliku JSON</exception>
    private IReadOnlyCollection<CardActions.Domain.Policies.CardActionRule> LoadRulesFromJson(string jsonPath)
    {
        try
        {
            var json = File.ReadAllText(jsonPath);
            var ruleDefinitions = JsonSerializer.Deserialize<List<CardActionRuleDefinition>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (ruleDefinitions == null || !ruleDefinitions.Any())
            {
                _logger.LogWarning("JSON file does not contain any card action rules");
                return new List<CardActions.Domain.Policies.CardActionRule>();
            }

            var rules = new List<CardActions.Domain.Policies.CardActionRule>();
            foreach (var definition in ruleDefinitions)
            {
                if (string.IsNullOrEmpty(definition.ActionName))
                {
                    _logger.LogWarning("Skipping rule definition with empty action name");
                    continue;
                }
                
                if (definition.CardTypes == null || !definition.CardTypes.Any())
                {
                    _logger.LogWarning("Skipping rule definition for action {ActionName} with no card types", definition.ActionName);
                    continue;
                }
                
                if (definition.CardStatuses == null || !definition.CardStatuses.Any())
                {
                    _logger.LogWarning("Skipping rule definition for action {ActionName} with no card statuses", definition.ActionName);
                    continue;
                }
                
                foreach (var cardType in definition.CardTypes)
                {
                    foreach (var cardStatus in definition.CardStatuses)
                    {
                        if (definition.PinDependency == PinDependency.None)
                        {
                            rules.Add(new CardActions.Domain.Policies.CardActionRule(
                                definition.ActionName,
                                cardType,
                                cardStatus,
                                true));
                        }
                        else if (definition.PinDependency == PinDependency.RequiresPin)
                        {
                            rules.Add(new CardActions.Domain.Policies.CardActionRule(
                                definition.ActionName,
                                cardType,
                                cardStatus,
                                true,
                                true));
                        }
                        else if (definition.PinDependency == PinDependency.RequiresNoPin)
                        {
                            rules.Add(new CardActions.Domain.Policies.CardActionRule(
                                definition.ActionName,
                                cardType,
                                cardStatus,
                                true,
                                false));
                        }
                    }
                }
            }

            _logger.LogInformation("Successfully loaded {RuleCount} card action rules from JSON file", rules.Count);
            return rules;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading card action rules from JSON file: {Message}", ex.Message);
            throw new InvalidOperationException($"Failed to load card action rules from JSON file: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Klasa pomocnicza do deserializacji reguł akcji z pliku JSON.
    /// </summary>
    private class CardActionRuleDefinition
    {
        public string ActionName { get; set; } = string.Empty;
        public List<CardType> CardTypes { get; set; } = new();
        public List<CardStatus> CardStatuses { get; set; } = new();
        public PinDependency PinDependency { get; set; } = PinDependency.None;
    }

    /// <summary>
    /// Enum określający zależność akcji od ustawienia PIN.
    /// </summary>
    private enum PinDependency
    {
        None,
        RequiresPin,
        RequiresNoPin
    }
} 