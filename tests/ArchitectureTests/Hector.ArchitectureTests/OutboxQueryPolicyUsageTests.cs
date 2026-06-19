using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Hector.BuildingBlocks.Persistence.Outbox;

namespace Hector.ArchitectureTests;

public class OutboxIsPoisonedUsageTests
{
    private static readonly Assembly PersistenceAssembly = typeof(OutboxProcessor).Assembly;
    private static readonly Type AllowedType = typeof(OutboxProcessingPolicy);
    private static readonly Type TargetType = typeof(OutboxMessage);

    private static readonly OpCode[] SingleByteOpCodes = new OpCode[0x100];
    private static readonly OpCode[] MultiByteOpCodes = new OpCode[0x100];

    private const string TargetGetterName = "get_IsPoisoned";

    static OutboxIsPoisonedUsageTests()
    {
        foreach (var field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.GetValue(null) is not OpCode opCode)
                continue;

            var value = unchecked((ushort)opCode.Value);

            if (value < 0x100)
            {
                SingleByteOpCodes[value] = opCode;
                continue;
            }

            if ((value & 0xFF00) == 0xFE00)
            {
                MultiByteOpCodes[value & 0xFF] = opCode;
            }
        }
    }

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
            // skip allowed type completely
            if (type == AllowedType)
                continue;

            foreach (var method in GetAllMethods(type))
            {
                if (CallsIsPoisonedGetter(method))
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
            .Where(t =>
                t.IsClass &&
                t.Namespace is not null &&
                !t.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false));
    }

    private static IEnumerable<MethodInfo> GetAllMethods(Type type)
    {
        return type.GetMethods(
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly);
    }

    private static bool CallsIsPoisonedGetter(MethodInfo method)
    {
        var body = method.GetMethodBody();
        if (body is null)
            return false;

        var il = body.GetILAsByteArray();
        if (il is null || il.Length == 0)
            return false;

        var offset = 0;

        while (offset < il.Length)
        {
            if (!TryReadOpCode(il, ref offset, out var opCode))
                return false;

            if (opCode == OpCodes.Call || opCode == OpCodes.Callvirt)
            {
                if (!TryReadInt32Operand(il, ref offset, out var metadataToken))
                    return false;

                if (ResolvesToIsPoisonedGetter(method, metadataToken))
                    return true;

                continue;
            }

            if (!TrySkipOperand(il, ref offset, opCode))
                return false;
        }

        return false;
    }

    private static bool TryReadOpCode(byte[] il, ref int offset, out OpCode opCode)
    {
        opCode = default;

        if (offset >= il.Length)
            return false;

        var firstByte = il[offset++];

        if (firstByte != 0xFE)
        {
            opCode = SingleByteOpCodes[firstByte];
            return opCode.Size != 0;
        }

        if (offset >= il.Length)
            return false;

        var secondByte = il[offset++];
        opCode = MultiByteOpCodes[secondByte];

        return opCode.Size != 0;
    }

    private static bool TryReadInt32Operand(byte[] il, ref int offset, out int value)
    {
        value = default;

        if (offset + sizeof(int) > il.Length)
            return false;

        value = BitConverter.ToInt32(il, offset);
        offset += sizeof(int);

        return true;
    }

    private static bool TrySkipOperand(byte[] il, ref int offset, OpCode opCode)
    {
        var operandSize = GetFixedOperandSize(opCode.OperandType);

        if (operandSize is not null)
        {
            if (offset + operandSize.Value > il.Length)
                return false;

            offset += operandSize.Value;
            return true;
        }

        if (opCode.OperandType == OperandType.InlineSwitch)
        {
            if (offset + sizeof(int) > il.Length)
                return false;

            var caseCount = BitConverter.ToInt32(il, offset);
            offset += sizeof(int);

            var switchOperandSize = caseCount * sizeof(int);

            if (caseCount < 0 || offset + switchOperandSize > il.Length)
                return false;

            offset += switchOperandSize;
            return true;
        }

        return false;
    }

    private static int? GetFixedOperandSize(OperandType operandType)
    {
        return operandType switch
        {
            OperandType.InlineNone => 0,

            OperandType.ShortInlineBrTarget => 1,
            OperandType.ShortInlineI => 1,
            OperandType.ShortInlineVar => 1,

            OperandType.InlineVar => 2,

            OperandType.InlineBrTarget => 4,
            OperandType.InlineField => 4,
            OperandType.InlineI => 4,
            OperandType.InlineMethod => 4,
            OperandType.InlineSig => 4,
            OperandType.InlineString => 4,
            OperandType.InlineTok => 4,
            OperandType.InlineType => 4,

            OperandType.InlineI8 => 8,
            OperandType.InlineR => 8,

            OperandType.ShortInlineR => 4,

            OperandType.InlineSwitch => null,

            _ => null
        };
    }

    private static bool ResolvesToIsPoisonedGetter(MethodInfo scanningMethod, int metadataToken)
    {
        try
        {
            var module = scanningMethod.Module;

            var genericTypeArguments =
                scanningMethod.DeclaringType?.IsGenericType == true
                    ? scanningMethod.DeclaringType.GetGenericArguments()
                    : null;

            var genericMethodArguments =
                scanningMethod.IsGenericMethod
                    ? scanningMethod.GetGenericArguments()
                    : null;

            var member = module.ResolveMember(
                metadataToken,
                genericTypeArguments,
                genericMethodArguments);

            return member is MethodInfo methodInfo &&
                   methodInfo.Name == TargetGetterName &&
                   methodInfo.DeclaringType == TargetType;
        }
        catch
        {
            return false;
        }
    }
}
