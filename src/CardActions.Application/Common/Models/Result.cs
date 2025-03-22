using System.Net;

namespace CardActions.Application.Common.Models;

/// <summary>
/// Klasa reprezentująca wynik operacji biznesowej
/// </summary>
/// <typeparam name="T">Typ danych zwracanych przez operację</typeparam>
public class Result<T>
{
    /// <summary>
    /// Informacja czy operacja zakończyła się pomyślnie
    /// </summary>
    public bool IsSuccess { get; private set; }
    
    /// <summary>
    /// Dane zwrócone przez operację (tylko dla pomyślnego rezultatu)
    /// </summary>
    public T? Data { get; private set; }
    
    /// <summary>
    /// Kod statusu HTTP dla odpowiedzi
    /// </summary>
    public HttpStatusCode StatusCode { get; private set; }
    
    /// <summary>
    /// Komunikat błędu (tylko dla niepomyślnego rezultatu)
    /// </summary>
    public string? ErrorMessage { get; private set; }
    
    /// <summary>
    /// Lista błędów walidacji (tylko dla niepomyślnego rezultatu)
    /// </summary>
    public Dictionary<string, List<string>>? ValidationErrors { get; private set; }

    private Result(bool isSuccess, HttpStatusCode statusCode, T? data, string? errorMessage, Dictionary<string, List<string>>? validationErrors)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Data = data;
        ErrorMessage = errorMessage;
        ValidationErrors = validationErrors;
    }

    /// <summary>
    /// Tworzy pomyślny wynik operacji
    /// </summary>
    public static Result<T> Success(T data) => 
        new(true, HttpStatusCode.OK, data, null, null);
        
    /// <summary>
    /// Tworzy pomyślny wynik operacji z określonym kodem statusu HTTP
    /// </summary>
    public static Result<T> Success(T data, HttpStatusCode statusCode) => 
        new(true, statusCode, data, null, null);

    /// <summary>
    /// Tworzy pomyślny wynik operacji Created (201)
    /// </summary>
    public static Result<T> Created(T data) => 
        new(true, HttpStatusCode.Created, data, null, null);
        
    /// <summary>
    /// Tworzy pomyślny wynik operacji NoContent (204)
    /// </summary>
    public static Result<T> NoContent() => 
        new(true, HttpStatusCode.NoContent, default, null, null);

    /// <summary>
    /// Tworzy niepomyślny wynik operacji - błąd biznesowy
    /// </summary>
    public static Result<T> Failure(HttpStatusCode statusCode, string errorMessage) => 
        new(false, statusCode, default, errorMessage, null);
        
    /// <summary>
    /// Tworzy niepomyślny wynik operacji - obiekt nie znaleziony
    /// </summary>
    public static Result<T> NotFound(string errorMessage) => 
        Failure(HttpStatusCode.NotFound, errorMessage);
        
    /// <summary>
    /// Tworzy niepomyślny wynik operacji - operacja niedozwolona
    /// </summary>
    public static Result<T> Forbidden(string errorMessage) => 
        Failure(HttpStatusCode.Forbidden, errorMessage);
        
    /// <summary>
    /// Tworzy niepomyślny wynik operacji - nieautoryzowany dostęp
    /// </summary>
    public static Result<T> Unauthorized(string errorMessage) => 
        Failure(HttpStatusCode.Unauthorized, errorMessage);
        
    /// <summary>
    /// Tworzy niepomyślny wynik operacji - konflikt
    /// </summary>
    public static Result<T> Conflict(string errorMessage) => 
        Failure(HttpStatusCode.Conflict, errorMessage);

    /// <summary>
    /// Tworzy niepomyślny wynik operacji - błąd walidacji
    /// </summary>
    public static Result<T> ValidationFailure(Dictionary<string, List<string>> validationErrors) => 
        new(false, HttpStatusCode.BadRequest, default, "One or more validation errors occurred", validationErrors);
        
    /// <summary>
    /// Tworzy niepomyślny wynik operacji - błąd walidacji z pojedynczym komunikatem
    /// </summary>
    public static Result<T> ValidationFailure(string propertyName, string errorMessage)
    {
        var errors = new Dictionary<string, List<string>>
        {
            { propertyName, new List<string> { errorMessage } }
        };
        
        return ValidationFailure(errors);
    }
} 