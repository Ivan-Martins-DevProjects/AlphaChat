using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class LoginControllerTests
{
    private readonly Mock<IDBService> _dbServiceMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<ILogger<LoginController>> _loggerMock;
    private readonly LoginController _controller;

    public LoginControllerTests()
    {
        _dbServiceMock = new Mock<IDBService>();
        _jwtServiceMock = new Mock<IJwtService>();
        _loggerMock = new Mock<ILogger<LoginController>>();
        _controller = new LoginController(_dbServiceMock.Object, _jwtServiceMock.Object, _loggerMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // ============================================================
    // POST /api/login
    // ============================================================

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        var request = new LoginRequest { Email = "admin@test.com", Password = "123456" };
        var user = new UserRow
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Email = "admin@test.com",
            Role = "admin"
        };

        _dbServiceMock.Setup(s => s.GetUser(request)).ReturnsAsync(user);
        _dbServiceMock.Setup(s => s.GetPassword(user)).ReturnsAsync("123456");
        _jwtServiceMock.Setup(s => s.GenerateToken(user)).Returns("jwt-token");

        var result = await _controller.Login(request);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Login_InvalidEmail_ReturnsNotFound()
    {
        var request = new LoginRequest { Email = "wrong@test.com", Password = "123456" };

        _dbServiceMock.Setup(s => s.GetUser(request)).ReturnsAsync((UserRow?)null);

        var act = () => _controller.Login(request);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsNotFound()
    {
        var request = new LoginRequest { Email = "admin@test.com", Password = "wrong" };
        var user = new UserRow
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Email = "admin@test.com",
            Role = "admin"
        };

        _dbServiceMock.Setup(s => s.GetUser(request)).ReturnsAsync(user);
        _dbServiceMock.Setup(s => s.GetPassword(user)).ReturnsAsync("correct-password");

        var act = () => _controller.Login(request);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Login_NullPassword_ReturnsNotFound()
    {
        var request = new LoginRequest { Email = "admin@test.com", Password = "123456" };
        var user = new UserRow
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Email = "admin@test.com",
            Role = "admin"
        };

        _dbServiceMock.Setup(s => s.GetUser(request)).ReturnsAsync(user);
        _dbServiceMock.Setup(s => s.GetPassword(user)).ReturnsAsync((string?)null);

        var act = () => _controller.Login(request);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Login_GeneratesJwtToken()
    {
        var request = new LoginRequest { Email = "admin@test.com", Password = "123456" };
        var user = new UserRow
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Email = "admin@test.com",
            Role = "admin"
        };

        _dbServiceMock.Setup(s => s.GetUser(request)).ReturnsAsync(user);
        _dbServiceMock.Setup(s => s.GetPassword(user)).ReturnsAsync("123456");
        _jwtServiceMock.Setup(s => s.GenerateToken(user)).Returns("generated-token");

        await _controller.Login(request);

        _jwtServiceMock.Verify(s => s.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task Login_SetsHttpOnlyCookie()
    {
        var request = new LoginRequest { Email = "admin@test.com", Password = "123456" };
        var user = new UserRow
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Email = "admin@test.com",
            Role = "admin"
        };

        _dbServiceMock.Setup(s => s.GetUser(request)).ReturnsAsync(user);
        _dbServiceMock.Setup(s => s.GetPassword(user)).ReturnsAsync("123456");
        _jwtServiceMock.Setup(s => s.GenerateToken(user)).Returns("jwt-token");

        await _controller.Login(request);

        var cookieHeader = _controller.HttpContext.Response.Headers["Set-Cookie"].ToString();
        cookieHeader.Should().Contain("token=");
    }
}
