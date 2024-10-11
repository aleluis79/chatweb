namespace api.Models;

public class Conversation
{
    public IList<long> Context { get; set; } = new List<long>();

    public CancellationTokenSource? TokenSource { get; set; }

    public string Adjunto { get; set; } = string.Empty;

}
