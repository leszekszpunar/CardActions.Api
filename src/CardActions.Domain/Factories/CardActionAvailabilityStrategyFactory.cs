using CardActions.Domain.Enums;
using CardActions.Domain.Strategies;
using CardActions.Domain.Strategies.Base;

namespace CardActions.Domain.Factories;

/// <summary>
/// Fabryka tworząca odpowiednie strategie dostępności akcji kart na podstawie nazwy akcji
/// </summary>
public class CardActionAvailabilityStrategyFactory
{
    private static readonly Dictionary<string, ICardActionAvailabilityStrategy> _strategies = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Statyczny konstruktor inicjalizujący mapowanie nazw akcji na strategie
    /// </summary>
    static CardActionAvailabilityStrategyFactory()
    {
        // Rejestracja strategii dla poszczególnych akcji
        _strategies["ACTION1"] = new ActiveCardsOnlyStrategy();
        _strategies["ACTION2"] = new ActiveCreditWithPinStrategy();
        _strategies["ACTION3"] = new AlwaysAvailableStrategy();
        _strategies["ACTION4"] = new StandardCardTypeStrategy();
        _strategies["ACTION5"] = new CreditCardOnlyStrategy();
        _strategies["ACTION6"] = new ComplexPinDependentStrategy();
        _strategies["ACTION7"] = new ComplexPinDependentStrategy(); // Podobna logika do ACTION6
        _strategies["ACTION8"] = new BasicOperationalStatusStrategy();
        _strategies["ACTION9"] = new AlwaysAvailableStrategy();
        _strategies["ACTION10"] = new BasicOperationalStatusStrategy();
        _strategies["ACTION11"] = new InactiveAndActiveStrategy();
        _strategies["ACTION12"] = new BasicOperationalStatusStrategy();
        _strategies["ACTION13"] = new BasicOperationalStatusStrategy();
    }

    /// <summary>
    /// Zwraca odpowiednią strategię dla podanej nazwy akcji
    /// </summary>
    /// <param name="actionName">Nazwa akcji</param>
    /// <returns>Strategia dostępności</returns>
    public static ICardActionAvailabilityStrategy GetStrategy(string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
            throw new ArgumentException("Nazwa akcji nie może być pusta", nameof(actionName));

        if (_strategies.TryGetValue(actionName, out var strategy))
            return strategy;

        // Domyślna strategia - akcja nigdy nie jest dostępna
        return new DefaultUnavailableStrategy();
    }

    /// <summary>
    /// Domyślna strategia zwracająca zawsze false - akcja nigdy nie jest dostępna
    /// </summary>
    private class DefaultUnavailableStrategy : BaseCardActionAvailabilityStrategy
    {
        public override bool IsAvailable(CardType cardType, CardStatus cardStatus, bool isPinSet)
        {
            return false;
        }
    }
} 