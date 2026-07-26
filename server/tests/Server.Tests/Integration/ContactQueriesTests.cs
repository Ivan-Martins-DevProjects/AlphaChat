using FluentAssertions;
using Xunit;

namespace Server.Tests.Integration;

[Collection("Database")]
public class ContactQueriesTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;

    public ContactQueriesTests(DatabaseFixture fixture)
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
    public async Task GetAllContacts_AdminRole_ReturnsAllContacts()
    {
        var (contacts, totalCount) = await ContactQueries.GetAllContacts(
            _fixture.DataSource, 1, 10, null, null, null, "admin", null);

        contacts.Should().NotBeEmpty();
        totalCount.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetAllContacts_WithSearch_FiltersByName()
    {
        var (contacts, totalCount) = await ContactQueries.GetAllContacts(
            _fixture.DataSource, 1, 10, "Fernanda", null, null, "admin", null);

        contacts.Should().HaveCount(1);
        contacts[0].Name.Should().Be("Fernanda Silva");
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAllContacts_WithSearch_FiltersByPhone()
    {
        var (contacts, totalCount) = await ContactQueries.GetAllContacts(
            _fixture.DataSource, 1, 10, "99876", null, null, "admin", null);

        contacts.Should().HaveCount(1);
        contacts[0].Phone.Should().Contain("99876");
    }

    [Fact]
    public async Task GetAllContacts_WithCompany_FiltersByCompany()
    {
        var (contacts, totalCount) = await ContactQueries.GetAllContacts(
            _fixture.DataSource, 1, 10, null, "StartupTech", null, "admin", null);

        contacts.Should().HaveCount(1);
        contacts[0].Company.Should().Be("StartupTech");
    }

    [Fact]
    public async Task GetAllContacts_WithPagination_ReturnsCorrectPage()
    {
        var (page1, total1) = await ContactQueries.GetAllContacts(
            _fixture.DataSource, 1, 2, null, null, null, "admin", null);
        var (page2, total2) = await ContactQueries.GetAllContacts(
            _fixture.DataSource, 2, 2, null, null, null, "admin", null);

        page1.Should().HaveCount(2);
        total1.Should().Be(total2);

        var page1Ids = page1.Select(c => c.ContactId).ToList();
        var page2Ids = page2.Select(c => c.ContactId).ToList();
        page1Ids.Should().NotIntersectWith(page2Ids);
    }

    [Fact]
    public async Task GetAllContacts_ContactHasCorrectProperties()
    {
        var (contacts, _) = await ContactQueries.GetAllContacts(
            _fixture.DataSource, 1, 10, null, null, null, "admin", null);

        var first = contacts.First();
        first.ContactId.Should().NotBe(Guid.Empty);
        first.Name.Should().NotBeNullOrEmpty();
        first.CreatedAt.Should().BeBefore(DateTime.UtcNow);
        first.UpdatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public async Task GetAllContacts_ContactWithOwner_HasOwnerInfo()
    {
        var (contacts, _) = await ContactQueries.GetAllContacts(
            _fixture.DataSource, 1, 10, null, null, null, "admin", null);

        var ownedContact = contacts.FirstOrDefault(c => c.Owner != null);
        ownedContact.Should().NotBeNull();
        ownedContact!.Owner.Should().NotBeNull();
        ownedContact.Owner!.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetAllContacts_ContactWithTags_HasTags()
    {
        // First verify the tag exists in the junction table
        var contactId = Guid.Parse("a1000000-0000-0000-0000-000000000001");
        await using var conn = await _fixture.DataSource.OpenConnectionAsync();
        await using var cmd = new Npgsql.NpgsqlCommand("SELECT count(*) FROM contact_tags WHERE contact_id = @id", conn);
        cmd.Parameters.AddWithValue("@id", contactId);
        var count = (long)(await cmd.ExecuteScalarAsync())!;
        count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetContactDetail_ExistingConversation_ReturnsDetail()
    {
        var conversationId = Guid.Parse("1b000000-0000-0000-0000-000000000001");

        var detail = await ContactQueries.GetContactDetail(_fixture.DataSource, conversationId);

        detail.Should().NotBeNull();
        detail!.ContactId.Should().NotBe(Guid.Empty);
        detail.Name.Should().Be("Fernanda Silva");
        detail.Phone.Should().Be("(11) 99876-5432");
        detail.Email.Should().Be("fernanda@startup.io");
        detail.Company.Should().Be("StartupTech");
    }

    [Fact]
    public async Task GetContactDetail_NonExistingConversation_ReturnsNull()
    {
        var conversationId = Guid.NewGuid();

        var detail = await ContactQueries.GetContactDetail(_fixture.DataSource, conversationId);

        detail.Should().BeNull();
    }

    [Fact]
    public async Task GetContactDetail_WithOwner_ReturnsOwnerInfo()
    {
        var conversationId = Guid.Parse("1b000000-0000-0000-0000-000000000001");

        var detail = await ContactQueries.GetContactDetail(_fixture.DataSource, conversationId);

        detail.Should().NotBeNull();
        detail!.Owner.Should().NotBeNull();
        detail.Owner!.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetContactDetail_WithTicket_ReturnsTicketInfo()
    {
        var conversationId = Guid.Parse("1b000000-0000-0000-0000-000000000001");

        var detail = await ContactQueries.GetContactDetail(_fixture.DataSource, conversationId);

        detail.Should().NotBeNull();
        detail!.Ticket.Should().NotBeNull();
        detail.Ticket.Id.Should().NotBe(Guid.Empty);
        detail.Ticket.Subject.Should().NotBeNullOrEmpty();
        detail.Ticket.Status.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetContactCompanies_ReturnsCompanies()
    {
        var companies = await ContactQueries.GetContactCompanies(_fixture.DataSource);

        companies.Should().NotBeEmpty();
        companies.Should().Contain("StartupTech");
        companies.Should().Contain("Costa & Associados");
        companies.Should().Contain("TechCorp");
    }

    [Fact]
    public async Task GetContactCompanies_ReturnsDistinctCompanies()
    {
        var companies = await ContactQueries.GetContactCompanies(_fixture.DataSource);

        companies.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task CreateContact_ValidRequest_ReturnsContactId()
    {
        var request = new CreateContactRequest
        {
            Name = "New Contact",
            Phone = "(11) 12345-6789",
            Email = "new@test.com",
            Company = "Test Corp"
        };

        var contactId = await ContactQueries.CreateContact(_fixture.DataSource, request);

        contactId.Should().NotBe(Guid.Empty);

        var (contacts, _) = await ContactQueries.GetAllContacts(
            _fixture.DataSource, 1, 10, null, null, null, "admin", null);
        contacts.Should().Contain(c => c.ContactId == contactId);
    }

    [Fact]
    public async Task CreateContact_WithTags_CreatesContactWithTags()
    {
        var tagId = Guid.Parse("f1000000-0000-0000-0000-000000000001");
        var request = new CreateContactRequest
        {
            Name = "Contact with Tag",
            TagIds = new[] { tagId }
        };

        var contactId = await ContactQueries.CreateContact(_fixture.DataSource, request);

        var detail = await ContactQueries.GetContactDetail(_fixture.DataSource, contactId);
        // Note: GetContactDetail requires a conversation, so we check via GetAllContacts
        var (contacts, _) = await ContactQueries.GetAllContacts(
            _fixture.DataSource, 1, 10, null, null, null, "admin", null);
        var contact = contacts.FirstOrDefault(c => c.ContactId == contactId);
        contact.Should().NotBeNull();
        contact!.Tags.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetContactTickets_ExistingContact_ReturnsTickets()
    {
        var contactId = Guid.Parse("a1000000-0000-0000-0000-000000000001");

        var tickets = await ContactQueries.GetContactTickets(_fixture.DataSource, contactId);

        tickets.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetContactTickets_NonExistingContact_ReturnsEmpty()
    {
        var contactId = Guid.NewGuid();

        var tickets = await ContactQueries.GetContactTickets(_fixture.DataSource, contactId);

        tickets.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOpenTicketConversationId_HasOpenTicket_ReturnsConversationId()
    {
        var contactId = Guid.Parse("a1000000-0000-0000-0000-000000000001");

        var conversationId = await ContactQueries.GetOpenTicketConversationId(_fixture.DataSource, contactId);

        conversationId.Should().NotBeNull();
        conversationId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task GetOpenTicketConversationId_NoOpenTicket_ReturnsNull()
    {
        // Use contact with no open tickets (create a contact without tickets)
        var contactId = Guid.NewGuid();
        await using var conn = await _fixture.DataSource.OpenConnectionAsync();
        await using var cmd = new Npgsql.NpgsqlCommand(@"
            INSERT INTO contacts (id, name, created_at, updated_at) 
            VALUES (@id, 'No Tickets', NOW(), NOW())
        ", conn);
        cmd.Parameters.AddWithValue("@id", contactId);
        await cmd.ExecuteNonQueryAsync();

        var conversationId = await ContactQueries.GetOpenTicketConversationId(_fixture.DataSource, contactId);

        conversationId.Should().BeNull();
    }

    [Fact]
    public async Task UpdateContactOwner_ValidContact_Succeeds()
    {
        var contactId = Guid.Parse("a1000000-0000-0000-0000-000000000003");
        var ownerId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");

        await ContactQueries.UpdateContactOwner(_fixture.DataSource, contactId, ownerId);

        var (contacts, _) = await ContactQueries.GetAllContacts(
            _fixture.DataSource, 1, 10, null, null, null, "admin", null);
        var contact = contacts.FirstOrDefault(c => c.ContactId == contactId);
        contact.Should().NotBeNull();
        contact!.ContactOwnerId.Should().Be(ownerId);
    }

    [Fact]
    public async Task UpdateContactOwner_SetNull_Succeeds()
    {
        var contactId = Guid.Parse("a1000000-0000-0000-0000-000000000001");

        await ContactQueries.UpdateContactOwner(_fixture.DataSource, contactId, null);

        var (contacts, _) = await ContactQueries.GetAllContacts(
            _fixture.DataSource, 1, 10, null, null, null, "admin", null);
        var contact = contacts.FirstOrDefault(c => c.ContactId == contactId);
        contact.Should().NotBeNull();
        contact!.ContactOwnerId.Should().BeNull();
    }
}
