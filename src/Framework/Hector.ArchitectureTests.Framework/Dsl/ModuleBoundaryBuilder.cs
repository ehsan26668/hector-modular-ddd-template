using System.Reflection;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests.Framework.Dsl;

internal sealed class ModuleBoundaryBuilder(
    IEnumerable<Assembly> assemblies)
    : IModuleBoundaryBuilder
{
    private readonly IReadOnlyList<Assembly> _assemblies = [.. assemblies
            .Where(a => !a.IsDynamic)
            .DistinctBy(a => a.GetName().Name)
            .OrderBy(a => a.GetName().Name, StringComparer.Ordinal)];

    private Func<Assembly, string> _moduleNameResolver = DefaultModuleNameResolver;

    private Func<Assembly, bool> _contractsAssemblyPredicate = DefaultContractsAssemblyPredicate;

    private bool _allowCrossModuleDependenciesOnlyThroughContracts;

    public IModuleBoundaryBuilder WithModuleNameResolver(Func<Assembly, string> resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        _moduleNameResolver = resolver;

        return this;
    }

    public IModuleBoundaryBuilder WithContractsAssemblyPredicate(Func<Assembly, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        _contractsAssemblyPredicate = predicate;

        return this;
    }

    public IModuleBoundaryBuilder AllowCrossModuleDependenciesOnlyThroughContracts()
    {
        _allowCrossModuleDependenciesOnlyThroughContracts = true;

        return this;
    }

    public ArchitectureRule Build(string id, string name)
    {
        return new ArchitectureRule(
            id,
            name,
            string.Empty,
            Evaluate);
    }

    private EvaluationResult Evaluate()
    {
        if (_assemblies.Count == 0)
        {
            return EvaluationResult.Failure(
            [
                "No module assemblies were provided to ModuleBoundary DSL."
            ]);
        }

        var diagnostics = new List<string>();

        if (_allowCrossModuleDependenciesOnlyThroughContracts)
        {
            diagnostics.AddRange(EvaluateCrossModuleDependenciesOnlyThroughContracts());
        }

        return diagnostics.Count == 0
            ? EvaluationResult.Success()
            : EvaluationResult.Failure(diagnostics);
    }

    private IReadOnlyList<string> EvaluateCrossModuleDependenciesOnlyThroughContracts()
    {
        var diagnostics = new List<string>();

        foreach (var sourceAssembly in _assemblies)
        {
            var sourceAssemblyName = GetAssemblyName(sourceAssembly);
            var sourceModuleName = _moduleNameResolver(sourceAssembly);

            var forbiddenTargetAssemblies = _assemblies
                .Where(targetAssembly => !IsSameModule(sourceAssembly, targetAssembly))
                .Where(targetAssembly => !_contractsAssemblyPredicate(targetAssembly))
                .OrderBy(GetAssemblyName, StringComparer.Ordinal)
                .ToList();

            foreach (var targetAssembly in forbiddenTargetAssemblies)
            {
                var targetAssemblyName = GetAssemblyName(targetAssembly);
                var targetModuleName = _moduleNameResolver(targetAssembly);

                var result = Types
                    .InAssembly(sourceAssembly)
                    .ShouldNot()
                    .HaveDependencyOn(targetAssemblyName)
                    .GetResult();

                if (result.IsSuccessful)
                {
                    continue;
                }

                diagnostics.AddRange(
                    BuildDiagnostics(
                        sourceAssemblyName,
                        sourceModuleName,
                        targetAssemblyName,
                        targetModuleName,
                        result));
            }
        }

        return diagnostics;
    }

    private IEnumerable<string> BuildDiagnostics(
        string sourceAssemblyName,
        string sourceModuleName,
        string targetAssemblyName,
        string targetModuleName,
        TestResult result)
    {
        var failingTypes = result.FailingTypes is not null && result.FailingTypes.Any()
            ? result.FailingTypes
                .OrderBy(t => t.FullName, StringComparer.Ordinal)
                .Select(t => t.FullName ?? t.Name)
                .ToList()
            : result.FailingTypeNames?.Any() == true
                ? result.FailingTypeNames
                    .OrderBy(name => name, StringComparer.Ordinal)
                    .ToList()
                : ["<unknown type>"];

        foreach (var failingType in failingTypes)
        {
            yield return
                $"Module boundary violation: Type '{failingType}' in assembly '{sourceAssemblyName}' " +
                $"from module '{sourceModuleName}' must not depend directly on assembly '{targetAssemblyName}' " +
                $"from module '{targetModuleName}'. Cross-module dependencies are allowed only through Contracts assemblies.";
        }
    }

    private bool IsSameModule(Assembly sourceAssembly, Assembly targetAssembly)
    {
        var sourceModuleName = _moduleNameResolver(sourceAssembly);
        var targetModuleName = _moduleNameResolver(targetAssembly);

        return string.Equals(sourceModuleName, targetModuleName, StringComparison.Ordinal);
    }

    private static string GetAssemblyName(Assembly assembly)
    {
        return assembly.GetName().Name ?? "<UnknownAssembly>";
    }

    private static bool DefaultContractsAssemblyPredicate(Assembly assembly)
    {
        var assemblyName = GetAssemblyName(assembly);

        return assemblyName.EndsWith(".Contracts", StringComparison.Ordinal);
    }

    private static string DefaultModuleNameResolver(Assembly assembly)
    {
        var assemblyName = GetAssemblyName(assembly);

        var parts = assemblyName.Split('.', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 3 && string.Equals(parts[1], "Modules", StringComparison.Ordinal))
        {
            return parts[2];
        }

        if (parts.Length >= 2)
        {
            return parts[^2];
        }

        return assemblyName;
    }
}