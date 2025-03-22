using CardActions.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CardActions.Api.Extensions;

/// <summary>
/// Rozszerzenia dla obiektu Result, ułatwiające konwersję na ActionResult
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Konwertuje Result na ActionResult
    /// </summary>
    /// <typeparam name="T">Typ danych w Result</typeparam>
    /// <param name="result">Obiekt Result do konwersji</param>
    /// <param name="controller">Kontroler, z którego jest wywoływana metoda</param>
    /// <returns>ActionResult odpowiadający stanowi Result</returns>
    public static ActionResult<T> ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return result.StatusCode switch
            {
                System.Net.HttpStatusCode.Created => controller.Created(string.Empty, result.Data),
                System.Net.HttpStatusCode.NoContent => controller.NoContent(),
                _ => controller.Ok(result.Data)
            };
        }

        if (result.ValidationErrors != null)
        {
            var validationProblem = new ValidationProblemDetails();
            foreach (var error in result.ValidationErrors)
            {
                validationProblem.Errors[error.Key] = error.Value.ToArray();
            }
            
            validationProblem.Status = (int)result.StatusCode;
            validationProblem.Title = "Validation Failed";
            validationProblem.Detail = result.ErrorMessage ?? "One or more validation errors occurred";
            
            return controller.BadRequest(validationProblem);
        }

        var problemDetails = new ProblemDetails
        {
            Status = (int)result.StatusCode,
            Title = GetTitle(result.StatusCode),
            Detail = result.ErrorMessage
        };

        return result.StatusCode switch
        {
            System.Net.HttpStatusCode.NotFound => controller.NotFound(problemDetails),
            System.Net.HttpStatusCode.BadRequest => controller.BadRequest(problemDetails),
            System.Net.HttpStatusCode.Unauthorized => controller.Unauthorized(),
            System.Net.HttpStatusCode.Forbidden => controller.Forbid(),
            System.Net.HttpStatusCode.Conflict => controller.Conflict(problemDetails),
            _ => controller.StatusCode((int)result.StatusCode, problemDetails)
        };
    }

    /// <summary>
    /// Zwraca tytuł problemu na podstawie kodu HTTP
    /// </summary>
    private static string GetTitle(System.Net.HttpStatusCode statusCode) => statusCode switch
    {
        System.Net.HttpStatusCode.BadRequest => "Bad Request",
        System.Net.HttpStatusCode.Unauthorized => "Unauthorized",
        System.Net.HttpStatusCode.Forbidden => "Forbidden",
        System.Net.HttpStatusCode.NotFound => "Not Found",
        System.Net.HttpStatusCode.Conflict => "Conflict",
        System.Net.HttpStatusCode.InternalServerError => "Server Error",
        _ => "Error"
    };
} 