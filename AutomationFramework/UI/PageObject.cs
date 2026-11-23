using OpenQA.Selenium;

namespace AutomationFramework.UI;

/// <summary>Provides helpers that reuse the WebDriver and base URL supplied by the scenario.</summary>
public abstract class PageObject(IWebDriver driver, Uri baseUri)
{
    /// <summary>Gets the navigation and browser-context helpers for the current session.</summary>
    protected BrowserActions Browser { get; } = new(driver);
    /// <summary>Gets element helpers bound to the same session.</summary>
    protected ElementActions Elements { get; } = new(driver);

    /// <summary>Resolves the supplied path against the configured base URL and navigates the existing browser.</summary>
    /// <param name="relativePath">The page path, such as <c>login</c>.</param>
    protected void NavigateTo(string relativePath) => Browser.Navigate(new Uri(baseUri, relativePath));
}
