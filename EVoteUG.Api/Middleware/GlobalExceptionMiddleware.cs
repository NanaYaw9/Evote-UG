using System.Net;
using System.Text.Json;
using EVoteUG.Core.Exceptions;
using EVoteUG.Shared.Responses;

namespace EVoteUG.Api.Middleware;

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException valEx => (HttpStatusCode.BadRequest, valEx.Message, valEx.Errors),
            ResourceNotFoundException notFoundEx => (HttpStatusCode.NotFound, notFoundEx.Message, new List<string>()),
            AlreadyVotedException votedEx => (HttpStatusCode.Conflict, votedEx.Message, new List<string>()),
            ConflictException conflictEx => (HttpStatusCode.Conflict, conflictEx.Message, new List<string>()),
            VoterNotEligibleException ineligibleEx => (HttpStatusCode.Forbidden, ineligibleEx.Message, new List<string>()),
            UnauthorizedActionException unauthActionEx => (HttpStatusCode.Forbidden, unauthActionEx.Message, new List<string>()),
            UnauthorizedAccessException unauthEx => (HttpStatusCode.Unauthorized, unauthEx.Message, new List<string>()),
            DomainException domainEx => (HttpStatusCode.BadRequest, domainEx.Message, new List<string>()),
            _ => (HttpStatusCode.InternalServerError, "An unexpected server error occurred. Please try again later.", new List<string>())
        };

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(message, errors);
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
