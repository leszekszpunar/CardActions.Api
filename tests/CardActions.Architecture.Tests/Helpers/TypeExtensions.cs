using System.Reflection;

namespace CardActions.Architecture.Tests.Helpers;

/// <summary>
/// Metody rozszerzające dla typu Type
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Sprawdza, czy typ jest rekordem
    /// </summary>
    /// <param name="type">Typ do sprawdzenia</param>
    /// <returns>True, jeśli typ jest rekordem, w przeciwnym razie false</returns>
    public static bool IsRecord(this Type type)
    {
        // Sprawdź, czy typ ma atrybut CompilerGeneratedAttribute i EqualityContract
        return type.GetMethod("<Clone>$", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null
            || type.GetMethod("op_Equality", BindingFlags.Static | BindingFlags.Public) != null
            || type.GetMethod("get_EqualityContract", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
    }
} 