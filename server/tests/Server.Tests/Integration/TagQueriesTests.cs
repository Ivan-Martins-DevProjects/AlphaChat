using FluentAssertions;
using Xunit;

namespace Server.Tests.Integration;

[Collection("Database")]
public class TagQueriesTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;

    public TagQueriesTests(DatabaseFixture fixture)
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
    public async Task GetAllTags_ReturnsTags()
    {
        var tags = await TagQueries.GetAllTags(_fixture.DataSource);

        tags.Should().NotBeEmpty();
        tags.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetAllTags_ReturnsTagsOrderedByName()
    {
        var tags = await TagQueries.GetAllTags(_fixture.DataSource);

        var names = tags.Select(t => t.Name).ToList();
        names.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetAllTags_TagHasCorrectProperties()
    {
        var tags = await TagQueries.GetAllTags(_fixture.DataSource);

        var enterprise = tags.FirstOrDefault(t => t.Name == "Enterprise");
        enterprise.Should().NotBeNull();
        enterprise!.ColorCode.Should().Be("#ef4444");
    }
}
