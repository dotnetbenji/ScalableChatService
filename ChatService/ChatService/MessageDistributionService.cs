namespace ChatService;

internal interface IMessageDistributuionService
{
    bool Subscribe(IMessageChannel channel);
    void Publish(string message);
}

internal sealed class MessageDistributionService : IMessageDistributuionService
{
    private readonly List<IMessageChannel> subscribers = [];

    public bool Subscribe(IMessageChannel channel)
    {
        subscribers.Add(channel);

        return true;
    }

    public void Publish(string message)
    {
        foreach(var sub in subscribers)
        {
            sub.Publish(message);
        }
    }
}
