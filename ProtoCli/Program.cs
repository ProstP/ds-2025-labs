using ProtoCli.Client;
using ProtoCli.Command;

internal class Program
{
    private static async Task Main(string[] args)
    {
        const string StorageHost = "http://localhost:5034/ProtoKey";

        ProtoKeyClient client = new(StorageHost);
        CommandHandler commandHandler = new(client);

        Console.WriteLine("ProtoCli started");
        await commandHandler.RunHandle();
        Console.WriteLine("ProtoCli closed");
    }
}