namespace Hector.ArchitectureTests.Framework.Dsl;

public interface ITypeFilter
{
    ITypeFilter ResideInNamespace(string @namespace);
    IConstraintBuilder Should();
    IEnumerable<Type> GetTypes();
}