using System.Net;
using System.Text.Json;
using GestionProyecto.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GestionProyecto.Api.Middleware;

/// <summary>
/// Middleware global para traducir excepciones de dominio y aplicación en respuestas ProblemDetails.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            NotFoundException => (
                HttpStatusCode.NotFound,
                "Recurso no encontrado",
                exception.Message),
            BusinessRuleException => (
                HttpStatusCode.Conflict,
                "Conflicto de regla de negocio",
                exception.Message),
            ArgumentException or InvalidOperationException => (
                HttpStatusCode.BadRequest,
                "Solicitud inválida",
                exception.Message),
            _ => (
                HttpStatusCode.InternalServerError,
                "Error interno del servidor",
                _environment.IsDevelopment()
                    ? exception.Message
                    : "Ocurrió un error inesperado al procesar la solicitud.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Excepción no controlada al procesar {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning("Error controlado ({StatusCode}): {Message}",
                (int)statusCode, exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }
}
