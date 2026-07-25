public class SendMessageRequest
{
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = "";
    public string MessageType { get; set; } = "text";
}
