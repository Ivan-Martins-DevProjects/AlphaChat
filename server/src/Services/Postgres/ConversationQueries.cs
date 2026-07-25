using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

public static class ConversationQueries
{
    public static readonly string BaseConversationsSql = @"
        SELECT
            c.id AS conversation_id,
            c.type AS conversation_type,
            c.owner,
            c.read AS conversation_read,
            t.id AS ticket_id,
            t.subject,
            t.status,
            ct.id AS contact_id,
            ct.owner_id AS contact_owner_id,
            COALESCE(
                NULLIF(BTRIM(ct.name), ''),
                ct.phone,
                'Contato sem identificação'
            ) AS contact_display_name,
            ct.phone AS contact_phone,
            ct.profile_pic AS contact_profile_pic,
            COALESCE(contact_tags.tags, '[]'::jsonb) AS tags,
            last_message.content AS last_message_content,
            last_message.created_at AS last_message_created_at
        FROM conversations c
        INNER JOIN tickets t ON t.id = c.ticket_id
        LEFT JOIN contacts ct ON ct.id = t.contact_id
        LEFT JOIN LATERAL (
            SELECT
                jsonb_agg(
                    jsonb_build_object(
                        'id', tg.id,
                        'name', tg.name,
                        'colorCode', tg.color_code
                    )
                    ORDER BY tg.name
                ) AS tags
            FROM jsonb_array_elements_text(
                COALESCE(ct.tags, '[]'::jsonb)
            ) AS tag_ids(tag_id)
            INNER JOIN tags tg ON tg.id = tag_ids.tag_id::uuid
        ) contact_tags ON TRUE
        LEFT JOIN LATERAL (
            SELECT m.content, m.created_at
            FROM messages m
            WHERE m.conversation_id = c.id AND m.is_deleted = FALSE
            ORDER BY m.created_at DESC
            LIMIT 1
        ) last_message ON TRUE
        {0}
        ORDER BY COALESCE(
            last_message.created_at,
            c.updated_at,
            c.created_at
        ) DESC";

    public static readonly string MarkAsReadSql = @"
        UPDATE conversations
        SET read = TRUE
        WHERE id = @conversationId";

    public static readonly string CreateConversationSql = @"
        INSERT INTO tickets (id, subject, status, contact_id, created_at)
        VALUES (@ticketId, @subject, 'open', @contactId, NOW())
        RETURNING id";

    public static readonly string InsertConversationSql = @"
        INSERT INTO conversations (id, type, owner, ticket_id, created_at)
        VALUES (@conversationId, 'direct', @ownerId, @ticketId, NOW())
        RETURNING id";

    public static TicketCard MapTicketCard(NpgsqlDataReader reader)
    {
        var tagsOrdinal = reader.GetOrdinal("tags");
        var tagsJson = reader.IsDBNull(tagsOrdinal)
            ? "[]"
            : reader.GetFieldValue<string>(tagsOrdinal);
        var tags = JsonSerializer.Deserialize<TagItem[]>(tagsJson) ?? [];

        return new TicketCard
        {
            ConversationId = reader.GetGuid(reader.GetOrdinal("conversation_id")),
            ConversationType = reader.IsDBNull(reader.GetOrdinal("conversation_type"))
                ? null
                : reader.GetString(reader.GetOrdinal("conversation_type")),
            Owner = reader.IsDBNull(reader.GetOrdinal("owner"))
                ? null
                : reader.GetGuid(reader.GetOrdinal("owner")),

            TicketId = reader.GetGuid(reader.GetOrdinal("ticket_id")),
            Subject = reader.GetString(reader.GetOrdinal("subject")),
            Status = reader.GetString(reader.GetOrdinal("status")),

            ContactId = reader.GetGuid(reader.GetOrdinal("contact_id")),
            ContactOwnerId = reader.IsDBNull(reader.GetOrdinal("contact_owner_id"))
                ? null
                : reader.GetGuid(reader.GetOrdinal("contact_owner_id")),
            ContactDisplayName = reader.GetString(reader.GetOrdinal("contact_display_name")),
            ContactPhone = reader.IsDBNull(reader.GetOrdinal("contact_phone"))
                ? null
                : reader.GetString(reader.GetOrdinal("contact_phone")),
            ContactProfilePic = reader.IsDBNull(reader.GetOrdinal("contact_profile_pic"))
                ? null
                : reader.GetString(reader.GetOrdinal("contact_profile_pic")),

            Tags = tags,

            LastMessageContent = reader.IsDBNull(reader.GetOrdinal("last_message_content"))
                ? null
                : reader.GetString(reader.GetOrdinal("last_message_content")),
            LastMessageCreatedAt = reader.IsDBNull(reader.GetOrdinal("last_message_created_at"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("last_message_created_at")),

            Read = reader.GetBoolean(reader.GetOrdinal("conversation_read")),
        };
    }

    public static async Task<TicketCard[]> GetAllConversations(NpgsqlDataSource dataSource, int offset, int limit)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        var sql = string.Format(BaseConversationsSql, "") + " LIMIT @limit OFFSET @offset";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@limit", limit);
        cmd.Parameters.AddWithValue("@offset", offset);
        await using var reader = await cmd.ExecuteReaderAsync();

        var tickets = new List<TicketCard>();
        while (await reader.ReadAsync())
        {
            tickets.Add(MapTicketCard(reader));
        }

        return tickets.ToArray();
    }

    public static async Task<TicketCard[]> GetConversationsByOwner(NpgsqlDataSource dataSource, Guid userId, int offset, int limit)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        var sql = string.Format(BaseConversationsSql, @"WHERE (c.owner = @userId OR c.owner IS NULL) AND (ct.owner_id = @userId OR ct.owner_id IS NULL)") + " LIMIT @limit OFFSET @offset";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@limit", limit);
        cmd.Parameters.AddWithValue("@offset", offset);
        await using var reader = await cmd.ExecuteReaderAsync();

        var tickets = new List<TicketCard>();
        while (await reader.ReadAsync())
        {
            tickets.Add(MapTicketCard(reader));
        }

        return tickets.ToArray();
    }

    public static async Task MarkAsRead(NpgsqlDataSource dataSource, Guid conversationId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(MarkAsReadSql, conn);
        cmd.Parameters.AddWithValue("@conversationId", conversationId);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<Guid> CreateConversation(NpgsqlDataSource dataSource, Guid contactId, Guid? ownerId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var transaction = await conn.BeginTransactionAsync();

        try
        {
            var ticketId = Guid.NewGuid();
            var conversationId = Guid.NewGuid();

            await using var ticketCmd = new NpgsqlCommand(CreateConversationSql, conn, transaction);
            var ticketIdParam = ticketCmd.Parameters.Add("@ticketId", NpgsqlDbType.Uuid);
            ticketIdParam.Value = ticketId;
            ticketCmd.Parameters.AddWithValue("@subject", "Nova conversa");
            ticketCmd.Parameters.AddWithValue("@contactId", contactId);
            await ticketCmd.ExecuteScalarAsync();

            await using var convCmd = new NpgsqlCommand(InsertConversationSql, conn, transaction);
            convCmd.Parameters.AddWithValue("@conversationId", conversationId);
            var ownerParam = convCmd.Parameters.Add("@ownerId", NpgsqlDbType.Uuid);
            ownerParam.Value = (object?)ownerId ?? DBNull.Value;
            convCmd.Parameters.AddWithValue("@ticketId", ticketId);
            await convCmd.ExecuteScalarAsync();

            await transaction.CommitAsync();
            return conversationId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
