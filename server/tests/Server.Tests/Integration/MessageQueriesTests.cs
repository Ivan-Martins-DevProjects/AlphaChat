using FluentAssertions;
using Xunit;

namespace Server.Tests.Integration;

[Collection("Database")]
public class MessageQueriesTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;

    public MessageQueriesTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
        await _fixture.SeedTestDataAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetMessagesByConversation_ExistingConversation_ReturnsMessages()
    {
        var conversationId = Guid.Parse("1b000000-0000-0000-0000-000000000001");

        var messages = await MessageQueries.GetMessagesByConversation(_fixture.DataSource, conversationId, 0);

        messages.Should().NotBeEmpty();
        messages.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMessagesByConversation_NonExistingConversation_ReturnsEmpty()
    {
        var conversationId = Guid.NewGuid();

        var messages = await MessageQueries.GetMessagesByConversation(_fixture.DataSource, conversationId, 0);

        messages.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMessagesByConversation_WithOffset_ReturnsCorrectPage()
    {
        var conversationId = Guid.Parse("1b000000-0000-0000-0000-000000000001");

        var messages = await MessageQueries.GetMessagesByConversation(_fixture.DataSource, conversationId, 1);

        messages.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetMessagesByConversation_ReturnsMessagesInDescendingOrder()
    {
        var conversationId = Guid.Parse("1b000000-0000-0000-0000-000000000001");

        var messages = await MessageQueries.GetMessagesByConversation(_fixture.DataSource, conversationId, 0);

        var dates = messages.Select(m => m.CreatedAt).ToList();
        dates.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GetMessagesByConversation_MessageHasCorrectProperties()
    {
        var conversationId = Guid.Parse("1b000000-0000-0000-0000-000000000001");

        var messages = await MessageQueries.GetMessagesByConversation(_fixture.DataSource, conversationId, 0);

        var first = messages.First();
        first.Id.Should().NotBe(Guid.Empty);
        first.ConversationId.Should().Be(conversationId);
        first.SenderId.Should().NotBe(Guid.Empty);
        first.Content.Should().NotBeNullOrEmpty();
        first.MessageType.Should().Be("text");
        first.IsEdited.Should().BeFalse();
        first.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task SaveMessage_ValidData_ReturnsMessage()
    {
        var conversationId = Guid.Parse("1b000000-0000-0000-0000-000000000001");
        var senderId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");
        var content = "Test message from integration test";

        var message = await MessageQueries.SaveMessage(_fixture.DataSource, conversationId, senderId, content, "text");

        message.Should().NotBeNull();
        message.Id.Should().NotBe(Guid.Empty);
        message.ConversationId.Should().Be(conversationId);
        message.SenderId.Should().Be(senderId);
        message.Content.Should().Be(content);
        message.MessageType.Should().Be("text");
        message.IsEdited.Should().BeFalse();
        message.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task SaveMessage_MessageIsPersisted()
    {
        var conversationId = Guid.Parse("1b000000-0000-0000-0000-000000000001");
        var senderId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");
        var content = "Persisted message";

        var saved = await MessageQueries.SaveMessage(_fixture.DataSource, conversationId, senderId, content, "text");

        var messages = await MessageQueries.GetMessagesByConversation(_fixture.DataSource, conversationId, 0);
        messages.Should().Contain(m => m.Id == saved.Id);
    }

    [Fact]
    public async Task SaveMessage_MarksConversationAsUnread()
    {
        var conversationId = Guid.Parse("1b000000-0000-0000-0000-000000000001");
        var senderId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");

        // First mark as read
        await ConversationQueries.MarkAsRead(_fixture.DataSource, conversationId);

        // Save a message
        await MessageQueries.SaveMessage(_fixture.DataSource, conversationId, senderId, "New message", "text");

        // Check conversation is now unread
        var conversations = await ConversationQueries.GetAllConversations(_fixture.DataSource, 0, 10);
        var conversation = conversations.FirstOrDefault(c => c.ConversationId == conversationId);
        conversation.Should().NotBeNull();
        conversation!.Read.Should().BeFalse();
    }
}
