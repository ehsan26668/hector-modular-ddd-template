using Hector.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.Modules.Projects.IntegrationTests;

public sealed class ProjectsIntegrationTestFixture : IDisposable
{
    private readonly TestApplicationFactory _factory;

    public ProjectsIntegrationTestFixture()
    {
        // Arrange
        _factory = new TestApplicationFactory();
    }

    public IServiceScope CreateScope()
    {
        return _factory.Services.CreateScope();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
