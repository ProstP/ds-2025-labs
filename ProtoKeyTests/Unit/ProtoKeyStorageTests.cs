using System.Threading.Channels;
using System.Threading.Tasks;
using Moq;
using ProtoKey.Application.Storage;
using ProtoKey.Application.Storage.Operations;
using ProtoKey.Application.Validator;

namespace ProtoKeyTests.Unit;

[TestFixture]
public class ProtoKeyStorageTests
{
    private Channel<Command> _commandChannel;
    private Channel<Response> _responseChannel;
    private ProtoKeyValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _commandChannel = Channel.CreateUnbounded<Command>();
        _responseChannel = Channel.CreateUnbounded<Response>();
        _validator = new();

        CreateAndRunStorage();
    }

    [Test]
    public async Task Set_ValidData_ResponseWithoutErrors()
    {
        string key = "key";
        int value = 0;

        var response = await SetKeyValueToStorageAsync(key, value);

        Assert.That(response.Type == ResponseType.SET);
        Assert.That(string.IsNullOrEmpty(response.ErrorMessage));

        _commandChannel.Writer.Complete();
    }
    [TestCase("+", 0)]
    [TestCase("a", null)]
    [TestCase("+", null)]
    public async Task Set_InvalidData_ResponseWithErrors(string key, int? value)
    {
        Command command = new Command()
        {
            Type = CommandType.SET,
            Key = key,
            Value = value
        };

        await _commandChannel.Writer.WriteAsync(command);
        Response response = await _responseChannel.Reader.ReadAsync();

        Assert.That(response.Type == ResponseType.ERROR);
        Assert.That(!string.IsNullOrEmpty(response.ErrorMessage));

        _commandChannel.Writer.Complete();
    }

    [Test]
    public async Task Get_UnknownKey_ReturnZero()
    {
        string key = "unknown-key";

        Response response = await GetFromStorageAsync(key);

        Assert.That(response.Type == ResponseType.GET);
        Assert.That(response.Value, Is.Zero);

        _commandChannel.Writer.Complete();
    }
    [Test]
    public async Task Get_ValidKey_ReturnValue()
    {
        string key = "key";
        int value = 10;

        await SetKeyValueToStorageAsync(key, value);

        Response response = await GetFromStorageAsync(key);

        Assert.That(response.Type == ResponseType.GET);
        Assert.That(response.Value, Is.EqualTo(value));

        _commandChannel.Writer.Complete();
    }
    [Test]
    public async Task Get_InvalidKey_ReturnError()
    {
        string key = ",";

        Response response = await GetFromStorageAsync(key);

        Assert.That(response.Type == ResponseType.ERROR);
        Assert.That(!string.IsNullOrEmpty(response.ErrorMessage));

        _commandChannel.Writer.Complete();
    }

    [Test]
    public async Task Keys_EmptyStorage_ReturnEmptyArray()
    {
        string prefix = "";

        Response response = await KeysFromStorageAsync(prefix);

        Assert.That(response.Type == ResponseType.KEYS);
        Assert.That(response.Keys.Length, Is.Zero);

        _commandChannel.Writer.Complete();
    }
    [TestCase("", 3)]
    [TestCase("S", 2)]
    [TestCase("Se", 0)]
    public async Task Keys_DifferentKeys_ReturnExpectedCount(string prefix, int expectedCount)
    {
        await SetKeyValueToStorageAsync("Arr", 3);
        await SetKeyValueToStorageAsync("Sss", 18);
        await SetKeyValueToStorageAsync("S123", 9);

        Response response = await KeysFromStorageAsync(prefix);

        Assert.That(response.Type == ResponseType.KEYS);
        Assert.That(response.Keys.Length, Is.EqualTo(expectedCount));

        _commandChannel.Writer.Complete();
    }

    private async Task<Response> SetKeyValueToStorageAsync(string key, int value)
    {
        return await SendCommandToStorageAsync(Command.CreateSet(key, value));
    }
    private async Task<Response> GetFromStorageAsync(string key)
    {
        return await SendCommandToStorageAsync(Command.CreateGet(key));
    }
    private async Task<Response> KeysFromStorageAsync(string prefix)
    {
        return await SendCommandToStorageAsync(Command.CreateKeys(prefix));
    }
    private async Task<Response> SendCommandToStorageAsync(Command command)
    {
        await _commandChannel.Writer.WriteAsync(command);
        return await _responseChannel.Reader.ReadAsync();
    }
    private void CreateAndRunStorage()
    {
        ProtoKeyStorage storage = new(_commandChannel, _responseChannel, _validator, );

        Task.Run(storage.RunAsync);
    }
}