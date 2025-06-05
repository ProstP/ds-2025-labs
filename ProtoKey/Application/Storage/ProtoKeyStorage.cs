using System.Threading.Channels;
using ProtoKey.Application.Storage.Operations;
using ProtoKey.Application.Validator;

namespace ProtoKey.Application.Storage;

public class ProtoKeyStorage
{
    Dictionary<string, int> _storage = [];
    private readonly ChannelReader<Command> _commandReader;
    private readonly Channel<Command> _saveChannel;
    private readonly ChannelWriter<Response> _responseWriter;
    private readonly ProtoKeyValidator _protoKeyValidator;

    public ProtoKeyStorage(
        Channel<Command> commandChannel,
        Channel<Command> saveChannel,
        Channel<Response> responseChannel,
        ProtoKeyValidator validator)
    {
        _commandReader = commandChannel.Reader;
        _responseWriter = responseChannel.Writer;
        _protoKeyValidator = validator;
        _saveChannel = saveChannel;
    }

    public void InitStorage(Command[] commands)
    {
        foreach (Command command in commands)
        {
            if (command.Type != CommandType.SET)
            {
                continue;
            }

            if (command.Value is int valueToSet && _protoKeyValidator.IsValidKey(command.Key))
            {
                HandleSet(command.Key, valueToSet);
            }
        }
    }

    public async void RunAsync()
    {
        await foreach (Command command in _commandReader.ReadAllAsync())
        {
            try
            {
                await HandleCommand(command);
            }
            catch (Exception e)
            {
                await _responseWriter.WriteAsync(Response.CreateError(e.Message));
            }
        }
    }

    private async Task HandleCommand(Command command)
    {
        Response response;

        switch (command.Type)
        {
            case CommandType.SET:
                if (command.Value is int valueToSet && _protoKeyValidator.IsValidKey(command.Key))
                {
                    HandleSet(command.Key, valueToSet);

                    response = Response.CreateSet();
                }
                else
                {
                    throw new ArgumentException("Key or value is not valid on set command");
                }
                break;
            case CommandType.GET:
                if (_protoKeyValidator.IsValidKey(command.Key))
                {
                    int valueToGet = HandleGet(command.Key);

                    response = Response.CreateGet(valueToGet);
                }
                else
                {
                    throw new ArgumentException("Key is not valid on get command");
                }
                break;
            case CommandType.KEYS:
                string[] keys = HandleKeys(command.Prefix);

                response = Response.CreateKeys(keys);
                break;
            default:
                throw new ArgumentException("Unknown type of command");
        }

        await _responseWriter.WriteAsync(response);

        if (response.Type != ResponseType.ERROR)
        {
            await _saveChannel.Writer.WriteAsync(command);
        }
    }
    private void HandleSet(string key, int value)
    {
        _storage[key] = value;
    }
    private int HandleGet(string key)
    {
        if (_storage.ContainsKey(key))
        {
            return _storage[key];
        }
        else
        {
            return 0;
        }
    }
    private string[] HandleKeys(string prefix)
    {
        return _storage.Keys.Where(k => k.StartsWith(prefix)).ToArray();
    }
}