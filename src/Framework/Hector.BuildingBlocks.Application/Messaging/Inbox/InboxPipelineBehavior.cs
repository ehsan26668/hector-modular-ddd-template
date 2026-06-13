namespace Hector.BuildingBlocks.Application.Messaging.Inbox;

public sealed class InboxPipelineBehavior<TRequest, TResponse>(IInboxStore inbox)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IInboxMessage inboxMessage) return await next();

        var stored = await inbox.TryStoreAsync(
            inboxMessage.MessageId,
            inboxMessage.Consumer,
            cancellationToken);

        if (!stored) return default!;

        return await next();
    }
}