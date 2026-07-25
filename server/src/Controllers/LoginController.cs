using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly IDBService _dbService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginController> _logger;

    public LoginController(IDBService dBService, IJwtService jwtService, ILogger<LoginController> logger)
    {
        _dbService = dBService;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Tentativa de login para o email: {Email}", request.Email);

            var user = await _dbService.GetUser(request);
            if (user is null)
            {
                _logger.LogWarning("Usuário não encontrado: {Email}", request.Email);
                throw new NotFoundException("Usuário", request.Email);
            }

            var password = await _dbService.GetPassword(user);
            if (password is null || password != request.Password)
            {
                _logger.LogWarning("Senha não encontrada para o usuário: {UserId}", user.Id);
                throw new NotFoundException("Usuário", request.Email);
            }

            var token = _jwtService.GenerateToken(user);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                MaxAge = TimeSpan.FromHours(8)
            };
            Response.Cookies.Append("token", token, cookieOptions);

            _logger.LogInformation("Login realizado com sucesso para o usuário: {UserId}", user.Id);
            return Ok();
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado durante o login para o email: {Email}", request.Email);
            throw new AppException("LOGIN_ERROR", "Erro durante o login", ex);
        }
    }
}
