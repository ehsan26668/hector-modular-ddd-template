using System.Reflection;

namespace Hector.ArchitectureTests.Framework.Dsl;

public interface ITypesSelection
{
    ITypeFilter That();
    ITypeFilter That(Assembly assembly);
}