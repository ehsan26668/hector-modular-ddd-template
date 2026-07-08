using System.Reflection;

namespace Hector.ArchitectureTests.Framework.Dsl;

public interface IModuleBoundarySelection
{
    IModuleBoundaryBuilder From(IEnumerable<Assembly> assemblies);
}