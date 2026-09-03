namespace Masareef.Entities.Common;


public enum ErrorType { None, NotFound, Validation, Conflict }
public class Result<T>
{
    public bool Success { get; private set; }
    public T? Data { get; private set; }
    public string? Message { get; private set; }

    public ErrorType Error { get; private set; }
    private Result() { }

    public static Result<T> Ok(T data) => new() { Success = true, Data = data };
    //public static Result<T> Fail(string msg) => new() { Success = false, Message = msg };

    public static Result<T> Fail(string msg, ErrorType e = ErrorType.Validation)
        => new() { Success = false, Message = msg, Error = e };
    public static Result<T> NotFound(string msg)
        => new() { Success = false, Message = msg, Error = ErrorType.NotFound };

}

public class Result
{
    public bool Success { get; private set; }
    public string? Message { get; private set; }

    private Result() { }

    public static Result Ok() => new() { Success = true };
    public static Result Fail(string msg) => new() { Success = false, Message = msg };
}