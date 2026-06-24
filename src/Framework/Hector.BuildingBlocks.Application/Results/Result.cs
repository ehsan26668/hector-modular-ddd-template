namespace Hector.BuildingBlocks.Application.Results;

public readonly struct Result
{
    private readonly Error? _error;

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error =>
        IsFailure
            ? _error!
            : throw new InvalidOperationException("Success result has no error.");

    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        _error = error;
    }

    public static Result Success()
        => new(true, null);

    public static Result Failure(Error error)
        => new(false, error);
}