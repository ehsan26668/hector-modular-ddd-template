namespace Hector.BuildingBlocks.Application.Messaging.Inbox;

public sealed class InboxPipelineBehavior<TRequest, TResponse>(
    IInboxStore inbox,
    IModuleIdentity moduleIdentity)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IIntegrationEvent integrationEvent) return await next();

        var consumer = moduleIdentity.ModuleName;

        var stored = await inbox.TryStoreAsync(
            integrationEvent.MessageId,
            consumer,
            cancellationToken);

        if (!stored) return default!;

        return await next();
    }
}