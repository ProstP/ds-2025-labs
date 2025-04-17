using System.Security.Cryptography;
using System.Text;
using DatabaseService;
using MessageBroker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        string id = Guid.NewGuid().ToString();

        string textKey = "TEXT-" + id;

        _logger.LogDebug($"Saved text: {text} with id: {id}");
        _logger.LogInformation($"Saved text: {text} with id: {id}");

        string similarityKey = "SIMILARITY-" + id;
        string hash = GetStrHash(text);
        double similarity = CalculateSimilarity(hash, out int count);

        _db.Set("MAIN", [
            new(id, region),
            new(hash, count.ToString()),
        ]);
        _db.Set(region, [
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

    private double CalculateSimilarity(string hash, out int count)
    {
        string textCount = _db.Get("MAIN", hash);

        if (int.TryParse(textCount, out int result))
        {
            count = result + 1;
            return 1;
        }
        else
        {
            count = 1;
            return 0;
        }
    }
    private string GetStrHash(string text)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

}
