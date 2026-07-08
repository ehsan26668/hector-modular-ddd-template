using System.Reflection;

namespace Hector.ArchitectureTests.Framework.Dsl;

public interface IModuleBoundaryBuilder
{
    IModuleBoundaryBuilder WithModuleNameResolver(Func<Assembly, string> resolver);

    IModuleBoundaryBuilder WithContractsAssemblyPredicate(Func<Assembly, bool> predicate);

    IModuleBoundaryBuilder AllowCrossModuleDependenciesOnlyThroughContracts();

    ArchitectureRule Build(string id, string name);
}