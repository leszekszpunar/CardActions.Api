using System.Collections.ObjectModel;
using CardActions.Domain.Enums;

namespace CardActions.Domain.Models;

/// <summary>
///     Reprezentuje kartę płatniczą jako główny obiekt domenowy (potencjalny Aggregate Root).
///     UWAGA: Ta klasa jest przykładem/dokumentacją pokazującą, jak w przyszłości
///     powinien wyglądać model domeny przy rozszerzaniu funkcjonalności systemu.
///     Obecnie system jest read-only, ale w przyszłości może obsługiwać:
///     - Zmianę statusu karty
///     - Ustawianie/zmianę PIN-u
///     - Blokowanie/odblokowywanie karty
///     - Śledzenie historii zmian
///     Wzorce DDD, które będą potrzebne:
///     - Aggregate Root: Card jako główny punkt dostępu do powiązanych obiektów
///     - Entity: Karta ma swoją tożsamość (CardNumber) niezależną od innych atrybutów
///     - Domain Events: Zdarzenia domenowe przy zmianach stanu
///     - Invariants: Reguły biznesowe zapewniające spójność danych
/// </summary>
public class Card
{
    // Private backing fields dla enkapsulacji
    private readonly List<CardAction> _allowedActions;
    private readonly List<string> _statusHistory;

    /// <summary>
    ///     Tworzy nową instancję karty.
    ///     W przyszłości: Fabryka może być odpowiedzialna za tworzenie kart.
    /// </summary>
    private Card(string cardNumber, string userId, CardType type)
    {
        CardNumber = cardNumber;
        UserId = userId;
        Type = type;
        Status = CardStatus.Inactive;
        IsPinSet = false;
        _allowedActions = new List<CardAction>();
        _statusHistory = new List<string> { $"Card created with status {Status}" };
    }

    /// <summary>
    ///     Unikalny numer karty, który identyfikuje ją w systemie.
    /// </summary>
    public string CardNumber { get; }

    /// <summary>
    ///     Identyfikator właściciela karty.
    /// </summary>
    public string UserId { get; }

    /// <summary>
    ///     Typ karty, niezmienny przez cały cykl życia.
    /// </summary>
    public CardType Type { get; }

    /// <summary>
    ///     Aktualny status karty (tylko do odczytu, zmiana przez dedykowane metody).
    /// </summary>
    public CardStatus Status { get; private set; }

    /// <summary>
    ///     Czy PIN jest ustawiony (tylko do odczytu, zmiana przez dedykowane metody).
    /// </summary>
    public bool IsPinSet { get; private set; }

    /// <summary>
    ///     Lista dozwolonych akcji (tylko do odczytu).
    /// </summary>
    public ReadOnlyCollection<CardAction> AllowedActions => _allowedActions.AsReadOnly();

    /// <summary>
    ///     Historia zmian statusów karty (tylko do odczytu).
    /// </summary>
    public ReadOnlyCollection<string> StatusHistory => _statusHistory.AsReadOnly();

    /// <summary>
    ///     Zmienia status karty.
    ///     W przyszłości: Będzie publikować event CardStatusChanged.
    /// </summary>
    /// <param name="newStatus">Nowy status karty</param>
    /// <param name="reason">Powód zmiany statusu</param>
    public void ChangeStatus(CardStatus newStatus, string reason)
    {
        // TODO: Implementacja w przyszłości
        // 1. Walidacja zmiany statusu
        // 2. Aktualizacja statusu
        // 3. Dodanie wpisu do historii
        // 4. Publikacja eventu CardStatusChanged
        // 5. Aktualizacja dozwolonych akcji
        throw new NotImplementedException("Feature planned for future implementation");
    }

    /// <summary>
    ///     Ustawia PIN dla karty.
    ///     W przyszłości: Będzie publikować event PinChanged.
    /// </summary>
    /// <param name="pin">Nowy PIN</param>
    public void SetPin(string pin)
    {
        // TODO: Implementacja w przyszłości
        // 1. Walidacja PIN-u
        // 2. Szyfrowanie PIN-u
        // 3. Aktualizacja flagi _isPinSet
        // 4. Publikacja eventu PinChanged
        // 5. Aktualizacja dozwolonych akcji
        throw new NotImplementedException("Feature planned for future implementation");
    }

    /// <summary>
    ///     Tworzy instancję karty z istniejących danych (np. z bazy).
    ///     Ta metoda jest używana w obecnym read-only systemie.
    /// </summary>
    public static Card FromExisting(
        string cardNumber,
        string userId,
        CardType type,
        CardStatus status,
        bool isPinSet,
        IEnumerable<CardAction> allowedActions)
    {
        var card = new Card(cardNumber, userId, type);
        card.Status = status;
        card.IsPinSet = isPinSet;
        card._allowedActions.AddRange(allowedActions);
        return card;
    }

    /// <summary>
    ///     Sprawdza, czy dana akcja jest dozwolona dla karty.
    /// </summary>
    /// <param name="actionName">Nazwa akcji do sprawdzenia</param>
    /// <returns>True jeśli akcja jest dozwolona, false w przeciwnym razie</returns>
    public bool CanExecuteAction(string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
            return false;

        return _allowedActions.Any(a => a.Name.Equals(actionName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     Aktualizuje listę dozwolonych akcji na podstawie aktualnego stanu karty.
    ///     W przyszłości: Będzie wywoływana automatycznie przy zmianach stanu.
    /// </summary>
    private void UpdateAllowedActions()
    {
        // TODO: Implementacja w przyszłości
        // 1. Pobranie reguł dla typu karty
        // 2. Filtrowanie po statusie
        // 3. Uwzględnienie ustawienia PIN-u
        // 4. Aktualizacja _allowedActions
        throw new NotImplementedException("Feature planned for future implementation");
    }

    /// <summary>
    ///     Fabryka do tworzenia nowych kart.
    ///     W przyszłości: Może być przeniesiona do osobnej klasy CardFactory.
    /// </summary>
    public static Card Create(string cardNumber, string userId, CardType type)
    {
        // TODO: Implementacja w przyszłości
        // 1. Walidacja parametrów
        // 2. Tworzenie nowej karty
        // 3. Publikacja eventu CardCreated
        throw new NotImplementedException("Feature planned for future implementation");
    }
}