public class PermissionService : IPermissionService
{
    private readonly IDBService _dbService;
    private readonly ILogger<PermissionService> _logger;
    private readonly Dictionary<string, Func<PermissionRow, bool>> _permissionMap;

    public PermissionService(IDBService dbService, ILogger<PermissionService> logger)
    {
        _dbService = dbService;
        _logger = logger;

        _permissionMap = new Dictionary<string, Func<PermissionRow, bool>>
        {
            // Users
            ["users.read"] = p => p.UsersRead,
            ["users.create"] = p => p.UsersCreate,
            ["users.update"] = p => p.UsersUpdate,
            ["users.delete"] = p => p.UsersDelete,

            // Conversations
            ["conversations.read"] = p => p.ConversationsRead,
            ["conversations.create"] = p => p.ConversationsCreate,
            ["conversations.update"] = p => p.ConversationsUpdate,
            ["conversations.delete"] = p => p.ConversationsDelete,

            // Messages
            ["messages.read"] = p => p.MessagesRead,
            ["messages.create"] = p => p.MessagesCreate,
            ["messages.update"] = p => p.MessagesUpdate,
            ["messages.delete"] = p => p.MessagesDelete,

            // Contacts
            ["contacts.read"] = p => p.ContactsRead,
            ["contacts.create"] = p => p.ContactsCreate,
            ["contacts.update"] = p => p.ContactsUpdate,
            ["contacts.delete"] = p => p.ContactsDelete,

            // Tickets
            ["tickets.read"] = p => p.TicketsRead,
            ["tickets.create"] = p => p.TicketsCreate,
            ["tickets.update"] = p => p.TicketsUpdate,
            ["tickets.delete"] = p => p.TicketsDelete,

            // Tags
            ["tags.read"] = p => p.TagsRead,
            ["tags.create"] = p => p.TagsCreate,
            ["tags.update"] = p => p.TagsUpdate,
            ["tags.delete"] = p => p.TagsDelete,

            // Roles
            ["roles.read"] = p => p.RolesRead,
            ["roles.create"] = p => p.RolesCreate,
            ["roles.update"] = p => p.RolesUpdate,
            ["roles.delete"] = p => p.RolesDelete,

            // Permissions
            ["permissions.read"] = p => p.PermissionsRead,
            ["permissions.create"] = p => p.PermissionsCreate,
            ["permissions.update"] = p => p.PermissionsUpdate,
            ["permissions.delete"] = p => p.PermissionsDelete,
        };
    }

    public async Task<bool> HasPermissionAsync(string role, string entity, string operation)
    {
        var key = $"{entity}.{operation}".ToLower();

        if (!_permissionMap.TryGetValue(key, out var checkPermission))
        {
            _logger.LogWarning("Permissão desconhecida: {Key}", key);
            return false;
        }

        var permissions = await _dbService.GetPermissionByRole(role);
        if (permissions is null)
        {
            _logger.LogWarning("Permissões não encontradas para o cargo: {Role}", role);
            return false;
        }

        return checkPermission(permissions);
    }

    public async Task<PermissionRow?> GetPermissionsAsync(string role)
    {
        return await _dbService.GetPermissionByRole(role);
    }
}
