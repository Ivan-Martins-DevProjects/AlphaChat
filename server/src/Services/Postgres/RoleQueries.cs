using Npgsql;

public static class RoleQueries
{
    public static readonly string GetAllSql = @"
        SELECT id, name, description, created_at, updated_at
        FROM roles
        ORDER BY name";

    public static readonly string GetByNameSql = @"
        SELECT id, name, description, created_at, updated_at
        FROM roles
        WHERE name = @name";

    public static readonly string InsertSql = @"
        INSERT INTO roles (name, description)
        VALUES (@name, @description)
        RETURNING id, created_at, updated_at";

    public static readonly string UpdateSql = @"
        UPDATE roles
        SET name = @name, description = @description, updated_at = NOW()
        WHERE name = @originalName
        RETURNING id, created_at, updated_at";

    public static readonly string DeleteSql = @"
        DELETE FROM roles
        WHERE name = @name";

    private static RoleRow MapRole(NpgsqlDataReader reader)
    {
        return new RoleRow
        {
            Id = reader.GetGuid(reader.GetOrdinal("id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Description = reader.IsDBNull(reader.GetOrdinal("description"))
                ? null
                : reader.GetString(reader.GetOrdinal("description")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
        };
    }

    public static async Task<RoleRow[]> GetAll(NpgsqlDataSource dataSource)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(GetAllSql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var roles = new List<RoleRow>();
        while (await reader.ReadAsync())
        {
            roles.Add(MapRole(reader));
        }

        return roles.ToArray();
    }

    public static async Task<RoleRow?> GetByName(NpgsqlDataSource dataSource, string name)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(GetByNameSql, conn);
        cmd.Parameters.AddWithValue("@name", name);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return MapRole(reader);
    }

    public static async Task<RoleRow> Create(NpgsqlDataSource dataSource, RoleRow role)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(InsertSql, conn);
        cmd.Parameters.AddWithValue("@name", role.Name);
        cmd.Parameters.AddWithValue("@description", (object?)role.Description ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();

        role.Id = reader.GetGuid(reader.GetOrdinal("id"));
        role.CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
        role.UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"));

        return role;
    }

    public static async Task<RoleRow?> Update(NpgsqlDataSource dataSource, string originalName, RoleRow role)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(UpdateSql, conn);
        cmd.Parameters.AddWithValue("@originalName", originalName);
        cmd.Parameters.AddWithValue("@name", role.Name);
        cmd.Parameters.AddWithValue("@description", (object?)role.Description ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        role.Id = reader.GetGuid(reader.GetOrdinal("id"));
        role.CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
        role.UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"));

        return role;
    }

    public static async Task<bool> Delete(NpgsqlDataSource dataSource, string name)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(DeleteSql, conn);
        cmd.Parameters.AddWithValue("@name", name);

        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}
