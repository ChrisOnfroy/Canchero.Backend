using Application.Errors;
using Domain.Errors;
using Infrastructure.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FluentValidationException = FluentValidation.ValidationException;
using DomainValidationException = Domain.Errors.ValidationException;

namespace Presentation.Errors;

/// <summary>
/// Traduce las excepciones de dominio, aplicación e infraestructura a respuestas
/// <c>application/problem+json</c> con el código HTTP que corresponde.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            // El cliente cerró la conexión: no es un error del servidor.
            httpContext.Response.StatusCode = 499;
            return true;
        }

        var problem = Map(exception);
        problem.Instance = httpContext.Request.Path;
        httpContext.Response.StatusCode = problem.Status!.Value;

        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Error no controlado en {Method} {Path}.",
                httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation(
                "Solicitud rechazada ({Status}) en {Method} {Path}: {Message}",
                problem.Status, httpContext.Request.Method, httpContext.Request.Path, exception.Message);
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }

    private ProblemDetails Map(Exception exception)
    {
        switch (exception)
        {
            case FluentValidationException validation:
            {
                var problem = Create(StatusCodes.Status400BadRequest, "La solicitud no es válida.",
                    "Uno o más campos no cumplen las validaciones.");
                problem.Extensions["errors"] = validation.Errors
                    .GroupBy(failure => failure.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());
                return problem;
            }
            case DomainValidationException:
                return Create(StatusCodes.Status400BadRequest, "La solicitud no es válida.", exception.Message);
            case BusinessRuleException:
                return Create(StatusCodes.Status422UnprocessableEntity, "Regla de negocio incumplida.", exception.Message);
            case DomainException:
                return Create(StatusCodes.Status400BadRequest, "La solicitud no es válida.", exception.Message);
            case UnauthorizedException:
                return Create(StatusCodes.Status401Unauthorized, "No autenticado.", exception.Message);
            case ForbiddenException:
                return Create(StatusCodes.Status403Forbidden, "Acceso denegado.", exception.Message);
            case NotFoundException:
                return Create(StatusCodes.Status404NotFound, "Recurso no encontrado.", exception.Message);
            case ConflictException:
                return Create(StatusCodes.Status409Conflict, "Conflicto con el estado actual.", exception.Message);
            case GeocodingProviderException:
                return Create(StatusCodes.Status502BadGateway, "Servicio de geocodificación no disponible.",
                    "No fue posible resolver la ubicación en este momento. Inténtalo de nuevo o usa coordenadas.");
            default:
                return Create(StatusCodes.Status500InternalServerError, "Error interno del servidor.",
                    environment.IsDevelopment()
                        ? exception.ToString()
                        : "Ocurrió un error inesperado. Inténtalo de nuevo más tarde.");
        }
    }

    private static ProblemDetails Create(int status, string title, string detail) => new()
    {
        Status = status,
        Title = title,
        Detail = detail
    };
}
