using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class ContactControllerTests
{
    private readonly Mock<IDBService> _dbServiceMock;
    private readonly ContactController _controller;

    public ContactControllerTests()
    {
        _dbServiceMock = new Mock<IDBService>();
        _controller = new ContactController(_dbServiceMock.Object);
    }

    private void SetupUserClaims(Guid userId, string role = "admin")
    {
        var claims = new System.Security.Claims.Claim[]
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
            new(System.Security.Claims.ClaimTypes.Role, role)
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = principal }
        };
    }

    // ============================================================
    // GET /api/contact/tags
    // ============================================================

    [Fact]
    public async Task GetTags_ReturnsOkWithTagItems()
    {
        var tags = new TagItem[]
        {
            new() { Id = Guid.NewGuid(), Name = "VIP", ColorCode = "#ff0000" },
            new() { Id = Guid.NewGuid(), Name = "Urgente", ColorCode = "#00ff00" }
        };
        _dbServiceMock.Setup(s => s.GetAllTags()).ReturnsAsync(tags);

        var result = await _controller.GetTags();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<TagItem[]>().Subject;
        returned.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTags_ReturnsEmptyArray_WhenNoTags()
    {
        _dbServiceMock.Setup(s => s.GetAllTags()).ReturnsAsync(Array.Empty<TagItem>());

        var result = await _controller.GetTags();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<TagItem[]>().Subject;
        returned.Should().BeEmpty();
    }

    // ============================================================
    // GET /api/contact/users
    // ============================================================

    [Fact]
    public async Task GetUsers_ReturnsOkWithUsers()
    {
        var users = new UserRow[]
        {
            new() { Id = Guid.NewGuid(), Name = "Admin", Email = "admin@test.com", Role = "admin" }
        };
        _dbServiceMock.Setup(s => s.GetAllUsers()).ReturnsAsync(users);

        var result = await _controller.GetUsers();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<UserRow[]>().Subject;
        returned.Should().HaveCount(1);
    }

    // ============================================================
    // POST /api/contact
    // ============================================================

    [Fact]
    public async Task CreateContact_ReturnsOkWithContactId()
    {
        var contactId = Guid.NewGuid();
        var request = new CreateContactRequest
        {
            Name = "João Silva",
            Phone = "(11) 99999-9999",
            Email = "joao@test.com",
            Company = "Test Corp"
        };
        _dbServiceMock.Setup(s => s.CreateContact(request)).ReturnsAsync(contactId);

        var result = await _controller.CreateContact(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dict = okResult.Value.Should().BeAssignableTo<dynamic>().Subject;
        ((Guid)dict.GetType().GetProperty("contactId")!.GetValue(dict)!).Should().Be(contactId);
    }

    [Fact]
    public async Task CreateContact_WithMinimalData_Succeeds()
    {
        var contactId = Guid.NewGuid();
        var request = new CreateContactRequest { Name = "Maria" };
        _dbServiceMock.Setup(s => s.CreateContact(request)).ReturnsAsync(contactId);

        var result = await _controller.CreateContact(request);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CreateContact_WithTags_Succeeds()
    {
        var contactId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var request = new CreateContactRequest
        {
            Name = "Pedro",
            TagIds = new[] { tagId }
        };
        _dbServiceMock.Setup(s => s.CreateContact(request)).ReturnsAsync(contactId);

        var result = await _controller.CreateContact(request);

        result.Should().BeOfType<OkObjectResult>();
        _dbServiceMock.Verify(s => s.CreateContact(request), Times.Once);
    }

    // ============================================================
    // GET /api/contact/all
    // ============================================================

    [Fact]
    public async Task GetAllContacts_ReturnsOkWithContacts()
    {
        SetupUserClaims(Guid.NewGuid());
        var contacts = new ContactDetail[]
        {
            new() { ContactId = Guid.NewGuid(), Name = "Fernanda" }
        };
        _dbServiceMock
            .Setup(s => s.GetAllContacts(1, 10, null, null, null, It.IsAny<string>(), It.IsAny<Guid?>()))
            .ReturnsAsync((contacts, 1));

        var result = await _controller.GetAllContacts(1, 10, null, null, null);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllContacts_WithSearch_FiltersCorrectly()
    {
        SetupUserClaims(Guid.NewGuid());
        var contacts = new ContactDetail[]
        {
            new() { ContactId = Guid.NewGuid(), Name = "Fernanda" }
        };
        _dbServiceMock
            .Setup(s => s.GetAllContacts(1, 10, "Fernanda", null, null, It.IsAny<string>(), It.IsAny<Guid?>()))
            .ReturnsAsync((contacts, 1));

        var result = await _controller.GetAllContacts(1, 10, "Fernanda", null, null);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllContacts_WithCompany_FiltersCorrectly()
    {
        SetupUserClaims(Guid.NewGuid());
        var contacts = Array.Empty<ContactDetail>();
        _dbServiceMock
            .Setup(s => s.GetAllContacts(1, 10, null, "StartupTech", null, It.IsAny<string>(), It.IsAny<Guid?>()))
            .ReturnsAsync((contacts, 0));

        var result = await _controller.GetAllContacts(1, 10, null, "StartupTech", null);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAllContacts_WithPagination_ReturnsCorrectPage()
    {
        SetupUserClaims(Guid.NewGuid());
        var contacts = Array.Empty<ContactDetail>();
        _dbServiceMock
            .Setup(s => s.GetAllContacts(2, 5, null, null, null, It.IsAny<string>(), It.IsAny<Guid?>()))
            .ReturnsAsync((contacts, 0));

        var result = await _controller.GetAllContacts(2, 5, null, null, null);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAllContacts_EmptyResult_ReturnsEmptyArray()
    {
        SetupUserClaims(Guid.NewGuid());
        _dbServiceMock
            .Setup(s => s.GetAllContacts(1, 10, null, null, null, It.IsAny<string>(), It.IsAny<Guid?>()))
            .ReturnsAsync((Array.Empty<ContactDetail>(), 0));

        var result = await _controller.GetAllContacts(1, 10, null, null, null);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    // ============================================================
    // GET /api/contact/companies
    // ============================================================

    [Fact]
    public async Task GetContactCompanies_ReturnsOkWithCompanies()
    {
        var companies = new[] { "StartupTech", "Costa & Associados" };
        _dbServiceMock.Setup(s => s.GetContactCompanies()).ReturnsAsync(companies);

        var result = await _controller.GetContactCompanies();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<string[]>().Subject;
        returned.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetContactCompanies_EmptyResult_ReturnsEmptyArray()
    {
        _dbServiceMock.Setup(s => s.GetContactCompanies()).ReturnsAsync(Array.Empty<string>());

        var result = await _controller.GetContactCompanies();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<string[]>().Subject;
        returned.Should().BeEmpty();
    }

    // ============================================================
    // GET /api/contact/{conversationId}
    // ============================================================

    [Fact]
    public async Task GetContactDetail_ExistingConversation_ReturnsOk()
    {
        var conversationId = Guid.NewGuid();
        var detail = new ContactDetail
        {
            ContactId = Guid.NewGuid(),
            Name = "Fernanda",
            Phone = "(11) 99999-9999"
        };
        _dbServiceMock.Setup(s => s.GetContactDetail(conversationId)).ReturnsAsync(detail);

        var result = await _controller.GetContactDetail(conversationId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeOfType<ContactDetail>().Subject;
        returned.Name.Should().Be("Fernanda");
    }

    [Fact]
    public async Task GetContactDetail_NonExistingConversation_ReturnsNotFound()
    {
        var conversationId = Guid.NewGuid();
        _dbServiceMock.Setup(s => s.GetContactDetail(conversationId)).ReturnsAsync((ContactDetail?)null);

        var result = await _controller.GetContactDetail(conversationId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ============================================================
    // PUT /api/contact/{conversationId}
    // ============================================================

    [Fact]
    public async Task UpdateContact_ValidRequest_ReturnsOk()
    {
        var conversationId = Guid.NewGuid();
        var request = new UpdateContactRequest
        {
            Name = "Fernanda Atualizada",
            Phone = "(11) 88888-8888"
        };
        _dbServiceMock.Setup(s => s.UpdateContact(conversationId, request)).Returns(Task.CompletedTask);

        var result = await _controller.UpdateContact(conversationId, request);

        result.Should().BeOfType<OkObjectResult>();
        _dbServiceMock.Verify(s => s.UpdateContact(conversationId, request), Times.Once);
    }

    [Fact]
    public async Task UpdateContact_WithNullFields_Succeeds()
    {
        var conversationId = Guid.NewGuid();
        var request = new UpdateContactRequest { Name = "Test" };
        _dbServiceMock.Setup(s => s.UpdateContact(conversationId, request)).Returns(Task.CompletedTask);

        var result = await _controller.UpdateContact(conversationId, request);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // GET /api/contact/{contactId}/tickets
    // ============================================================

    [Fact]
    public async Task GetContactTickets_WithTickets_ReturnsOk()
    {
        var contactId = Guid.NewGuid();
        var tickets = new ContactTicket[]
        {
            new() { TicketId = Guid.NewGuid(), Subject = "Dúvida", Status = "open", ConversationId = Guid.NewGuid() }
        };
        _dbServiceMock.Setup(s => s.GetContactTickets(contactId)).ReturnsAsync(tickets);

        var result = await _controller.GetContactTickets(contactId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<ContactTicket[]>().Subject;
        returned.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetContactTickets_NoTickets_ReturnsEmptyArray()
    {
        var contactId = Guid.NewGuid();
        _dbServiceMock.Setup(s => s.GetContactTickets(contactId)).ReturnsAsync(Array.Empty<ContactTicket>());

        var result = await _controller.GetContactTickets(contactId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<ContactTicket[]>().Subject;
        returned.Should().BeEmpty();
    }

    // ============================================================
    // GET /api/contact/{contactId}/open-ticket
    // ============================================================

    [Fact]
    public async Task GetOpenTicket_Exists_ReturnsExistsTrue()
    {
        var contactId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        _dbServiceMock.Setup(s => s.GetOpenTicketConversationId(contactId)).ReturnsAsync(conversationId);

        var result = await _controller.GetOpenTicket(contactId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dict = okResult.Value.Should().BeAssignableTo<dynamic>().Subject;
        ((bool)dict.GetType().GetProperty("exists")!.GetValue(dict)!).Should().BeTrue();
    }

    [Fact]
    public async Task GetOpenTicket_NotExists_ReturnsExistsFalse()
    {
        var contactId = Guid.NewGuid();
        _dbServiceMock.Setup(s => s.GetOpenTicketConversationId(contactId)).ReturnsAsync((Guid?)null);

        var result = await _controller.GetOpenTicket(contactId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dict = okResult.Value.Should().BeAssignableTo<dynamic>().Subject;
        ((bool)dict.GetType().GetProperty("exists")!.GetValue(dict)!).Should().BeFalse();
    }

    // ============================================================
    // POST /api/contact/{contactId}/assume
    // ============================================================

    [Fact]
    public async Task AssumeContact_SetsOwnerAndReturnsOk()
    {
        var contactId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _dbServiceMock.Setup(s => s.UpdateContactOwner(contactId, userId)).Returns(Task.CompletedTask);

        // Setup controller with user claims
        var claims = new System.Security.Claims.Claim[]
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
            new("name", "Test User"),
            new("profilePic", "pic.jpg")
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = principal }
        };

        var result = await _controller.AssumeContact(contactId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
        _dbServiceMock.Verify(s => s.UpdateContactOwner(contactId, userId), Times.Once);
    }

    // ============================================================
    // DELETE /api/contact/{contactId}/owner
    // ============================================================

    [Fact]
    public async Task RemoveContactOwner_ClearsOwnerAndReturnsOk()
    {
        var contactId = Guid.NewGuid();
        _dbServiceMock.Setup(s => s.UpdateContactOwner(contactId, null)).Returns(Task.CompletedTask);

        var result = await _controller.RemoveContactOwner(contactId);

        result.Should().BeOfType<OkObjectResult>();
        _dbServiceMock.Verify(s => s.UpdateContactOwner(contactId, null), Times.Once);
    }
}
