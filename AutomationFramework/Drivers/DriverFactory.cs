using AutomationFramework.Utilities;
using OpenQA.Selenium;

namespace AutomationFramework.Drivers;

public static class DriverFactory
{
    public static IWebDriver Create(string? downloadDirectory = null)
    {
        var browser = ConfigHelper.Browser;
        IBrowserDriverProvider provider = browser switch
        {
            "chrome" => new ChromeDriverProvider(),
            "firefox" => new FirefoxDriverProvider(),
            _ => throw new ArgumentException($"Unsupported browser: {browser}")
        };

        return provider.Create(ConfigHelper.Headless, downloadDirectory);
    }
}
