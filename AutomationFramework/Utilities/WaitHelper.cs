using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AutomationFramework.Utilities;

public static class WaitHelper
{
    public static IWebElement Visible(IWebDriver driver, By locator)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15))
        {
            Message = $"Element did not become visible: {locator}"
        };
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
        return wait.Until(d => d.FindElements(locator).FirstOrDefault(element => element.Displayed))!;
    }
}
