using System.Reflection;

namespace Hector.BuildingBlocks.Persistence;

public sealed class CompositeStronglyTypedIdAssemblyProvider
    : IStronglyTypedIdAssemblyProvider
{
    private readonly IEnumerable<IStronglyTypedIdAssemblyProvider> _providers;

    public CompositeStronglyTypedIdAssemblyProvider(
        IEnumerable<IStronglyTypedIdAssemblyProvider> providers)
    {
        _providers = providers;
    }

    public IReadOnlyCollection<Assembly> GetAssemblies()
    {
        return _providers
            .SelectMany(p => p.GetAssemblies())
            .Distinct()
            .ToArray();
    }
}
