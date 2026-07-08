using System.Reflection;
using FluentAssertions;
using Hector.ArchitectureTests.Common;

namespace Hector.ArchitectureTests.Framework.Unit.RuleTests;

[Trait("Category", "ArchitectureTests")]
[Trait("ADR", "ADR-0057")]
[Trait("Validation", "Framework")]
public sealed class TypeSelectionTests
{
    [Fact(DisplayName = "ADR-0057 | TC-01 | Should aggregate types from all assemblies when multi-assembly selection is used")]
    public void Should_AggregateTypesFromAllAssemblies_When_MultiAssemblySelectionIsUsed()
    {
        // Arrange
        var buildingBlocks = AssemblyCatalog.BuildingBlocks;

        // Find two distinct assemblies within the building blocks to test aggregation
        var domainAssembly = buildingBlocks.FirstOrDefault(a => a.GetName().Name!.EndsWith(".Domain", StringComparison.Ordinal));
        var applicationAssembly = buildingBlocks.FirstOrDefault(a => a.GetName().Name!.EndsWith(".Application", StringComparison.Ordinal));

        // Fallback to domain and application layers if building blocks are empty/not loaded
        domainAssembly ??= AssemblyCatalog.Domain.First();
        applicationAssembly ??= AssemblyCatalog.Application.First();

        var targetAssemblies = new[] { domainAssembly, applicationAssembly };
        var sut = ArchitectureRule.Types();

        // Act
        var filter = sut.That(targetAssemblies);

        var resultTypes = filter
            .ResideInNamespace("Hector")
            .GetTypes();

        // Assert
        resultTypes.Should()
                   .NotBeEmpty("because types should be aggregated from all provided assemblies");
        resultTypes.Should()
                   .Contain(t => t.Assembly
                                 == domainAssembly,
                                 "because types from the domain assembly must be aggregated");
        resultTypes.Should()
                   .Contain(t => t.Assembly
                                 == applicationAssembly,
                                 "because types from the application assembly must be aggregated");
    }

    [Fact(DisplayName = "ADR-0057 | TC-02 | Should throw ArgumentNullException when assemblies collection is null")]
    public void Should_ThrowArgumentNullException_When_AssembliesCollectionIsNull()
    {
        // Arrange
        IEnumerable<Assembly>? assemblies = null;
        var sut = ArchitectureRule.Types();

        // Act
        var act = () => sut.That(assemblies!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "ADR-0057 | TC-03 | Should include types only from selected assemblies")]
    public void Should_IncludeTypesOnlyFromSelectedAssemblies_When_UsingMultiAssemblySelection()
    {
        // Arrange
        var domainAssembly = AssemblyCatalog.Domain.First();
        var targetAssemblies = new[] { domainAssembly };
        var sut = ArchitectureRule.Types();

        // Act
        var filter = sut.That(targetAssemblies);

        var types = filter
            .ResideInNamespace("Hector")
            .GetTypes();

        // Assert
        types.Should().NotBeEmpty();
        types.Should()
             .OnlyContain(t => t.Assembly
                               == domainAssembly,
                               "because only types from the selected assembly should be loaded without leakage from other assemblies");
    }
}