using Hector.BuildingBlocks.Application.Results;
using Microsoft.AspNetCore.Http;

namespace Hector.BuildingBlocks.Web.Results;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        return ResultHttpMapper.ToHttpResult(result);
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return ResultHttpMapper.ToHttpResult(result);
    }
}