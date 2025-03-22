using System;
using System.Threading.Tasks;
using CardActions.Domain.Enums;
using CardActions.Domain.Factories;
using CardActions.Domain.Strategies.Base;

namespace CardActions.Domain.Models;

/// <summary>
///     Reprezentuje akcję, która może być wykonana na karcie płatniczej.
///     Klasa ta implementuje kombinację wzorców projektowych:
///     - Value Object: Obiekt wartościowy, którego tożsamość określona jest przez jego wartość (Name)
///     - Strategy: Deleguje sprawdzanie dostępności do klasy strategii
///     - Factory Method: Korzysta z fabryki do tworzenia odpowiedniej strategii dostępności
///     Zalety:
///     - Oddzielenie logiki sprawdzania dostępności od samej akcji
///     - Możliwość łatwej zmiany logiki dostępności bez modyfikacji kodu akcji
///     - Ponowne użycie strategii dla różnych akcji o podobnej logice dostępności
/// </summary>
public class CardAction
{
    private readonly ICardActionAvailabilityStrategy _availabilityStrategy;

    /// <summary>
    ///     Nazwa akcji, która jednoznacznie ją identyfikuje.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    ///     Nazwa wyświetlana akcji
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    ///     Czy akcja jest dostępna
    /// </summary>
    public bool IsAvailable { get; protected set; }

    /// <summary>
    ///     Inicjalizuje nową instancję klasy <see cref="CardAction" />.
    /// </summary>
    /// <param name="name">Nazwa akcji</param>
    /// <param name="displayName">Nazwa wyświetlana akcji</param>
    /// <param name="availabilityStrategy">Strategia sprawdzania dostępności</param>
    /// <exception cref="ArgumentNullException">Rzucany, gdy nazwa lub strategia jest null</exception>
    public CardAction(string name, string displayName, ICardActionAvailabilityStrategy availabilityStrategy)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DisplayName = displayName ?? name;
        _availabilityStrategy = availabilityStrategy ?? throw new ArgumentNullException(nameof(availabilityStrategy));
        IsAvailable = false;
    }

    /// <summary>
    ///     Metoda fabrykująca tworząca nowy obiekt CardAction.
    /// </summary>
    /// <param name="name">Nazwa akcji</param>
    /// <param name="displayName">Opcjonalna nazwa wyświetlana akcji</param>
    /// <returns>Nowy obiekt CardAction</returns>
    /// <exception cref="ArgumentException">Rzucany, gdy nazwa jest pusta lub składa się tylko z białych znaków</exception>
    public static CardAction Create(string name, string displayName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nazwa akcji nie może być pusta", nameof(name));

        // Użycie fabryki do pobrania odpowiedniej strategii dla danej akcji
        var strategy = CardActionAvailabilityStrategyFactory.GetStrategy(name);
        
        return new CardAction(name, displayName, strategy);
    }

    /// <summary>
    ///     Sprawdza czy akcja jest dostępna dla podanych parametrów karty
    /// </summary>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <returns>Dostępność akcji</returns>
    public virtual bool CheckAvailability(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        // Delegowanie sprawdzania dostępności do strategii
        IsAvailable = _availabilityStrategy.IsAvailable(cardType, cardStatus, isPinSet);
        return IsAvailable;
    }

    /// <summary>
    ///     Wykonuje akcję asynchronicznie
    /// </summary>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <returns>Rezultat wykonania akcji</returns>
    public virtual async Task<CardActionResult> ExecuteAsync(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        if (!CheckAvailability(cardType, cardStatus, isPinSet))
        {
            return CardActionResult.ActionNotAvailable();
        }

        // Implementacja wykonania akcji
        await Task.Delay(30); // Symulacja operacji asynchronicznej

        return CardActionResult.SuccessResult($"Wykonano akcję {Name}");
    }
    
    /// <summary>
    ///     Wykonuje akcję z parametrami
    /// </summary>
    /// <param name="parameters">Parametry wykonania akcji</param>
    /// <returns>Rezultat wykonania akcji</returns>
    public virtual async Task<CardActionResult> ExecuteAsync(CardActionParameters parameters)
    {
        if (parameters == null)
        {
            return CardActionResult.Failure("Brak parametrów akcji");
        }
        
        return await ExecuteAsync(parameters.CardType, parameters.CardStatus, parameters.IsPinSet);
    }
    
    /// <summary>
    ///     Wykonuje akcję (dla zachowania wstecznej kompatybilności)
    /// </summary>
    /// <returns>Rezultat wykonania akcji</returns>
    public virtual async Task<CardActionResult> ExecuteAsync()
    {
        // Domyślnie wywołujemy nową wersję z parametrami dla karty typu Debit w statusie Active z ustawionym PIN
        return await ExecuteAsync(Enums.CardType.Debit, Enums.CardStatus.Active, true);
    }

    /// <summary>
    ///     Porównuje ten obiekt z innym obiektem.
    ///     Dwa obiekty CardAction są równe, jeśli mają taką samą nazwę (ignorując wielkość liter).
    /// </summary>
    /// <param name="obj">Obiekt do porównania</param>
    /// <returns>True, jeśli obiekty są równe; w przeciwnym razie false</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not CardAction other)
            return false;

        return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Zwraca kod hash dla tego obiektu.
    ///     Dwa równe obiekty CardAction mają taki sam kod hash.
    /// </summary>
    /// <returns>Kod hash</returns>
    public override int GetHashCode()
    {
        return Name.ToLowerInvariant().GetHashCode();
    }

    /// <summary>
    ///     Zwraca tekstową reprezentację tego obiektu.
    /// </summary>
    /// <returns>Nazwa akcji</returns>
    public override string ToString()
    {
        return Name;
    }
    
    /// <summary>
    ///     Zwraca informację o akcji
    /// </summary>
    /// <returns>Informacja o akcji</returns>
    public ActionInfo GetInfo()
    {
        return new ActionInfo
        {
            Name = Name,
            DisplayName = DisplayName,
            IsAvailable = IsAvailable
        };
    }
}