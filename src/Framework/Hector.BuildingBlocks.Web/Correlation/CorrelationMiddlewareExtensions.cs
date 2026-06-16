using Microsoft.AspNetCore.Builder;

namespace Hector.BuildingBlocks.Web.Correlation;

public static class CorrelationMiddlewareExtensions
{
    public static IApplicationBuilder UseHectorCorrelation(
        this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<CorrelationMiddleware>();
    }
}