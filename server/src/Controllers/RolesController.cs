using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IDBService _dBService;

    public RolesController(IDBService dBService)
    {
        _dBService = dBService;
    }

    [HttpGet]
    [RequirePermission("roles", "read")]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _dBService.GetAllRoles();
        return Ok(roles);
    }

    [HttpGet("{name}")]
    [RequirePermission("roles", "read")]
    public async Task<IActionResult> GetByName(string name)
    {
        var role = await _dBService.GetRoleByName(name);

        if (role is null)
        {
            return NotFound(new { message = "Cargo não encontrado" });
        }

        return Ok(role);
    }

    [HttpPost]
    [RequirePermission("roles", "create")]
    public async Task<IActionResult> Create([FromBody] RoleRow role)
    {
        try
        {
            var created = await _dBService.CreateRole(role);
            return CreatedAtAction(nameof(GetByName), new { name = created.Name }, created);
        }
        catch (AppException ex) when (ex.Code == "23505")
        {
            return Conflict(new { message = "Já existe um cargo com este nome" });
        }
    }

    [HttpPut("{name}")]
    [RequirePermission("roles", "update")]
    public async Task<IActionResult> Update(string name, [FromBody] RoleRow role)
    {
        var updated = await _dBService.UpdateRole(name, role);

        if (updated is null)
        {
            return NotFound(new { message = "Cargo não encontrado" });
        }

        return Ok(updated);
    }

    [HttpDelete("{name}")]
    [RequirePermission("roles", "delete")]
    public async Task<IActionResult> Delete(string name)
    {
        var deleted = await _dBService.DeleteRole(name);

        if (!deleted)
        {
            return NotFound(new { message = "Cargo não encontrado" });
        }

        return NoContent();
    }
}
