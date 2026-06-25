using Hector.BuildingBlocks.Application.Results;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace Hector.BuildingBlocks.Web.Results;

internal sealed class ResultEndpointFilter : IEndpointFilter
{
    private static readonly MethodInfo GenericMapperMethod =
        typeof(ResultHttpMapper)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(m =>
                m.Name == "ToHttpResultGeneric" &&
                m.IsGenericMethodDefinition);

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var result = await next(context);

        if (result is Result r)
        {
            return ResultHttpMapper.ToHttpResult(r);
        }

        if (result is null)
        {
            return null;
        }

        var type = result.GetType();

        if (type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(Result<>))
        {
            return InvokeGenericMapper(result, type);
        }

        return result;
    }

    private static object InvokeGenericMapper(object result, Type resultType)
    {
        var closedMethod = GenericMapperMethod.MakeGenericMethod(
            resultType.GetGenericArguments()[0]);

        return closedMethod.Invoke(null, [result])!;
    }
}
