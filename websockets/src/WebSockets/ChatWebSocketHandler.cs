using System.IO;
using System.Net.WebSockets;

public class ChatWebSocketHandler
{
    private readonly ConnectionManager _connections;
    private readonly ILogger<ChatWebSocketHandler> _logger;

    public ChatWebSocketHandler(ConnectionManager connections, ILogger<ChatWebSocketHandler> logger)
    {
        _connections = connections;
        _logger = logger;
    }

    public async Task HandleAsync(HttpContext context, WebSocket socket)
    {
        var userIdStr = context.Request.Query["userId"].FirstOrDefault();

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            _logger.LogWarning("WebSocket conexão rejeitada: userId inválido '{UserId}'", userIdStr ?? "(null)");
            await socket.CloseAsync(
                WebSocketCloseStatus.InvalidPayloadData,
                "userId is required",
                CancellationToken.None);
            return;
        }

        _logger.LogInformation("WebSocket conexão aceita: userId={UserId}", userId);
        _connections.Add(userId, socket);

        var buffer = new byte[4096];

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;
            }
        }
        catch (WebSocketException)
        {
            // Cliente fechou a conexão sem handshake (reload, navegação, etc)
        }
        catch (IOException)
        {
            // Cliente resetou a conexão
        }
        finally
        {
            _connections.Remove(userId, socket);

            if (socket.State == WebSocketState.CloseReceived)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "",
                    CancellationToken.None);
            }
        }
    }
}
