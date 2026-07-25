public record ErrorResponse
{
    public string Code { get; init; } = "";
    public string Message { get; init; } = "";
    public Dictionary<string, string[]>? Errors { get; init; }
}
