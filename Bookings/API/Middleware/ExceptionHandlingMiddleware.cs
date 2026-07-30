using System.Net;
using System.Net.Mime;
using FluentValidation;
using Reservation.BookingsService.BLL.DTO;
using Reservation.BookingsService.BLL.Exceptions;

namespace Reservation.BookingsService.API.Middleware;

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

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(httpContext, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        if (httpContext.Response.HasStarted)
        {
            _logger.LogError(exception, "Exceção após o início da resposta HTTP.");
            throw exception;
        }

        (HttpStatusCode statusCode, ApiErrorResponse errorResponse) = MapException(exception);

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Erro interno não tratado.");
        }
        else
        {
            _logger.LogWarning(exception, "Erro tratado: {Message}", exception.Message);
        }

        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        httpContext.Response.StatusCode = (int)statusCode;
        await httpContext.Response.WriteAsJsonAsync(errorResponse);
    }

    private (HttpStatusCode StatusCode, ApiErrorResponse Response) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                new ApiErrorResponse(
                    "validation_error",
                    "Validação falhou.",
                    GroupValidationErrors(validationException))
            ),

            InvalidReferenceException invalidReferenceException => (
                HttpStatusCode.BadRequest,
                new ApiErrorResponse("invalid_reference", invalidReferenceException.Message)
            ),

            BookingConflictException bookingConflictException => (
                HttpStatusCode.Conflict,
                new ApiErrorResponse("booking_conflict", bookingConflictException.Message)
            ),

            NotFoundException notFoundException => (
                HttpStatusCode.NotFound,
                new ApiErrorResponse("not_found", notFoundException.Message)
            ),

            ExternalServiceUnavailableException externalServiceException => (
                HttpStatusCode.ServiceUnavailable,
                new ApiErrorResponse("service_unavailable", externalServiceException.Message)
            ),

            BusinessException businessException => (
                HttpStatusCode.BadRequest,
                new ApiErrorResponse("business_error", businessException.Message)
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                new ApiErrorResponse(
                    "internal_error",
                    _environment.IsDevelopment()
                        ? exception.Message
                        : "Ocorreu um erro interno. Tente novamente mais tarde.")
            )
        };
    }

    private static IReadOnlyDictionary<string, string[]> GroupValidationErrors(ValidationException validationException)
    {
        return validationException.Errors
            .GroupBy(error => string.IsNullOrWhiteSpace(error.PropertyName) ? "Request" : error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).Distinct().ToArray());
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
