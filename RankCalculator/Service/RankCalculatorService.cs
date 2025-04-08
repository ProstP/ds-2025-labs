using System.Threading.Tasks;
using MessageBroker;
using StackExchange.Redis;

namespace RankCalculator.Service;

public class RankCalculatorService
{
    private readonly IDatabase _db;
    private readonly string _rankCalculateMessageBrokerExchangeName;
    private readonly string _rankCalculatedRoutingKey;
    private readonly IMessageBroker _messageBroker;

    public RankCalculatorService(IDatabase db, IMessageBroker messageBroker)
    {
        _db = db;
        _rankCalculateMessageBrokerExchangeName = Environment.GetEnvironmentVariable("EVENT_LOGGER_RABBIT_MQ_EXCHANGE_NAME");
        _rankCalculatedRoutingKey = Environment.GetEnvironmentVariable("EVENT_RANK_CALCULATED_ROUTING_KEY");
        _messageBroker = messageBroker;
    }

    public async Task Proccess(string id)
    {
        string text = _db.StringGet($"TEXT-{id}");

        Console.WriteLine($"Calculate rank for {text} with id: {id}");
        double rank = CalculateRank(text);
        Console.WriteLine($"{id} has rank: {rank}");

        await SendRankAsync(id, rank);

        _db.StringSet($"RANK-{id}", rank);
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
    private async Task SendRankAsync(string id, double rank)
    {

        await _messageBroker.SendMessageAsync(
            _rankCalculateMessageBrokerExchangeName,
            $"id: {id}, rank: {rank}",
            _rankCalculatedRoutingKey);
    }
}