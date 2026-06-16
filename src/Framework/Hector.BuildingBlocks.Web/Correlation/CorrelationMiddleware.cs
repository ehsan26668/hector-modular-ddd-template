using Hector.BuildingBlocks.Application.Messaging.Correlation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Hector.BuildingBlocks.Web.Correlation;

public sealed class CorrelationMiddleware(
    RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ICorrelationContextAccessor correlationContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(correlationContextAccessor);

        Guid correlationId = ResolveCorrelationId(context.Request.Headers);
        string? traceId = ResolveTraceId(context.Request.Headers);

        correlationContextAccessor.Set(new CorrelationContext(
            correlationId,
            CausationId: null,
            TraceId: traceId));

        context.Response.Headers[CorrelationHeaderNames.CorrelationId] =
            correlationId.ToString();

        try
        {
            await next(context);
        }
        finally
        {
            correlationContextAccessor.Clear();
        }
    }

    private static Guid ResolveCorrelationId(IHeaderDictionary headers)
    {
        if (!headers.TryGetValue(CorrelationHeaderNames.CorrelationId, out StringValues values))
        {
            return Guid.NewGuid();
        }

        string? rawValue = values.FirstOrDefault();

        return Guid.TryParse(rawValue, out Guid correlationId)
            ? correlationId
            : Guid.NewGuid();
    }

    private static string? ResolveTraceId(IHeaderDictionary headers)
    {
        if (!headers.TryGetValue(CorrelationHeaderNames.TraceParent, out StringValues values))
        {
            return null;
        }

        string? traceParent = values.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(traceParent))
        {
            return null;
        }

        string[] parts = traceParent.Split('-');

        return parts.Length >= 4 && parts[1].Length == 32
            ? parts[1]
            : traceParent;
    }
}