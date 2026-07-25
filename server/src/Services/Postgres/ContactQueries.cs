using Npgsql;
using NpgsqlTypes;

public static class ContactQueries
{
    public static readonly string GetAllContactsSql = @"
        SELECT
            ct.id AS contact_id,
            ct.name,
            ct.phone,
            ct.email,
            ct.company,
            ct.document,
            ct.notes,
            ct.profile_pic,
            ct.owner_id AS contact_owner_id,
            ct.created_at,
            ct.updated_at,
            COALESCE(contact_tags.tags, '[]'::jsonb) AS tags,
            u.name AS owner_name,
            u.profile_pic AS owner_profile_pic
        FROM contacts ct
        LEFT JOIN users u ON u.id = ct.owner_id
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
        WHERE (@search IS NULL OR ct.name ILIKE '%' || @search || '%' OR ct.phone ILIKE '%' || @search || '%')
          AND (@company IS NULL OR ct.company = @company)
          AND (@tagId IS NULL OR ct.tags @> to_jsonb(@tagId::text))
          AND (@isAdmin = TRUE OR ct.owner_id IS NULL OR ct.owner_id = @userId)
        ORDER BY ct.name
        LIMIT @limit OFFSET @offset";

    public static readonly string CountAllContactsSql = @"
        SELECT COUNT(*)
        FROM contacts ct
        WHERE (@search IS NULL OR ct.name ILIKE '%' || @search || '%' OR ct.phone ILIKE '%' || @search || '%')
          AND (@company IS NULL OR ct.company = @company)
          AND (@tagId IS NULL OR ct.tags @> to_jsonb(@tagId::text))
          AND (@isAdmin = TRUE OR ct.owner_id IS NULL OR ct.owner_id = @userId)";

    public static readonly string GetContactCompaniesSql = @"
        SELECT DISTINCT company
        FROM contacts
        WHERE company IS NOT NULL AND company != ''
        ORDER BY company";

    public static readonly string GetContactDetailSql = @"
        SELECT
            ct.id AS contact_id,
            ct.name,
            ct.phone,
            ct.email,
            ct.company,
            ct.document,
            ct.notes,
            ct.profile_pic,
            ct.owner_id AS contact_owner_id,
            ct.created_at,
            ct.updated_at,
            t.id AS ticket_id,
            t.subject,
            t.status AS ticket_status,
            t.created_at AS ticket_created_at,
            u.id AS owner_id,
            u.name AS owner_name,
            u.profile_pic AS owner_profile_pic
        FROM conversations c
        INNER JOIN tickets t ON t.id = c.ticket_id
        LEFT JOIN contacts ct ON ct.id = t.contact_id
        LEFT JOIN users u ON u.id = c.owner
        WHERE c.id = @conversationId";

    public static readonly string GetContactTagsSql = @"
        SELECT tg.id, tg.name, tg.color_code
        FROM contact_tags ctg
        INNER JOIN tags tg ON tg.id = ctg.tag_id
        WHERE ctg.contact_id = @contactId
        ORDER BY tg.name";

    public static readonly string GetContactTicketsSql = @"
        SELECT
            t.id AS ticket_id,
            t.subject,
            t.status,
            t.created_at,
            t.closed_at,
            c.id AS conversation_id
        FROM tickets t
        INNER JOIN conversations c ON c.ticket_id = t.id
        WHERE t.contact_id = @contactId
        ORDER BY t.created_at DESC";

    public static readonly string GetOpenTicketByContactSql = @"
        SELECT c.id AS conversation_id
        FROM tickets t
        INNER JOIN conversations c ON c.ticket_id = t.id
        WHERE t.contact_id = @contactId AND t.status = 'open'
        ORDER BY t.created_at DESC
        LIMIT 1";

    public static readonly string GetIdsFromConversationSql = @"
        SELECT t.contact_id, t.id AS ticket_id
        FROM conversations c
        INNER JOIN tickets t ON t.id = c.ticket_id
        WHERE c.id = @conversationId";

