using System.Net;
using System.Text.Json;
using CardActions.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CardActions.Api.Middleware;

/// <summary>
///     Globalny middleware obsługi wyjątków, zwracający `ProblemDetails`.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var problemDetails = new ProblemDetails
        {
            Title = "An error occurred",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        switch (exception)
        {
            case ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Status = response.StatusCode;
                problemDetails.Title = "Validation error";
                problemDetails.Extensions["errors"] = validationException.Errors
                    .ToDictionary(e => e.PropertyName, e => e.ErrorMessage);
                break;

            case NotFoundException notFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                problemDetails.Status = response.StatusCode;
                problemDetails.Title = notFoundException.Title;
                problemDetails.Detail = notFoundException.Message;
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                problemDetails.Status = response.StatusCode;
                problemDetails.Title = "Resource not found";
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                problemDetails.Status = response.StatusCode;
                problemDetails.Title = "Unauthorized";
                break;

            case NotImplementedException:
                response.StatusCode = (int)HttpStatusCode.NotImplemented;
                problemDetails.Status = response.StatusCode;
                problemDetails.Title = "Not Implemented";
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                problemDetails.Status = response.StatusCode;
                problemDetails.Title = "Internal Server Error";
                break;
        }

        return response.WriteAsync(JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}