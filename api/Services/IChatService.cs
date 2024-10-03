using api.Models;

namespace api.Services;

public interface IChatService
{
    Conversation CreateConversation(Guid contextId);
    Conversation? GetConversation(Guid contextId);
    void AddContext(Guid contextId, IList<long> context);
    void CancelToken(Guid contextId);
}
