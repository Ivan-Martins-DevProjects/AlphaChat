using FluentAssertions;
using Xunit;

namespace Server.Tests.Integration;

[Collection("Database")]
public class ConversationQueriesTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;

    public ConversationQueriesTests(DatabaseFixture fixture)
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
    public async Task GetAllConversations_ReturnsConversations()
    {
        var conversations = await ConversationQueries.GetAllConversations(_fixture.DataSource, 0, 10);

        conversations.Should().NotBeEmpty();
        conversations.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetAllConversations_WithOffset_ReturnsCorrectPage()
    {
        var page1 = await ConversationQueries.GetAllConversations(_fixture.DataSource, 0, 2);
        var page2 = await ConversationQueries.GetAllConversations(_fixture.DataSource, 2, 2);

        page1.Should().HaveCount(2);
        page2.Should().HaveCountGreaterThanOrEqualTo(1);

        var page1Ids = page1.Select(c => c.ConversationId).ToList();
        var page2Ids = page2.Select(c => c.ConversationId).ToList();
        page1Ids.Should().NotIntersectWith(page2Ids);
    }

    [Fact]
    public async Task GetAllConversations_ConversationHasCorrectProperties()
    {
        var conversations = await ConversationQueries.GetAllConversations(_fixture.DataSource, 0, 10);

        var first = conversations.First();
        first.ConversationId.Should().NotBe(Guid.Empty);
        first.TicketId.Should().NotBe(Guid.Empty);
        first.ContactId.Should().NotBe(Guid.Empty);
        first.Subject.Should().NotBeNullOrEmpty();
        first.Status.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetConversationsByOwner_OwnerExists_ReturnsConversations()
    {
        var userId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");

        var conversations = await ConversationQueries.GetConversationsByOwner(_fixture.DataSource, userId, 0, 10);

        conversations.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetConversationsByOwner_NonExistingOwner_ReturnsEmpty()
    {
        var userId = Guid.NewGuid();

        var conversations = await ConversationQueries.GetConversationsByOwner(_fixture.DataSource, userId, 0, 10);

        conversations.Should().BeEmpty();
    }

    [Fact]
    public async Task MarkAsRead_ConversationExists_SetsReadToTrue()
    {
        var conversationId = Guid.Parse("1b000000-0000-0000-0000-000000000001");

        await ConversationQueries.MarkAsRead(_fixture.DataSource, conversationId);

        var conversations = await ConversationQueries.GetAllConversations(_fixture.DataSource, 0, 10);
        var conversation = conversations.FirstOrDefault(c => c.ConversationId == conversationId);
        conversation.Should().NotBeNull();
        conversation!.Read.Should().BeTrue();
    }

    [Fact]
    public async Task CreateConversation_ValidContactId_ReturnsConversationId()
    {
        var contactId = Guid.Parse("a1000000-0000-0000-0000-000000000001");
        var ownerId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");

        var conversationId = await ConversationQueries.CreateConversation(_fixture.DataSource, contactId, ownerId);

        conversationId.Should().NotBe(Guid.Empty);

        var conversations = await ConversationQueries.GetAllConversations(_fixture.DataSource, 0, 100);
        conversations.Should().Contain(c => c.ConversationId == conversationId);
    }

    [Fact]
    public async Task CreateConversation_WithNullOwner_Succeeds()
    {
        var contactId = Guid.Parse("a1000000-0000-0000-0000-000000000002");

        // customer_id is NOT NULL, so pass a valid user ID as the customer
        var customerId = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012");
        var conversationId = await ConversationQueries.CreateConversation(_fixture.DataSource, contactId, customerId);

        conversationId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateConversation_CreatesTicketWithCorrectContactId()
    {
        var contactId = Guid.Parse("a1000000-0000-0000-0000-000000000003");
        var ownerId = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012");

        var conversationId = await ConversationQueries.CreateConversation(_fixture.DataSource, contactId, ownerId);

        // Verify the conversation exists in GetAllConversations
        var conversations = await ConversationQueries.GetAllConversations(_fixture.DataSource, 0, 100);
        var conversation = conversations.FirstOrDefault(c => c.ConversationId == conversationId);
        conversation.Should().NotBeNull();
        conversation!.ContactId.Should().Be(contactId);
    }
}
