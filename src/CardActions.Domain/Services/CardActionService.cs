using System.Collections.Concurrent;
using CardActions.Domain.Enums;
using CardActions.Domain.Models;
using CardActions.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CardActions.Domain.Services;

/// <summary>
///     Serwis domenowy dostarczający funkcjonalności związane z akcjami karty.
///     Klasa ta implementuje kombinację wzorców projektowych:
///     - Service Layer: Działa jako fasada dla operacji domenowych związanych z akcjami karty
///     - Registry Pattern: Utrzymuje rejestr dostępnych akcji, które są ładowane na żądanie
///     - Proxy Pattern: Deleguje sprawdzanie dostępności do odpowiednich strategii poprzez obiekty CardAction
///     - Double-Check Locking: Zapewnia bezpieczne ładowanie akcji w środowisku wielowątkowym
///     Zalety:
///     - Dynamiczne ładowanie akcji tylko raz, przy pierwszym żądaniu
///     - Bezpieczna obsługa wielu równoległych żądań (thread-safe)
///     - Łatwe dodawanie nowych typów akcji bez modyfikacji kodu serwisu
///     - Oddzielenie logiki sprawdzania dostępności od mechanizmu pobierania akcji
/// </summary>
public class CardActionService : ICardActionService
{
    // Lista standardowych nazw akcji i ich nazw wyświetlanych
    private static readonly (string Name, string DisplayName)[] _standardActions = {
        ("ACTION1", "Akcja 1"),
        ("ACTION2", "Akcja 2"),
        ("ACTION3", "Akcja 3"),
        ("ACTION4", "Akcja 4"),
        ("ACTION5", "Akcja 5"),
        ("ACTION6", "Akcja 6"),
        ("ACTION7", "Akcja 7"),
        ("ACTION8", "Akcja 8"),
        ("ACTION9", "Akcja 9"),
        ("ACTION10", "Akcja 10"),
        ("ACTION11", "Akcja 11"),
        ("ACTION12", "Akcja 12"),
        ("ACTION13", "Akcja 13")
    };

    private readonly ILogger<CardActionService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ConcurrentDictionary<string, CardAction> _availableActions = new();
    private bool _actionsLoaded;

    /// <summary>
    ///     Inicjalizuje nową instancję klasy <see cref="CardActionService" />.
    /// </summary>
    /// <param name="logger">Logger</param>
    public CardActionService(ILogger<CardActionService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Pobiera listę wszystkich dostępnych akcji dla karty.
    /// </summary>
    /// <returns>Lista wszystkich możliwych akcji</returns>
    public IReadOnlyList<CardAction> GetAllActions()
    {
        EnsureActionsLoadedAsync().GetAwaiter().GetResult();
        return _availableActions.Values.ToList();
    }

    /// <summary>
    ///     Pobiera listę dozwolonych akcji dla karty o określonych parametrach.
    ///     Metoda asynchroniczna.
    /// </summary>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <returns>Lista dozwolonych akcji</returns>
    public async Task<List<CardAction>> GetAllowedActionsAsync(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        _logger.LogInformation("GetAllowedActionsAsync called for: Type={CardType}, Status={CardStatus}, IsPinSet={IsPinSet}",
            cardType, cardStatus, isPinSet);

        await EnsureActionsLoadedAsync();

        var allowedActions = new List<CardAction>();
        
        foreach (var action in _availableActions.Values)
        {
            if (action.CheckAvailability(cardType, cardStatus, isPinSet))
            {
                allowedActions.Add(action);
            }
        }
        
        _logger.LogInformation("GetAllowedActionsAsync found {Count} allowed actions", allowedActions.Count);
        return allowedActions;
    }

    /// <summary>
    ///     Pobiera listę dozwolonych akcji dla karty o określonych parametrach.
    ///     Metoda synchroniczna (dla kompatybilności wstecznej).
    /// </summary>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <returns>Lista dozwolonych akcji</returns>
    public List<CardAction> GetAllowedActions(Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        _logger.LogInformation("GetAllowedActions called for: Type={CardType}, Status={CardStatus}, IsPinSet={IsPinSet}",
            cardType, cardStatus, isPinSet);

        // Ponieważ metoda asynchroniczna jest główną implementacją,
        // wywołujemy ją asynchronicznie i blokujemy, aby uzyskać synchroniczny wynik.
        // Nie jest to idealne podejście, ale zapewnia spójność logiki.
        return GetAllowedActionsAsync(cardType, cardStatus, isPinSet).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Wykonuje akcję o podanej nazwie dla karty o podanych parametrach
    /// </summary>
    /// <param name="actionName">Nazwa akcji</param>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isPinSet">Czy PIN jest ustawiony</param>
    /// <returns>Rezultat wykonania akcji</returns>
    public async Task<CardActionResult> ExecuteActionAsync(string actionName, Enums.CardType cardType, Enums.CardStatus cardStatus, bool isPinSet)
    {
        await EnsureActionsLoadedAsync();
        
        if (!_availableActions.TryGetValue(actionName, out var action))
        {
            return CardActionResult.Failure($"Nie znaleziono akcji o nazwie {actionName}");
        }
        
        return await action.ExecuteAsync(cardType, cardStatus, isPinSet);
    }

    /// <summary>
    /// Upewnia się, że akcje zostały załadowane
    /// </summary>
    private async Task EnsureActionsLoadedAsync()
    {
        if (_actionsLoaded) return;
        
        await _semaphore.WaitAsync();
        try
        {
            if (_actionsLoaded) return;
            
            _logger.LogInformation("Ładowanie dostępnych akcji kart...");
            
            // Asynchroniczne ładowanie akcji
            await LoadActionsAsync();
            
            _actionsLoaded = true;
            _logger.LogInformation("Załadowano {Count} akcji kart", _availableActions.Count);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Ładuje dostępne akcje kart
    /// </summary>
    private async Task LoadActionsAsync()
    {
        try
        {
            // Symulacja długotrwałej operacji
            await Task.Delay(100);
            
            // Ładowanie wszystkich standardowych akcji z użyciem fabryki CardAction
            foreach (var (name, displayName) in _standardActions)
            {
                RegisterAction(CardAction.Create(name, displayName));
            }
            
            // W rzeczywistej implementacji można by ładować akcje dynamicznie z zewnętrznego źródła
            // np. bazy danych, plików konfiguracyjnych itp.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas ładowania akcji kart");
            throw;
        }
    }

    /// <summary>
    /// Rejestruje akcję
    /// </summary>
    /// <param name="action">Akcja</param>
    private void RegisterAction(CardAction action)
    {
        _availableActions[action.Name] = action;
        _logger.LogDebug("Zarejestrowano akcję: {ActionName} ({DisplayName})", action.Name, action.DisplayName);
    }
}