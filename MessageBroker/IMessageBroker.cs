namespace MessageBroker;

public interface IMessageBroker
{
    public Task SendMessageAsync(string queueName, string message);
    public Task ReceiveMessageAsync(string queueName, Action<string> messageHandler);
}
