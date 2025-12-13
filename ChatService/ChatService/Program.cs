using AuthenticationService.Sessions.Validators;
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
builder.Services.AddTransient<ISessionValidator, RedisSessionValidator>();

builder.Services.AddHostedService<RedisSubscriberService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/sse", async (IMessageDistributuionService messageDistributionService, IMessageChannel messageChannel, HttpContext ctx) =>
{
    ctx.Response.Headers.Append("Content-Type", "text/event-stream");

    await ctx.Response.WriteAsync($"data: Welcome\n\n", ctx.RequestAborted);
    await ctx.Response.Body.FlushAsync(ctx.RequestAborted);

    var success = messageDistributionService.Subscribe(messageChannel);

    await foreach (var msg in messageChannel.ReadAllMessages(ctx.RequestAborted))
    {
        await ctx.Response.WriteAsync($"data: {msg}\n\n", ctx.RequestAborted);
        await ctx.Response.Body.FlushAsync(ctx.RequestAborted);
    }
});

app.MapPost("/send", async (HttpContext ctx, SendMessageRequest messageRequest, IConnectionMultiplexer connectionMultiplexer, ISessionValidator sessionValidator) =>
{
    var tokenString = GetSessionTokenFromRequest(ctx);
    if (tokenString == null)
        return Results.Unauthorized();

    User? user = await sessionValidator.Validate(new SessionToken(tokenString));
    if (user is null)
        return Results.Unauthorized();

    var pubsub = connectionMultiplexer.GetSubscriber();

    _ = pubsub.PublishAsync(RedisSubscriberService.Channel, $"{user.Username}: {messageRequest.Message}", CommandFlags.FireAndForget);

    return Results.Ok();
});

app.Run();


static string? GetSessionTokenFromRequest(HttpContext ctx)
{
    var header = ctx.Request.Headers.Authorization.ToString();

    if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        return null;

    string encoded = header["Bearer ".Length..].Trim();

    try
    {
        return encoded;
    }
    catch
    {
        return null;
    }
}

internal sealed record SendMessageRequest(string Message);