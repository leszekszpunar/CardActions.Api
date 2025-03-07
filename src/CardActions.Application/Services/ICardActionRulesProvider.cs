using System.Collections.Generic;
using CardActions.Domain.Policies;

namespace CardActions.Application.Services;

/// <summary>
/// Definiuje dostawcę reguł akcji karty.
/// </summary>
public interface ICardActionRulesProvider
{
    /// <summary>
    /// Pobiera wszystkie reguły akcji karty.
    /// </summary>
    /// <returns>Kolekcja reguł akcji karty</returns>
    IReadOnlyCollection<CardActions.Domain.Policies.CardActionRule> GetAllRules();
    
    /// <summary>
    /// Pobiera wszystkie nazwy akcji karty.
    /// </summary>
    /// <returns>Lista nazw akcji karty</returns>
    IReadOnlyList<string> GetAllActionNames();
} 