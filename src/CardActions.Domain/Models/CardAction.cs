namespace CardActions.Domain.Models;

/// <summary>
///     Reprezentuje akcję, która może być wykonana na karcie płatniczej.
///     Klasa ta implementuje wzorzec Value Object z DDD.
///     Wzorce projektowe:
///     - Value Object: Obiekt wartościowy, którego tożsamość określona jest przez jego wartość (Name),
///     a nie przez identyfikator. Dwa obiekty CardAction z tym samym Name są równe.
///     - Immutability: Klasa jest niemutowalna (ma tylko gettery, brak setterów), co zwiększa
///     bezpieczeństwo i przewidywalność w aplikacji.
///     - Factory Method: Metoda statyczna Create służy jako fabryka obiektów CardAction,
///     enkapsulując logikę tworzenia i walidacji.
///     Zalety:
///     - Niemutowalność zapobiega przypadkowym zmianom stanu
///     - Implementacja Equals i GetHashCode umożliwia poprawne porównywanie i używanie w kolekcjach
///     - Enkapsulacja logiki walidacji w metodzie fabrycznej
/// </summary>
public class CardAction
{
    /// <summary>
    ///     Prywatny konstruktor zgodny z wzorcem Value Object.
    ///     Obiekty tworzone są tylko przez metodę fabrykującą Create.
    /// </summary>
    /// <param name="name">Nazwa akcji</param>
    /// <exception cref="ArgumentNullException">Rzucany, gdy nazwa jest null</exception>
    private CardAction(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    ///     Nazwa akcji, która jednoznacznie ją identyfikuje.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Metoda fabrykująca tworząca nowy obiekt CardAction.
    /// </summary>
    /// <param name="name">Nazwa akcji</param>
    /// <returns>Nowy obiekt CardAction</returns>
    /// <exception cref="ArgumentException">Rzucany, gdy nazwa jest pusta lub składa się tylko z białych znaków</exception>
    public static CardAction Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nazwa akcji nie może być pusta", nameof(name));

        return new CardAction(name);
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
}