namespace Hector.BuildingBlocks.Application.Messaging.Inbox;

public sealed class InboxBehavior<TRequest>
{
    private readonly IInboxStore _inbox;
    private readonly Guid _messageId;
    private readonly string _consumer;

    public InboxBehavior(
        IInboxStore inbox,
        Guid messageId,
        string consumer)
    {
        _inbox = inbox;
        _messageId = messageId;
        _consumer = consumer;
    }

    public async Task<Tresponse?> Handle<Tresponse>(
        TRequest request,
        Func<Task<Tresponse>> next,
        CancellationToken cancellationToken)
    {
        if (await _inbox.ExistsAsync(_messageId, _consumer, cancellationToken))
        {
            return default;
        }

        var response = await next();

        await _inbox.StoreAsync(_messageId, _consumer, cancellationToken);

        return response;
    }
}