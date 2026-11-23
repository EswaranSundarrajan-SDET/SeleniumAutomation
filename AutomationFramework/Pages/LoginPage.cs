using AutomationFramework.Utilities;
using OpenQA.Selenium;

namespace AutomationFramework.Pages;

public class LoginPage(IWebDriver driver)
{
    public void Open(Uri baseUri) => driver.Navigate().GoToUrl(baseUri);
    public void SignIn(string user, string password)
    {
        WaitHelper.Visible(driver, "user").SendKeys(user);
        WaitHelper.Visible(driver, "password").SendKeys(password);
        WaitHelper.Visible(driver, "login").Click();
    }
    public string Error => WaitHelper.Visible(driver, "error").Text;
}
