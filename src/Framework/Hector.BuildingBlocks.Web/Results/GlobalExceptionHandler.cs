using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hector.BuildingBlocks.Web.Results;

internal sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // TC-05: Log the unhandled exception
        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        // TC-01 & TC-02: Create standardized ProblemDetails
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "InternalServerError",
            Type = "https://hector/errors/internal-server-error",
            Detail = "An unexpected error occurred on the server."
        };

        // TC-04: Include TraceId
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        // TC-01: Set status code
        httpContext.Response.StatusCode = problemDetails.Status.Value;

        // TC-06: Use the correct overload to force application/problem+json
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            type: typeof(ProblemDetails),
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true;
    }
}
