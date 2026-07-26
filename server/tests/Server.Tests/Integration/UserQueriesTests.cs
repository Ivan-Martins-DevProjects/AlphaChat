using FluentAssertions;
using Xunit;

namespace Server.Tests.Integration;

[Collection("Database")]
public class UserQueriesTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;

    public UserQueriesTests(DatabaseFixture fixture)
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
    public async Task GetAllUsers_ReturnsUsers()
    {
        var users = await UserQueries.GetAllUsers(_fixture.DataSource);

        users.Should().NotBeEmpty();
        users.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsUsersOrderedByName()
    {
        var users = await UserQueries.GetAllUsers(_fixture.DataSource);

        var names = users.Select(u => u.Name).ToList();
        names.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetAllUsers_UserHasCorrectProperties()
    {
        var users = await UserQueries.GetAllUsers(_fixture.DataSource);

        var maria = users.FirstOrDefault(u => u.Email == "maria@alphachat.com");
        maria.Should().NotBeNull();
        maria!.Name.Should().Be("Maria Santos");
        maria.Role.Should().Be("admin");
    }
}
