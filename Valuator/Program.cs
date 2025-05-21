using DatabaseService;
using DatabaseService.Redis;
using MessageBroker;
using MessageBroker.Rabbit;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Valuator;

public class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Login";
                options.LogoutPath = "/Logout";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            });

        builder.Services.AddSingleton<IDatabaseService, RedisDatabase>();

        RabbitMqService rabbitMqService = await RabbitMqService.CreateAsync(
            Environment.GetEnvironmentVariable("RABBIT_HOSTNAME"),
            Environment.GetEnvironmentVariable("RABBIT_USERNAME"),
            Environment.GetEnvironmentVariable("RABBIT_PASSWORD")
        );

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

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();
        app.UseAuthentication();

        app.MapRazorPages();

        app.Run();
    }
}
