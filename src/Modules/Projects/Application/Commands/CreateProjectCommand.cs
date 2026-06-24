using Hector.BuildingBlocks.Application.Messaging;
using Hector.Modules.Projects.Domain;

namespace Hector.Modules.Projects.Application.Commands;

public sealed record CreateProjectCommand(string Name) : ICommand<ProjectId>;