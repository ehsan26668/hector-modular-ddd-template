using System.Reflection;

namespace Hector.ArchitectureTests.Common;

/// <summary>
/// Central registry for all Hector assemblies used in architecture tests.
///
/// Guarantees:
/// - Discover Hector assemblies that are not loaded yet (loads from AppContext.BaseDirectory)
/// - Layer classification (Domain, Application, Infrastructure, Contracts, Web)
/// - Test assembly discovery
/// - Module-aware grouping
/// - Cached reflection for performance
/// </summary>
public static class AssemblyCatalog
{
    private static readonly Lazy<Assembly[]> _all =
        new(BuildAllAssemblies);

    private static readonly Lazy<Assembly[]> _production =
        new(() => [.. _all.Value.Where(a => !IsTestAssembly(a))]);

    private static readonly Lazy<Assembly[]> _tests =
        new(() => [.. _all.Value.Where(IsTestAssembly)]);

    private static readonly Lazy<Assembly[]> _domain =
        new(() => [.. _production.Value.Where(a => a.GetName().Name!.EndsWith(".Domain", StringComparison.Ordinal))]);

    private static readonly Lazy<Assembly[]> _application =
        new(() => [.. _production.Value.Where(a => a.GetName().Name!.EndsWith(".Application", StringComparison.Ordinal))]);

    private static readonly Lazy<Assembly[]> _infrastructure =
        new(() =>
            [.. _production.Value.Where(a =>
                a.GetName().Name!.EndsWith(".Infrastructure", StringComparison.Ordinal) ||
                a.GetName().Name!.EndsWith(".Persistence", StringComparison.Ordinal))]);

    private static readonly Lazy<Assembly[]> _contracts =
        new(() => [.. _production.Value.Where(a => a.GetName().Name!.EndsWith(".Contracts", StringComparison.Ordinal))]);

    private static readonly Lazy<Assembly[]> _web =
        new(() =>
            [.. _production.Value.Where(a =>
                a.GetName().Name!.EndsWith(".Web", StringComparison.Ordinal) ||
                a.GetName().Name!.EndsWith(".Host", StringComparison.Ordinal))]);

    public static IReadOnlyCollection<Assembly> All => _all.Value;
    public static IReadOnlyCollection<Assembly> Production => _production.Value;
    public static IReadOnlyCollection<Assembly> Tests => _tests.Value;

    public static IReadOnlyCollection<Assembly> Domain => _domain.Value;
    public static IReadOnlyCollection<Assembly> Application => _application.Value;
    public static IReadOnlyCollection<Assembly> Infrastructure => _infrastructure.Value;
    public static IReadOnlyCollection<Assembly> Contracts => _contracts.Value;
    public static IReadOnlyCollection<Assembly> Web => _web.Value;

    public static IReadOnlyCollection<Assembly> Modules =>
        [.. _production.Value.Where(a => a.GetName().Name!.Contains(".Modules.", StringComparison.Ordinal))];

    public static IReadOnlyCollection<Assembly> BuildingBlocks =>
        [.. _production.Value.Where(a => a.GetName().Name!.Contains("BuildingBlocks", StringComparison.Ordinal))];

    private static Assembly[] BuildAllAssemblies()
    {
        // 1) already loaded
        var loaded = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(IsHectorAssembly)
            .ToList();

        // 2) proactively load from base dir (typical test output directory)
        var baseDir = AppContext.BaseDirectory;
        if (Directory.Exists(baseDir))
        {
            foreach (var file in Directory.EnumerateFiles(baseDir, "Hector.*.dll", SearchOption.TopDirectoryOnly))
            {
                TryLoadAssemblyFrom(file, loaded);
            }
        }

        return [.. loaded.DistinctBy(a => a.FullName)];
    }

    private static void TryLoadAssemblyFrom(string path, List<Assembly> loaded)
    {
        try
        {
            var name = AssemblyName.GetAssemblyName(path);
            if (string.IsNullOrWhiteSpace(name.Name) || !name.Name.StartsWith("Hector.", StringComparison.Ordinal))
                return;

            // skip if already loaded
            if (loaded.Any(a => string.Equals(a.GetName().Name, name.Name, StringComparison.Ordinal)))
                return;

            var assembly = Assembly.Load(name);
            if (IsHectorAssembly(assembly))
                loaded.Add(assembly);
        }
        catch
        {
            // swallow: best-effort load; a broken or native dll should not fail architecture tests discovery
        }
    }

    private static bool IsHectorAssembly(Assembly assembly)
    {
        var name = assembly.GetName().Name;
        return !string.IsNullOrWhiteSpace(name) &&
               name.StartsWith("Hector.", StringComparison.Ordinal);
    }

    private static bool IsTestAssembly(Assembly assembly)
    {
        var name = assembly.GetName().Name;
        return !string.IsNullOrWhiteSpace(name) &&
               (name.EndsWith(".UnitTests", StringComparison.Ordinal) ||
                name.EndsWith(".IntegrationTests", StringComparison.Ordinal) ||
                name.EndsWith(".ArchitectureTests", StringComparison.Ordinal) ||
                name.EndsWith(".TemplateTests", StringComparison.Ordinal));
    }
}
