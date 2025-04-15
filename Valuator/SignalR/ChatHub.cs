using Microsoft.AspNetCore.SignalR;

namespace Valuator.SignalR;

public class ChatHub : Hub
{
    public async Task Send(string type, string value)
    {
        await Clients.All.SendAsync("Receive", type, value);
    }
}