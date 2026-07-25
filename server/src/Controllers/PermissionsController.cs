using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IDBService _dBService;

    public PermissionsController(IDBService dBService)
    {
        _dBService = dBService;
    }

    [HttpGet]
    [RequirePermission("permissions", "read")]
    public async Task<IActionResult> GetAll()
    {
        var permissions = await _dBService.GetAllPermissions();
        return Ok(permissions);
    }

    [HttpGet("{role}")]
    [RequirePermission("permissions", "read")]
    public async Task<IActionResult> GetByRole(string role)
    {
        var permission = await _dBService.GetPermissionByRole(role);

        if (permission is null)
        {
            return NotFound(new { message = "Permissões não encontradas para este cargo" });
        }

        return Ok(permission);
    }

    [HttpPost]
    [RequirePermission("permissions", "create")]
    public async Task<IActionResult> Create([FromBody] PermissionRow permission)
    {
        try
        {
            var created = await _dBService.CreatePermission(permission);
            return CreatedAtAction(nameof(GetByRole), new { role = created.Role }, created);
        }
        catch (AppException ex) when (ex.Code == "23505")
        {
            return Conflict(new { message = "Já existem permissões para este cargo" });
        }
    }

    [HttpPut("{role}")]
    [RequirePermission("permissions", "update")]
    public async Task<IActionResult> Update(string role, [FromBody] PermissionRow permission)
    {
        permission.Role = role;
        var updated = await _dBService.UpdatePermission(permission);

        if (updated is null)
        {
            return NotFound(new { message = "Permissões não encontradas para este cargo" });
        }

        return Ok(updated);
    }

    [HttpDelete("{role}")]
    [RequirePermission("permissions", "delete")]
    public async Task<IActionResult> Delete(string role)
    {
        var deleted = await _dBService.DeletePermission(role);

        if (!deleted)
        {
            return NotFound(new { message = "Permissões não encontradas para este cargo" });
        }

        return NoContent();
    }
}
