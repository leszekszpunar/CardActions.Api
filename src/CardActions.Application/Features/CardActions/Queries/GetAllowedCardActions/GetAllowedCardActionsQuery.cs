using System.ComponentModel.DataAnnotations;
using CardActions.Application.Common.Models;
using MediatR;

namespace CardActions.Application.Features.CardActions.Queries.GetAllowedCardActions;

/// <summary>
///     Zapytanie o dozwolone akcje dla karty użytkownika.
/// </summary>
public record GetAllowedCardActionsQuery(
    [Required(ErrorMessage = "UserIdRequired")]
    [StringLength(50, ErrorMessage = "UserIdTooLong")]
    string UserId,
    
    [Required(ErrorMessage = "CardNumberRequired")]
    [StringLength(20, ErrorMessage = "CardNumberTooLong")]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "CardNumberInvalidFormat")]
    string CardNumber) : IRequest<Result<GetAllowedCardActionsResponse>>;