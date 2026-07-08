using System.Reflection;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests.Framework.Dsl;

internal sealed class TypesSelection : ITypesSelection
{
    public ITypeFilter That()
    {
        var domainAssembly = Assembly.Load("Hector.BuildingBlocks.Domain");
        return new TypeFilter(Types.InAssembly(domainAssembly));
    }

    public ITypeFilter That(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return new TypeFilter(Types.InAssembly(assembly));
    }

    public ITypeFilter That(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        return new TypeFilter(Types.InAssemblies(assemblies));
    }
}