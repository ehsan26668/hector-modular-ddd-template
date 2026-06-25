using Microsoft.AspNetCore.Builder;

namespace Hector.BuildingBlocks.Web;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseHectorWebBuildingBlocks(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseExceptionHandler();

        return app;
    }
}
