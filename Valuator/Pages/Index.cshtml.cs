using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IConnectionMultiplexer _redis;

    public IndexModel(ILogger<IndexModel> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text)
    {
        _logger.LogDebug(text);

        IDatabase db = _redis.GetDatabase();

        string id = Guid.NewGuid().ToString();

        string textKey = "TEXT-" + id;
        // TODO: (pa1) сохранить в БД (Redis) text по ключу textKey
        db.StringSet(textKey, text);

        string rankKey = "RANK-" + id;
        // TODO: (pa1) посчитать rank и сохранить в БД (Redis) по ключу rankKey
        double rank = CalculateRank(text);
        db.StringSet(rankKey, rank);

        string similarityKey = "SIMILARITY-" + id;
        // TODO: (pa1) посчитать similarity и сохранить в БД (Redis) по ключу similarityKey
        double similarity = CalculateSimilarity(db, text, textKey);
        db.StringSet(similarityKey, similarity);

        return Redirect($"summary?id={id}");
    }

    private double CalculateRank(string text)
    {
        double count = 0;

        foreach (char ch in text)
        {
            if (!char.IsLetter(ch))
            {
                count++;
            }
        }

        return count / text.Length;
    }
    private double CalculateSimilarity(IDatabase db, string text, string textKey)
    {
        IServer server = _redis.GetServer(_redis.GetEndPoints().First());

        IEnumerable<RedisKey> keys = server.Keys(pattern: "TEXT-*");

        foreach (RedisKey key in keys)
        {
            if (string.Compare(key, textKey) == 0)
            {
                continue;
            }

            if (string.Compare(text, db.StringGet(key)) == 0)
            {
                return 1;
            }
        }

        return 0;
    }
}
