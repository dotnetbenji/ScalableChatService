using ChatService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddSingleton<IMessageChannel, SseMessageChannel>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/sse-channel", async (IMessageChannel stream, HttpContext ctx) =>
{
    ctx.Response.Headers.Append("Content-Type", "text/event-stream");

    await ctx.Response.WriteAsync($"data: Welcome\n\n", ctx.RequestAborted);
    await ctx.Response.Body.FlushAsync(ctx.RequestAborted);

    await foreach (var msg in stream.Subscribe(ctx.RequestAborted))
    {
        await ctx.Response.WriteAsync($"data: {msg}\n\n", ctx.RequestAborted);
        await ctx.Response.Body.FlushAsync(ctx.RequestAborted);
    }
});

app.MapPost("/send", (SendMessageRequest messageRequest, IMessageChannel stream) =>
{
    stream.Publish(messageRequest.Message);
    return Results.Ok();
});

app.Run();

internal sealed record SendMessageRequest(string Message);