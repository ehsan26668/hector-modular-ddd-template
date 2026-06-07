using Hector.BuildingBlocks.Application.Messaging;
using Hector.Modules.Projects.Domain;

namespace Hector.Modules.Projects.Application.Commands;

public sealed class CreateProjectCommandHandler : ICommandHandler<CreateProjectCommand, ProjectId>
{
    private readonly IProjectRepository _repository;

    public CreateProjectCommandHandler(IProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProjectId> HandleAsync(
        CreateProjectCommand request,
        CancellationToken cancellationToken = default)
    {
        var project = Project.Create(request.Name);

        await _repository.AddAsync(project, cancellationToken);

        return project.Id;
    }
}