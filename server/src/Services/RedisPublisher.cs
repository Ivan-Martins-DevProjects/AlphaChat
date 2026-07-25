using System.Text.Json;
using StackExchange.Redis;

public class RedisPublisher : IRedisPublisher
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisPublisher> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public RedisPublisher(IConnectionMultiplexer redis, ILogger<RedisPublisher> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task PublishMessageCreated(Guid messageId, Guid conversationId, Guid senderId, string content, string messageType, DateTime createdAt)
    {
        var subscriber = _redis.GetSubscriber();

        var payload = new
        {
            messageId,
            conversationId,
            senderId,
            content,
            messageType,
            createdAt,
        };

        var json = JsonSerializer.Serialize(payload, _jsonOptions);

        await subscriber.PublishAsync(RedisChannel.Literal("chat:message-created"), json);

        _logger.LogDebug("Evento message-created publicado: {MessageId}", messageId);
    }
}
