using MessageBroker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly IMessageBroker _messageBroker;
    private readonly string _rankCalculatorMessageBrokerQueueName;

    public IndexModel(
        ILogger<IndexModel> logger,
        IConnectionMultiplexer redis,
        IMessageBroker messageBroker,
        IConfiguration configuration)
    {
        _logger = logger;
        _redis = redis;
        _messageBroker = messageBroker;
        _rankCalculatorMessageBrokerQueueName = configuration["RankCalculatorRabbitMq:QueueName"];
    }

    public void OnGet()
    {

    }

    public async Task<IActionResult> OnPost(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Redirect("index");
        }

        IDatabase db = _redis.GetDatabase();

        string id = Guid.NewGuid().ToString();

        string textKey = "TEXT-" + id;
        db.StringSet(textKey, text);

        _logger.LogDebug($"Saved text: {text} with id: {id}");
        _logger.LogInformation($"Saved text: {text} with id: {id}");

        string similarityKey = "SIMILARITY-" + id;
        double similarity = CalculateSimilarity(db, text, textKey);
        db.StringSet(similarityKey, similarity);

        await _messageBroker.SendMessageAsync(_rankCalculatorMessageBrokerQueueName, id);

        return Redirect($"summary?id={id}");
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
