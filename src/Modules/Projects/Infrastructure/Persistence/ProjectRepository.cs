using Hector.Modules.Projects.Domain;

namespace Hector.Modules.Projects.Infrastructure.Persistence;

public sealed class ProjectRepository(
    ProjectsDbContext context)
    : IProjectRepository
{
    public async Task AddAsync(Project project, CancellationToken cancellationToken)
    {
        await context.Projects.AddAsync(project, cancellationToken);
    }
}