using System.Reflection;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxEventContractOptions
{
    private readonly List<Assembly> _assemblies = [];

    public IReadOnlyCollection<Assembly> Assemblies => _assemblies;

    public void AddAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        if (_assemblies.Contains(assembly)) return;

        _assemblies.Add(assembly);
    }
}