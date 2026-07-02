using Hector.ArchitectureTests.Common;
using Hector.BuildingBlocks.Application.Messaging;

namespace Hector.ArchitectureTests;

public sealed class ModuleCompositionArchitectureTests : ArchitectureTestBase
{
    [Fact]
    public void Should_ExposeExactlyOneModuleCompositionRoot_When_AnalyzingModules()
    {
        // Arrange, Act & Assert
        ArchitectureAssertions.ShouldExposeExactlyOneImplementationPerAssembly(
            InfrastructureAssemblies,
            contractType: typeof(IModule),
            assemblyFilter: a => a.GetName().Name!.Contains(".Modules.", StringComparison.Ordinal),
            because: "ADR-0037 requires every module infrastructure assembly to expose exactly one composition root (IModule).");
    }
}
