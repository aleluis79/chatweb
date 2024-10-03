using api.Models;

namespace api.Services;

public class ChatService : IChatService
{

    private Dictionary<Guid, Conversation> _conversations = new Dictionary<Guid, Conversation>();

    Conversation IChatService.CreateConversation(Guid contextId)
    {
        if (!_conversations.ContainsKey(contextId)) {
            _conversations.Add(contextId, new Conversation());
        }
        return _conversations[contextId];
    }

    public Conversation? GetConversation(Guid contextId)
    {
        if (!_conversations.ContainsKey(contextId)) {
            return null;
        }        
        return _conversations[contextId];
    }

    public void AddContext(Guid contextId, IList<long> context)
    {
        if (_conversations.ContainsKey(contextId)) {
            var currentContext = _conversations[contextId].Context;
            _conversations[contextId].Context = currentContext.Concat(context).ToList();
        } else {
            var conversation = new Conversation {
                Context = context
            };
            _conversations.Add(contextId, conversation);
        }
    }

    public void CancelToken(Guid contextId)
    {
        if (_conversations.ContainsKey(contextId) && _conversations[contextId].tokenSource != null) {
            _conversations[contextId].tokenSource?.Cancel();
        }

    }


}
