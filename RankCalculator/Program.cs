using MessageBroker.Rabbit;
using RankCalculator.Service;
using StackExchange.Redis;

public class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("RankCalculator start word");

        RabbitMqService messageBroker = await RabbitMqService.
            CreateAsync(Environment.GetEnvironmentVariable("RABBIT_HOSTNAME"));
        await messageBroker.DeclareTopologyAsync(
            Environment.GetEnvironmentVariable("RANK_CALCULATOR_RABBIT_MQ_QUEUE_NAME"),
            Environment.GetEnvironmentVariable("RANK_CALCULATOR_RABBIT_MQ_EXCHANGE_NAME"));
        await messageBroker.DeclareTopologyAsync(
            Environment.GetEnvironmentVariable("EVENT_LOGGER_RABBIT_MQ_QUEUE_NAME"),
            Environment.GetEnvironmentVariable("EVENT_LOGGER_RABBIT_MQ_EXCHANGE_NAME"),
            Environment.GetEnvironmentVariable("EVENT_RANK_CALCULATED_ROUTING_KEY")
        );

        RankCalculatorService rankCalculatorService = new(
            ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_CONNECTION_STR")).GetDatabase(),
            messageBroker
        );

        await messageBroker.ReceiveMessageAsync(Environment.GetEnvironmentVariable("RANK_CALCULATOR_RABBIT_MQ_QUEUE_NAME"),
           async message => await rankCalculatorService.Proccess(message));

        TaskCompletionSource<bool> exitEvent = new TaskCompletionSource<bool>();
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