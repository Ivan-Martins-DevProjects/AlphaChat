public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Unauthorized")
        : base("UNAUTHORIZED", message, 401)
    {
    }
}
