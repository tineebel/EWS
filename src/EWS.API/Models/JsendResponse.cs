namespace EWS.API.Models;

public class JsendResponse
{
    public string Status { get; init; } = "success";
    public object Data { get; init; } = new { };
    public string? Message { get; init; }

    public static JsendResponse Success<T>(T data) =>
        new() { Status = "success", Data = data! };

    public static JsendResponse Fail(string message, object? data = null) =>
        new() { Status = "fail", Data = data ?? new { }, Message = message };

    public static JsendResponse Error(string message) =>
        new() { Status = "error", Data = new { }, Message = message };
}
