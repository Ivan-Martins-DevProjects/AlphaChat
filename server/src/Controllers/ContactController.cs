using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly IDBService _dBService;

    public ContactController(IDBService dBService)
    {
        _dBService = dBService;
    }

    [HttpGet("users")]
    [RequirePermission("users", "read")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _dBService.GetAllUsers();
        return Ok(users);
    }

    [HttpGet("tags")]
    [RequirePermission("tags", "read")]
    public async Task<IActionResult> GetTags()
    {
        var tags = await _dBService.GetAllTags();
        return Ok(tags);
    }

    [HttpPost]
    [RequirePermission("contacts", "create")]
    public async Task<IActionResult> CreateContact([FromBody] CreateContactRequest request)
    {
        var contactId = await _dBService.CreateContact(request);
        return Ok(new { contactId });
    }

    [HttpGet("all")]
    [RequirePermission("contacts", "read")]
    public async Task<IActionResult> GetAllContacts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? company = null,
        [FromQuery] Guid? tagId = null)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        Guid? userId = null;

        if (Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var parsed))
            userId = parsed;

        var result = await _dBService.GetAllContacts(page, pageSize, search, company, tagId, role, userId);
        return Ok(new { contacts = result.contacts, totalCount = result.totalCount });
    }

    [HttpGet("companies")]
    [RequirePermission("contacts", "read")]
    public async Task<IActionResult> GetContactCompanies()
    {
        var companies = await _dBService.GetContactCompanies();
        return Ok(companies);
    }

    [HttpGet("{conversationId}")]
    [RequirePermission("contacts", "read")]
    public async Task<IActionResult> GetContactDetail(Guid conversationId)
    {
        var detail = await _dBService.GetContactDetail(conversationId);

        if (detail is null)
        {
            return NotFound(new { message = "Conversa não encontrada" });
        }

        return Ok(detail);
    }

    [HttpPut("{conversationId}")]
    [RequirePermission("contacts", "update")]
    public async Task<IActionResult> UpdateContact(Guid conversationId, [FromBody] UpdateContactRequest request)
    {
        await _dBService.UpdateContact(conversationId, request);
        return Ok(new { message = "Contato atualizado com sucesso" });
    }

    [HttpGet("{contactId}/tickets")]
    [RequirePermission("tickets", "read")]
    public async Task<IActionResult> GetContactTickets(Guid contactId)
    {
        var tickets = await _dBService.GetContactTickets(contactId);
        return Ok(tickets);
    }

    [HttpGet("{contactId}/open-ticket")]
    [RequirePermission("tickets", "read")]
    public async Task<IActionResult> GetOpenTicket(Guid contactId)
    {
        var conversationId = await _dBService.GetOpenTicketConversationId(contactId);

        if (conversationId.HasValue)
            return Ok(new { exists = true, conversationId = conversationId.Value });

        return Ok(new { exists = false });
    }

    [HttpPost("{contactId}/assume")]
    [RequirePermission("contacts", "update")]
    public async Task<IActionResult> AssumeContact(Guid contactId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var userName = User.FindFirst("name")?.Value ?? "";
        var userPic = User.FindFirst("profilePic")?.Value;

        await _dBService.UpdateContactOwner(contactId, userId);

        return Ok(new
        {
            ownerId = userId,
            ownerName = userName,
            ownerProfilePic = userPic
        });
    }

    [HttpDelete("{contactId}/owner")]
    [RequirePermission("contacts", "update")]
    public async Task<IActionResult> RemoveContactOwner(Guid contactId)
    {
        await _dBService.UpdateContactOwner(contactId, null);
        return Ok(new { message = "Responsável removido com sucesso" });
    }
}
