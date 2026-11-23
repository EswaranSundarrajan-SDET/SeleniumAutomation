using AutomationFramework.UI;
using AutomationTests.Configuration;
using OpenQA.Selenium;

namespace AutomationTests.UI.Pages;

/// <summary>Models the page that adds a message to the DOM after a loading delay.</summary>
public sealed class DynamicLoadingPage(IWebDriver driver) : PageObject(driver, TestSettings.BaseUri)
{
    private static readonly By StartButton = By.CssSelector("#start button");
    private static readonly By LoadedMessage = By.CssSelector("#finish h4");

    /// <summary>Opens the dynamic loading example whose result is initially absent.</summary>
    public void Open() => NavigateTo("dynamic_loading/2");

    /// <summary>Clicks the Start button to begin loading the result.</summary>
    public void Start() => Elements.Click(StartButton);

    /// <summary>Waits for the loaded message to become visible and returns its text.</summary>
    public string Message => Elements.Text(LoadedMessage);
}
