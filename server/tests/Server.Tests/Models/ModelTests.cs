using FluentAssertions;
using Xunit;

public class ModelTests
{
    // ============================================================
    // SendMessageRequest
    // ============================================================

    [Fact]
    public void SendMessageRequest_DefaultValues_AreCorrect()
    {
        var request = new SendMessageRequest();

        request.ConversationId.Should().Be(Guid.Empty);
        request.Content.Should().Be("");
        request.MessageType.Should().Be("text");
    }

    [Fact]
    public void SendMessageRequest_SetValues_AreStored()
    {
        var id = Guid.NewGuid();
        var request = new SendMessageRequest
        {
            ConversationId = id,
            Content = "Hello",
            MessageType = "image"
        };

        request.ConversationId.Should().Be(id);
        request.Content.Should().Be("Hello");
        request.MessageType.Should().Be("image");
    }

    // ============================================================
    // CreateConversationRequest
    // ============================================================

    [Fact]
    public void CreateConversationRequest_DefaultValues_AreCorrect()
    {
        var request = new CreateConversationRequest();

        request.ContactId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void CreateConversationRequest_SetValues_AreStored()
    {
        var contactId = Guid.NewGuid();
        var request = new CreateConversationRequest { ContactId = contactId };

        request.ContactId.Should().Be(contactId);
    }

    // ============================================================
    // CreateContactRequest
    // ============================================================

    [Fact]
    public void CreateContactRequest_DefaultValues_AreCorrect()
    {
        var request = new CreateContactRequest();

        request.Name.Should().Be("");
        request.Phone.Should().BeNull();
        request.Email.Should().BeNull();
        request.Company.Should().BeNull();
        request.Document.Should().BeNull();
        request.Notes.Should().BeNull();
        request.TagIds.Should().BeNull();
    }

    [Fact]
    public void CreateContactRequest_SetValues_AreStored()
    {
        var tagId = Guid.NewGuid();
        var request = new CreateContactRequest
        {
            Name = "Test",
            Phone = "123",
            Email = "test@test.com",
            Company = "Corp",
            Document = "123456",
            Notes = "Note",
            TagIds = new[] { tagId }
        };

        request.Name.Should().Be("Test");
        request.Phone.Should().Be("123");
        request.Email.Should().Be("test@test.com");
        request.Company.Should().Be("Corp");
        request.Document.Should().Be("123456");
        request.Notes.Should().Be("Note");
        request.TagIds.Should().HaveCount(1);
    }

    // ============================================================
    // UpdateContactRequest
    // ============================================================

    [Fact]
    public void UpdateContactRequest_DefaultValues_AreCorrect()
    {
        var request = new UpdateContactRequest();

        request.Name.Should().BeNull();
        request.Phone.Should().BeNull();
        request.Email.Should().BeNull();
        request.Company.Should().BeNull();
        request.Notes.Should().BeNull();
        request.TagIds.Should().BeNull();
    }

    // ============================================================
    // ContactDetail
    // ============================================================

    [Fact]
    public void ContactDetail_DefaultValues_AreCorrect()
    {
        var detail = new ContactDetail();

        detail.Name.Should().Be("");
        detail.Tags.Should().NotBeNull().And.BeEmpty();
        detail.Ticket.Should().NotBeNull();
    }

    [Fact]
    public void ContactDetail_WithOwner_ReturnsOwnerInfo()
    {
        var owner = new OwnerInfo
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            ProfilePic = "pic.jpg"
        };
        var detail = new ContactDetail { Owner = owner };

        detail.Owner.Should().NotBeNull();
        detail.Owner!.Name.Should().Be("Admin");
    }

    // ============================================================
    // TagItem
    // ============================================================

    [Fact]
    public void TagItem_DefaultValues_AreCorrect()
    {
        var tag = new TagItem();

        tag.Name.Should().Be("");
        tag.ColorCode.Should().Be("");
    }

    [Fact]
    public void TagItem_SetValues_AreStored()
    {
        var tag = new TagItem
        {
            Id = Guid.NewGuid(),
            Name = "VIP",
            ColorCode = "#ff0000"
        };

        tag.Name.Should().Be("VIP");
        tag.ColorCode.Should().Be("#ff0000");
    }

    // ============================================================
    // TicketCard
    // ============================================================

    [Fact]
    public void TicketCard_DefaultValues_AreCorrect()
    {
        var card = new TicketCard();

        card.Subject.Should().Be("");
        card.Status.Should().Be("");
        card.Tags.Should().NotBeNull().And.BeEmpty();
    }

    // ============================================================
    // ContactTicket
    // ============================================================

    [Fact]
    public void ContactTicket_DefaultValues_AreCorrect()
    {
        var ticket = new ContactTicket();

        ticket.Subject.Should().Be("");
        ticket.Status.Should().Be("");
        ticket.ClosedAt.Should().BeNull();
    }

    // ============================================================
    // MessageRow
    // ============================================================

    [Fact]
    public void MessageRow_DefaultValues_AreCorrect()
    {
        var message = new MessageRow();

        message.Content.Should().Be("");
        message.UpdatedAt.Should().BeNull();
    }

    // ============================================================
    // UserRow
    // ============================================================

    [Fact]
    public void UserRow_SetValues_AreStored()
    {
        var user = new UserRow
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Email = "admin@test.com",
            Role = "admin",
            ProfilePic = "pic.jpg"
        };

        user.Name.Should().Be("Admin");
        user.Email.Should().Be("admin@test.com");
        user.Role.Should().Be("admin");
    }
}
