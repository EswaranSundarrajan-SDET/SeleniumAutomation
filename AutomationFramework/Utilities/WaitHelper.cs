using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AutomationFramework.Utilities;

public static class WaitHelper
{
    public static WebDriverWait Create(IWebDriver driver, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(driver, timeout ?? TimeSpan.FromSeconds(15));
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
        return wait;
    }

    public static IWebElement Present(IWebDriver driver, By locator) =>
        ForElement(driver, locator, _ => true, "present");

    public static IWebElement Visible(IWebDriver driver, By locator) =>
        ForElement(driver, locator, element => element.Displayed, "visible");

    public static IWebElement Enabled(IWebDriver driver, By locator) =>
        ForElement(driver, locator, element => element.Displayed && element.Enabled, "visible and enabled");

    public static void UntilInvisible(IWebDriver driver, By locator)
    {
        var wait = Create(driver);
        wait.Message = $"Element remained visible: {locator}";
        wait.Until(d => d.FindElements(locator).All(element => !element.Displayed));
    }

    private static IWebElement ForElement(IWebDriver driver, By locator, Func<IWebElement, bool> condition, string state)
    {
        var wait = Create(driver);
        wait.Message = $"Element did not become {state}: {locator}";
        return wait.Until(d => d.FindElements(locator).FirstOrDefault(condition))!;
    }
}
