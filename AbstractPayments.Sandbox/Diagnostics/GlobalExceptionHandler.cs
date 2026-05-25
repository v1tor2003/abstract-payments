namespace AbstractPayments.Sandbox.Diagnostics;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AbstractPayments.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// Enterprise exception handler translating system faults into compliant RFC 7807 Problem Details.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionHandler"/> class.
    /// </summary>
    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled system execution fault: {Message}", exception.Message);

        var (statusCode, title) = exception switch
        {
            GatewayNotRegisteredException _ => (StatusCodes.Status400BadRequest, "Provider Not Registered"),
            WebhookSignatureValidationException _ => (StatusCodes.Status400BadRequest, "Webhook Signature Verification Failed"),
            WebhookProcessingException _ => (StatusCodes.Status400BadRequest, "Webhook Processing Failed"),
            ProviderConfigurationException _ => (StatusCodes.Status400BadRequest, "Provider Configuration Error"),
            GatewayTypeMismatchException _ => (StatusCodes.Status400BadRequest, "Gateway Type Mismatch"),
            ArgumentException _ or InvalidOperationException _ => (StatusCodes.Status400BadRequest, "Bad Request"),
            UnauthorizedAccessException _ => (StatusCodes.Status401Unauthorized, "Unauthorized Access"),
            KeyNotFoundException _ => (StatusCodes.Status404NotFound, "Resource Not Found"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatus.es/{statusCode}",
            Detail = exception.Message,
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        };

        var didWrite = await _problemDetailsService.TryWriteAsync(context);

        if (!didWrite)
        {
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        }

        return true;
    }
}
