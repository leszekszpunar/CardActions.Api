using CardActions.Domain.Models;

namespace CardActions.Application.Services;

public interface ICardActionRulesProvider
{
    IReadOnlyList<string> GetAllowedActions(CardType cardType, CardStatus cardStatus, bool isPinSet);
} 