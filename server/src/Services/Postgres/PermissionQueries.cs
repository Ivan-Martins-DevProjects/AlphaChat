using Npgsql;

public static class PermissionQueries
{
    public static readonly string GetByRoleSql = @"
        SELECT id, role,
            users_read, users_create, users_update, users_delete,
            conversations_read, conversations_create, conversations_update, conversations_delete,
            messages_read, messages_create, messages_update, messages_delete,
            contacts_read, contacts_create, contacts_update, contacts_delete,
            tickets_read, tickets_create, tickets_update, tickets_delete,
            tags_read, tags_create, tags_update, tags_delete,
            created_at, updated_at
        FROM permissions
        WHERE role = @role";

    public static readonly string GetAllSql = @"
        SELECT id, role,
            users_read, users_create, users_update, users_delete,
            conversations_read, conversations_create, conversations_update, conversations_delete,
            messages_read, messages_create, messages_update, messages_delete,
            contacts_read, contacts_create, contacts_update, contacts_delete,
            tickets_read, tickets_create, tickets_update, tickets_delete,
            tags_read, tags_create, tags_update, tags_delete,
            created_at, updated_at
        FROM permissions
        ORDER BY role";

    public static readonly string InsertSql = @"
        INSERT INTO permissions (role,
            users_read, users_create, users_update, users_delete,
            conversations_read, conversations_create, conversations_update, conversations_delete,
            messages_read, messages_create, messages_update, messages_delete,
            contacts_read, contacts_create, contacts_update, contacts_delete,
            tickets_read, tickets_create, tickets_update, tickets_delete,
            tags_read, tags_create, tags_update, tags_delete
        ) VALUES (@role,
            @usersRead, @usersCreate, @usersUpdate, @usersDelete,
            @conversationsRead, @conversationsCreate, @conversationsUpdate, @conversationsDelete,
            @messagesRead, @messagesCreate, @messagesUpdate, @messagesDelete,
            @contactsRead, @contactsCreate, @contactsUpdate, @contactsDelete,
            @ticketsRead, @ticketsCreate, @ticketsUpdate, @ticketsDelete,
            @tagsRead, @tagsCreate, @tagsUpdate, @tagsDelete
        )
        RETURNING id, created_at, updated_at";

    public static readonly string UpdateSql = @"
        UPDATE permissions
        SET
            users_read = @usersRead, users_create = @usersCreate,
            users_update = @usersUpdate, users_delete = @usersDelete,
            conversations_read = @conversationsRead, conversations_create = @conversationsCreate,
            conversations_update = @conversationsUpdate, conversations_delete = @conversationsDelete,
            messages_read = @messagesRead, messages_create = @messagesCreate,
            messages_update = @messagesUpdate, messages_delete = @messagesDelete,
            contacts_read = @contactsRead, contacts_create = @contactsCreate,
            contacts_update = @contactsUpdate, contacts_delete = @contactsDelete,
            tickets_read = @ticketsRead, tickets_create = @ticketsCreate,
            tickets_update = @ticketsUpdate, tickets_delete = @ticketsDelete,
            tags_read = @tagsRead, tags_create = @tagsCreate,
            tags_update = @tagsUpdate, tags_delete = @tagsDelete,
            updated_at = NOW()
        WHERE role = @role
        RETURNING id, created_at, updated_at";

    public static readonly string DeleteSql = @"
        DELETE FROM permissions
        WHERE role = @role";

