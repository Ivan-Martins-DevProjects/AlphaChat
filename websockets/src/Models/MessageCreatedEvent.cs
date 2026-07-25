namespace Websockets.Models;

public class MessageCreatedEvent
{
    public Guid MessageId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = "";
    public string MessageType { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
