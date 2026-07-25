public interface IRedisPublisher
{
    Task PublishMessageCreated(Guid messageId, Guid conversationId, Guid senderId, string content, string messageType, DateTime createdAt);
}
