using CardActions.Domain.Models;

namespace CardActions.Domain.Policies;

/// <summary>
///     Reprezentuje regułę określającą, czy dana akcja jest dozwolona dla określonego typu i statusu karty.
///     Implementuje wzorzec Value Object z DDD.
///     Wzorce projektowe:
///     - Value Object: Ta klasa jest niemodyfikowalna po utworzeniu, a jej tożsamość jest określona
///     przez wartości jej właściwości, a nie przez identyfikator.
///     - Immutability: Wszystkie właściwości są tylko do odczytu, co zapobiega niepożądanym modyfikacjom
///     i zwiększa bezpieczeństwo w środowisku wielowątkowym.
///     Zalety:
///     - Enkapsulacja reguły jako samodzielnego obiektu biznesowego
///     - Możliwość łatwego testowania każdej reguły osobno
///     - Niemutowalność zapobiega przypadkowym zmianom stanu
/// </summary>
public class CardActionRule
{
    /// <summary>
    ///     Inicjalizuje nową instancję klasy <see cref="CardActionRule" />.
    /// </summary>
    /// <param name="actionName">Nazwa akcji</param>
    /// <param name="cardType">Typ karty</param>
    /// <param name="cardStatus">Status karty</param>
    /// <param name="isAllowed">Czy akcja jest dozwolona</param>
    /// <param name="requiresPinSet">Czy akcja wymaga ustawionego PIN-u</param>
    /// <exception cref="ArgumentNullException">Rzucany, gdy actionName jest null</exception>
    public CardActionRule(string actionName, CardType cardType, CardStatus cardStatus, bool isAllowed,
        bool? requiresPinSet = null)
    {
        ActionName = actionName ?? throw new ArgumentNullException(nameof(actionName));
        CardType = cardType;
        CardStatus = cardStatus;
        IsAllowed = isAllowed;
        RequiresPinSet = requiresPinSet;
    }

    /// <summary>
    ///     Nazwa akcji, której dotyczy reguła.
    /// </summary>
    public string ActionName { get; }

    /// <summary>
    ///     Typ karty, dla którego obowiązuje reguła.
    /// </summary>
    public CardType CardType { get; }

    /// <summary>
    ///     Status karty, dla którego obowiązuje reguła.
    /// </summary>
    public CardStatus CardStatus { get; }

    /// <summary>
    ///     Określa, czy akcja jest dozwolona dla podanego typu i statusu karty.
    /// </summary>
    public bool IsAllowed { get; }

    /// <summary>
    ///     Określa, czy akcja wymaga ustawionego PIN-u.
    ///     Null oznacza, że akcja nie zależy od ustawienia PIN-u.
    ///     True oznacza, że akcja wymaga ustawionego PIN-u.
    ///     False oznacza, że akcja wymaga nieustawionego PIN-u.
    /// </summary>
    public bool? RequiresPinSet { get; }
}