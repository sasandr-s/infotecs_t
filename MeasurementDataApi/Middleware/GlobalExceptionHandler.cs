using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MeasurementDataApi.Exceptions;
using MeasurementDataApi.Models.Dto;

namespace MeasurementDataApi.Middleware;

/// <summary>
/// Глобальный обработчик исключений (стандарт .NET 8+).
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Произошла необработанная ошибка: {Message}", exception.Message);

        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                new ValidationErrorDto { Errors = validationEx.Errors }),
            
            FileParsingException parsingEx => (
                StatusCodes.Status400BadRequest,
                new ValidationErrorDto { Errors = parsingEx.Errors }),
            
            _ => (
                StatusCodes.Status500InternalServerError,
                new ValidationErrorDto { Errors = new List<string> { $"Внутренняя ошибка сервера: {exception.Message}" } })
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
