using System.Net;
using System.Net.Mime;
using FluentValidation;
using Reservation.UsersService.API.DTOs;
using Reservation.UsersService.BLL.Exceptions;

namespace Reservation.UsersService.API.Middleware;

public sealed class ExceptionHandlingMiddleware
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
            _logger.LogError(exception, "Exception thrown after the HTTP response started.");
            throw exception;
        }

        (HttpStatusCode statusCode, ApiErrorResponse errorResponse) = MapException(exception);

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled internal server error.");
        }
        else
        {
            _logger.LogWarning(exception, "Business or validation error: {Message}", exception.Message);
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
                    "Validation failed.",
                    GroupValidationErrors(validationException),
                    nameof(ValidationException))),

            InvalidCredentialsException invalidCredentialsException => (
                HttpStatusCode.Unauthorized,
                new ApiErrorResponse(invalidCredentialsException.Message, Type: nameof(InvalidCredentialsException))),

            ConflictException conflictException => (
                HttpStatusCode.Conflict,
                new ApiErrorResponse(conflictException.Message, Type: nameof(ConflictException))),

            NotFoundException notFoundException => (
                HttpStatusCode.NotFound,
                new ApiErrorResponse(notFoundException.Message, Type: nameof(NotFoundException))),

            BusinessException businessException => (
                HttpStatusCode.BadRequest,
                new ApiErrorResponse(businessException.Message, Type: nameof(BusinessException))),

            _ => (
                HttpStatusCode.InternalServerError,
                new ApiErrorResponse(
                    _environment.IsDevelopment()
                        ? exception.Message
                        : "An internal error occurred. Please try again later.",
                    Type: exception.GetType().Name))
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