    private static PermissionRow MapPermission(NpgsqlDataReader reader)
    {
        return new PermissionRow
        {
            Id = reader.GetGuid(reader.GetOrdinal("id")),
            Role = reader.GetString(reader.GetOrdinal("role")),

            UsersRead = reader.GetBoolean(reader.GetOrdinal("users_read")),
            UsersCreate = reader.GetBoolean(reader.GetOrdinal("users_create")),
            UsersUpdate = reader.GetBoolean(reader.GetOrdinal("users_update")),
            UsersDelete = reader.GetBoolean(reader.GetOrdinal("users_delete")),

            ConversationsRead = reader.GetBoolean(reader.GetOrdinal("conversations_read")),
            ConversationsCreate = reader.GetBoolean(reader.GetOrdinal("conversations_create")),
            ConversationsUpdate = reader.GetBoolean(reader.GetOrdinal("conversations_update")),
            ConversationsDelete = reader.GetBoolean(reader.GetOrdinal("conversations_delete")),

            MessagesRead = reader.GetBoolean(reader.GetOrdinal("messages_read")),
            MessagesCreate = reader.GetBoolean(reader.GetOrdinal("messages_create")),
            MessagesUpdate = reader.GetBoolean(reader.GetOrdinal("messages_update")),
            MessagesDelete = reader.GetBoolean(reader.GetOrdinal("messages_delete")),

            ContactsRead = reader.GetBoolean(reader.GetOrdinal("contacts_read")),
            ContactsCreate = reader.GetBoolean(reader.GetOrdinal("contacts_create")),
            ContactsUpdate = reader.GetBoolean(reader.GetOrdinal("contacts_update")),
            ContactsDelete = reader.GetBoolean(reader.GetOrdinal("contacts_delete")),

            TicketsRead = reader.GetBoolean(reader.GetOrdinal("tickets_read")),
            TicketsCreate = reader.GetBoolean(reader.GetOrdinal("tickets_create")),
            TicketsUpdate = reader.GetBoolean(reader.GetOrdinal("tickets_update")),
            TicketsDelete = reader.GetBoolean(reader.GetOrdinal("tickets_delete")),

            TagsRead = reader.GetBoolean(reader.GetOrdinal("tags_read")),
            TagsCreate = reader.GetBoolean(reader.GetOrdinal("tags_create")),
            TagsUpdate = reader.GetBoolean(reader.GetOrdinal("tags_update")),
            TagsDelete = reader.GetBoolean(reader.GetOrdinal("tags_delete")),

            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
        };
    }

