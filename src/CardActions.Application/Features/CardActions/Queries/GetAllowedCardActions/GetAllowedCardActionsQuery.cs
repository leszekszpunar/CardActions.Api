using MediatR;

namespace CardActions.Application.Features.CardActions.Queries.GetAllowedCardActions;

public sealed record GetAllowedCardActionsQuery(string UserId, string CardNumber) : IRequest<GetAllowedCardActionsResponse>; 