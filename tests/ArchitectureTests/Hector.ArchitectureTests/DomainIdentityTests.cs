using System.Reflection;
using FluentAssertions;
using Hector.BuildingBlocks.Domain;
using Hector.Modules.Projects.Domain;

namespace Hector.ArchitectureTests;

public sealed class DomainIdentityTests
{
    [Fact]
    public void Should_NotUseGuidNewGuid_When_InsideDomainAsseblies()
    {
        // Arrange
        var domainAssemblies = new[]
        {
          typeof(DomainAssemblyMarker).Assembly,
          typeof(ProjectsDomainAssemblyMarker).Assembly
        };

        var guidNewGuid = typeof(Guid)
            .GetMethod(nameof(Guid.NewGuid), BindingFlags.Public | BindingFlags.Static)!;

        var violations = new List<string>();

        // Act
        foreach (var assembly in domainAssemblies)
        {
            var types = assembly.GetTypes()
                .Where(t => t.Namespace != null && t.Namespace.Contains("Domain"));

            foreach (var type in types)
            {
                var methods = type.GetMethods(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.DeclaredOnly);

                foreach (var method in methods)
                {
                    var body = method.GetMethodBody();
                    if (body == null)
                        continue;

                    var il = body.GetILAsByteArray();
                    if (il == null)
                        continue;

                    var callsGuidNewGuid = method
                        .GetMethodBody()!
                        .LocalVariables
                        .Any(); // placeholder check

                    if (MethodCallsGuidNewGuid(method, guidNewGuid))
                    {
                        violations.Add($"{type.FullName}.{method.Name}");
                    }
                }
            }
        }

        // Assert
        violations.Should().BeEmpty(
            "Domain layer must not generate identifiers using Guid.NewGuid() directly. Use StronglyTypedId.New() as defined in ADR-0018.");
    }

    private static bool MethodCallsGuidNewGuid(MethodInfo method, MethodInfo target)
    {
        var body = method.GetMethodBody();
        if (body == null)
            return false;

        var module = method.Module;
        var il = body.GetILAsByteArray();

        for (int i = 0; i < il!.Length - 4; i++)
        {
            if (il[i] == 0x28) // call opcode
            {
                var token = BitConverter.ToInt32(il, i + 1);

                try
                {
                    var called = module.ResolveMethod(token);
                    if (called == target)
                        return true;
                }
                catch
                {
                    // ignore resolution failures
                }
            }
        }

        return false;
    }
}