    public static async Task<PermissionRow?> GetByRole(NpgsqlDataSource dataSource, string role)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(GetByRoleSql, conn);
        cmd.Parameters.AddWithValue("@role", role);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return MapPermission(reader);
    }

    public static async Task<PermissionRow[]> GetAll(NpgsqlDataSource dataSource)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(GetAllSql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var permissions = new List<PermissionRow>();
        while (await reader.ReadAsync())
        {
            permissions.Add(MapPermission(reader));
        }

        return permissions.ToArray();
    }

    public static async Task<PermissionRow> Create(NpgsqlDataSource dataSource, PermissionRow permission)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(InsertSql, conn);
        cmd.Parameters.AddWithValue("@role", permission.Role);

        cmd.Parameters.AddWithValue("@usersRead", permission.UsersRead);
        cmd.Parameters.AddWithValue("@usersCreate", permission.UsersCreate);
        cmd.Parameters.AddWithValue("@usersUpdate", permission.UsersUpdate);
        cmd.Parameters.AddWithValue("@usersDelete", permission.UsersDelete);

        cmd.Parameters.AddWithValue("@conversationsRead", permission.ConversationsRead);
        cmd.Parameters.AddWithValue("@conversationsCreate", permission.ConversationsCreate);
        cmd.Parameters.AddWithValue("@conversationsUpdate", permission.ConversationsUpdate);
        cmd.Parameters.AddWithValue("@conversationsDelete", permission.ConversationsDelete);

        cmd.Parameters.AddWithValue("@messagesRead", permission.MessagesRead);
        cmd.Parameters.AddWithValue("@messagesCreate", permission.MessagesCreate);
        cmd.Parameters.AddWithValue("@messagesUpdate", permission.MessagesUpdate);
        cmd.Parameters.AddWithValue("@messagesDelete", permission.MessagesDelete);

        cmd.Parameters.AddWithValue("@contactsRead", permission.ContactsRead);
        cmd.Parameters.AddWithValue("@contactsCreate", permission.ContactsCreate);
        cmd.Parameters.AddWithValue("@contactsUpdate", permission.ContactsUpdate);
        cmd.Parameters.AddWithValue("@contactsDelete", permission.ContactsDelete);

        cmd.Parameters.AddWithValue("@ticketsRead", permission.TicketsRead);
        cmd.Parameters.AddWithValue("@ticketsCreate", permission.TicketsCreate);
        cmd.Parameters.AddWithValue("@ticketsUpdate", permission.TicketsUpdate);
        cmd.Parameters.AddWithValue("@ticketsDelete", permission.TicketsDelete);

        cmd.Parameters.AddWithValue("@tagsRead", permission.TagsRead);
        cmd.Parameters.AddWithValue("@tagsCreate", permission.TagsCreate);
        cmd.Parameters.AddWithValue("@tagsUpdate", permission.TagsUpdate);
        cmd.Parameters.AddWithValue("@tagsDelete", permission.TagsDelete);

        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();

        permission.Id = reader.GetGuid(reader.GetOrdinal("id"));
        permission.CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
        permission.UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"));

        return permission;
    }

    public static async Task<PermissionRow?> Update(NpgsqlDataSource dataSource, PermissionRow permission)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(UpdateSql, conn);
        cmd.Parameters.AddWithValue("@role", permission.Role);

        cmd.Parameters.AddWithValue("@usersRead", permission.UsersRead);
        cmd.Parameters.AddWithValue("@usersCreate", permission.UsersCreate);
        cmd.Parameters.AddWithValue("@usersUpdate", permission.UsersUpdate);
        cmd.Parameters.AddWithValue("@usersDelete", permission.UsersDelete);

        cmd.Parameters.AddWithValue("@conversationsRead", permission.ConversationsRead);
        cmd.Parameters.AddWithValue("@conversationsCreate", permission.ConversationsCreate);
        cmd.Parameters.AddWithValue("@conversationsUpdate", permission.ConversationsUpdate);
        cmd.Parameters.AddWithValue("@conversationsDelete", permission.ConversationsDelete);

        cmd.Parameters.AddWithValue("@messagesRead", permission.MessagesRead);
        cmd.Parameters.AddWithValue("@messagesCreate", permission.MessagesCreate);
        cmd.Parameters.AddWithValue("@messagesUpdate", permission.MessagesUpdate);
        cmd.Parameters.AddWithValue("@messagesDelete", permission.MessagesDelete);

        cmd.Parameters.AddWithValue("@contactsRead", permission.ContactsRead);
        cmd.Parameters.AddWithValue("@contactsCreate", permission.ContactsCreate);
        cmd.Parameters.AddWithValue("@contactsUpdate", permission.ContactsUpdate);
        cmd.Parameters.AddWithValue("@contactsDelete", permission.ContactsDelete);

        cmd.Parameters.AddWithValue("@ticketsRead", permission.TicketsRead);
        cmd.Parameters.AddWithValue("@ticketsCreate", permission.TicketsCreate);
        cmd.Parameters.AddWithValue("@ticketsUpdate", permission.TicketsUpdate);
        cmd.Parameters.AddWithValue("@ticketsDelete", permission.TicketsDelete);

        cmd.Parameters.AddWithValue("@tagsRead", permission.TagsRead);
        cmd.Parameters.AddWithValue("@tagsCreate", permission.TagsCreate);
        cmd.Parameters.AddWithValue("@tagsUpdate", permission.TagsUpdate);
        cmd.Parameters.AddWithValue("@tagsDelete", permission.TagsDelete);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        permission.Id = reader.GetGuid(reader.GetOrdinal("id"));
        permission.CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
        permission.UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"));

        return permission;
    }

    public static async Task<bool> Delete(NpgsqlDataSource dataSource, string role)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(DeleteSql, conn);
        cmd.Parameters.AddWithValue("@role", role);

        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}
