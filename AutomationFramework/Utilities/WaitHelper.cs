using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AutomationFramework.Utilities;

/// <summary>Polls specific WebDriver conditions within a bounded timeout.</summary>
public static class WaitHelper
{
    /// <summary>Creates a 15-second explicit wait by default and tolerates stale elements during polling.</summary>
    /// <remarks>Browser providers leave implicit waits at the WebDriver default of zero.</remarks>
    public static WebDriverWait Create(IWebDriver driver, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(driver, timeout ?? TimeSpan.FromSeconds(15));
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
        return wait;
    }

    /// <summary>Waits for the first DOM match regardless of visibility.</summary>
    public static IWebElement Present(IWebDriver driver, By locator) =>
        ForElement(driver, locator, _ => true, "present");

    /// <summary>Waits for the first displayed match.</summary>
    public static IWebElement Visible(IWebDriver driver, By locator) =>
        ForElement(driver, locator, element => element.Displayed, "visible");

    /// <summary>Waits for a match that is both displayed and enabled.</summary>
    public static IWebElement Enabled(IWebDriver driver, By locator) =>
        ForElement(driver, locator, element => element.Displayed && element.Enabled, "visible and enabled");

    /// <summary>Waits until every match is hidden or the locator matches nothing.</summary>
    public static void UntilInvisible(IWebDriver driver, By locator)
    {
        var wait = Create(driver);
        wait.Message = $"Element remained visible: {locator}";
        wait.Until(d => d.FindElements(locator).All(element => !element.Displayed));
    }

    /// <summary>Waits until a previously located element detaches from the document.</summary>
    public static void UntilStale(IWebDriver driver, IWebElement element)
    {
        var wait = Create(driver);
        wait.Message = "The original element remained attached to the document.";
        wait.Until(driverContext =>
        {
            try
            {
                _ = element.Enabled;
                return false;
            }
            catch (StaleElementReferenceException)
            {
                return true;
            }
        });
    }

    /// <summary>Requeries the locator on each poll and reports the expected state on timeout.</summary>
    private static IWebElement ForElement(IWebDriver driver, By locator, Func<IWebElement, bool> condition, string state)
    {
        var wait = Create(driver);
        wait.Message = $"Element did not become {state}: {locator}";
        return wait.Until(d => d.FindElements(locator).FirstOrDefault(condition))!;
    }
}
