using Hector.BuildingBlocks.Persistence.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hector.BuildingBlocks.Persistence.Outbox;

internal sealed class OutboxCleanupBackgroundService(
    IServiceProvider serviceProvider,
    IPeriodicTimer timer,
    ILogger<OutboxCleanupBackgroundService> logger)
    : PeriodicBackgroundService(serviceProvider, timer, logger)
{
    protected override async Task ExecuteInScopeAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var cleaner = serviceProvider.GetRequiredService<IOutboxCleaner>();

        await cleaner.CleanupAsync(cancellationToken);
    }
}
