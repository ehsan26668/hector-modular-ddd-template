using System.Reflection;
using System.Runtime.CompilerServices;

namespace Hector.ArchitectureTests.Common;

internal static class ReflectionExtensions
{
    public static bool IsCompilerGenerated(this Type type)
    {
        return type.GetCustomAttributes(
            typeof(CompilerGeneratedAttribute),
            inherit: false).Length != 0;
    }

    public static bool HasAttribute<T>(this MemberInfo member)
        where T : Attribute
    {
        return member.GetCustomAttributes(typeof(T), inherit: true).Length != 0;
    }

    public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(t => t is not null)!;
        }
    }
}
