using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Messaging.Inbox;
using Hector.BuildingBlocks.Persistence.Inbox;
using Hector.BuildingBlocks.Persistence.Outbox;
using Hector.BuildingBlocks.Persistence.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Hector.BuildingBlocks.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddHectorPersistenceBuildingBlocks(
        this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(EfCoreTransactionPipelineBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(InboxPipelineBehavior<,>));

        services.TryAddScoped<IInboxStore, EfCoreInboxStore>();

        services.TryAddScoped<IOutboxMessageFactory, DefaultOutboxMessageFactory>();

        services.TryAddSingleton<IOutboxEventTypeResolver>(sp =>
        {
            OutboxEventContractOptions options =
                sp.GetRequiredService<IOptions<OutboxEventContractOptions>>().Value;

            return new AttributedOutboxEventTypeResolver(options.Assemblies);
        });

        services.TryAddSingleton<IOutboxEventSerializer, SystemTextJsonOutboxEventSerializer>();

        return services;
    }
}
