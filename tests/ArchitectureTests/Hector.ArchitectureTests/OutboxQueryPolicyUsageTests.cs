using System.Reflection;
using FluentAssertions;
using Hector.BuildingBlocks.Persistence.Outbox;
using Xunit;

namespace Hector.ArchitectureTests;

public class OutboxIsPoisonedUsageTests
{
    private static readonly Assembly PersistenceAssembly = typeof(OutboxProcessor).Assembly;
    private static readonly Type AllowedType = typeof(OutboxProcessingPolicy);
    private static readonly Type TargetType = typeof(OutboxMessage);

    private const string TargetGetterName = "get_IsPoisoned";

    [Fact]
    public void Only_OutboxProcessingPolicy_May_Access_IsPoisoned_Property()
    {
        // Arrange
        var violations = FindIllegalIsPoisonedUsages();

        // Act
        var hasViolations = violations.Any();

        // Assert
        hasViolations.Should().BeFalse(
            $"Only {AllowedType.Name} is allowed to access {TargetType.Name}.IsPoisoned. " +
            $"Violations found in: {string.Join(", ", violations)}");
    }

    // ------------------------------
    // Internal Architecture Scanner
    // ------------------------------

    private static IReadOnlyList<string> FindIllegalIsPoisonedUsages()
    {
        var violations = new List<string>();

        foreach (var type in GetCandidateTypes())
        {
            foreach (var method in GetAllMethods(type))
            {
                if (CallsIsPoisonedGetter(method) && type != AllowedType)
                {
                    violations.Add($"{type.FullName}.{method.Name}");
                }
            }
        }

        return violations;
    }

    private static IEnumerable<Type> GetCandidateTypes()
    {
        return PersistenceAssembly
            .GetTypes()
            .Where(t => t.IsClass && t.Namespace != null);
    }

    private static IEnumerable<MethodInfo> GetAllMethods(Type type)
    {
        return type.GetMethods(
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic);
    }

    private static bool CallsIsPoisonedGetter(MethodInfo method)
    {
        var body = method.GetMethodBody();
        if (body == null) return false;

        var il = body.GetILAsByteArray();
        if (il == null || il.Length == 0) return false;

        var module = method.Module;

        for (int i = 0; i < il.Length; i++)
        {
            if (IsCallOpcode(il[i]))
            {
                var token = BitConverter.ToInt32(il, i + 1);

                if (ResolvesToIsPoisonedGetter(module, token))
                    return true;
            }
        }

        return false;
    }

    private static bool IsCallOpcode(byte opcode)
        => opcode is 0x28 or 0x6F; // call / callvirt

    private static bool ResolvesToIsPoisonedGetter(Module module, int token)
    {
        try
        {
            var member = module.ResolveMember(token);

            return member is MethodInfo mi &&
                   mi.Name == TargetGetterName &&
                   mi.DeclaringType == TargetType;
        }
        catch
        {
            return false;
        }
    }
}
