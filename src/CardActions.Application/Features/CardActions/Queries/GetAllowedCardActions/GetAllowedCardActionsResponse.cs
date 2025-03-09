namespace CardActions.Application.Features.CardActions.Queries.GetAllowedCardActions;

/// <summary>
///     Reprezentuje odpowiedź zawierającą dozwolone akcje dla karty.
/// </summary>
public sealed record GetAllowedCardActionsResponse
{
    /// <summary>
    ///     Inicjalizuje nową instancję klasy <see cref="GetAllowedCardActionsResponse" />.
    /// </summary>
    /// <param name="allowedActions">Lista dozwolonych akcji</param>
    public GetAllowedCardActionsResponse(IReadOnlyList<string> allowedActions)
    {
        AllowedActions = allowedActions ?? throw new ArgumentNullException(nameof(allowedActions));
    }

    /// <summary>
    ///     Lista dozwolonych akcji dla karty.
    /// </summary>
    public IReadOnlyList<string> AllowedActions { get; }
}