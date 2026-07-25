using Npgsql;

public static class TagQueries
{
    public static readonly string GetAllTagsSql = @"
        SELECT id, name, color_code
        FROM tags
        ORDER BY name";

    public static async Task<TagItem[]> GetAllTags(NpgsqlDataSource dataSource)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(GetAllTagsSql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var tags = new List<TagItem>();
        while (await reader.ReadAsync())
        {
            tags.Add(new TagItem
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                ColorCode = reader.GetString(reader.GetOrdinal("color_code")),
            });
        }

        return tags.ToArray();
    }
}
