namespace CardActions.Application.DTOs;

/// <summary>
///     Obiekt transferu danych (DTO) zawierający informacje o dozwolonych akcjach dla karty.
/// </summary>
/// <param name="CardNumber">Numer karty.</param>
/// <param name="AllowedActions">Lista dozwolonych akcji dla karty.</param>
public sealed record CardActionsResponseDto(string CardNumber, IEnumerable<string> AllowedActions);