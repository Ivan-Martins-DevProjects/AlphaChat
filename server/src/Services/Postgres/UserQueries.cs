using Npgsql;

public static class UserQueries
{
    public static readonly string GetAllUsersSql = @"
        SELECT id, name, email, role, profile_pic
        FROM users
        ORDER BY name";

    public static async Task<UserRow[]> GetAllUsers(NpgsqlDataSource dataSource)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(GetAllUsersSql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var users = new List<UserRow>();
        while (await reader.ReadAsync())
        {
            users.Add(new UserRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                Role = reader.GetString(reader.GetOrdinal("role")),
                ProfilePic = reader.IsDBNull(reader.GetOrdinal("profile_pic"))
                    ? ""
                    : reader.GetString(reader.GetOrdinal("profile_pic")),
            });
        }

        return users.ToArray();
    }
}