    public static readonly string UpdateContactSql = @"
        UPDATE contacts
        SET name = @name, phone = @phone, email = @email,
            company = @company, notes = @notes, owner_id = @contactOwnerId, updated_at = NOW()
        WHERE id = @contactId";

    public static readonly string UpdateContactOwnerSql = @"
        UPDATE contacts
        SET owner_id = @ownerId, updated_at = NOW()
        WHERE id = @contactId";

    public static readonly string CreateContactSql = @"
        INSERT INTO contacts (id, name, phone, email, company, document, notes, tags, created_at, updated_at)
        VALUES (@id, @name, @phone, @email, @company, @document, @notes, @tags, NOW(), NOW())
        RETURNING id";

    public static readonly string DeleteContactTagsSql = @"
        DELETE FROM contact_tags
        WHERE contact_id = @contactId";

    public static readonly string InsertContactTagSql = @"
        INSERT INTO contact_tags (contact_id, tag_id)
        VALUES (@contactId, @tagId)";

    public static readonly string UpdateConversationOwnerSql = @"
        UPDATE conversations
        SET owner = @ownerId
        WHERE id = @conversationId";

    public static async Task<(ContactDetail[] contacts, int totalCount)> GetAllContacts(
        NpgsqlDataSource dataSource, int page, int pageSize, string? search, string? company, Guid? tagId, string? role, Guid? userId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        var isAdmin = role == "admin";

        // Get total count
        await using var countCmd = new NpgsqlCommand(CountAllContactsSql, conn);
        var searchParam = countCmd.Parameters.Add("@search", NpgsqlDbType.Text);
        searchParam.Value = (object?)search ?? DBNull.Value;
        var companyParam = countCmd.Parameters.Add("@company", NpgsqlDbType.Text);
        companyParam.Value = (object?)company ?? DBNull.Value;
        var tagIdParam = countCmd.Parameters.Add("@tagId", NpgsqlDbType.Text);
        tagIdParam.Value = (object?)tagId?.ToString() ?? DBNull.Value;
        countCmd.Parameters.AddWithValue("@isAdmin", isAdmin);
        var userIdParamCount = countCmd.Parameters.Add("@userId", NpgsqlDbType.Uuid);
        userIdParamCount.Value = (object?)userId ?? DBNull.Value;
        var totalCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

        // Get contacts with pagination
        await using var cmd = new NpgsqlCommand(GetAllContactsSql, conn);
        var searchParam2 = cmd.Parameters.Add("@search", NpgsqlDbType.Text);
        searchParam2.Value = (object?)search ?? DBNull.Value;
        var companyParam2 = cmd.Parameters.Add("@company", NpgsqlDbType.Text);
        companyParam2.Value = (object?)company ?? DBNull.Value;
        var tagIdParam2 = cmd.Parameters.Add("@tagId", NpgsqlDbType.Text);
        tagIdParam2.Value = (object?)tagId?.ToString() ?? DBNull.Value;
        cmd.Parameters.AddWithValue("@isAdmin", isAdmin);
        var userIdParam = cmd.Parameters.Add("@userId", NpgsqlDbType.Uuid);
        userIdParam.Value = (object?)userId ?? DBNull.Value;
        cmd.Parameters.AddWithValue("@limit", pageSize);
        cmd.Parameters.AddWithValue("@offset", (page - 1) * pageSize);
        await using var reader = await cmd.ExecuteReaderAsync();

        var contacts = new List<ContactDetail>();
        while (await reader.ReadAsync())
        {
            var tagsRaw = reader[reader.GetOrdinal("tags")];
            var tagsJson = tagsRaw.ToString() ?? "[]";
            var tags = System.Text.Json.JsonSerializer.Deserialize<TagItem[]>(tagsJson) ?? [];

            var ownerNameOrdinal = reader.GetOrdinal("owner_name");
            var ownerPicOrdinal = reader.GetOrdinal("owner_profile_pic");

            contacts.Add(new ContactDetail
            {
                ContactId = reader.GetGuid(reader.GetOrdinal("contact_id")),
                Name = reader.IsDBNull(reader.GetOrdinal("name"))
                    ? ""
                    : reader.GetString(reader.GetOrdinal("name")),
                Phone = reader.IsDBNull(reader.GetOrdinal("phone"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("phone")),
                Email = reader.IsDBNull(reader.GetOrdinal("email"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("email")),
                Company = reader.IsDBNull(reader.GetOrdinal("company"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("company")),
                Document = reader.IsDBNull(reader.GetOrdinal("document"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("document")),
                Notes = reader.IsDBNull(reader.GetOrdinal("notes"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("notes")),
                ProfilePic = reader.IsDBNull(reader.GetOrdinal("profile_pic"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("profile_pic")),
                ContactOwnerId = reader.IsDBNull(reader.GetOrdinal("contact_owner_id"))
                    ? null
                    : reader.GetGuid(reader.GetOrdinal("contact_owner_id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
                Tags = tags,
                Owner = reader.IsDBNull(ownerNameOrdinal)
                    ? null
                    : new OwnerInfo
                    {
                        Id = reader.IsDBNull(reader.GetOrdinal("contact_owner_id"))
                            ? Guid.Empty
                            : reader.GetGuid(reader.GetOrdinal("contact_owner_id")),
                        Name = reader.GetString(ownerNameOrdinal),
                        ProfilePic = reader.IsDBNull(ownerPicOrdinal)
                            ? null
                            : reader.GetString(ownerPicOrdinal),
                    },
            });
        }

        return (contacts.ToArray(), totalCount);
    }

    public static async Task<string[]> GetContactCompanies(NpgsqlDataSource dataSource)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(GetContactCompaniesSql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var companies = new List<string>();
        while (await reader.ReadAsync())
        {
            companies.Add(reader.GetString(0));
        }

        return companies.ToArray();
    }

    public static async Task<ContactDetail?> GetContactDetail(NpgsqlDataSource dataSource, Guid conversationId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(GetContactDetailSql, conn);
        cmd.Parameters.AddWithValue("@conversationId", conversationId);
        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        var contactId = reader.GetGuid(reader.GetOrdinal("contact_id"));

        var detail = new ContactDetail
        {
            ContactId = contactId,
            Name = reader.IsDBNull(reader.GetOrdinal("name"))
                ? ""
                : reader.GetString(reader.GetOrdinal("name")),
            Phone = reader.IsDBNull(reader.GetOrdinal("phone"))
                ? null
                : reader.GetString(reader.GetOrdinal("phone")),
            Email = reader.IsDBNull(reader.GetOrdinal("email"))
                ? null
                : reader.GetString(reader.GetOrdinal("email")),
            Company = reader.IsDBNull(reader.GetOrdinal("company"))
                ? null
                : reader.GetString(reader.GetOrdinal("company")),
            Document = reader.IsDBNull(reader.GetOrdinal("document"))
                ? null
                : reader.GetString(reader.GetOrdinal("document")),
            Notes = reader.IsDBNull(reader.GetOrdinal("notes"))
                ? null
                : reader.GetString(reader.GetOrdinal("notes")),
            ProfilePic = reader.IsDBNull(reader.GetOrdinal("profile_pic"))
                ? null
                : reader.GetString(reader.GetOrdinal("profile_pic")),
            ContactOwnerId = reader.IsDBNull(reader.GetOrdinal("contact_owner_id"))
                ? null
                : reader.GetGuid(reader.GetOrdinal("contact_owner_id")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),

            Ticket = new TicketInfo
            {
                Id = reader.GetGuid(reader.GetOrdinal("ticket_id")),
                Subject = reader.GetString(reader.GetOrdinal("subject")),
                Status = reader.GetString(reader.GetOrdinal("ticket_status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("ticket_created_at")),
            },

            Owner = reader.IsDBNull(reader.GetOrdinal("owner_id"))
                ? null
                : new OwnerInfo
                {
                    Id = reader.GetGuid(reader.GetOrdinal("owner_id")),
                    Name = reader.GetString(reader.GetOrdinal("owner_name")),
                    ProfilePic = reader.IsDBNull(reader.GetOrdinal("owner_profile_pic"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("owner_profile_pic")),
                },
        };

        await reader.CloseAsync();

        await using var tagsCmd = new NpgsqlCommand(GetContactTagsSql, conn);
        tagsCmd.Parameters.AddWithValue("@contactId", contactId);
        await using var tagsReader = await tagsCmd.ExecuteReaderAsync();

        var tags = new List<TagItem>();
        while (await tagsReader.ReadAsync())
        {
            tags.Add(new TagItem
            {
                Id = tagsReader.GetGuid(tagsReader.GetOrdinal("id")),
                Name = tagsReader.GetString(tagsReader.GetOrdinal("name")),
                ColorCode = tagsReader.GetString(tagsReader.GetOrdinal("color_code")),
            });
        }

        detail.Tags = tags.ToArray();

        return detail;
    }

    public static async Task<ContactTicket[]> GetContactTickets(NpgsqlDataSource dataSource, Guid contactId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(GetContactTicketsSql, conn);
        cmd.Parameters.AddWithValue("@contactId", contactId);
        await using var reader = await cmd.ExecuteReaderAsync();

        var tickets = new List<ContactTicket>();
        while (await reader.ReadAsync())
        {
            tickets.Add(new ContactTicket
            {
                TicketId = reader.GetGuid(reader.GetOrdinal("ticket_id")),
                Subject = reader.GetString(reader.GetOrdinal("subject")),
                Status = reader.GetString(reader.GetOrdinal("status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                ClosedAt = reader.IsDBNull(reader.GetOrdinal("closed_at"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("closed_at")),
                ConversationId = reader.GetGuid(reader.GetOrdinal("conversation_id")),
            });
        }

        return tickets.ToArray();
    }

    public static async Task<Guid?> GetOpenTicketConversationId(NpgsqlDataSource dataSource, Guid contactId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(GetOpenTicketByContactSql, conn);
        cmd.Parameters.AddWithValue("@contactId", contactId);
        var result = await cmd.ExecuteScalarAsync();

        if (result == null || result == DBNull.Value)
            return null;

        return (Guid)result;
    }

    public static async Task UpdateContact(NpgsqlDataSource dataSource, Guid conversationId, UpdateContactRequest request)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var tx = await conn.BeginTransactionAsync();

        await using var getIdsCmd = new NpgsqlCommand(GetIdsFromConversationSql, conn, tx);
        getIdsCmd.Parameters.AddWithValue("@conversationId", conversationId);
        await using var idsReader = await getIdsCmd.ExecuteReaderAsync();

        if (!await idsReader.ReadAsync())
        {
            await tx.RollbackAsync();
            throw new AppException("NOT_FOUND", "Conversa não encontrada");
        }

        var contactId = idsReader.GetGuid(idsReader.GetOrdinal("contact_id"));
        var ticketId = idsReader.GetGuid(idsReader.GetOrdinal("ticket_id"));
        await idsReader.CloseAsync();

        await using var updateContactCmd = new NpgsqlCommand(UpdateContactSql, conn, tx);
        updateContactCmd.Parameters.AddWithValue("@contactId", contactId);
        updateContactCmd.Parameters.AddWithValue("@name", request.Name ?? "");
        updateContactCmd.Parameters.AddWithValue("@phone", (object?)request.Phone ?? DBNull.Value);
        updateContactCmd.Parameters.AddWithValue("@email", (object?)request.Email ?? DBNull.Value);
        updateContactCmd.Parameters.AddWithValue("@company", (object?)request.Company ?? DBNull.Value);
        updateContactCmd.Parameters.AddWithValue("@notes", (object?)request.Notes ?? DBNull.Value);
        updateContactCmd.Parameters.AddWithValue("@contactOwnerId", (object?)request.ContactOwnerId ?? DBNull.Value);
        await updateContactCmd.ExecuteNonQueryAsync();

        if (request.TagIds is not null)
        {
            await using var deleteTagsCmd = new NpgsqlCommand(DeleteContactTagsSql, conn, tx);
            deleteTagsCmd.Parameters.AddWithValue("@contactId", contactId);
            await deleteTagsCmd.ExecuteNonQueryAsync();

            foreach (var tagId in request.TagIds)
            {
                await using var insertTagCmd = new NpgsqlCommand(InsertContactTagSql, conn, tx);
                insertTagCmd.Parameters.AddWithValue("@contactId", contactId);
                insertTagCmd.Parameters.AddWithValue("@tagId", tagId);
                await insertTagCmd.ExecuteNonQueryAsync();
            }
        }

        await using var updateOwnerCmd = new NpgsqlCommand(UpdateConversationOwnerSql, conn, tx);
        updateOwnerCmd.Parameters.AddWithValue("@ownerId", (object?)request.OwnerId ?? DBNull.Value);
        updateOwnerCmd.Parameters.AddWithValue("@conversationId", conversationId);
        await updateOwnerCmd.ExecuteNonQueryAsync();

        if (request.TicketSubject is not null || request.TicketStatus is not null)
        {
            var updates = new List<string>();
            var cmd = new NpgsqlCommand("", conn, tx);

            if (request.TicketSubject is not null)
            {
                updates.Add("subject = @subject");
                cmd.Parameters.AddWithValue("@subject", request.TicketSubject);
            }

            if (request.TicketStatus is not null)
            {
                var dbStatus = request.TicketStatus switch
                {
                    "resolved" => "resolvido",
                    "cancelled" => "cancelado",
                    "closed" => "resolvido",
                    _ => request.TicketStatus
                };
                updates.Add("status = @status::ticket_status");

                if (request.TicketStatus is "resolved" or "cancelled" or "resolvido" or "cancelado")
                {
                    updates.Add("closed_at = NOW()");
                }

                cmd.Parameters.AddWithValue("@status", dbStatus);
            }

            if (updates.Count > 0)
            {
                var updateTicketSql = $"UPDATE tickets SET {string.Join(", ", updates)} WHERE id = @ticketId";
                cmd.CommandText = updateTicketSql;
                cmd.Parameters.AddWithValue("@ticketId", ticketId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        await tx.CommitAsync();
    }

    public static async Task UpdateContactOwner(NpgsqlDataSource dataSource, Guid contactId, Guid? ownerId)
    {
        await using var conn = await dataSource.OpenConnectionAsync();

        await using var cmd = new NpgsqlCommand(UpdateContactOwnerSql, conn);
        cmd.Parameters.AddWithValue("@contactId", contactId);
        var ownerParam = cmd.Parameters.Add("@ownerId", NpgsqlDbType.Uuid);
        ownerParam.Value = (object?)ownerId ?? DBNull.Value;
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<Guid> CreateContact(NpgsqlDataSource dataSource, CreateContactRequest request)
    {
        await using var conn = await dataSource.OpenConnectionAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            var contactId = Guid.NewGuid();
            var tagsJson = request.TagIds != null && request.TagIds.Length > 0
                ? System.Text.Json.JsonSerializer.Serialize(request.TagIds)
                : "[]";

            await using var cmd = new NpgsqlCommand(CreateContactSql, conn, tx);
            cmd.Parameters.AddWithValue("@id", contactId);
            cmd.Parameters.AddWithValue("@name", request.Name);
            cmd.Parameters.AddWithValue("@phone", (object?)request.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@email", (object?)request.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@company", (object?)request.Company ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@document", (object?)request.Document ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@notes", (object?)request.Notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tags", tagsJson);
            await cmd.ExecuteScalarAsync();

            if (request.TagIds is not null && request.TagIds.Length > 0)
            {
                foreach (var tagId in request.TagIds)
                {
                    await using var insertTagCmd = new NpgsqlCommand(InsertContactTagSql, conn, tx);
                    insertTagCmd.Parameters.AddWithValue("@contactId", contactId);
                    insertTagCmd.Parameters.AddWithValue("@tagId", tagId);
                    await insertTagCmd.ExecuteNonQueryAsync();
                }
            }

            await tx.CommitAsync();
            return contactId;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
