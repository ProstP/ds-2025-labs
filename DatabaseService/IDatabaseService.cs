using StackExchange.Redis;

namespace DatabaseService;

public interface IDatabaseService
{
    string Get(string shardKey, string key);
    string[] Get(string shardKey, string[] key);

    void Set(string shardKey, string key, string value);
    void Set(string shardKey, KeyValuePair<string, string> pair);
    void Set(string shardKey, KeyValuePair<string, string>[] pairs);

    IServer GetServerOfDB(string shardKey);
}
