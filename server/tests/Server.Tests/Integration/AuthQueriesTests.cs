using FluentAssertions;
using Xunit;

namespace Server.Tests.Integration;

[Collection("Database")]
public class AuthQueriesTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;

    public AuthQueriesTests(DatabaseFixture fixture)
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
    public async Task GetUser_ExistingEmail_ReturnsUser()
    {
        var user = await AuthQueries.GetUser(_fixture.DataSource, "maria@alphachat.com");

        user.Should().NotBeNull();
        user!.Name.Should().Be("Maria Santos");
        user.Email.Should().Be("maria@alphachat.com");
        user.Role.Should().Be("admin");
    }

    [Fact]
    public async Task GetUser_NonExistingEmail_ReturnsNull()
    {
        var user = await AuthQueries.GetUser(_fixture.DataSource, "nonexistent@test.com");

        user.Should().BeNull();
    }

    [Fact]
    public async Task GetPassword_ExistingUser_ReturnsPassword()
    {
        var user = await AuthQueries.GetUser(_fixture.DataSource, "maria@alphachat.com");
        user.Should().NotBeNull();

        var password = await AuthQueries.GetPassword(_fixture.DataSource, user!.Id);

        password.Should().Be("123456");
    }

    [Fact]
    public async Task GetPassword_NonExistingUser_ReturnsNull()
    {
        var password = await AuthQueries.GetPassword(_fixture.DataSource, Guid.NewGuid());

        password.Should().BeNull();
    }
}
