namespace Hector.Modules.Projects.Domain;

public interface IProjectRepository
{
    Task AddAsync(Project project, CancellationToken cancellationToken);
}
