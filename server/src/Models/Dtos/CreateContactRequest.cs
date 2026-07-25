public class CreateContactRequest
{
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Company { get; set; }
    public string? Document { get; set; }
    public string? Notes { get; set; }
    public Guid[]? TagIds { get; set; }
}
