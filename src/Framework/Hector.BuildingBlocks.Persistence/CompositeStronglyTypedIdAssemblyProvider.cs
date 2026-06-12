using System.Reflection;

namespace Hector.BuildingBlocks.Persistence;

public sealed class CompositeStronglyTypedIdAssemblyProvider(
    IEnumerable<IStronglyTypedIdAssemblyProvider> providers)
        : IStronglyTypedIdAssemblyProvider
{
    public IReadOnlyCollection<Assembly> GetAssemblies()
    {
        return [.. providers
            .SelectMany(p => p.GetAssemblies())
            .Distinct()];
    }
}
