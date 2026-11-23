using AutomationFramework.Utilities;
using AutomationTests.Configuration;
using OpenQA.Selenium;

namespace AutomationTests.Pages;

public class LoginPage(IWebDriver driver)
{
    public void Open() => driver.Navigate().GoToUrl(new Uri(TestSettings.BaseUri, "login"));

    public void SignIn(string user, string password)
    {
        WaitHelper.Visible(driver, By.Id("username")).SendKeys(user);
        WaitHelper.Visible(driver, By.Id("password")).SendKeys(password);
        WaitHelper.Visible(driver, By.CssSelector("#login button[type='submit']")).Click();
    }

    public string SecureAreaHeading => WaitHelper.Visible(driver, By.CssSelector("#content h2")).Text;
    public string SuccessMessage => WaitHelper.Visible(driver, By.CssSelector("#flash.success")).Text;
    public string ErrorMessage => WaitHelper.Visible(driver, By.CssSelector("#flash.error")).Text;
}
