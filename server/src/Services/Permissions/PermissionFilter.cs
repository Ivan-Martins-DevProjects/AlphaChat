using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class PermissionFilter : IAsyncAuthorizationFilter
{
    private readonly IPermissionService _permissionService;
    private readonly PermissionRequirement _requirement;
    private readonly ILogger<PermissionFilter> _logger;

    public PermissionFilter(
        IPermissionService permissionService,
        PermissionRequirement requirement,
        ILogger<PermissionFilter> logger)
    {
        _permissionService = permissionService;
        _requirement = requirement;
        _logger = logger;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var role = context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(role))
        {
            _logger.LogWarning("Cargo não encontrado no token");
            context.Result = new UnauthorizedObjectResult(new { message = "Cargo não encontrado no token" });
            return;
        }

        var hasPermission = await _permissionService.HasPermissionAsync(
            role,
            _requirement.Entity,
            _requirement.Operation);

        if (!hasPermission)
        {
            _logger.LogWarning(
                "Acesso negado: cargo {Role} não tem permissão para {Entity}.{Operation}",
                role,
                _requirement.Entity,
                _requirement.Operation);

            context.Result = new ForbidResult();
        }
    }
}
