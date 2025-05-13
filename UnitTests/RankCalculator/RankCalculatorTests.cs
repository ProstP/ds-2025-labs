using Newtonsoft.Json;
using RankCalculator.Service;

namespace UnitTests.RankCalculator;

public class RankCalculatorTests
{
    private const string FILE_PATH_WITH_DATA = "../../../TestingData/rank_calculator_test_data.json";

    [Theory]
    [MemberData(nameof(GetTestData))]
    public void CalculateRank_ShouldReturnExpectedResult(string text, double expectedRank)
    {
        double rank = RankCalculatorService.CalculateRank(text);

        Assert.Equal(expectedRank, rank, precision: 5);
    }

    public static IEnumerable<object[]> GetTestData()
    {
        IEnumerable<RankCalculatorTestData> data;
        using (StreamReader reader = new(FILE_PATH_WITH_DATA))
        {
            data = JsonConvert.DeserializeObject<IEnumerable<RankCalculatorTestData>>(reader.ReadToEnd());
        }

        foreach (var item in data)
        {
            yield return new object[] { item.Text, item.ExpectedRank };
        }
    }
}