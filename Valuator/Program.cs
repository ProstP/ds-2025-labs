using MessageBroker;
using MessageBroker.Rabbit;
using StackExchange.Redis;
using Valuator.SignalR;

namespace Valuator;

public class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        builder.Services.AddSingleton<IConnectionMultiplexer>(options =>
        {
            string configuration = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STR");
            return ConnectionMultiplexer.Connect(configuration);
        });

        RabbitMqService rabbitMqService = await RabbitMqService.
            CreateAsync(Environment.GetEnvironmentVariable("RABBIT_HOSTNAME"));
        await rabbitMqService.DeclareTopologyAsync(
            Environment.GetEnvironmentVariable("RANK_CALCULATOR_RABBIT_MQ_QUEUE_NAME"),
            Environment.GetEnvironmentVariable("RANK_CALCULATOR_RABBIT_MQ_EXCHANGE_NAME")
        );
        await rabbitMqService.DeclareTopologyAsync(
            Environment.GetEnvironmentVariable("EVENT_LOGGER_RABBIT_MQ_QUEUE_NAME"),
            Environment.GetEnvironmentVariable("EVENT_LOGGER_RABBIT_MQ_EXCHANGE_NAME"),
            Environment.GetEnvironmentVariable("EVENT_SIMILARITY_CALCULATED_ROUTING_KEY")
        );

        builder.Services.AddSingleton<IMessageBroker>(rabbitMqService);

        builder.Services.AddSignalR();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseRouting();

        app.UseAuthorization();

        app.UseWebSockets();

        app.MapHub<ChatHub>("/chat");

        app.UseStaticFiles();

        app.MapRazorPages();

        app.Run();
    }
}
