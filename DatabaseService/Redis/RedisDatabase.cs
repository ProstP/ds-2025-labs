using StackExchange.Redis;

namespace DatabaseService.Redis;

public class RedisDatabase : IDatabaseService
{
    public string[] Get(string shardKey, string[] keys)
    {
        IDatabase db = ConnectToDatabase(shardKey);

        List<string> values = [];
        for (int i = 0; i < keys.Length; i++)
        {
            values.Add(db.StringGet(keys[i]));
            Console.WriteLine($"Key: {keys[i]}, segment: {shardKey}");
        }

        return values.ToArray();
    }

    public string Get(string shardKey, string key)
    {
        IDatabase db = ConnectToDatabase(shardKey);

        Console.WriteLine($"Key: {key}, segment: {shardKey}");
        return db.StringGet(key);
    }

    public void Set(string shardKey, KeyValuePair<string, string>[] pairs)
    {
        IDatabase db = ConnectToDatabase(shardKey);

        for (int i = 0; i < pairs.Length; i++)
        {
            string key = pairs[i].Key;
            string value = pairs[i].Value;

            db.StringSet(key, value);
        }
    }

    public void Set(string shardKey, KeyValuePair<string, string> pair)
    {
        IDatabase db = ConnectToDatabase(shardKey);

        db.StringSet(pair.Key, pair.Value);
    }

    private IDatabase ConnectToDatabase(string shardKey)
    {
        return GetConnectionMultiplexer(shardKey).GetDatabase();
    }
    private IConnectionMultiplexer GetConnectionMultiplexer(string shardKey)
    {
        string environmentConnectionLocation = $"REDIS_{shardKey}_CONNECTION_STR";
        string environmentPasswordLocation = $"REDIS_{shardKey}_PASSWORD";
        string connectionStr = Environment.GetEnvironmentVariable(environmentConnectionLocation);
        string password = Environment.GetEnvironmentVariable(environmentPasswordLocation);

        if (string.IsNullOrWhiteSpace(connectionStr) || string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Unknown shard key");
        }

        ConfigurationOptions options = ConfigurationOptions.Parse(connectionStr);
        options.Password = password;
        return ConnectionMultiplexer.Connect(options);
    }

    public IServer GetServerOfDB(string shardKey)
    {
        IConnectionMultiplexer connectionMultiplexer = GetConnectionMultiplexer(shardKey);

        return connectionMultiplexer.GetServer(connectionMultiplexer.GetEndPoints().First());
    }

    public void Set(string shardKey, string key, string value)
    {
        Set(shardKey, new KeyValuePair<string, string>(key, value));
    }
}