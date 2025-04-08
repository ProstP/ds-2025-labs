using MessageBroker.Rabbit;
using Microsoft.Extensions.Configuration;
using RankCalculator.Service;
using StackExchange.Redis;

public class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("RankCalculator start word");

        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        RankCalculatorService rankCalculatorService = new(
            ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_CONNECTION_STR"))
                                 .GetDatabase()
        );

        RabbitMqService messageBroker = await RabbitMqService.CreateAsync(
                Environment.GetEnvironmentVariable("RABBIT_HOSTNAME"),
                config.GetSection("RankCalculatorRabbitMq")["QueueName"],
                config.GetSection("RankCalculatorRabbitMq")["ExchangeName"]
        );

        await messageBroker.ReceiveMessageAsync(config.GetSection("RankCalculatorRabbitMq")["QueueName"],
            rankCalculatorService.Proccess);

        var exitEvent = new TaskCompletionSource<bool>();
        Console.CancelKeyPress += async (sender, args) =>
        {
            Console.WriteLine("Stopping rankCalculator");
            await messageBroker.DisposeAsync();
            args.Cancel = true;
            exitEvent.SetResult(true);
        };
        await exitEvent.Task;

        Console.WriteLine("RankCalculator stoped");
    }
}