using CardActions.Application.Common.Interfaces;
using FluentValidation;

namespace CardActions.Application.Features.CardActions.Queries.GetAllowedCardActions;

/// <summary>
///     Walidator zapytania o dozwolone akcje dla karty.
/// </summary>
public class GetAllowedCardActionsQueryValidator : AbstractValidator<GetAllowedCardActionsQuery>
{
    public GetAllowedCardActionsQueryValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(localizationService.GetString("Validation.UserIdRequired"))
            .MaximumLength(50)
            .WithMessage(localizationService.GetString("Validation.UserIdTooLong"));

        RuleFor(x => x.CardNumber)
            .NotEmpty()
            .WithMessage(localizationService.GetString("Validation.CardNumberRequired"))
            .MaximumLength(20)
            .WithMessage(localizationService.GetString("Validation.CardNumberTooLong"))
            .Matches("^[A-Za-z0-9]+$")
            .WithMessage(localizationService.GetString("Validation.CardNumberInvalidFormat"));
    }
}