using Hector.BuildingBlocks.Application.Results;

namespace Hector.Modules.Projects.Application.Errors;

public static class ProjectErrors
{
    public static readonly Error ProjectAlreadyExists = new(
        "PROJECT_ALREADY_EXISTS",
        "A project with the same name already exists.",
        ErrorCategory.Conflict);

    public static readonly Error ProjectNotFound = new(
        "PROJECT_NOT_FOUND",
        "The project was not found.",
        ErrorCategory.NotFound);
}
