using Npgsql;

public static class AuthQueries
{
    public static readonly string GetUserSql = @"
        SELECT id, name, email, role, profile_pic
        FROM users
        WHERE email = @email";

    public static readonly string GetPasswordSql = @"
        SELECT password
        FROM users
        WHERE id = @id";

    public static async Task<UserRow?> GetUser(NpgsqlDataSource dataSource, string email)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(GetUserSql, conn);
        cmd.Parameters.AddWithValue("@email", email);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return new UserRow
        {
            Id = reader.GetGuid(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2),
            Role = reader.GetString(3),
            ProfilePic = reader.GetString(4),
        };
    }

    public static async Task<string?> GetPassword(NpgsqlDataSource dataSource, Guid userId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(GetPasswordSql, conn);
        cmd.Parameters.AddWithValue("@id", userId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return reader.GetString(0);
    }
}
