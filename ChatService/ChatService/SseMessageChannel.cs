using System.Threading.Channels;

namespace ChatService;

internal interface IMessageChannel
{
    void Publish(string message);
    IAsyncEnumerable<string> ReadAllMessages(CancellationToken token);
}

public class SseMessageChannel : IMessageChannel
{
    private readonly Channel<string> channel = Channel.CreateUnbounded<string>();

    public void Publish(string message) => channel.Writer.TryWrite(message);

    public IAsyncEnumerable<string> ReadAllMessages(CancellationToken token) =>
        channel.Reader.ReadAllAsync(token);
}
