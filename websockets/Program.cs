using System.Security.Cryptography.X509Certificates;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var port = builder.Configuration.GetValue<string>("PORT") ?? "5272";
var certPath = builder.Configuration.GetValue<string>("CERT_PATH");
var keyPath = builder.Configuration.GetValue<string>("KEY_PATH");

if (!string.IsNullOrEmpty(certPath) && !string.IsNullOrEmpty(keyPath))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(int.Parse(port), listenOptions =>
        {
            listenOptions.UseHttps(httpsOptions =>
            {
                var cert = X509Certificate2.CreateFromPemFile(certPath, keyPath);
                httpsOptions.ServerCertificate = cert;
            });
        });
    });
}
else
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(int.Parse(port));
    });
}

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
    AllowedOrigins = { "http://localhost:4003", "https://localhost:4200", "https://localhost:7233" }
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
