using BusinessLogicLayer.Exceptions;
using DataAccessLayer.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace APILayer.MiddleWare;

public class GlobalExpectionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExpectionHandlingMiddleware> _logger;

    public GlobalExpectionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExpectionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
            if (context.Response.StatusCode == StatusCodes.Status404NotFound && !context.Response.HasStarted)
                await HandleNotFoundApiAsync(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleGlobalExceptionAsync(context, ex);
        }
    }

    private async Task HandleNotFoundApiAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        var response = new ErrorDetails(StatusCodes.Status404NotFound, $"The endpoint with url {context.Request.Path} not found");
        await context.Response.WriteAsync(response.ToString());
    }

    private async Task HandleGlobalExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        
        IEnumerable<string>? errors = null;

        // 1. Determine Status Code and Errors
        int code = ex switch
        {
            NotFoundExpection => StatusCodes.Status404NotFound,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            VaildationException validationException => GetValidationErrors(validationException, out errors),
            _ => StatusCodes.Status500InternalServerError
        };

        // 2. Create the record
        var response = new ErrorDetails(code, ex.Message, errors);

        context.Response.StatusCode = code;
        await context.Response.WriteAsync(response.ToString());
    }

    private int GetValidationErrors(VaildationException ex, out IEnumerable<string> errors)
    {
        errors = ex.Errors;
        return StatusCodes.Status400BadRequest;
    }
}
