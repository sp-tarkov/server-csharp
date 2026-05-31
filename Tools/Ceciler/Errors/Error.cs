namespace Ceciler.Errors;

public class Error(int code, string message)
{
    public int Code { get; } = code;
    public string Message { get; } = message;
    public string Parameter { get; set; }
}
