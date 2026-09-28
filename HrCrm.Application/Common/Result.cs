namespace HrCrm.Application.Common;

public class Result
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public ErrorType? ErrorType { get; set; }

    protected Result(bool isSuccess, string? error, ErrorType? errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string error, ErrorType errorType) => new(false, error, errorType);
}

public class Result<T> : Result
{
    public T? Value { get; set; }
    private Result(bool isSuccess, string? error, ErrorType? errorType, T? value) : base(isSuccess, error, errorType)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, null, null, value);
    public static Result<T> Failure(string error, ErrorType errorType) => new(false, error, errorType, default);
}