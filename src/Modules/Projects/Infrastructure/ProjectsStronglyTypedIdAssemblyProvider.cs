using System.Reflection;
using Hector.BuildingBlocks.Persistence;
using Hector.Modules.Projects.Domain;

namespace Hector.Modules.Projects.Infrastructure;

public sealed class ProjectsStronglyTypedIdAssemblyProvider
    : IStronglyTypedIdAssemblyProvider
{
    public IReadOnlyCollection<Assembly> GetAssemblies()
        => [typeof(ProjectsDomainAssemblyMarker).Assembly];
}
