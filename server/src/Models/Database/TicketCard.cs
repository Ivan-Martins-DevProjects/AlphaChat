using System.Text.Json.Serialization;

public class TicketCard
{
    public Guid ConversationId { get; set; }
    public string? ConversationType { get; set; }

    public Guid? Owner { get; set; }

    public Guid TicketId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public Guid ContactId { get; set; }
    public Guid? ContactOwnerId { get; set; }
    public string ContactDisplayName { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? ContactProfilePic { get; set; }

    public TagItem[] Tags { get; set; } = [];

    public string? LastMessageContent { get; set; }
    public DateTime? LastMessageCreatedAt { get; set; }

    public bool Read { get; set; }
}

public class TagItem
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("colorCode")]
    public string ColorCode { get; set; } = string.Empty;
}
