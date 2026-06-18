using Hector.BuildingBlocks.Application.Messaging;
using Hector.Modules.Projects.Domain;

namespace Hector.Modules.Projects.Application.Commands;

public sealed class CreateProjectCommandHandler(
    IProjectRepository repository)
        : ICommandHandler<CreateProjectCommand, ProjectId>
{

    public async Task<ProjectId> HandleAsync(
        CreateProjectCommand request,
        CancellationToken cancellationToken = default)
    {
        var project = Project.Create(request.Name);

        await repository.AddAsync(project, cancellationToken);

        return project.Id;
    }
}