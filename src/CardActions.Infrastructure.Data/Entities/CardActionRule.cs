using CardActions.Domain.Enums;
using CardActions.Domain.Models;

namespace CardActions.Infrastructure.Data.Entities;

public class CardActionRule
{
    public int Id { get; set; }
    public required string ActionName { get; set; }
    public required CardType CardType { get; set; }
    public required CardStatus CardStatus { get; set; }
    public bool? RequiresPinSet { get; set; }
    public required bool IsAllowed { get; set; }

    public int CardActionId { get; set; }
    public CardAction CardAction { get; set; } = null!;
}