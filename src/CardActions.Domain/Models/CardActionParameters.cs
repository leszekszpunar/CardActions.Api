using CardActions.Domain.Enums;
using System.Collections.Generic;

namespace CardActions.Domain.Models;

/// <summary>
/// Parametry wykonania akcji na karcie.
/// Klasa ta pełni rolę Data Transfer Object (DTO), przekazującego potrzebne parametry do wykonania akcji.
/// </summary>
public class CardActionParameters
{
    /// <summary>
    /// Typ karty
    /// </summary>
    public CardType CardType { get; set; }
    
    /// <summary>
    /// Status karty
    /// </summary>
    public CardStatus CardStatus { get; set; }
    
    /// <summary>
    /// Czy PIN jest ustawiony
    /// </summary>
    public bool IsPinSet { get; set; }
    
    /// <summary>
    /// Dodatkowe parametry w formacie klucz-wartość
    /// </summary>
    public Dictionary<string, object> AdditionalParams { get; set; } = new Dictionary<string, object>();
    
    /// <summary>
    /// Konstruktor domyślny
    /// </summary>
    public CardActionParameters()
    {
    }
    
    /// <summary>
    /// Konstruktor z podstawowymi parametrami
    /// </summary>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    public CardActionParameters(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        CardType = cardType;
        CardStatus = cardStatus;
        IsPinSet = isPinSet;
    }
    
    /// <summary>
    /// Dodaje dodatkowy parametr
    /// </summary>
    /// <param name="key">Klucz</param>
    /// <param name="value">Wartość</param>
    public void AddParameter(string key, object value)
    {
        AdditionalParams[key] = value;
    }
    
    /// <summary>
    /// Pobiera dodatkowy parametr
    /// </summary>
    /// <typeparam name="T">Typ parametru</typeparam>
    /// <param name="key">Klucz</param>
    /// <param name="defaultValue">Domyślna wartość, jeśli parametr nie istnieje</param>
    /// <returns>Wartość parametru lub wartość domyślna</returns>
    public T GetParameter<T>(string key, T defaultValue = default)
    {
        if (AdditionalParams.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        
        return defaultValue;
    }
} 