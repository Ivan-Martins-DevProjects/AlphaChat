using System.Security.Cryptography.X509Certificates;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(7233, listenOptions =>
    {
        listenOptions.UseHttps(httpsOptions =>
        {
            var cert = X509Certificate2.CreateFromPemFile("certs/cert.pem", "certs/key.pem");
            httpsOptions.ServerCertificate = cert;
        });
    });
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
    AllowedOrigins = { "https://localhost:4200", "https://localhost:7233" }
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
