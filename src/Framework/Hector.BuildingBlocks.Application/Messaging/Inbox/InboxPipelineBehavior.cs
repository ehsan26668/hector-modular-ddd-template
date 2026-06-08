namespace Hector.BuildingBlocks.Application.Messaging.Inbox;

public sealed class InboxPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IInboxStore _inbox;

    public InboxPipelineBehavior(IInboxStore inbox)
    {
        _inbox = inbox;
    }

    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IInboxMessage inboxMessage)
        {
            return await next();
        }

        var messageId = inboxMessage.MessageId;
        var consumer = inboxMessage.Consumer;

        if (await _inbox.ExistsAsync(messageId, consumer, cancellationToken))
        {
            return default!;
        }

        var response = await next();

        await _inbox.StoreAsync(messageId, consumer, cancellationToken);

        return response;
    }
}