using System;

namespace CardActions.Domain.Models;

/// <summary>
/// Reprezentuje akcję, którą można wykonać na karcie płatniczej jako obiekt wartości (Value Object).
/// Implementuje wzorzec Value Object z DDD.
/// </summary>
public class CardAction
{
    /// <summary>
    /// Nazwa akcji, która jednoznacznie ją identyfikuje.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Prywatny konstruktor zgodny z wzorcem Value Object.
    /// Obiekty tworzone są tylko przez metodę fabrykującą Create.
    /// </summary>
    /// <param name="name">Nazwa akcji</param>
    /// <exception cref="ArgumentNullException">Rzucany, gdy nazwa jest null</exception>
    private CardAction(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Metoda fabrykująca tworząca nowy obiekt CardAction.
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
    /// Porównuje ten obiekt z innym obiektem.
    /// Dwa obiekty CardAction są równe, jeśli mają taką samą nazwę (ignorując wielkość liter).
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
    /// Zwraca kod hash dla tego obiektu.
    /// Dwa równe obiekty CardAction mają taki sam kod hash.
    /// </summary>
    /// <returns>Kod hash</returns>
    public override int GetHashCode()
    {
        return Name.ToLowerInvariant().GetHashCode();
    }

    /// <summary>
    /// Zwraca tekstową reprezentację tego obiektu.
    /// </summary>
    /// <returns>Nazwa akcji</returns>
    public override string ToString()
    {
        return Name;
    }
} 