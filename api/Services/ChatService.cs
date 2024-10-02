using System;

namespace api.Services;

public class ChatService : IChatService
{

    private Dictionary<Guid, IList<long>> _chats = new Dictionary<Guid, IList<long>>();

    public void AddContext(Guid contextId, IList<long> context)
    {
        if (_chats.ContainsKey(contextId)) {
            var currentContext = _chats[contextId];
            _chats[contextId] = currentContext.Concat(context).ToList();
        } else {
            _chats.Add(contextId, context);
        }
    }

    public IList<long> GetContext(Guid contextId)
    {
        return _chats[contextId];
    }
}
