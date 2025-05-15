using DatabaseService;
using MessageBroker;

namespace RankCalculator.Service;

public class RankCalculatorService
{
    private readonly IDatabaseService _db;
    private readonly string _rankCalculateMessageBrokerExchangeName;
    private readonly string _rankCalculatedRoutingKey;
    private readonly IMessageBroker _messageBroker;

    public RankCalculatorService(IDatabaseService db, IMessageBroker messageBroker)
    {
        _db = db;
        _rankCalculateMessageBrokerExchangeName = Environment.GetEnvironmentVariable("EVENT_LOGGER_RABBIT_MQ_EXCHANGE_NAME");
        _rankCalculatedRoutingKey = Environment.GetEnvironmentVariable("EVENT_RANK_CALCULATED_ROUTING_KEY");
        _messageBroker = messageBroker;
    }

    public async Task Proccess(string id)
    {
        string shardKey = _db.Get("MAIN", id);
        string text = _db.Get(shardKey, $"TEXT-{id}");

        Console.WriteLine($"Calculate rank for {text} with id: {id}");
        double rank = CalculateRank(text);
        Console.WriteLine($"{id} has rank: {rank}");

        await SendRankAsync(id, rank);

        _db.Set(shardKey, $"RANK-{id}", rank.ToString());
    }

    private double CalculateRank(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

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
    private async Task SendRankAsync(string id, double rank)
    {
        await _messageBroker.SendMessageAsync(
            _rankCalculateMessageBrokerExchangeName,
            $"id: {id}, rank: {rank}",
            _rankCalculatedRoutingKey);
    }
}