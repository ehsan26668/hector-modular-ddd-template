namespace Hector.BuildingBlocks.Application.Results;

public sealed record Error(
    string Code,
    string Message,
    ErrorCategory Category,
    IReadOnlyDictionary<string, object>? Metadata = null
);