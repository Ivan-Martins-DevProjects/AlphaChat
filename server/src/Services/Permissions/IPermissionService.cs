public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string role, string entity, string operation);
    Task<PermissionRow?> GetPermissionsAsync(string role);
}
