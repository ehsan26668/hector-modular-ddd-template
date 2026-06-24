using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Results;
using Hector.Modules.Projects.Domain;

namespace Hector.Modules.Projects.Application.Commands;

public sealed class CreateProjectCommandHandler(
    IProjectRepository repository)
    : ICommandHandler<CreateProjectCommand, ProjectId>
{

    public async Task<Result<ProjectId>> Handle(
        CreateProjectCommand request,
        CancellationToken cancellationToken = default)
    {
        var project = Project.Create(request.Name);

        await repository.AddAsync(project, cancellationToken);

        return Result<ProjectId>.Success(project.Id);
    }
}