using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Maham.Application.DTOs.Common;

namespace Maham.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An internal server error occurred.";
        List<string>? errors = null;

        switch (exception)
        {
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Forbidden; // If token is valid but access is forbidden
                message = exception.Message;
                break;
            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
                break;
            case ArgumentException:
            case InvalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;
            default:
                if (_env.IsDevelopment())
                {
                    message = exception.Message;
                    errors = new List<string> { exception.StackTrace ?? string.Empty };
                }
                break;
        }

        // Adjust specific status codes
        if (exception is UnauthorizedAccessException && exception.Message.Contains("Invalid email or password"))
        {
            statusCode = HttpStatusCode.Unauthorized;
        }

        context.Response.StatusCode = (int)statusCode;
        var response = ApiResponseDto<object>.FailureResponse(message, errors);
        
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, options);

        return context.Response.WriteAsync(json);
    }
}
