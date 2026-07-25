using System.Text.Json;
using StackExchange.Redis;
using Websockets.Models;

public class RedisSubscribeWorker : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ConnectionManager _connections;
    private readonly ILogger<RedisSubscribeWorker> _logger;

    public RedisSubscribeWorker(
        IConnectionMultiplexer redis,
        ConnectionManager connections,
        ILogger<RedisSubscribeWorker> logger)
    {
        _redis = redis;
        _connections = connections;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _redis.GetSubscriber();

        await subscriber.SubscribeAsync(RedisChannel.Literal("chat:message-created"), async (channel, message) =>
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var evt = JsonSerializer.Deserialize<MessageCreatedEvent>(message!, options);

                if (evt is null) return;

                await _connections.BroadcastAsync(new
                {
                    type = "message_created",
                    messageId = evt.MessageId,
                    conversationId = evt.ConversationId,
                    senderId = evt.SenderId,
                    content = evt.Content,
                    messageType = evt.MessageType,
                    createdAt = evt.CreatedAt,
                });

                _logger.LogDebug("Evento message-created processado: {MessageId}", evt.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento message-created");
            }
        });
    }
}
