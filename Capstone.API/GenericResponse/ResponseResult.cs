namespace Capstone.API.GenericResponse;

public class ResponseResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ResponseResult<T> Ok(T? data, string message = "Request completed successfully.") => new()
    {
        Success = true,
        Message = message,
        Data = data,
        Errors = null
    };

    public static ResponseResult<T> Fail(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Data = default,
        Errors = errors
    };
}