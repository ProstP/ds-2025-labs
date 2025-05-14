using E2ETests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll;

namespace E2ETests.Tests;

[Binding]
public class TextTestSteps : IDisposable
{
    IWebDriver _webDriver;
    IndexPage _indexPage;
    SummaryPage _summaryPage;

    const string URL = "http://localhost:8080/";

    [Given(@"Пользователь открывает веб-интерфейс")]
    public void OpenBrowser()
    {
        _webDriver = new ChromeDriver();

        _webDriver.Navigate().GoToUrl(URL);
        _indexPage = new(_webDriver);
        _summaryPage = new(_webDriver);
    }

    [When(@"Он отправляет текст ""(.*)""")]
    public void SendText(string text)
    {
        _indexPage.SetTextToArea(text);
        _indexPage.SumbitText();
    }

    [Then(@"Система отображает результат с рангом = (.*)")]
    public void CheckResult(double expectedRank)
    {
        Assert.True(_summaryPage.IsRankEqualTo(expectedRank));
    }

    public void Dispose()
    {
        _webDriver.Quit();
    }
}
