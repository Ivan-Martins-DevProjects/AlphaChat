using Npgsql;

public class PGService : IDBService
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<PGService> _logger;

    public PGService(NpgsqlDataSource dataSource, ILogger<PGService> logger)
    {
        _logger = logger;
        _dataSource = dataSource;
        _logger.LogInformation("Conexão com o banco de dados inicializada");
    }

    public async Task<UserRow?> GetUser(LoginRequest request)
    {
        try
        {
            _logger.LogDebug("Buscando usuário com email: {Email}", request.Email);
            var user = await AuthQueries.GetUser(_dataSource, request.Email);

            if (user is null)
                _logger.LogWarning("Usuário não encontrado: {Email}", request.Email);
            else
                _logger.LogInformation("Usuário encontrado: {UserId}", user.Id);

            return user;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuário: {Email}", request.Email);
            throw new AppException("DATABASE_ERROR", "Erro ao buscar usuário", ex);
        }
    }

    public async Task<string?> GetPassword(UserRow user)
    {
        try
        {
            _logger.LogDebug("Buscando senha do usuário: {UserId}", user.Id);
            var password = await AuthQueries.GetPassword(_dataSource, user.Id);

            if (password is null)
                _logger.LogWarning("Senha não encontrada para o usuário: {UserId}", user.Id);
            else
                _logger.LogInformation("Senha encontrada para o usuário: {UserId}", user.Id);

            return password;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar senha do usuário: {UserId}", user.Id);
            throw new AppException("DATABASE_ERROR", "Erro ao buscar senha", ex);
        }
    }

    public async Task<TicketCard[]> GetAllConversations(int offset, int limit)
    {
        try
        {
            _logger.LogDebug("Buscando conversas (offset={Offset}, limit={Limit})", offset, limit);
            var tickets = await ConversationQueries.GetAllConversations(_dataSource, offset, limit);
            _logger.LogInformation("Encontradas {Count} conversas", tickets.Length);
            return tickets;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar conversas");
            throw new AppException("DATABASE_ERROR", "Erro ao buscar conversas", ex);
        }
    }

    public async Task<TicketCard[]> GetConversationsByOwner(Guid userId, int offset, int limit)
    {
        try
        {
            _logger.LogDebug("Buscando conversas do usuário: {UserId} (offset={Offset}, limit={Limit})", userId, offset, limit);
            var tickets = await ConversationQueries.GetConversationsByOwner(_dataSource, userId, offset, limit);
            _logger.LogInformation("Encontradas {Count} conversas para o usuário: {UserId}", tickets.Length, userId);
            return tickets;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar conversas do usuário: {UserId}", userId);
            throw new AppException("DATABASE_ERROR", "Erro ao buscar conversas do usuário", ex);
        }
    }

    public async Task<MessageRow[]> GetMessagesByConversation(Guid conversationId, int offset)
    {
        try
        {
            _logger.LogDebug("Buscando mensagens da conversa: {ConversationId}", conversationId);
            var messages = await MessageQueries.GetMessagesByConversation(_dataSource, conversationId, offset);
            _logger.LogInformation("Encontradas {Count} mensagens para a conversa: {ConversationId}", messages.Length, conversationId);
            return messages;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar mensagens da conversa: {ConversationId}", conversationId);
            throw new AppException("DATABASE_ERROR", "Erro ao buscar mensagens", ex);
        }
    }

    public async Task<MessageRow> SaveMessage(Guid conversationId, Guid senderId, string content, string messageType)
    {
        try
        {
            _logger.LogDebug("Salvando mensagem na conversa: {ConversationId}", conversationId);
            var message = await MessageQueries.SaveMessage(_dataSource, conversationId, senderId, content, messageType);
            _logger.LogInformation("Mensagem salva: {MessageId} na conversa: {ConversationId}", message.Id, conversationId);
            return message;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar mensagem na conversa: {ConversationId}", conversationId);
            throw new AppException("DATABASE_ERROR", "Erro ao salvar mensagem", ex);
        }
    }

    public async Task<ContactDetail?> GetContactDetail(Guid conversationId)
    {
        try
        {
            _logger.LogDebug("Buscando detalhes do contato da conversa: {ConversationId}", conversationId);
            var detail = await ContactQueries.GetContactDetail(_dataSource, conversationId);

            if (detail is null)
                _logger.LogWarning("Conversa não encontrada: {ConversationId}", conversationId);
            else
                _logger.LogInformation("Detalhes do contato encontrados: {ContactId}", detail.ContactId);

            return detail;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar detalhes do contato da conversa: {ConversationId}", conversationId);
            throw new AppException("DATABASE_ERROR", "Erro ao buscar detalhes do contato", ex);
        }
    }

    public async Task<(ContactDetail[] contacts, int totalCount)> GetAllContacts(int page, int pageSize, string? search, string? company, Guid? tagId, string? role, Guid? userId)
    {
        try
        {
            _logger.LogDebug("Buscando todos os contatos (page={Page}, pageSize={PageSize}, search={Search}, company={Company}, tagId={TagId}, role={Role}, userId={UserId})", page, pageSize, search, company, tagId, role, userId);
            var result = await ContactQueries.GetAllContacts(_dataSource, page, pageSize, search, company, tagId, role, userId);
            _logger.LogInformation("Encontrados {Count} contatos de {Total}", result.contacts.Length, result.totalCount);
            return result;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contatos");
            throw new AppException("DATABASE_ERROR", "Erro ao buscar contatos", ex);
        }
    }

    public async Task<Guid> CreateContact(CreateContactRequest request)
    {
        try
        {
            _logger.LogDebug("Criando contato: {Name}", request.Name);
            var contactId = await ContactQueries.CreateContact(_dataSource, request);
            _logger.LogInformation("Contato criado: {ContactId}", contactId);
            return contactId;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar contato");
            throw new AppException("DATABASE_ERROR", "Erro ao criar contato", ex);
        }
    }

    public async Task<string[]> GetContactCompanies()
    {
        try
        {
            _logger.LogDebug("Buscando empresas dos contatos");
            var companies = await ContactQueries.GetContactCompanies(_dataSource);
            _logger.LogInformation("Encontradas {Count} empresas", companies.Length);
            return companies;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar empresas");
            throw new AppException("DATABASE_ERROR", "Erro ao buscar empresas", ex);
        }
    }

    public async Task<UserRow[]> GetAllUsers()
    {
        try
        {
            _logger.LogDebug("Buscando todos os usuários");
            var users = await UserQueries.GetAllUsers(_dataSource);
            _logger.LogInformation("Encontrados {Count} usuários", users.Length);
            return users;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuários");
            throw new AppException("DATABASE_ERROR", "Erro ao buscar usuários", ex);
        }
    }

    public async Task<TagItem[]> GetAllTags()
    {
        try
        {
            _logger.LogDebug("Buscando todas as tags");
            var tags = await TagQueries.GetAllTags(_dataSource);
            _logger.LogInformation("Encontradas {Count} tags", tags.Length);
            return tags;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar tags");
            throw new AppException("DATABASE_ERROR", "Erro ao buscar tags", ex);
        }
    }

    public async Task UpdateContact(Guid conversationId, UpdateContactRequest request)
    {
        try
        {
            _logger.LogDebug("Atualizando contato da conversa: {ConversationId}", conversationId);
            await ContactQueries.UpdateContact(_dataSource, conversationId, request);
            _logger.LogInformation("Contato atualizado na conversa: {ConversationId}", conversationId);
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar contato da conversa: {ConversationId}", conversationId);
            throw new AppException("DATABASE_ERROR", "Erro ao atualizar contato", ex);
        }
    }

    public async Task UpdateContactOwner(Guid contactId, Guid? ownerId)
    {
        try
        {
            _logger.LogDebug("Atualizando owner do contato: {ContactId}", contactId);
            await ContactQueries.UpdateContactOwner(_dataSource, contactId, ownerId);
            _logger.LogInformation("Owner do contato atualizado: {ContactId}", contactId);
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar owner do contato: {ContactId}", contactId);
            throw new AppException("DATABASE_ERROR", "Erro ao atualizar responsável", ex);
        }
    }

    public async Task<ContactTicket[]> GetContactTickets(Guid contactId)
    {
        try
        {
            _logger.LogDebug("Buscando tickets do contato: {ContactId}", contactId);
            var tickets = await ContactQueries.GetContactTickets(_dataSource, contactId);
            _logger.LogInformation("Encontrados {Count} tickets para o contato: {ContactId}", tickets.Length, contactId);
            return tickets;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar tickets do contato: {ContactId}", contactId);
            throw new AppException("DATABASE_ERROR", "Erro ao buscar tickets do contato", ex);
        }
    }

    public async Task<Guid?> GetOpenTicketConversationId(Guid contactId)
    {
        try
        {
            _logger.LogDebug("Buscando ticket aberto do contato: {ContactId}", contactId);
            var conversationId = await ContactQueries.GetOpenTicketConversationId(_dataSource, contactId);

            if (conversationId.HasValue)
                _logger.LogInformation("Ticket aberto encontrado: {ConversationId} para o contato: {ContactId}", conversationId.Value, contactId);
            else
                _logger.LogInformation("Nenhum ticket aberto encontrado para o contato: {ContactId}", contactId);

            return conversationId;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar ticket aberto do contato: {ContactId}", contactId);
            throw new AppException("DATABASE_ERROR", "Erro ao buscar ticket aberto", ex);
        }
    }

    public async Task MarkAsRead(Guid conversationId)
    {
        try
        {
            _logger.LogDebug("Marcando conversa como lida: {ConversationId}", conversationId);
            await ConversationQueries.MarkAsRead(_dataSource, conversationId);
            _logger.LogInformation("Conversa marcada como lida: {ConversationId}", conversationId);
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar conversa como lida: {ConversationId}", conversationId);
            throw new AppException("DATABASE_ERROR", "Erro ao marcar conversa como lida", ex);
        }
    }

    public async Task<Guid> CreateConversation(Guid contactId, Guid? ownerId)
    {
        try
        {
            _logger.LogDebug("Criando conversa para o contato: {ContactId}", contactId);
            var conversationId = await ConversationQueries.CreateConversation(_dataSource, contactId, ownerId);
            _logger.LogInformation("Conversa criada: {ConversationId} para o contato: {ContactId}", conversationId, contactId);
            return conversationId;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar conversa para o contato: {ContactId}", contactId);
            throw new AppException("DATABASE_ERROR", "Erro ao criar conversa", ex);
        }
    }

    public async Task<PermissionRow?> GetPermissionByRole(string role)
    {
        try
        {
            _logger.LogDebug("Buscando permissões para o cargo: {Role}", role);
            var permission = await PermissionQueries.GetByRole(_dataSource, role);

            if (permission is null)
                _logger.LogWarning("Permissões não encontradas para o cargo: {Role}", role);
            else
                _logger.LogInformation("Permissões encontradas para o cargo: {Role}", role);

            return permission;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar permissões para o cargo: {Role}", role);
            throw new AppException("DATABASE_ERROR", "Erro ao buscar permissões", ex);
        }
    }

    public async Task<PermissionRow[]> GetAllPermissions()
    {
        try
        {
            _logger.LogDebug("Buscando todas as permissões");
            var permissions = await PermissionQueries.GetAll(_dataSource);
            _logger.LogInformation("Encontradas {Count} permissões", permissions.Length);
            return permissions;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar permissões");
            throw new AppException("DATABASE_ERROR", "Erro ao buscar permissões", ex);
        }
    }

    public async Task<PermissionRow> CreatePermission(PermissionRow permission)
    {
        try
        {
            _logger.LogDebug("Criando permissões para o cargo: {Role}", permission.Role);
            var created = await PermissionQueries.Create(_dataSource, permission);
            _logger.LogInformation("Permissões criadas para o cargo: {Role}", permission.Role);
            return created;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar permissões para o cargo: {Role}", permission.Role);
            throw new AppException("DATABASE_ERROR", "Erro ao criar permissões", ex);
        }
    }

    public async Task<PermissionRow?> UpdatePermission(PermissionRow permission)
    {
        try
        {
            _logger.LogDebug("Atualizando permissões para o cargo: {Role}", permission.Role);
            var updated = await PermissionQueries.Update(_dataSource, permission);

            if (updated is null)
                _logger.LogWarning("Permissões não encontradas para o cargo: {Role}", permission.Role);
            else
                _logger.LogInformation("Permissões atualizadas para o cargo: {Role}", permission.Role);

            return updated;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar permissões para o cargo: {Role}", permission.Role);
            throw new AppException("DATABASE_ERROR", "Erro ao atualizar permissões", ex);
        }
    }

    public async Task<bool> DeletePermission(string role)
    {
        try
        {
            _logger.LogDebug("Removendo permissões para o cargo: {Role}", role);
            var deleted = await PermissionQueries.Delete(_dataSource, role);

            if (deleted)
                _logger.LogInformation("Permissões removidas para o cargo: {Role}", role);
            else
                _logger.LogWarning("Permissões não encontradas para o cargo: {Role}", role);

            return deleted;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover permissões para o cargo: {Role}", role);
            throw new AppException("DATABASE_ERROR", "Erro ao remover permissões", ex);
        }
    }

    public async Task<RoleRow[]> GetAllRoles()
    {
        try
        {
            _logger.LogDebug("Buscando todos os cargos");
            var roles = await RoleQueries.GetAll(_dataSource);
            _logger.LogInformation("Encontrados {Count} cargos", roles.Length);
            return roles;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cargos");
            throw new AppException("DATABASE_ERROR", "Erro ao buscar cargos", ex);
        }
    }

    public async Task<RoleRow?> GetRoleByName(string name)
    {
        try
        {
            _logger.LogDebug("Buscando cargo: {Name}", name);
            var role = await RoleQueries.GetByName(_dataSource, name);

            if (role is null)
                _logger.LogWarning("Cargo não encontrado: {Name}", name);
            else
                _logger.LogInformation("Cargo encontrado: {Name}", name);

            return role;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cargo: {Name}", name);
            throw new AppException("DATABASE_ERROR", "Erro ao buscar cargo", ex);
        }
    }

    public async Task<RoleRow> CreateRole(RoleRow role)
    {
        try
        {
            _logger.LogDebug("Criando cargo: {Name}", role.Name);
            var created = await RoleQueries.Create(_dataSource, role);
            _logger.LogInformation("Cargo criado: {Name}", role.Name);
            return created;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar cargo: {Name}", role.Name);
            throw new AppException("DATABASE_ERROR", "Erro ao criar cargo", ex);
        }
    }

    public async Task<RoleRow?> UpdateRole(string originalName, RoleRow role)
    {
        try
        {
            _logger.LogDebug("Atualizando cargo: {OriginalName}", originalName);
            var updated = await RoleQueries.Update(_dataSource, originalName, role);

            if (updated is null)
                _logger.LogWarning("Cargo não encontrado: {OriginalName}", originalName);
            else
                _logger.LogInformation("Cargo atualizado: {OriginalName} -> {NewName}", originalName, role.Name);

            return updated;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar cargo: {OriginalName}", originalName);
            throw new AppException("DATABASE_ERROR", "Erro ao atualizar cargo", ex);
        }
    }

    public async Task<bool> DeleteRole(string name)
    {
        try
        {
            _logger.LogDebug("Removendo cargo: {Name}", name);
            var deleted = await RoleQueries.Delete(_dataSource, name);

            if (deleted)
                _logger.LogInformation("Cargo removido: {Name}", name);
            else
                _logger.LogWarning("Cargo não encontrado: {Name}", name);

            return deleted;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover cargo: {Name}", name);
            throw new AppException("DATABASE_ERROR", "Erro ao remover cargo", ex);
        }
    }
}
