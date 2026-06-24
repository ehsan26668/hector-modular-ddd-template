namespace Hector.BuildingBlocks.Application.Results;

public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("Failure result has no value.");

    public Error Error =>
        IsFailure
            ? _error!
            : throw new InvalidOperationException("Success result has no error.");

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
        _error = null;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        _error = error;
        _value = default;
    }

    public static Result<T> Success(T value)
        => new(value);

    public static Result<T> Failure(Error error)
        => new(error);
}