public class UpdateContactRequest
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Company { get; set; }
    public string? Notes { get; set; }
    public Guid[]? TagIds { get; set; }
    public Guid? OwnerId { get; set; }
    public Guid? ContactOwnerId { get; set; }
    public string? TicketSubject { get; set; }
    public string? TicketStatus { get; set; }
}
