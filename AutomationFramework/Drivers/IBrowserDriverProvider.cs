using OpenQA.Selenium;

namespace AutomationFramework.Drivers;

internal interface IBrowserDriverProvider
{
    IWebDriver Create(bool headless, string? downloadDirectory);
}
