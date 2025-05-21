using MessageBroker.Rabbit;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Events logger start word");

        RabbitMqService messageBroker = await RabbitMqService.CreateAsync(
            Environment.GetEnvironmentVariable("RABBIT_HOSTNAME"),
            Environment.GetEnvironmentVariable("RABBIT_USERNAME"),
            Environment.GetEnvironmentVariable("RABBIT_PASSWORD")
        );

        await messageBroker.DeclareTopologyAsync(
            Environment.GetEnvironmentVariable("EVENT_LOGGER_RABBIT_MQ_QUEUE_NAME"),
            Environment.GetEnvironmentVariable("EVENT_LOGGER_RABBIT_MQ_EXCHANGE_NAME"),
            Environment.GetEnvironmentVariable("EVENT_RANK_CALCULATED_ROUTING_KEY")
        );
        await messageBroker.DeclareTopologyAsync(
            Environment.GetEnvironmentVariable("EVENT_LOGGER_RABBIT_MQ_QUEUE_NAME"),
            Environment.GetEnvironmentVariable("EVENT_LOGGER_RABBIT_MQ_EXCHANGE_NAME"),
            Environment.GetEnvironmentVariable("EVENT_SIMILARITY_CALCULATED_ROUTING_KEY")
        );

        await messageBroker.ReceiveMessageAsync(Environment.GetEnvironmentVariable("EVENT_LOGGER_RABBIT_MQ_QUEUE_NAME"),
            message =>
            {
                Console.WriteLine("Update data:");
                Console.WriteLine(message);
            }
        );

        TaskCompletionSource<bool> exitEvent = new();
        Console.CancelKeyPress += (sender, args) =>
        {
            Console.WriteLine("Stopping events logger");
            args.Cancel = true;
            exitEvent.SetResult(true);
        };
        await exitEvent.Task;

        Console.WriteLine("Events logger stoped");
    }
}