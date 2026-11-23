using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AutomationFramework.Utilities;

public static class WaitHelper
{
    public static IWebElement Visible(IWebDriver driver, string id)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
        return wait.Until(d => d.FindElements(By.Id(id)).FirstOrDefault(e => e.Displayed))!;
    }
}
