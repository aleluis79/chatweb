namespace api.Services;

public class ChatService : IChatService
{

    private Dictionary<Guid, IList<long>> _chats = new Dictionary<Guid, IList<long>>();
    private Dictionary<Guid, CancellationTokenSource> _token = new Dictionary<Guid, CancellationTokenSource>();

    public void AddContext(Guid contextId, IList<long> context)
    {
        if (_chats.ContainsKey(contextId)) {
            var currentContext = _chats[contextId];
            _chats[contextId] = currentContext.Concat(context).ToList();
        } else {
            _chats.Add(contextId, context);
        }
    }

    public IList<long>? GetContext(Guid contextId)
    {
        if (!_chats.ContainsKey(contextId)) {
            return null;
        }        
        return _chats[contextId];
    }

    public void AddToken(Guid contextId, CancellationTokenSource token)
    {
        if (_token.ContainsKey(contextId)) {
            _token[contextId] = token;
        } else {
            _token.Add(contextId, token);
        }
    }

    public void CancelToken(Guid contextId)
    {
        if (_token.ContainsKey(contextId)) {
            _token[contextId].Cancel();
        }

    }
}
