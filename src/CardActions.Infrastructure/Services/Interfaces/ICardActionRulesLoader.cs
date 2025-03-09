using CardActions.Domain.Policies;

namespace CardActions.Infrastructure.Services.Interfaces;

/// <summary>
///     Definiuje interfejs dla wczytywaczy reguł akcji karty.
///     Ten interfejs odpowiada za wczytywanie reguł z zewnętrznego źródła danych.
/// </summary>
public interface ICardActionRulesLoader
{
    /// <summary>
    ///     Wczytuje reguły akcji karty z zewnętrznego źródła danych.
    /// </summary>
    /// <returns>Kolekcja reguł akcji karty</returns>
    IReadOnlyCollection<CardActionRule> LoadRules();
}