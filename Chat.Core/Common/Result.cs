using System.Runtime.CompilerServices;

namespace Chat.Core.Common;


public class Result(bool isSuccess, string? errorMessage = null, Exception? exception = null)
{
    public bool IsSuccess { get; } = isSuccess;
    public string? ErrorMessage { get; } = errorMessage;
    public Exception? Exception { get; } = exception;

    // Static factory methods for success and failure cases
    public static Result Success() 
    => new(isSuccess: true, errorMessage: null, exception: null); 
    public static Result Failure(string errorMessage, Exception? ex = default) 
    => new(isSuccess: false, errorMessage: errorMessage, exception: ex);

    public static Result Failure()
    {
        throw new NotImplementedException();
    }
}

public class Result<T>(bool isSuccess, string? errorMessage = null, Exception? exception = null, T? value = default)
{
    public T? Value { get; } = value;
    public bool IsSuccess { get; } = isSuccess;
    public string? ErrorMessage { get; } = errorMessage;
    public Exception? Exception { get; } = exception;

    // Static factory methods for success and failure cases
    public static Result<T> Success(T value) 
    => new(isSuccess: true, errorMessage: null, exception: null, value: value); 
    public static Result<T> Failure(string errorMessage, Exception? ex = default) 
    => new(isSuccess: false, errorMessage: errorMessage, exception: ex, value: default);
}