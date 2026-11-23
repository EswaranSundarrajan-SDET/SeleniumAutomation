using AutomationFramework.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace AutomationFramework.Drivers;

public static class DriverFactory
{
    public static IWebDriver Create()
    {
        switch (ConfigHelper.Browser)
        {
            case "chrome":
                var chrome = new ChromeOptions();
                if (ConfigHelper.Headless) chrome.AddArgument("--headless=new");
                chrome.AddArgument("--window-size=1440,1000");
                return new ChromeDriver(chrome);
            case "firefox":
                var firefox = new FirefoxOptions();
                if (ConfigHelper.Headless) firefox.AddArgument("-headless");
                return new FirefoxDriver(firefox);
            default:
                throw new ArgumentException($"Unsupported browser: {ConfigHelper.Browser}");
        }
    }
}
