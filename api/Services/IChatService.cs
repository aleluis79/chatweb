using api.Models;

namespace api.Services;

public interface IChatService
{
    Conversation? GetConversation(Guid contextId);
    void CancelToken(Guid contextId);
}
