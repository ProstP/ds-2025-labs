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
        }

        return values.ToArray();
    }

    public string Get(string shardKey, string key)
    {
        IDatabase db = ConnectToDatabase(shardKey);

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
        string connectionStr = Environment.GetEnvironmentVariable($"REDIS_{shardKey}_CONNECTION_STR");

        if (string.IsNullOrWhiteSpace(connectionStr))
        {
            throw new ArgumentException("Unknown shard key");
        }

        return ConnectionMultiplexer.Connect(connectionStr).GetDatabase();
    }
}