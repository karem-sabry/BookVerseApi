using System.Net;
using System.Text.Json;
using BookVerse.Core.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace BookVerse.Api.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = exception switch
        {
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        httpContext.Response.ContentType = "application/json";

        _logger.LogError(exception,
            $"An unhandled exception occurred. StatusCode:{httpContext.Response.StatusCode}, Message: {exception.Message}");

        var errorDetails = new ErrorDetails
        {
            StatusCode = httpContext.Response.StatusCode,
            Message = GetUserFriendlyMessage(exception, httpContext.Response.StatusCode)
        };

        await httpContext.Response.WriteAsync(
            errorDetails.ToString(),
            cancellationToken);

        // Return true to indicate the exception has been handled
        return true;
    }

    private static string GetUserFriendlyMessage(Exception exception, int statusCode)
    {
        return statusCode switch
        {
            (int)HttpStatusCode.NotFound => exception.Message,
            (int)HttpStatusCode.BadRequest => exception.Message,
            (int)HttpStatusCode.Unauthorized => "Unauthorized access.",
            _ => "An unexpected error occurred. Please try again later."
        };
    }
}