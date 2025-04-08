using MessageBroker;
using MessageBroker.Rabbit;
using StackExchange.Redis;

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

        builder.Services.AddSingleton<IMessageBroker>(
            await RabbitMqService.CreateAsync(
                Environment.GetEnvironmentVariable("RABBIT_HOSTNAME"),
                Environment.GetEnvironmentVariable("RANK_CALCULATOR_RABBIT_MQ_QUEUE_NAME"),
                Environment.GetEnvironmentVariable("RANK_CALCULATOR_RABBIT_MQ_EXCHANGE_NAME")
            )
        );

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}
