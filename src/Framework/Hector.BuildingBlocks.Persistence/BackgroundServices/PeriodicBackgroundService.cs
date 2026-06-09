using Hector.BuildingBlocks.Persistence.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hector.BuildingBlocks.Persistence.BackgroundServices;

internal abstract class PeriodicBackgroundService(
    IServiceProvider serviceProvider,
    IPeriodicTimer timer,
    ILogger logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = serviceProvider.CreateScope();

                await ExecuteInScopeAsync(scope.ServiceProvider, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background job execution failed.");
            }
        }
    }

    protected abstract Task ExecuteInScopeAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}