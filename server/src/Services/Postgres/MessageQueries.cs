using Npgsql;

public static class MessageQueries
{
    public static readonly string GetMessagesSql = @"
        SELECT
            m.id,
            m.conversation_id,
            m.sender_id,
            m.content,
            m.message_type,
            m.is_edited,
            m.is_deleted,
            m.created_at,
            m.updated_at
        FROM messages m
        WHERE m.conversation_id = @conversationId
        ORDER BY m.created_at DESC
        LIMIT 20
        OFFSET @offset";

    public static readonly string InsertMessageSql = @"
        INSERT INTO messages (id, conversation_id, sender_id, content, message_type, is_edited, is_deleted, created_at, ticket_id)
        VALUES (@id, @conversationId, @senderId, @content, @messageType, @isEdited, @isDeleted, @createdAt, @ticketId)";

    public static readonly string MarkConversationUnreadSql = @"
        UPDATE conversations
        SET read = FALSE
        WHERE id = @conversationId";

    public static async Task<MessageRow[]> GetMessagesByConversation(NpgsqlDataSource dataSource, Guid conversationId, int offset)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(GetMessagesSql, conn);
        cmd.Parameters.AddWithValue("@conversationId", conversationId);
        cmd.Parameters.AddWithValue("@offset", offset);
        await using var reader = await cmd.ExecuteReaderAsync();

        var messages = new List<MessageRow>();
        while (await reader.ReadAsync())
        {
            messages.Add(new MessageRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                ConversationId = reader.GetGuid(reader.GetOrdinal("conversation_id")),
                SenderId = reader.GetGuid(reader.GetOrdinal("sender_id")),
                Content = reader.GetString(reader.GetOrdinal("content")),
                MessageType = reader.GetString(reader.GetOrdinal("message_type")),
                IsEdited = reader.GetBoolean(reader.GetOrdinal("is_edited")),
                IsDeleted = reader.GetBoolean(reader.GetOrdinal("is_deleted")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("updated_at")),
            });
        }

        return messages.ToArray();
    }

    public static async Task<MessageRow> SaveMessage(NpgsqlDataSource dataSource, Guid conversationId, Guid senderId, string content, string messageType)
    {
        var message = new MessageRow
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content,
            MessageType = messageType,
            IsEdited = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
        };

        await using var conn = await dataSource.OpenConnectionAsync();

        // Look up the ticket_id from the conversation
        await using var lookupCmd = new NpgsqlCommand(@"
            SELECT t.id FROM conversations c
            INNER JOIN tickets t ON t.id = c.ticket_id
            WHERE c.id = @conversationId", conn);
        lookupCmd.Parameters.AddWithValue("@conversationId", conversationId);
        var ticketId = await lookupCmd.ExecuteScalarAsync();

        await using var cmd = new NpgsqlCommand(InsertMessageSql, conn);
        cmd.Parameters.AddWithValue("@id", message.Id);
        cmd.Parameters.AddWithValue("@conversationId", message.ConversationId);
        cmd.Parameters.AddWithValue("@senderId", message.SenderId);
        cmd.Parameters.AddWithValue("@content", message.Content);
        cmd.Parameters.AddWithValue("@messageType", message.MessageType);
        cmd.Parameters.AddWithValue("@isEdited", message.IsEdited);
        cmd.Parameters.AddWithValue("@isDeleted", message.IsDeleted);
        cmd.Parameters.AddWithValue("@createdAt", message.CreatedAt);
        cmd.Parameters.AddWithValue("@ticketId", ticketId ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();

        await using var updateCmd = new NpgsqlCommand(MarkConversationUnreadSql, conn);
        updateCmd.Parameters.AddWithValue("@conversationId", conversationId);
        await updateCmd.ExecuteNonQueryAsync();

        return message;
    }
}
