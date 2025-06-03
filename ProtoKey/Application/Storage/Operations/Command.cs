namespace ProtoKey.Application.Storage.Operations;

public enum CommandType
{
    GET,
    SET,
    KEYS
}

public struct Command
{
    public CommandType Type;
    public string Key;
    public int? Value;
    public string Prefix;

    public static Command CreateSet(string key, int value)
    {
        return new Command()
        {
            Type = CommandType.SET,
            Key = key,
            Value = value,
            Prefix = null,
        };
    }
    public static Command CreateGet(string key)
    {
        return new Command()
        {
            Type = CommandType.GET,
            Key = key,
            Value = null,
            Prefix = null,
        };
    }
    public static Command CreateKeys(string prefix)
    {
        return new Command()
        {
            Type = CommandType.KEYS,
            Key = null,
            Value = null,
            Prefix = prefix,
        };
    }
}
