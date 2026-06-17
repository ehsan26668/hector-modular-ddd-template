using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Persistence;
using Hector.Modules.Projects.Application;
using Hector.Modules.Projects.Domain;
using Hector.Modules.Projects.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests;

public sealed class ModuleLayerRulesTests
{
    private const string DomainNamespace = "Hector.Modules.Projects.Domain";
    private const string ApplicationNamespace = "Hector.Modules.Projects.Application";
    private const string InfrastructureNamespace = "Hector.Modules.Projects.Infrastructure";

    [Fact]
    public void Should_NotDependOnApplication_When_InDomainLayer()
    {
        // Arrange
        var domainAssembly = typeof(ProjectsDomainAssemblyMarker).Assembly;

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
        var domainAssembly = typeof(ProjectsDomainAssemblyMarker).Assembly;

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
    public void Should_HaveSingleDbContext_When_ModuleUsesPersistence()
    {
        // Arrange
        var infrastructureAssembly =
            typeof(ProjectsDbContext).Assembly;

        // Act
        var dbContexts = infrastructureAssembly
            .GetTypes()
            .Where(t =>
                typeof(DbContext).IsAssignableFrom(t) &&
                !t.IsAbstract)
            .ToList();

        // Assert
        dbContexts.Should().ContainSingle(
            "Each module must define exactly one DbContext according to ADR-0020.");
    }

    [Fact]
    public void Should_HaveSingleModuleCompositionRoot_When_ModuleExists()
    {
        // Arrange
        var infrastructureAssembly =
            typeof(ProjectsDbContext).Assembly;

        // Act
        var modules = infrastructureAssembly
            .GetTypes()
            .Where(t =>
                typeof(IModule).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsInterface)
            .ToList();

        // Assert
        modules.Should().ContainSingle(
            "Each module must define exactly one module composition root according to ADR-0037.");
    }

    [Fact]
    public void Should_NotDependOnInfrastructure_When_InApplicationLayer()
    {
        // Arrange
        var applicationAssembly =
            typeof(ProjectsApplicationAssemblyMarker).Assembly;

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
    public void Should_HaveStronglyTypedIdAssemblyProvider_When_ModuleUsesStronglyTypedIds()
    {
        // Arrange
        var infrastructureAssembly =
            typeof(ProjectsDbContext).Assembly;

        // Act
        var providers = infrastructureAssembly
            .GetTypes()
            .Where(t =>
                typeof(IStronglyTypedIdAssemblyProvider).IsAssignableFrom(t) &&
                !t.IsAbstract)
            .ToList();

        // Assert
        providers.Should().ContainSingle(
            "Each module must expose exactly one StronglyTypedIdAssemblyProvider according to ADR-0019.");
    }
}
