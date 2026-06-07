using Hector.Modules.Projects.Domain;

namespace Hector.Modules.Projects.Infrastructure.Persistence;

public sealed class ProjectRepository : IProjectRepository
{
    private readonly ProjectsDbContext _context;

    public ProjectRepository(ProjectsDbContext context) => _context = context;

    public async Task AddAsync(Project project, CancellationToken cancellationToken)
    {
        await _context.Projects.AddAsync(project, cancellationToken);
    }
}