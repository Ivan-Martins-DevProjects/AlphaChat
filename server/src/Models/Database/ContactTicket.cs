public class ContactTicket
{
    public Guid TicketId { get; set; }
    public string Subject { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public Guid ConversationId { get; set; }
}
