namespace Hector.BuildingBlocks.Application.Results;

public enum ErrorCategory
{
    Validation = 1,
    NotFound = 2,
    Unauthorized = 3,
    Forbidden = 4,
    Conflict = 5,
    BusinessRule = 6,
    Infrastructure = 7,
    Unexpected = 8
}