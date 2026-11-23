using AutomationFramework.UI;
using AutomationTests.Configuration;
using OpenQA.Selenium;

namespace AutomationTests.UI.Pages;

public sealed class DynamicLoadingPage(IWebDriver driver) : PageObject(driver, TestSettings.BaseUri)
{
    private static readonly By StartButton = By.CssSelector("#start button");
    private static readonly By LoadedMessage = By.CssSelector("#finish h4");

    public void Open() => NavigateTo("dynamic_loading/2");

    public void Start() => Elements.Click(StartButton);

    public string Message => Elements.Text(LoadedMessage);
}
