using api.Models;

namespace api.Services;

public class ChatService : IChatService
{

    private Dictionary<Guid, Conversation> _conversations = new Dictionary<Guid, Conversation>();

    public Conversation? GetConversation(Guid contextId)
    {
        if (!_conversations.ContainsKey(contextId)) {
            _conversations.Add(contextId, new Conversation());
        }        
        return _conversations[contextId];
    }

    public void CancelToken(Guid contextId)
    {
        if (_conversations.ContainsKey(contextId) && _conversations[contextId].TokenSource != null) {
            _conversations[contextId].TokenSource?.Cancel();
        }

    }


}
