using DatabaseService;
using MessageBroker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDatabaseService _db;
    private readonly IMessageBroker _messageBroker;
    private readonly string _rankCalculatorMessageBrokerExchangeName;
    private readonly string _similarityCalculateMessageBrokerExchangeName;
    private readonly string _similarityCalculatedRoutingKey;

    public IndexModel(
        ILogger<IndexModel> logger,
        IDatabaseService db,
        IMessageBroker messageBroker)
    {
        _logger = logger;
        _db = db;
        _messageBroker = messageBroker;
        _rankCalculatorMessageBrokerExchangeName = Environment.GetEnvironmentVariable("RANK_CALCULATOR_RABBIT_MQ_EXCHANGE_NAME");
        _similarityCalculateMessageBrokerExchangeName = Environment.GetEnvironmentVariable("EVENT_LOGGER_RABBIT_MQ_EXCHANGE_NAME");
        _similarityCalculatedRoutingKey = Environment.GetEnvironmentVariable("EVENT_SIMILARITY_CALCULATED_ROUTING_KEY");
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string text, string region)
    {
        Console.WriteLine(region);

        if (string.IsNullOrWhiteSpace(text))
        {
            return Redirect("index");
        }

        string username = User.Identity.Name;

        if (string.IsNullOrWhiteSpace(username))
        {
            return Redirect("login");
        }

        string id = Guid.NewGuid().ToString();

        string textKey = "TEXT-" + id;

        _logger.LogDebug($"Saved text: {text} with id: {id}");
        _logger.LogInformation($"Saved text: {text} with id: {id}");

        string similarityKey = "SIMILARITY-" + id;
        double similarity = CalculateSimilarity(text, textKey, region);

        _db.Set("MAIN", id, region);
        _db.Set(region, [
            new(id, username),
            new(textKey, text),
            new(similarityKey, similarity.ToString()),
        ]);

        await _messageBroker.SendMessageAsync(
            _similarityCalculateMessageBrokerExchangeName,
            $"id: {id}, similarity: {similarity}",
            _similarityCalculatedRoutingKey);

        await _messageBroker.SendMessageAsync(_rankCalculatorMessageBrokerExchangeName, id);

        return Redirect($"summary?id={id}");
    }

    private double CalculateSimilarity(string text, string textKey, string shardKey)
    {
        IServer server = _db.GetServerOfDB(shardKey);

        string[] keys = server.Keys(pattern: "TEXT-*")
                              .Select(k => (string)k)
                              .ToArray();

        string[] values = _db.Get(shardKey, keys.ToArray());

        for (int i = 0; i < keys.Length; i++)
        {
            if (string.Compare(keys[i], textKey) == 0)
            {
                continue;
            }

            if (string.Compare(text, values[i]) == 0)
            {
                return 1;
            }
        }

        return 0;
    }
}
