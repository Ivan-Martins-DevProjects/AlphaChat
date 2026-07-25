using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IDBService _dBService;

    public ChatController(IDBService dBService)
    {
        _dBService = dBService;
    }

    [HttpGet]
    [RequirePermission("messages", "read")]
    public async Task<IActionResult> GetConversation([FromQuery] string conversationId, int offset)
    {
        MessageRow[] messages = await _dBService.GetMessagesByConversation(Guid.Parse(conversationId), offset);
        Guid UserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        return Ok(new { userId = UserId, messages });
    }

    [HttpPost]
    [RequirePermission("messages", "create")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        Guid senderId;

        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && Guid.TryParse(claim.Value, out var parsed))
        {
            senderId = parsed;
        }
        else
        {
            senderId = Guid.Parse("bf0150fd-50ab-421c-a98d-5d57e484d024");
        }

        var message = await _dBService.SaveMessage(
            request.ConversationId,
            senderId,
            request.Content,
            request.MessageType
        );

        return Ok(new { message });
    }

    [HttpPost("conversation")]
    [RequirePermission("conversations", "create")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        Guid? ownerId = null;

        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && Guid.TryParse(claim.Value, out var parsed))
        {
            ownerId = parsed;
        }

        var conversationId = await _dBService.CreateConversation(request.ContactId, ownerId);

        return Ok(new { conversationId });
    }

    [HttpPut("{conversationId}/read")]
    [RequirePermission("conversations", "update")]
    public async Task<IActionResult> MarkAsRead(Guid conversationId)
    {
        await _dBService.MarkAsRead(conversationId);
        return Ok(new { message = "Conversa marcada como lida" });
    }
}
