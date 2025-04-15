namespace DatabaseService;

public interface IDatabaseService
{
    string[] Get(string shardKey, string[] key);
    string Get(string shardKey, string key);

    void Set(string shardKey, KeyValuePair<string, string>[] pairs);
    void Set(string shardKey, KeyValuePair<string, string> pair);
}
