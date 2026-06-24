using System.Reflection;
using FluentAssertions;
using Hector.BuildingBlocks.Application.Results;

namespace Hector.ArchitectureTests.ResultRules;

public sealed class ErrorContractSnapshotTests
{
    private const string SnapshotFileName = "error-contracts.snapshot";
    private const string ModuleApplicationAssemblySearchPattern = "Hector.Modules.*.Application.dll";

    [Fact]
    public void ErrorContracts_Should_Match_Snapshot()
    {
        // Arrange
        var snapshotPath = GetSnapshotPath();
        var currentSnapshot = GetErrorContractsSnapshot();

        // Act
        var normalizedCurrent = NormalizeLineEndings(currentSnapshot);

        // Assert
        if (!File.Exists(snapshotPath))
        {
            File.WriteAllText(snapshotPath, currentSnapshot);
            throw new InvalidOperationException(
                $"Snapshot created at '{snapshotPath}'. Review and commit.");
        }

        var expectedSnapshot = NormalizeLineEndings(File.ReadAllText(snapshotPath));

        normalizedCurrent.Should().Be(
            expectedSnapshot,
            "error contracts are stable API contracts and must not change accidentally");
    }

    private static string GetErrorContractsSnapshot()
    {
        var assemblies = GetModuleApplicationAssemblies().ToList();

        var errorFields = assemblies
            .SelectMany(GetAllTypes)
            .SelectMany(type => type.GetFields(
                    BindingFlags.Public |
                    BindingFlags.Static |
                    BindingFlags.FlattenHierarchy)
                .Where(field => field.FieldType == typeof(Error))
                .Select(field => (Error)field.GetValue(null)!))
            .ToList();

        var lines = errorFields
            .Select(error => $"{error.Code}|{error.Category}|{error.Message}")
            .Distinct(StringComparer.Ordinal)
            .OrderBy(line => line, StringComparer.Ordinal)
            .ToList();

        return lines.Count == 0
            ? string.Empty
            : string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static IEnumerable<Assembly> GetModuleApplicationAssemblies()
    {
        var assemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (IsModuleApplicationAssembly(assembly))
            {
                assemblies[assembly.FullName!] = assembly;
            }
        }

        var baseDirectory = AppContext.BaseDirectory;

        foreach (var assemblyPath in Directory.EnumerateFiles(
                     baseDirectory,
                     ModuleApplicationAssemblySearchPattern,
                     SearchOption.TopDirectoryOnly))
        {
            var assembly = LoadAssemblySafely(assemblyPath);
            if (assembly is null || !IsModuleApplicationAssembly(assembly))
            {
                continue;
            }

            assemblies[assembly.FullName!] = assembly;
        }

        return assemblies.Values
            .OrderBy(assembly => assembly.GetName().Name, StringComparer.Ordinal);
    }

    private static Assembly? LoadAssemblySafely(string assemblyPath)
    {
        try
        {
            var assemblyName = AssemblyName.GetAssemblyName(assemblyPath);

            var alreadyLoadedAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(assembly =>
                    string.Equals(
                        assembly.GetName().Name,
                        assemblyName.Name,
                        StringComparison.OrdinalIgnoreCase));

            if (alreadyLoadedAssembly is not null)
            {
                return alreadyLoadedAssembly;
            }

            return Assembly.LoadFrom(assemblyPath);
        }
        catch
        {
            return null;
        }
    }

    private static bool IsModuleApplicationAssembly(Assembly assembly)
    {
        var assemblyName = assembly.GetName().Name;

        return assembly.IsDynamic is false &&
               assemblyName is not null &&
               assemblyName.StartsWith("Hector.Modules.", StringComparison.Ordinal) &&
               assemblyName.EndsWith(".Application", StringComparison.Ordinal);
    }

    private static IEnumerable<Type> GetAllTypes(Assembly assembly)
    {
        foreach (var type in GetLoadableTypes(assembly))
        {
            yield return type;

            foreach (var nestedType in GetNestedTypesRecursive(type))
            {
                yield return nestedType;
            }
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type is not null)!;
        }
    }

    private static IEnumerable<Type> GetNestedTypesRecursive(Type type)
    {
        foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
        {
            yield return nestedType;

            foreach (var child in GetNestedTypesRecursive(nestedType))
            {
                yield return child;
            }
        }
    }

    private static string GetSnapshotPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, SnapshotFileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            var projectFile = Path.Combine(directory.FullName, "Hector.ArchitectureTests.csproj");
            if (File.Exists(projectFile))
            {
                return Path.Combine(directory.FullName, SnapshotFileName);
            }

            directory = directory.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, SnapshotFileName);
    }

    private static string NormalizeLineEndings(string value)
        => value.Replace("\r\n", "\n").TrimEnd();
}
