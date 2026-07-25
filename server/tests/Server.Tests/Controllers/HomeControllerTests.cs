using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class HomeControllerTests
{
    private readonly Mock<IDBService> _dbServiceMock;
    private readonly Mock<ILogger<HomeController>> _loggerMock;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _dbServiceMock = new Mock<IDBService>();
        _loggerMock = new Mock<ILogger<HomeController>>();
        _controller = new HomeController(_dbServiceMock.Object, _loggerMock.Object);
    }

    private void SetupUserClaims(Guid userId, string role = "agent")
    {
        var claims = new System.Security.Claims.Claim[]
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
            new(System.Security.Claims.ClaimTypes.Email, "user@test.com"),
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
    // GET /api/home
    // ============================================================

    [Fact]
    public async Task Home_AdminUser_ReturnsAllConversations()
    {
        var userId = Guid.NewGuid();
        SetupUserClaims(userId, "admin");

        var tickets = new TicketCard[]
        {
            new() { ConversationId = Guid.NewGuid(), Subject = "Ticket 1" },
            new() { ConversationId = Guid.NewGuid(), Subject = "Ticket 2" }
        };
        _dbServiceMock
            .Setup(s => s.GetAllConversations(0, 20))
            .ReturnsAsync(tickets);

        var result = await _controller.Home(0, 20);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
        _dbServiceMock.Verify(s => s.GetAllConversations(0, 20), Times.Once);
    }

    [Fact]
    public async Task Home_AgentUser_ReturnsOnlyOwnedConversations()
    {
        var userId = Guid.NewGuid();
        SetupUserClaims(userId, "agent");

        var tickets = new TicketCard[]
        {
            new() { ConversationId = Guid.NewGuid(), Subject = "My Ticket" }
        };
        _dbServiceMock
            .Setup(s => s.GetConversationsByOwner(userId, 0, 20))
            .ReturnsAsync(tickets);

        var result = await _controller.Home(0, 20);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
        _dbServiceMock.Verify(s => s.GetConversationsByOwner(userId, 0, 20), Times.Once);
    }

    [Fact]
    public async Task Home_DefaultParams_UsesOffset0Limit20()
    {
        var userId = Guid.NewGuid();
        SetupUserClaims(userId, "admin");

        _dbServiceMock
            .Setup(s => s.GetAllConversations(0, 20))
            .ReturnsAsync(Array.Empty<TicketCard>());

        var result = await _controller.Home();

        result.Should().BeOfType<OkObjectResult>();
        _dbServiceMock.Verify(s => s.GetAllConversations(0, 20), Times.Once);
    }

    [Fact]
    public async Task Home_EmptyConversations_ReturnsEmptyArray()
    {
        var userId = Guid.NewGuid();
        SetupUserClaims(userId, "admin");

        _dbServiceMock
            .Setup(s => s.GetAllConversations(0, 20))
            .ReturnsAsync(Array.Empty<TicketCard>());

        var result = await _controller.Home();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Home_CustomPagination_ReturnsCorrectPage()
    {
        var userId = Guid.NewGuid();
        SetupUserClaims(userId, "admin");

        _dbServiceMock
            .Setup(s => s.GetAllConversations(10, 5))
            .ReturnsAsync(Array.Empty<TicketCard>());

        var result = await _controller.Home(10, 5);

        result.Should().BeOfType<OkObjectResult>();
        _dbServiceMock.Verify(s => s.GetAllConversations(10, 5), Times.Once);
    }

    [Fact]
    public async Task Home_DatabaseException_ThrowsAppException()
    {
        var userId = Guid.NewGuid();
        SetupUserClaims(userId, "admin");

        _dbServiceMock
            .Setup(s => s.GetAllConversations(0, 20))
            .ThrowsAsync(new AppException("DB_ERROR", "Database error"));

        var act = () => _controller.Home();

        await act.Should().ThrowAsync<AppException>();
    }
}
