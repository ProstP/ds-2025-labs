using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IConnectionMultiplexer _redis;

    public SummaryModel(ILogger<SummaryModel> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redis = redis;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet(string id)
    {
        _logger.LogDebug(id);

        IDatabase db = _redis.GetDatabase();

        RedisValue rankValue = db.StringGet($"RANK-{id}");
        if (!string.IsNullOrWhiteSpace(rankValue))
        {
            Rank = (double)rankValue;
        }

        RedisValue similarityValue = db.StringGet($"SIMILARITY-{id}");
        if (!string.IsNullOrWhiteSpace(similarityValue))
        {
            Similarity = (double)similarityValue;
        }
    }
}
