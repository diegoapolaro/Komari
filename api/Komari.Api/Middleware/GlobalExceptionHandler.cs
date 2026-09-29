using FluentValidation;
using Komari.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Komari.Api.Middleware;

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
        _logger.LogError(exception, "Ocorreu um erro durante a requisição: {Message}", exception.Message);

        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                CreateValidationProblemDetails(httpContext, validationException)
            ),
            NotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Recurso não encontrado",
                    Detail = notFoundException.Message,
                    Instance = httpContext.Request.Path
                }
            ),
            ConflictException conflictException => (
                StatusCodes.Status409Conflict,
                new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflito de dados",
                    Detail = conflictException.Message,
                    Instance = httpContext.Request.Path
                }
            ),
            ArgumentException argumentException => (
                StatusCodes.Status400BadRequest,
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Requisição inválida",
                    Detail = argumentException.Message,
                    Instance = httpContext.Request.Path
                }
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Erro interno do servidor",
                    Detail = "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.",
                    Instance = httpContext.Request.Path
                }
            )
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateValidationProblemDetails(HttpContext context, ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Erro de validação",
            Detail = "Um ou mais campos contêm valores inválidos.",
            Instance = context.Request.Path
        };
    }
}
