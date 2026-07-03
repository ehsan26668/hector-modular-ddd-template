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
        return new TypeFilter(Types.InAssembly(assembly));
    }
}