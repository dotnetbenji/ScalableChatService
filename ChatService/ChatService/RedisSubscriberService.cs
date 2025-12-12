using StackExchange.Redis;

namespace ChatService;

internal sealed class RedisSubscriberService(IConnectionMultiplexer _connectionMultiplexer, IMessageDistributuionService _messageDistributuionService) : BackgroundService
{
    public static RedisChannel Channel = new("MessageChannel", RedisChannel.PatternMode.Literal);

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _connectionMultiplexer.GetSubscriber();

        subscriber.Subscribe(Channel, (channel, message) =>
        {
            _messageDistributuionService.Publish(message);
        });

        return Task.CompletedTask;
    }
}