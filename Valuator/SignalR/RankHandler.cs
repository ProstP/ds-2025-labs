using MessageBroker;
using Microsoft.AspNetCore.SignalR;

namespace Valuator.SignalR;

public class RankHandler
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IMessageBroker _messageBroker;

    public RankHandler(IHubContext<ChatHub> hubContext, IMessageBroker messageBroker)
    {
        _hubContext = hubContext;
        _messageBroker = messageBroker;
    }

    public async Task HandleMessage(string message)
    {
        string type;
        if (message.Contains("rank"))
        {
            type = "rank";
        }
        else if (message.Contains("similarity"))
        {
            type = "similarity";
        }
        else
        {
            return;
        }

        int index = message.LastIndexOf(':') + 1;

        string value = message.Substring(index);

        Console.WriteLine($"{type} - {value}");

        await SendMessageSignalR(type, value);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _messageBroker.ReceiveMessageAsync(
               Environment.GetEnvironmentVariable("VALUATOR_LOGGER_RABBIT_MQ_QUEUE_NAME"),
               async (message) => await HandleMessage(message));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    private async Task SendMessageSignalR(string type, string value)
    {
        await _hubContext.Clients.All.SendAsync("Receive", type, value);
        Console.WriteLine("Message sended");
    }
}