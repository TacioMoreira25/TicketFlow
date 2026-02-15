using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace TicketFlow.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    
                var problemDetails = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Erro de validação",
                    Detail = "Consulte o campo 'errors' para detalhes.", 
                    Type = "https://tools.ietf.org/html/rfc7807"
                };

                if (ex.Errors != null)
                {
                    problemDetails.Extensions["errors"] = ex.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key, 
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );
                }

                await WriteProblemDetailsAsync(context, problemDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred.");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var problemDetails = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "An error occurred.",
                    Detail = "Please try again later.",
                    Type = "https://tools.ietf.org/html/rfc7807"
                };
                await WriteProblemDetailsAsync(context, problemDetails);
            }
        }

        private static Task WriteProblemDetailsAsync(HttpContext context, ProblemDetails problemDetails)
        {
            context.Response.ContentType = "application/problem+json";
            return JsonSerializer.SerializeAsync(context.Response.Body, problemDetails);
        }
    }
}
