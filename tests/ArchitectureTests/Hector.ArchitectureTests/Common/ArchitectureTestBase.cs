using System.Reflection;

namespace Hector.ArchitectureTests.Common;

public abstract class ArchitectureTestBase
{
    protected static IReadOnlyCollection<Assembly> AllAssemblies => AssemblyCatalog.All;
    protected static IReadOnlyCollection<Assembly> ProductionAssemblies => AssemblyCatalog.Production;
    protected static IReadOnlyCollection<Assembly> TestAssemblies => AssemblyCatalog.Tests;

    protected static IReadOnlyCollection<Assembly> DomainAssemblies => AssemblyCatalog.Domain;
    protected static IReadOnlyCollection<Assembly> ApplicationAssemblies => AssemblyCatalog.Application;
    protected static IReadOnlyCollection<Assembly> InfrastructureAssemblies => AssemblyCatalog.Infrastructure;

    protected static IReadOnlyCollection<Assembly> ModuleAssemblies => AssemblyCatalog.Modules;
    protected static IReadOnlyCollection<Assembly> BuildingBlockAssemblies => AssemblyCatalog.BuildingBlocks;

    protected static IEnumerable<Type> GetAllTypes(IEnumerable<Assembly> assemblies)
        => assemblies.SelectMany(a => a.GetLoadableTypes());

    protected static IEnumerable<MethodInfo> GetTestMethods(IEnumerable<Assembly> assemblies)
        => GetAllTypes(assemblies)
            .Where(t => !t.IsCompilerGenerated())
            .SelectMany(t =>
                t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            .Where(m => m.HasAttribute<FactAttribute>() || m.HasAttribute<TheoryAttribute>());

    protected static Assembly GetModuleAssemblyOrFail(string assemblySimpleName)
    {
        var assembly = ModuleAssemblies.FirstOrDefault(a =>
            string.Equals(a.GetName().Name, assemblySimpleName, StringComparison.Ordinal));

        if (assembly is not null)
            return assembly;

        throw new Xunit.Sdk.XunitException($"""
            Module assembly not found: '{assemblySimpleName}'.
            Available module assemblies:
            - {string.Join(Environment.NewLine + "- ", ModuleAssemblies.Select(a => a.GetName().Name))}
            """);
    }

    protected static Assembly GetModuleDomainAssembly(string moduleName, Type fallbackTypeInModuleDomain)
    {
        _ = fallbackTypeInModuleDomain.Assembly;

        var expectedName = $"Hector.Modules.{moduleName}.Domain";

        return ModuleAssemblies.FirstOrDefault(a =>
                   string.Equals(a.GetName().Name, expectedName, StringComparison.Ordinal))
               ?? fallbackTypeInModuleDomain.Assembly;
    }
}
