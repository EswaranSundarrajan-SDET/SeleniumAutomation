using OpenQA.Selenium;

namespace AutomationFramework.UI;

public abstract class PageObject(IWebDriver driver, Uri baseUri)
{
    protected BrowserActions Browser { get; } = new(driver);
    protected ElementActions Elements { get; } = new(driver);

    protected void NavigateTo(string relativePath) => Browser.Navigate(new Uri(baseUri, relativePath));
}
