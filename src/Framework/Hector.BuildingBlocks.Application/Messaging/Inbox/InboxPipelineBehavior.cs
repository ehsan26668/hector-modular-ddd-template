using Hector.BuildingBlocks.Application.Messaging.Correlation;

namespace Hector.BuildingBlocks.Application.Messaging.Inbox;

public sealed class InboxPipelineBehavior<TRequest, TResponse>(
    IInboxStore inbox,
    IInboxConsumerNameProvider consumerNameProvider,
    ICorrelationContextAccessor correlationContextAccessor)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IIntegrationEvent integrationEvent)
        {
            return await next();
        }

        var consumer = consumerNameProvider.ConsumerName;

        var stored = await inbox.TryStoreAsync(
            integrationEvent.MessageId,
            consumer,
            cancellationToken);

        if (!stored)
        {
            return default!;
        }

        var context = new CorrelationContext(
            integrationEvent.CorrelationId,
            integrationEvent.MessageId,
            integrationEvent.TraceId);

        using var scope = correlationContextAccessor.BeginScope(context);

        return await next();
    }
}
