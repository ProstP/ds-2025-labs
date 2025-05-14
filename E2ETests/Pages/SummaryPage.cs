using System.Globalization;
using OpenQA.Selenium;

namespace E2ETests.Pages;

public class SummaryPage
{
    IWebDriver _webDriver;
    private static readonly By
        _rankTextXPath = By.XPath("//p[@id='rank']"),
        _similarityTextXPath = By.XPath("//p[@id='similarity']");

    public SummaryPage(IWebDriver webDriver)
    {
        _webDriver = webDriver;
    }

    public IWebElement GetRankText()
    {
        return _webDriver.FindElement(_rankTextXPath);
    }
    public IWebElement GetSimilarityText()
    {
        return _webDriver.FindElement(_similarityTextXPath);
    }

    public bool IsRankEqualTo(double rank)
    {
        string expectedText = $"Оценка содержания: {rank.ToString(CultureInfo.InvariantCulture)}";
        string actualText = GetRankText().Text;

        return expectedText == actualText;
    }
    public bool IsSimilarityEqualTo(double similarity)
    {
        string expectedText = $"Плагиат: {similarity.ToString(CultureInfo.InvariantCulture)}";
        string actualText = GetSimilarityText().Text;

        return expectedText == actualText;
    }
}