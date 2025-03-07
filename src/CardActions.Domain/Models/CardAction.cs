using System;

namespace CardActions.Domain.Models;

/// <summary>
/// Reprezentuje akcję, którą można wykonać na karcie płatniczej jako obiekt wartości.
/// </summary>
public class CardAction
{
    public string Name { get; }

    private CardAction(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public static CardAction Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nazwa akcji nie może być pusta", nameof(name));

        return new CardAction(name);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CardAction other)
            return false;

        return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return Name.ToLowerInvariant().GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }
} 