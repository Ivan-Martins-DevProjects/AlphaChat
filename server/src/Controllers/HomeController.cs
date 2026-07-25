using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    private readonly IDBService _dbService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IDBService dbService, ILogger<HomeController> logger)
    {
        _dbService = dbService;
        _logger = logger;
    }

    [HttpGet]
    [RequirePermission("conversations", "read")]
    public async Task<IActionResult> Home([FromQuery] int offset = 0, [FromQuery] int limit = 20)
    {
        try
        {
            _logger.LogInformation("Recebida requisição GET /api/home (offset={Offset}, limit={Limit})", offset, limit);

            var claims = new TokenClaims
            {
                UserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value),
                Email = User.FindFirst(ClaimTypes.Email)!.Value,
                Role = User.FindFirst(ClaimTypes.Role)!.Value,
            };

            TicketCard[] conversations = [];

            if (claims.Role == "admin")
            {
                conversations = await _dbService.GetAllConversations(offset, limit);
            }
            else
            {
                conversations = await _dbService.GetConversationsByOwner(claims.UserId, offset, limit);
            }

            _logger.LogInformation("Retornando {Count} conversas", conversations.Length);
            return Ok(new { userId = claims.UserId, conversations });
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar conversas");
            throw new AppException("CONTROLLER_ERROR", "Erro ao processar requisição", ex);
        }
    }
}
