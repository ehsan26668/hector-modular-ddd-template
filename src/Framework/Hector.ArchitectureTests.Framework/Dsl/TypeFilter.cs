using NetArchTest.Rules;

namespace Hector.ArchitectureTests.Framework.Dsl;

internal sealed class TypeFilter(
    Types types)
    : ITypeFilter
{
    private readonly List<string> _namespaces = [];

    public ITypeFilter ResideInNamespace(string @namespace)
    {
        _namespaces.Add(@namespace);
        return this;
    }

    public IConstraintBuilder Should()
    {
        return new ConstraintBuilder(types, _namespaces);
    }
}
