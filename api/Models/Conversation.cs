namespace api.Models;

public class Conversation
{
    public long[] Context { get; set; } = [];

    public CancellationTokenSource? TokenSource { get; set; }

    public string Adjunto { get; set; } = string.Empty;

}
