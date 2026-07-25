public interface IDBService
{
    Task<UserRow?> GetUser(LoginRequest request);
    Task<string?> GetPassword(UserRow user);
    Task<TicketCard[]> GetAllConversations(int offset, int limit);
    Task<TicketCard[]> GetConversationsByOwner(Guid userId, int offset, int limit);
    Task<MessageRow[]> GetMessagesByConversation(Guid conversationId, int offset);
    Task<MessageRow> SaveMessage(Guid conversationId, Guid senderId, string content, string messageType);
    Task<ContactDetail?> GetContactDetail(Guid conversationId);
    Task<(ContactDetail[] contacts, int totalCount)> GetAllContacts(int page, int pageSize, string? search, string? company, Guid? tagId, string? role, Guid? userId);
    Task<Guid> CreateContact(CreateContactRequest request);
    Task<string[]> GetContactCompanies();
    Task<UserRow[]> GetAllUsers();
    Task<TagItem[]> GetAllTags();
    Task UpdateContact(Guid conversationId, UpdateContactRequest request);
    Task UpdateContactOwner(Guid contactId, Guid? ownerId);
    Task<ContactTicket[]> GetContactTickets(Guid contactId);
    Task<Guid?> GetOpenTicketConversationId(Guid contactId);
    Task MarkAsRead(Guid conversationId);
    Task<Guid> CreateConversation(Guid contactId, Guid? ownerId);

    // Roles
    Task<RoleRow[]> GetAllRoles();
    Task<RoleRow?> GetRoleByName(string name);
    Task<RoleRow> CreateRole(RoleRow role);
    Task<RoleRow?> UpdateRole(string originalName, RoleRow role);
    Task<bool> DeleteRole(string name);

    // Permissions
    Task<PermissionRow?> GetPermissionByRole(string role);
    Task<PermissionRow[]> GetAllPermissions();
    Task<PermissionRow> CreatePermission(PermissionRow permission);
    Task<PermissionRow?> UpdatePermission(PermissionRow permission);
    Task<bool> DeletePermission(string role);
}
