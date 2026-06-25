using Hector.BuildingBlocks.Application.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Hector.BuildingBlocks.Web.Results;

internal static class ResultHttpMapper
{
    public static IResult ToHttpResult(Result result)
    {
        if (result.IsSuccess)
        {
            return HttpResults.Ok();
        }

        var error = result.Error;

        var statusCode = MapStatusCode(error.Category);

        var problem = CreateProblemDetails(error, statusCode);

        return HttpResults.Problem(problem);
    }

    private static IResult ToHttpResultGeneric<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return HttpResults.Ok(result.Value);
        }

        var error = result.Error;

        var statusCode = MapStatusCode(error.Category);

        var problem = CreateProblemDetails(error, statusCode);

        return HttpResults.Problem(problem);
    }

    private static int MapStatusCode(ErrorCategory category)
    {
        return category switch
        {
            ErrorCategory.Validation => StatusCodes.Status400BadRequest,
            ErrorCategory.NotFound => StatusCodes.Status404NotFound,
            ErrorCategory.Conflict => StatusCodes.Status409Conflict,
            ErrorCategory.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorCategory.Forbidden => StatusCodes.Status403Forbidden,
            ErrorCategory.Infrastructure => StatusCodes.Status503ServiceUnavailable,
            ErrorCategory.Unexpected => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static ProblemDetails CreateProblemDetails(Error error, int statusCode)
    {
        var problem = new ProblemDetails
        {
            Title = error.Code,
            Detail = error.Message,
            Status = statusCode,
            Type = $"https://hector/errors/{error.Code}"
        };

        if (error.Metadata is null)
        {
            return problem;
        }

        foreach (var metadata in error.Metadata)
        {
            if (error.Category == ErrorCategory.Validation &&
                metadata.Key.Equals("errors", StringComparison.OrdinalIgnoreCase))
            {
                problem.Extensions["errors"] = metadata.Value;
                continue;
            }

            problem.Extensions[metadata.Key] = metadata.Value;
        }

        return problem;
    }
}
