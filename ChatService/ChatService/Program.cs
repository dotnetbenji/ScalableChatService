using ChatService;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    string? redisConnectionString = builder.Configuration.GetSection("Redis:ConnectionString").Value;
    if (redisConnectionString is null)
        throw new Exception("Redis connection string not found in config");

    return ConnectionMultiplexer.Connect(redisConnectionString);
});

builder.Services.AddScoped<IMessageChannel, SseMessageChannel>();
builder.Services.AddSingleton<IMessageDistributuionService, MessageDistributionService>();

builder.Services.AddHostedService<RedisSubscriberService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/sse", async (IMessageDistributuionService messageDistributionService, IMessageChannel stream, HttpContext ctx) =>
{
    ctx.Response.Headers.Append("Content-Type", "text/event-stream");

    await ctx.Response.WriteAsync($"data: Welcome\n\n", ctx.RequestAborted);
    await ctx.Response.Body.FlushAsync(ctx.RequestAborted);

    var success = messageDistributionService.Subscribe(stream);

    await foreach (var msg in stream.Subscribe(ctx.RequestAborted))
    {
        await ctx.Response.WriteAsync($"data: {msg}\n\n", ctx.RequestAborted);
        await ctx.Response.Body.FlushAsync(ctx.RequestAborted);
    }
});

app.MapPost("/send", async (SendMessageRequest messageRequest, IConnectionMultiplexer connectionMultiplexer) =>
{
    var pubsub = connectionMultiplexer.GetSubscriber();

    _ = pubsub.PublishAsync(RedisSubscriberService.Channel, messageRequest.Message, CommandFlags.FireAndForget);

    return Results.Ok();
});

app.Run();

internal sealed record SendMessageRequest(string Message);