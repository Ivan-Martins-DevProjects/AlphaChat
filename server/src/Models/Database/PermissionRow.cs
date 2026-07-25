public class PermissionRow
{
    public Guid Id { get; set; }
    public string Role { get; set; } = "";

    // Users
    public bool UsersRead { get; set; }
    public bool UsersCreate { get; set; }
    public bool UsersUpdate { get; set; }
    public bool UsersDelete { get; set; }

    // Conversations
    public bool ConversationsRead { get; set; }
    public bool ConversationsCreate { get; set; }
    public bool ConversationsUpdate { get; set; }
    public bool ConversationsDelete { get; set; }

    // Messages
    public bool MessagesRead { get; set; }
    public bool MessagesCreate { get; set; }
    public bool MessagesUpdate { get; set; }
    public bool MessagesDelete { get; set; }

    // Contacts
    public bool ContactsRead { get; set; }
    public bool ContactsCreate { get; set; }
    public bool ContactsUpdate { get; set; }
    public bool ContactsDelete { get; set; }

    // Tickets
    public bool TicketsRead { get; set; }
    public bool TicketsCreate { get; set; }
    public bool TicketsUpdate { get; set; }
    public bool TicketsDelete { get; set; }

    // Tags
    public bool TagsRead { get; set; }
    public bool TagsCreate { get; set; }
    public bool TagsUpdate { get; set; }
    public bool TagsDelete { get; set; }

    // Roles
    public bool RolesRead { get; set; }
    public bool RolesCreate { get; set; }
    public bool RolesUpdate { get; set; }
    public bool RolesDelete { get; set; }

    // Permissions
    public bool PermissionsRead { get; set; }
    public bool PermissionsCreate { get; set; }
    public bool PermissionsUpdate { get; set; }
    public bool PermissionsDelete { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
