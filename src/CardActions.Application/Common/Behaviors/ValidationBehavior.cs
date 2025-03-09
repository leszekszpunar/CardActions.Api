using FluentValidation;
using MediatR;

namespace CardActions.Application.Common.Behaviors;

/// <summary>
///     Zachowanie potoku MediatR, które wykonuje walidację żądania przed jego obsługą.
///     Proces biznesowy:
///     1. Przygotowanie walidacji
///     - Zbiera wszystkie zarejestrowane walidatory dla danego typu żądania
///     2. Wykonanie walidacji
///     - Uruchamia wszystkie walidatory równolegle
///     - Zbiera wyniki walidacji
///     3. Obsługa wyników
///     - Jeśli są błędy, przerywa proces i zwraca informacje o błędach
///     - Jeśli nie ma błędów, przekazuje żądanie dalej w potoku
///     Implementuje wzorzec projektowy Decorator, który dodaje funkcjonalność walidacji
///     do istniejącego potoku obsługi żądań.
/// </summary>
/// <typeparam name="TRequest">Typ żądania</typeparam>
/// <typeparam name="TResponse">Typ odpowiedzi</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    ///     Inicjalizuje nową instancję klasy <see cref="ValidationBehavior{TRequest,TResponse}" />.
    /// </summary>
    /// <param name="validators">Kolekcja walidatorów dla danego typu żądania</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    ///     Obsługuje żądanie, wykonując walidację przed przekazaniem go do kolejnego handlera w potoku.
    ///     Proces walidacji:
    ///     1. Sprawdzenie czy istnieją walidatory dla żądania
    ///     2. Równoległe uruchomienie wszystkich walidatorów
    ///     3. Zebranie i analiza wyników walidacji
    ///     4. W przypadku błędów - przerwanie procesu
    ///     5. W przypadku sukcesu - przekazanie żądania dalej
    /// </summary>
    /// <param name="request">Żądanie do obsługi</param>
    /// <param name="next">Delegat do kolejnego handlera w potoku</param>
    /// <param name="cancellationToken">Token anulowania</param>
    /// <returns>Wynik obsługi żądania</returns>
    /// <exception cref="ValidationException">Rzucany, gdy żądanie nie przejdzie walidacji</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0) throw new ValidationException(failures);
        }

        return await next();
    }
}