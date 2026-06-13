namespace Hector.BuildingBlocks.Application.Messaging.Inbox;

public sealed class InboxBehavior<TRequest>(
    IInboxStore inbox,
    Guid messageId,
    string consumer)
{
    public async Task<Tresponse?> Handle<Tresponse>(
        TRequest request,
        Func<Task<Tresponse>> next,
        CancellationToken cancellationToken)
    {
        var stored = await inbox.TryStoreAsync(
            messageId,
            consumer,
            cancellationToken);

        if (!stored) return default!;

        return await next();
    }
}