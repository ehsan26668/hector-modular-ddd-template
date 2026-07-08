using System.Reflection;

namespace Hector.ArchitectureTests.Framework.Dsl;

internal sealed class ModuleBoundarySelection : IModuleBoundarySelection
{
    public IModuleBoundaryBuilder From(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        return new ModuleBoundaryBuilder(assemblies);
    }
}