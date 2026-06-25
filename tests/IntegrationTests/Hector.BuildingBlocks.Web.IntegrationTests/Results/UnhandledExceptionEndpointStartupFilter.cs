using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hector.BuildingBlocks.Web.IntegrationTests.Results;

internal sealed class UnhandledExceptionEndpointStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            next(app);

            // TC-07: Unhandled Exception
            app.Map("/test/unhandled-exception", builder =>
            {
                builder.Run(_ => throw new InvalidOperationException("Critical system failure!"));
            });

            // TC-08: Failure Result
            app.Map("/test/failure-result", builder =>
            {
                builder.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    var problemDetails = new ProblemDetails
                    {
                        Title = "Domain/Application Error Title",
                        Status = (int)HttpStatusCode.BadRequest,
                        Detail = "Business rule violation occurred."
                    };
                    problemDetails.Extensions["errorCode"] = "ERR_001";

                    await context.Response.WriteAsJsonAsync(problemDetails);
                });
            });

            // TC-09: Exception after registration
            app.Map("/test/exception-after-middleware", builder =>
            {
                builder.Run(_ => throw new InvalidOperationException("Exception after registration"));
            });

            // TC-10: Unexpected exceptions
            app.Map("/test/throwing-endpoint", builder =>
            {
                builder.Run(_ => throw new Exception("Unexpected error"));
            });
        };
    }
}
