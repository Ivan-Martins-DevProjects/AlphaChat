using FluentAssertions;
using Xunit;

namespace Server.Tests.Integration;

[Collection("Database")]
public class RoleQueriesTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;

    public RoleQueriesTests(DatabaseFixture fixture)
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
    public async Task GetAll_ReturnsRoles()
    {
        var roles = await RoleQueries.GetAll(_fixture.DataSource);

        roles.Should().NotBeEmpty();
        roles.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAll_ReturnsRolesOrderedByName()
    {
        var roles = await RoleQueries.GetAll(_fixture.DataSource);

        var names = roles.Select(r => r.Name).ToList();
        names.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetByName_ExistingRole_ReturnsRole()
    {
        var role = await RoleQueries.GetByName(_fixture.DataSource, "admin");

        role.Should().NotBeNull();
        role!.Name.Should().Be("admin");
        role.Description.Should().Be("Administrador do sistema com acesso total");
    }

    [Fact]
    public async Task GetByName_NonExistingRole_ReturnsNull()
    {
        var role = await RoleQueries.GetByName(_fixture.DataSource, "nonexistent");

        role.Should().BeNull();
    }

    [Fact]
    public async Task Create_NewRole_ReturnsCreatedRole()
    {
        var newRole = new RoleRow
        {
            Name = "moderator",
            Description = "Moderador com acesso limitado"
        };

        var created = await RoleQueries.Create(_fixture.DataSource, newRole);

        created.Should().NotBeNull();
        created.Id.Should().NotBe(Guid.Empty);
        created.Name.Should().Be("moderator");
        created.Description.Should().Be("Moderador com acesso limitado");
        created.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Create_RoleWithNameAndDescription_StoresCorrectly()
    {
        var newRole = new RoleRow
        {
            Name = "tester",
            Description = "Tester role"
        };

        await RoleQueries.Create(_fixture.DataSource, newRole);
        var retrieved = await RoleQueries.GetByName(_fixture.DataSource, "tester");

        retrieved.Should().NotBeNull();
        retrieved!.Description.Should().Be("Tester role");
    }

    [Fact]
    public async Task Update_ExistingRole_ReturnsUpdatedRole()
    {
        var role = await RoleQueries.GetByName(_fixture.DataSource, "admin");
        role.Should().NotBeNull();

        var updatedRole = new RoleRow
        {
            Name = "superadmin",
            Description = "Super Administrador"
        };

        var updated = await RoleQueries.Update(_fixture.DataSource, "admin", updatedRole);

        updated.Should().NotBeNull();
        updated!.Name.Should().Be("superadmin");
    }

    [Fact]
    public async Task Update_NonExistingRole_ReturnsNull()
    {
        var role = new RoleRow { Name = "test" };

        var result = await RoleQueries.Update(_fixture.DataSource, "nonexistent", role);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Delete_ExistingRole_ReturnsTrue()
    {
        // Create a role to delete
        var role = new RoleRow { Name = "to_delete", Description = "Delete me" };
        await RoleQueries.Create(_fixture.DataSource, role);

        var deleted = await RoleQueries.Delete(_fixture.DataSource, "to_delete");

        deleted.Should().BeTrue();

        var retrieved = await RoleQueries.GetByName(_fixture.DataSource, "to_delete");
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task Delete_NonExistingRole_ReturnsFalse()
    {
        var deleted = await RoleQueries.Delete(_fixture.DataSource, "nonexistent");

        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task Create_ThenDelete_RoleIsRemoved()
    {
        var role = new RoleRow { Name = "temp_role", Description = "Temporary" };
        await RoleQueries.Create(_fixture.DataSource, role);

        var retrieved = await RoleQueries.GetByName(_fixture.DataSource, "temp_role");
        retrieved.Should().NotBeNull();

        await RoleQueries.Delete(_fixture.DataSource, "temp_role");

        retrieved = await RoleQueries.GetByName(_fixture.DataSource, "temp_role");
        retrieved.Should().BeNull();
    }
}
