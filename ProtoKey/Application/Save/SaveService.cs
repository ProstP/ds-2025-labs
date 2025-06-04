using System.Text.Json;
using System.Threading.Channels;
using ProtoKey.Application.Storage.Operations;

namespace ProtoKey.Application.Save;

public class SaveService
{
    const string DATA_FILE_PATH = "ProtoKey.data";

    Queue<Command> _commandsToSave = new();

    private readonly Channel<Command> _channel;
    public SaveService(Channel<Command> channel)
    {
        _channel = channel;
    }

    public async Task LoadData(Channel<Command> channelToCommands)
    {
        if (!File.Exists(DATA_FILE_PATH))
        {
            return;
        }

        foreach (string line in File.ReadAllLines(DATA_FILE_PATH))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            Command command = JsonSerializer.Deserialize<Command>(line);

            if (command.Type == CommandType.SET)
            {
                await channelToCommands.Writer.WriteAsync(command);
            }
        }
    }

    public async void RunAsync()
    {
        Timer timer = new(_ => SaveCommands(), null, 0, 1000);

        await foreach (var command in _channel.Reader.ReadAllAsync())
        {
            _commandsToSave.Enqueue(command);
        }
    }

    private void SaveCommands()
    {
        if (_commandsToSave.Count <= 0)
        {
            return;
        }
        Queue<Command> commands = new(_commandsToSave);
        _commandsToSave.Clear();

        while (commands.TryDequeue(out Command command))
        {
            string jsonData = JsonSerializer.Serialize(command) + Environment.NewLine;
            File.AppendAllText(DATA_FILE_PATH, jsonData);
        }
    }
}