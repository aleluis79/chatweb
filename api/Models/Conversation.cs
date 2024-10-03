namespace api.Models;

public class Conversation
{
    public IList<long> Context { get; set; } = new List<long>();

    public CancellationTokenSource? tokenSource { get; set; }

}
