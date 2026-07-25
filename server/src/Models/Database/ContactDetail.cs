public class ContactDetail
{
    public Guid ContactId { get; set; }
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Company { get; set; }
    public string? Document { get; set; }
    public string? Notes { get; set; }
    public string? ProfilePic { get; set; }
    public Guid? ContactOwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public TagItem[] Tags { get; set; } = [];

    public OwnerInfo? Owner { get; set; }

    public TicketInfo Ticket { get; set; } = new();
}

public class OwnerInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? ProfilePic { get; set; }
}

public class TicketInfo
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
