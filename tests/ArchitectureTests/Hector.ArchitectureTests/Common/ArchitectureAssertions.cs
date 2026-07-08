using System.Reflection;
using FluentAssertions;
using Mono.Cecil;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests.Common;

internal static class ArchitectureAssertions
{
    public static void ShouldFollowTestMethodNamingConvention(IEnumerable<MethodInfo> methods)
    {
        var invalidMethods = methods
            .Where(m =>
                !m.Name.StartsWith("Should_", StringComparison.Ordinal) ||
                !m.Name.Contains("_When_", StringComparison.Ordinal))
            .Select(m => $"{m.DeclaringType?.FullName ?? "<UnknownType>"}.{m.Name}")
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

        if (invalidMethods.Count == 0)
            return;

        var message =
            $"""
            All test methods must follow naming convention:

            Should_<ExpectedBehavior>_When_<Condition>

            Found {invalidMethods.Count} invalid test method(s):

            {string.Join(Environment.NewLine, invalidMethods.Select(x => $"- {x}"))}

            Examples of valid names:
            - Should_ReturnValidationErrors_When_RequestIsInvalid
            - Should_ResolveQueryHandlers_When_ModulesAreLoaded
            - Should_EnforceUniqueConstraint_When_DuplicateMessageIsSaved
            """;

        Assert.Fail(message);
    }

    public static void ShouldNotUseGuidNewGuidForStronglyTypedIds(IEnumerable<Assembly> domainAssemblies)
    {
        var violations = new List<string>();

        foreach (var assembly in domainAssemblies)
        {
            var definition = AssemblyDefinition.ReadAssembly(assembly.Location);

            foreach (var type in definition.MainModule.Types)
            {
                var isStronglyTypedId = type.BaseType is not null &&
                                        type.BaseType.Name.StartsWith("StronglyTypedId", StringComparison.Ordinal);

                if (!isStronglyTypedId)
                    continue;

                foreach (var method in type.Methods.Where(m => m.HasBody))
                {
                    var usesGuidNewGuid = method.Body.Instructions.Any(inst =>
                        inst.OpCode.Name == "call" &&
                        inst.Operand is MethodReference mr &&
                        mr.FullName == "System.Guid System.Guid::NewGuid()");

                    if (usesGuidNewGuid)
                        violations.Add($"{type.FullName}.{method.Name}");
                }
            }
        }

        violations.Should().BeEmpty(
            "StronglyTypedId implementations must not generate identifiers using Guid.NewGuid() directly.");
    }

    public static void ShouldNotDependOnAny(
        IEnumerable<Assembly> sourceAssemblies,
        string[] forbiddenDependencies,
        string because)
    {
        var result = Types.InAssemblies(sourceAssemblies)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenDependencies)
            .GetResult();

        result.AssertSuccessful(because);
    }

    public static void ShouldExposeExactlyOneImplementationPerAssembly(
        IEnumerable<Assembly> assemblies,
        Type contractType,
        Func<Assembly, bool>? assemblyFilter,
        string because)
    {
        var targetAssemblies = assemblyFilter is null
            ? assemblies.ToList()
            : [.. assemblies.Where(assemblyFilter)];

        targetAssemblies.Should().NotBeEmpty(because);

        var violations = targetAssemblies
            .Select(a => new
            {
                AssemblyName = a.GetName().Name ?? "<UnknownAssembly>",
                Implementations = a.GetLoadableTypes()
                    .Where(t => contractType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                    .ToList()
            })
            .Where(x => x.Implementations.Count != 1)
            .Select(x => $"{x.AssemblyName} (found: {x.Implementations.Count})")
            .ToList();

        violations.Should().BeEmpty(because);
    }

    public static void ShouldOnlyDependOnContractsAcrossModules(
    IEnumerable<Assembly> moduleAssemblies,
    string because)
    {
        var violations = new List<string>();

        foreach (var sourceAssembly in moduleAssemblies)
        {
            var sourceAssemblyName = sourceAssembly.GetName().Name!;
            var sourceModuleName = GetModuleName(sourceAssemblyName);

            var forbiddenDependencies = moduleAssemblies
                .Select(assembly => assembly.GetName().Name!)
                .Where(targetAssemblyName =>
                {
                    var targetModuleName = GetModuleName(targetAssemblyName);

                    if (string.Equals(sourceModuleName, targetModuleName, StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return !targetAssemblyName.EndsWith(".Contracts", StringComparison.Ordinal);
                })
                .ToArray();

            var result = Types.InAssemblies([sourceAssembly])
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenDependencies)
                .GetResult();

            if (!result.IsSuccessful)
            {
                var failingTypes = result.FailingTypeNames?.Any() == true
                    ? string.Join(", ", result.FailingTypeNames)
                    : "<unknown types>";

                violations.Add($"{sourceAssemblyName}: {failingTypes}");
            }
        }

        violations.Should().BeEmpty(because);

        static string GetModuleName(string assemblyName)
        {
            var parts = assemblyName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 3 ? parts[2] : assemblyName;
        }
    }
}
