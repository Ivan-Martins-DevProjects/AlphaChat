using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public class ConnectionManager
{
    private readonly ConcurrentDictionary<Guid, List<WebSocket>> _connections = new();

    public void Add(Guid userId, WebSocket socket)
    {
        _connections.AddOrUpdate(
            userId,
            _ => new List<WebSocket> { socket },
            (_, sockets) =>
            {
                sockets.Add(socket);
                return sockets;
            });
    }

    public void Remove(Guid userId, WebSocket socket)
    {
        if (!_connections.TryGetValue(userId, out var sockets))
            return;

        sockets.Remove(socket);

        if (sockets.Count == 0)
            _connections.TryRemove(userId, out _);
    }

    public async Task SendAsync(Guid userId, object payload)
    {
        if (!_connections.TryGetValue(userId, out var sockets))
            return;

        var json = JsonSerializer.Serialize(payload);

        var bytes = Encoding.UTF8.GetBytes(json);

        foreach (var socket in sockets)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(
                    bytes,
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
        }
    }

    public async Task BroadcastAsync(object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);

        foreach (var sockets in _connections.Values)
        {
            foreach (var socket in sockets)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(
                        bytes,
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
            }
        }
    }
}
