using System.Reflection;

namespace Hector.BuildingBlocks.Persistence;

public interface IStronglyTypedIdAssemblyProvider
{
    IReadOnlyCollection<Assembly> GetAssemblies();
}
