using StackExchange.Redis;

namespace RankCalculator.Service;

public class RankCalculatorService
{
    private readonly IDatabase _db;

    public RankCalculatorService(IDatabase db)
    {
        _db = db;
    }

    public void Proccess(string id)
    {
        string text = _db.StringGet($"TEXT-{id}");

        Console.WriteLine($"Calculate rank for {text} with id: {id}");
        double rank = CalculateRank(text);
        Console.WriteLine($"{id} has rank: {rank}");

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
}