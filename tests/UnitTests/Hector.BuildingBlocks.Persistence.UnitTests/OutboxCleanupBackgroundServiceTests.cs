using Hector.BuildingBlocks.Persistence.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class OutboxCleanupBackgroundServiceTests
{
    [Fact]
    public async Task Should_InvokeCleaner_When_TimerTicks()
    {
        // Arrange

        var cleaner = Substitute.For<IOutboxCleaner>();

        var timer = new FakePeriodicTimer(1);

        var services = new ServiceCollection();
        services.AddScoped<IOutboxCleaner>(_ => cleaner);

        var provider = services.BuildServiceProvider();

        var logger = Substitute.For<ILogger<OutboxCleanupBackgroundService>>();

        var service = new OutboxCleanupBackgroundService(
            provider,
            timer,
            logger);

        // Act

        await service.StartAsync(CancellationToken.None);

        await Task.Delay(50);

        await service.StopAsync(CancellationToken.None);

        // Assert

        await cleaner.Received(1)
            .CleanupAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_NotInvokeCleaner_When_TimerDoesNotTick()
    {
        // Arrange

        var cleaner = Substitute.For<IOutboxCleaner>();

        var timer = new FakePeriodicTimer(0);

        var services = new ServiceCollection();
        services.AddScoped<IOutboxCleaner>(_ => cleaner);

        var provider = services.BuildServiceProvider();

        var logger = Substitute.For<ILogger<OutboxCleanupBackgroundService>>();

        var service = new OutboxCleanupBackgroundService(
            provider,
            timer,
            logger);

        // Act

        await service.StartAsync(CancellationToken.None);

        await Task.Delay(50);

        await service.StopAsync(CancellationToken.None);

        // Assert

        await cleaner.DidNotReceive()
            .CleanupAsync(Arg.Any<CancellationToken>());
    }

    private sealed class FakePeriodicTimer : IPeriodicTimer
    {
        private int _remainingTicks;

        public FakePeriodicTimer(int ticks)
        {
            _remainingTicks = ticks;
        }

        public Task<bool> WaitForNextTickAsync(CancellationToken cancellationToken)
        {
            if (_remainingTicks-- > 0)
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}