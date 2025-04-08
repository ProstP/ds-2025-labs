using System.Text;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBroker.Rabbit;

public class RabbitMqService : IMessageBroker, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    private RabbitMqService(
        IConnection connection,
        IChannel channel)
    {
        _connection = connection;
        _channel = channel;
    }

    public static Task<RabbitMqService> CreateAsync(string hostname)
    {
        AsyncRetryPolicy retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(2));

        return retryPolicy.ExecuteAsync(async () =>
        {
            ConnectionFactory factory = new ConnectionFactory()
            {
                HostName = hostname,
            };

            IConnection connection = await factory.CreateConnectionAsync();
            IChannel channel = await connection.CreateChannelAsync();

            return new RabbitMqService(connection, channel);
        });
    }
    public async Task DeclareTopologyAsync(string queueName, string exchangeName, string routingKey = "")
    {
        await _channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic
        );
        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );
        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey
        );
    }

    public async Task ReceiveMessageAsync(string queueName, Action<string> messageHandler)
    {
        AsyncEventingBasicConsumer consumer = new(_channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            string message = Encoding.UTF8.GetString(args.Body.ToArray());

            messageHandler(message);

            await _channel.BasicAckAsync(deliveryTag: args.DeliveryTag, multiple: false);
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer
        );
    }

    public async Task SendMessageAsync(string exchangeName, string message, string routingKey = "")
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        await _channel.BasicPublishAsync(exchange: exchangeName, routingKey: routingKey, body: data);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        _channel.Dispose();
        await _connection.CloseAsync();
        _connection.Dispose();
    }
}