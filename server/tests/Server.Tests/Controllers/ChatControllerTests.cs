using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class ChatControllerTests
{
    private readonly Mock<IDBService> _dbServiceMock;
    private readonly ChatController _controller;

    public ChatControllerTests()
    {
        _dbServiceMock = new Mock<IDBService>();
        _controller = new ChatController(_dbServiceMock.Object);
    }

    private void SetupUserClaims(Guid userId)
    {
        var claims = new System.Security.Claims.Claim[]
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = principal }
        };
    }

    // ============================================================
    // GET /api/chat
    // ============================================================

    [Fact]
    public async Task GetConversation_WithMessages_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        SetupUserClaims(userId);

        var messages = new MessageRow[]
        {
            new() { Id = Guid.NewGuid(), ConversationId = conversationId, Content = "Olá!" }
        };
        _dbServiceMock
            .Setup(s => s.GetMessagesByConversation(conversationId, 0))
            .ReturnsAsync(messages);

        var result = await _controller.GetConversation(conversationId.ToString(), 0);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetConversation_EmptyConversation_ReturnsEmptyArray()
    {
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        SetupUserClaims(userId);

        _dbServiceMock
            .Setup(s => s.GetMessagesByConversation(conversationId, 0))
            .ReturnsAsync(Array.Empty<MessageRow>());

        var result = await _controller.GetConversation(conversationId.ToString(), 0);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetConversation_WithOffset_ReturnsCorrectPage()
    {
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        SetupUserClaims(userId);

        _dbServiceMock
            .Setup(s => s.GetMessagesByConversation(conversationId, 20))
            .ReturnsAsync(Array.Empty<MessageRow>());

        var result = await _controller.GetConversation(conversationId.ToString(), 20);

        result.Should().BeOfType<OkObjectResult>();
        _dbServiceMock.Verify(s => s.GetMessagesByConversation(conversationId, 20), Times.Once);
    }

    // ============================================================
    // POST /api/chat
    // ============================================================

    [Fact]
    public async Task SendMessage_ValidRequest_ReturnsOkWithMessage()
    {
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        SetupUserClaims(userId);

        var message = new MessageRow
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = userId,
            Content = "Hello!",
            MessageType = "text"
        };
        _dbServiceMock
            .Setup(s => s.SaveMessage(conversationId, userId, "Hello!", "text"))
            .ReturnsAsync(message);

        var request = new SendMessageRequest
        {
            ConversationId = conversationId,
            Content = "Hello!",
            MessageType = "text"
        };

        var result = await _controller.SendMessage(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task SendMessage_DefaultMessageType_Succeeds()
    {
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        SetupUserClaims(userId);

        var message = new MessageRow
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Content = "Test"
        };
        _dbServiceMock
            .Setup(s => s.SaveMessage(conversationId, userId, "Test", "text"))
            .ReturnsAsync(message);

        var request = new SendMessageRequest
        {
            ConversationId = conversationId,
            Content = "Test"
        };

        var result = await _controller.SendMessage(request);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ============================================================
    // POST /api/chat/conversation
    // ============================================================

    [Fact]
    public async Task CreateConversation_ValidRequest_ReturnsOkWithConversationId()
    {
        var userId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        SetupUserClaims(userId);

        _dbServiceMock
            .Setup(s => s.CreateConversation(contactId, userId))
            .ReturnsAsync(conversationId);

        var request = new CreateConversationRequest { ContactId = contactId };

        var result = await _controller.CreateConversation(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateConversation_SetsOwnerIdFromClaims()
    {
        var userId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        SetupUserClaims(userId);

        _dbServiceMock
            .Setup(s => s.CreateConversation(contactId, userId))
            .ReturnsAsync(conversationId);

        var request = new CreateConversationRequest { ContactId = contactId };

        await _controller.CreateConversation(request);

        _dbServiceMock.Verify(s => s.CreateConversation(contactId, userId), Times.Once);
    }

    // ============================================================
    // PUT /api/chat/{conversationId}/read
    // ============================================================

    [Fact]
    public async Task MarkAsRead_ValidConversation_ReturnsOk()
    {
        var conversationId = Guid.NewGuid();
        _dbServiceMock.Setup(s => s.MarkAsRead(conversationId)).Returns(Task.CompletedTask);

        var result = await _controller.MarkAsRead(conversationId);

        result.Should().BeOfType<OkObjectResult>();
        _dbServiceMock.Verify(s => s.MarkAsRead(conversationId), Times.Once);
    }
}
