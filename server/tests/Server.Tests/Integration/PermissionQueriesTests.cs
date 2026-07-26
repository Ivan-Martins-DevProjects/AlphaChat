using FluentAssertions;
using Xunit;

namespace Server.Tests.Integration;

[Collection("Database")]
public class PermissionQueriesTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;

    public PermissionQueriesTests(DatabaseFixture fixture)
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
    public async Task GetAll_ReturnsPermissions()
    {
        var permissions = await PermissionQueries.GetAll(_fixture.DataSource);

        permissions.Should().NotBeEmpty();
        permissions.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAll_ReturnsPermissionsOrderedByRole()
    {
        var permissions = await PermissionQueries.GetAll(_fixture.DataSource);

        var roles = permissions.Select(p => p.Role).ToList();
        roles.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetByRole_ExistingRole_ReturnsPermission()
    {
        var permission = await PermissionQueries.GetByRole(_fixture.DataSource, "admin");

        permission.Should().NotBeNull();
        permission!.Role.Should().Be("admin");
        permission.UsersRead.Should().BeTrue();
        permission.UsersCreate.Should().BeTrue();
        permission.ConversationsRead.Should().BeTrue();
    }

    [Fact]
    public async Task GetByRole_NonExistingRole_ReturnsNull()
    {
        var permission = await PermissionQueries.GetByRole(_fixture.DataSource, "nonexistent");

        permission.Should().BeNull();
    }

    [Fact]
    public async Task Create_NewPermission_ReturnsCreatedPermission()
    {
        var newPermission = new PermissionRow
        {
            Role = "moderator",
            UsersRead = true,
            UsersCreate = false,
            UsersUpdate = false,
            UsersDelete = false,
            ConversationsRead = true,
            ConversationsCreate = true,
            ConversationsUpdate = false,
            ConversationsDelete = false,
            MessagesRead = true,
            MessagesCreate = true,
            MessagesUpdate = false,
            MessagesDelete = false,
            ContactsRead = true,
            ContactsCreate = false,
            ContactsUpdate = false,
            ContactsDelete = false,
            TicketsRead = true,
            TicketsCreate = false,
            TicketsUpdate = false,
            TicketsDelete = false,
            TagsRead = true,
            TagsCreate = false,
            TagsUpdate = false,
            TagsDelete = false
        };

        var created = await PermissionQueries.Create(_fixture.DataSource, newPermission);

        created.Should().NotBeNull();
        created.Id.Should().NotBe(Guid.Empty);
        created.Role.Should().Be("moderator");
        created.UsersRead.Should().BeTrue();
        created.UsersCreate.Should().BeFalse();
    }

    [Fact]
    public async Task Update_ExistingPermission_ReturnsUpdatedPermission()
    {
        var permission = await PermissionQueries.GetByRole(_fixture.DataSource, "admin");
        permission.Should().NotBeNull();

        var updatedPermission = new PermissionRow
        {
            Role = "admin",
            UsersRead = true,
            UsersCreate = true,
            UsersUpdate = true,
            UsersDelete = true,
            ConversationsRead = true,
            ConversationsCreate = true,
            ConversationsUpdate = true,
            ConversationsDelete = true,
            MessagesRead = true,
            MessagesCreate = true,
            MessagesUpdate = true,
            MessagesDelete = true,
            ContactsRead = true,
            ContactsCreate = true,
            ContactsUpdate = true,
            ContactsDelete = true,
            TicketsRead = true,
            TicketsCreate = true,
            TicketsUpdate = true,
            TicketsDelete = true,
            TagsRead = true,
            TagsCreate = true,
            TagsUpdate = true,
            TagsDelete = true
        };

        var updated = await PermissionQueries.Update(_fixture.DataSource, updatedPermission);

        updated.Should().NotBeNull();
        updated!.Role.Should().Be("admin");
        updated.UsersCreate.Should().BeTrue();
    }

    [Fact]
    public async Task Update_NonExistingPermission_ReturnsNull()
    {
        var permission = new PermissionRow { Role = "nonexistent" };

        var result = await PermissionQueries.Update(_fixture.DataSource, permission);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Delete_ExistingPermission_ReturnsTrue()
    {
        // Create a permission to delete
        var permission = new PermissionRow
        {
            Role = "to_delete",
            UsersRead = false,
            ConversationsRead = false,
            MessagesRead = false,
            ContactsRead = false,
            TicketsRead = false,
            TagsRead = false
        };
        await PermissionQueries.Create(_fixture.DataSource, permission);

        var deleted = await PermissionQueries.Delete(_fixture.DataSource, "to_delete");

        deleted.Should().BeTrue();

        var retrieved = await PermissionQueries.GetByRole(_fixture.DataSource, "to_delete");
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task Delete_NonExistingPermission_ReturnsFalse()
    {
        var deleted = await PermissionQueries.Delete(_fixture.DataSource, "nonexistent");

        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task Create_ThenGetByRole_ReturnsCorrectData()
    {
        var newPermission = new PermissionRow
        {
            Role = "custom_role",
            UsersRead = true,
            UsersCreate = false,
            UsersUpdate = false,
            UsersDelete = false,
            ConversationsRead = true,
            ConversationsCreate = false,
            ConversationsUpdate = false,
            ConversationsDelete = false,
            MessagesRead = true,
            MessagesCreate = false,
            MessagesUpdate = false,
            MessagesDelete = false,
            ContactsRead = true,
            ContactsCreate = false,
            ContactsUpdate = false,
            ContactsDelete = false,
            TicketsRead = true,
            TicketsCreate = false,
            TicketsUpdate = false,
            TicketsDelete = false,
            TagsRead = true,
            TagsCreate = false,
            TagsUpdate = false,
            TagsDelete = false
        };

        await PermissionQueries.Create(_fixture.DataSource, newPermission);

        var retrieved = await PermissionQueries.GetByRole(_fixture.DataSource, "custom_role");

        retrieved.Should().NotBeNull();
        retrieved!.Role.Should().Be("custom_role");
        retrieved.UsersRead.Should().BeTrue();
        retrieved.UsersCreate.Should().BeFalse();
    }
}
