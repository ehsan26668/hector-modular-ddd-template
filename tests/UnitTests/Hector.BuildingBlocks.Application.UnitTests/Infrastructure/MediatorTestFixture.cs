using Hector.BuildingBlocks.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Application.UnitTests.Infrastructure;

internal sealed class MediatorTestFixture
{
    private readonly ServiceProvider _serviceProvider;

    public MediatorTestFixture(Action<IServiceCollection>? configureServices = null)
    {
        var services = new ServiceCollection();

        services.AddSingleton<List<string>>();
        services.AddSingleton<IMediator, Mediator>();

        configureServices?.Invoke(services);

        _serviceProvider = services.BuildServiceProvider();
        Mediator = _serviceProvider.GetRequiredService<IMediator>();
        ExecutionOrder = _serviceProvider.GetRequiredService<List<string>>();
    }

    public IMediator Mediator { get; }

    public List<string> ExecutionOrder { get; }

    public T GetRequiredService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    public IReadOnlyList<T> GetServices<T>() where T : notnull
    {
        return _serviceProvider.GetServices<T>().ToList();
    }
}