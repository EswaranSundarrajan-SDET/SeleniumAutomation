using AutomationFramework.UI;
using AutomationTests.Configuration;
using OpenQA.Selenium;

namespace AutomationTests.UI.Pages;

public class DynamicLoadingPage(IWebDriver driver)
{
    private readonly ElementActions elements = new(driver);
    private readonly BrowserActions browser = new(driver);

    public void Open() => browser.Navigate(new Uri(TestSettings.BaseUri, "dynamic_loading/2"));

    public void Start() => elements.Click(By.CssSelector("#start button"));

    public string Message => elements.Text(By.CssSelector("#finish h4"));
}
