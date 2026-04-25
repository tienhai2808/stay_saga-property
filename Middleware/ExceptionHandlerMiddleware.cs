using Microsoft.AspNetCore.WebUtilities;
using PropertyService.DTOs;
using PropertyService.Exceptions;

namespace PropertyService.Middleware;

public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            await WriteProblemDetails(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblemDetails(context, 500, "Internal server error");
        }
    }

    private static async Task WriteProblemDetails(HttpContext context, int statusCode, string detail)
    {
        if (context.Response.HasStarted)
            return;

        var title = ReasonPhrases.GetReasonPhrase(statusCode);
        var response = new ApiResponse<object>(
          statusCode,
          detail,
          null
        );

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    }
}
