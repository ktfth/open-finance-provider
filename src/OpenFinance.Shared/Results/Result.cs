namespace OpenFinance.Shared.Results;

public class Result
{
    protected Result(bool isSuccess, string? error = null)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    public static Result Success() => new(true);
    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
}

public class Result<T> : Result
{
    private Result(T value) : base(true) => Value = value;
    private Result(string error) : base(false, error) => Value = default!;

    public T Value { get; }

    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(string error) => new(error);
}
