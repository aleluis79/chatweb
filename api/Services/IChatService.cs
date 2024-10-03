using System;

namespace api.Services;

public interface IChatService
{
    IList<long>? GetContext(Guid contextId);

    void AddContext(Guid contextId, IList<long> context);

    void AddToken(Guid contextId, CancellationTokenSource token);

    void CancelToken(Guid contextId);
}
