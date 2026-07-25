public class AppException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    public AppException(string code, string message, int statusCode = 500)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    public AppException(string code, string message, Exception inner, int statusCode = 500)
        : base(message, inner)
    {
        Code = code;
        StatusCode = statusCode;
    }
}
