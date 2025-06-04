using ProtoCli.Client;

namespace ProtoCli.Command;

public class CommandHandler
{
    const string GET_COMMAND = "Get";
    const string SET_COMMAND = "Set";
    const string KEYS_COMMAND = "Keys";
    const string EXIT_COMMAND = "...";

    private readonly ProtoKeyClient _client;
    public CommandHandler(ProtoKeyClient client)
    {
        _client = client;
    }

    public async Task RunHandle()
    {
        string line = Console.ReadLine();

        while (line != EXIT_COMMAND)
        {
            try
            {
                await Handle(line);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine();
            line = Console.ReadLine();
        }
    }

    private async Task Handle(string str)
    {
        string[] command = ParseCommand(str);

        switch (command[0])
        {
            case SET_COMMAND:
                await HandleSetCommand(command);
                break;
            case GET_COMMAND:
                await HandleGetCommand(command);
                break;
            case KEYS_COMMAND:
                await HandleKeysCommand(command);
                break;
            default:
                throw new ArgumentException("Unknown command");
        }
    }
    private string[] ParseCommand(string command)
    {
        return command.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArray();
    }

    private async Task HandleSetCommand(string[] command)
    {
        if (command.Length != 3)
        {
            throw new ArgumentException("Command count in set must equal 3");
        }

        if (int.TryParse(command[2], out int value))
        {
            await _client.Set(command[1], value);

            Console.WriteLine("Storage set success!");
        }
        else
        {
            throw new ArgumentException("Value in set must be int");
        }
    }
    private async Task HandleGetCommand(string[] command)
    {
        if (command.Length != 2)
        {
            throw new ArgumentException("Command count in get must equal 2");
        }

        int value = await _client.GetAsync(command[1]);
        Console.WriteLine($"{command[1]}: {value}");
    }
    private async Task HandleKeysCommand(string[] command)
    {
        if (command.Length != 2)
        {
            throw new ArgumentException("Command count in keys must equal 2");
        }

        string[] keys = await _client.KeysAsync(command[1]);

        Console.WriteLine("<------>");
        foreach (string key in keys)
        {
            Console.WriteLine(key);
        }
        Console.WriteLine("<------>");
    }
}