using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lab3_kpz.Core.WebApi.Exceptions;

/// <summary>
/// Глобальний обробник винятків (RFC 9457 ProblemDetails).
/// Перетворює відомі винятки предметної області на відповідні HTTP-коди,
/// а для непередбачених помилок (500) приховує внутрішні деталі в Production-середовищі.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment env,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            var validationProblem = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Instance = context.Request.Path
            };

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                Exception = exception,
                ProblemDetails = validationProblem
            });
        }

        var (status, title) = exception switch
        {
            ResourceNotFoundException =>
                (StatusCodes.Status404NotFound, "Resource Not Found"),
            ConflictException =>
                (StatusCodes.Status409Conflict, "Data Conflict"),
            DbUpdateConcurrencyException =>
                (StatusCodes.Status409Conflict, "Concurrency Conflict"),
            BusinessRuleException =>
                (StatusCodes.Status422UnprocessableEntity, "Business Rule Violation"),
            UnauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, "Unauthorized"),
            _ =>
                (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        if (status >= 500)
        {
            logger.LogError(exception, "Unhandled exception while executing request {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }

        // Для відомих клієнтських помилок (4xx) передаємо зрозуміле повідомлення предметної області,
        // а для 500 Internal Server Error текст винятку показуємо виключно в середовищі Development.
        var detail = status < 500
            ? exception.Message
            : (env.IsDevelopment() ? exception.Message : null);

        context.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails =
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            }
        });
    }
}

