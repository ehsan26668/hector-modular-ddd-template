using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Hector.BuildingBlocks.Web.Results;

public static class ResultEndpointFilterExtensions
{
    public static RouteHandlerBuilder WithResultMapping(this RouteHandlerBuilder builder)
    {
        builder.AddEndpointFilter<ResultEndpointFilter>();

        return builder;
    }

    public static RouteGroupBuilder WithResultMapping(this RouteGroupBuilder builder)
    {
        builder.AddEndpointFilter<ResultEndpointFilter>();

        return builder;
    }
}
