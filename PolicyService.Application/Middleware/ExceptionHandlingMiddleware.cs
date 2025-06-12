using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolicyService.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace PolicyService.Application.Middleware
{
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                TraceId = context.TraceIdentifier,
                Instance = context.Request.Path
            };

            switch (exception)
            {
                case PolicyNotFoundException ex:
                    response.Title = "Policy Not Found";
                    response.Detail = ex.Message;
                    response.Status = (int)HttpStatusCode.NotFound;
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case PolicyValidationException ex:
                    response.Title = "Policy Validation Failed";
                    response.Detail = ex.Message;
                    response.Status = (int)HttpStatusCode.BadRequest;
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case PaymentFailedException ex:
                    response.Title = "Payment Processing Failed";
                    response.Detail = ex.Message;
                    response.Status = (int)HttpStatusCode.BadRequest;
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case ArgumentException ex:
                    response.Title = "Invalid Request";
                    response.Detail = ex.Message;
                    response.Status = (int)HttpStatusCode.BadRequest;
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case InvalidOperationException ex when ex.Message.Contains("Circuit breaker"):
                    response.Title = "Service Temporarily Unavailable";
                    response.Detail = "Payment service is currently unavailable. Please try again later.";
                    response.Status = (int)HttpStatusCode.ServiceUnavailable;
                    context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    break;

                case PaymentValidationException ex:
                    response.Title = "Payment Validation Failed";
                    response.Detail = ex.Message;
                    response.Status = (int)HttpStatusCode.BadRequest;
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                default:
                    response.Title = "Internal Server Error";
                    response.Detail = "An unexpected error occurred while processing your request";
                    response.Status = (int)HttpStatusCode.InternalServerError;
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, _jsonOptions);

            await context.Response.WriteAsync(jsonResponse);
        }

        private class ErrorResponse
        {
            public string Title { get; set; } = string.Empty;
            public string Detail { get; set; } = string.Empty;
            public int Status { get; set; }
            public string TraceId { get; set; } = string.Empty;
            public string Instance { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        }
    }
}