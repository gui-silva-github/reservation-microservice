using System.Net;
using System.Net.Mime;
using FluentValidation;
using Reservation.NotificationsService.API.DTOs;
using Reservation.NotificationsService.BLL.Exceptions;

namespace Reservation.NotificationsService.API.Middleware
{
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
                _logger.LogError(exception, "Exceção lançada após o início da resposta HTTP.");
                throw exception;
            }

            (HttpStatusCode httpStatusCode, ApiErrorResponse errorResponse) = MapException(exception);

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(exception, "Erro interno não tratado.");
            }
            else
            {
                _logger.LogWarning(exception, "Erro de negócio ou validação: {Message}", exception.Message);
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
                        "Validação falhou.",
                        GroupValidationErrors(validationException),
                        nameof(ValidationException))),

                ForbiddenException forbiddenException => (
                    HttpStatusCode.Forbidden,
                    new ApiErrorResponse(forbiddenException.Message, Type: nameof(ForbiddenException))),

                BusinessException businessException => (
                    HttpStatusCode.BadRequest,
                    new ApiErrorResponse(businessException.Message, Type: nameof(BusinessException))),

                _ => (
                    HttpStatusCode.InternalServerError,
                    new ApiErrorResponse(
                        _environment.IsDevelopment()
                            ? exception.Message
                            : "Ocorreu um erro interno. Tente novamente mais tarde.",
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
        public static void UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
