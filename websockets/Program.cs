using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var port = builder.Configuration.GetValue<string>("PORT") ?? "5272";

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = builder.Configuration.GetValue<string>("Redis:ConnectionString")
        ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(config);
});

builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddSingleton<ChatWebSocketHandler>();
builder.Services.AddHostedService<RedisSubscribeWorker>();

var app = builder.Build();

app.UseWebSockets(new WebSocketOptions
{
    AllowedOrigins = { "http://localhost:4003", "http://localhost:4200", "http://localhost:5272" }
});

app.Map("/ws/chat", async (
    HttpContext context,
    ChatWebSocketHandler handler) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    var socket = await context.WebSockets.AcceptWebSocketAsync();

    await handler.HandleAsync(context, socket);
});

app.Run();
