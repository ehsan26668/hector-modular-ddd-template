using FluentAssertions;
using Hector.BuildingBlocks.Application;
using Hector.BuildingBlocks.Domain;
using Hector.Modules.Projects.Domain;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests;

public sealed class LayerDependencyTests
{
    private const string DomainNamespace = "Hector.Modules.Projects.Domain";
    private const string ApplicationNamespace = "Hector.Modules.Projects.Application";
    private const string InfrastructureNamespace = "Hector.Modules.Projects.Infrastructure";

    [Fact]
    public void Should_NotDependOnApplication_When_InDomainLayer()
    {
        // Arrange
        var domainAssembly = typeof(DomainAssemblyMarker).Assembly;

        // Act
        var result = Types
            .InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Should_NotDependOnInfrastructure_When_InDomainLayer()
    {
        // Arrange
        var domainAssembly = typeof(DomainAssemblyMarker).Assembly;

        // Act
        var result = Types
            .InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Should_NotDependOnInfrastructure_When_InApplicationLayer()
    {
        // Arrange
        var applicationAssembly = typeof(ApplicationAssemblyMarker).Assembly;

        // Act
        var result = Types
            .InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Should_NotDependOnOtherModules_When_InFeatureModule()
    {
        // Arrange
        var domainAssembly = typeof(ProjectsDomainAssemblyMarker).Assembly;

        // Act
        var result = Types
            .InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Hector.Modules.")
            .GetResult();

        // Assert

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Should_InheritFromStronglyTypedId_When_DefiningDomainIdentifiers()
    {
        // Arrange

        var domainAssembly = typeof(ProjectId).Assembly;

        // Act

        var result = Types
            .InAssembly(domainAssembly)
            .That()
            .HaveNameEndingWith("Id")
            .Should()
            .Inherit(typeof(BuildingBlocks.Domain.Primitives.StronglyTypedId<>))
            .GetResult();

        // Assert

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Should_NotDependOnContracts_When_InDomainLayer()
    {
        // Arrange
        var domainAssembly = typeof(ProjectsDomainAssemblyMarker).Assembly;

        // Act
        var result = Types
            .InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Hector.Modules.Projects.Contracts")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain layer must not depend on integration contracts. Contracts belong to cross-module communication and must remain outside the domain model.");
    }
}