using AutomationFramework.UI;
using AutomationTests.Configuration;
using OpenQA.Selenium;

namespace AutomationTests.Pages;

public class LoginPage(IWebDriver driver)
{
    private readonly ElementActions elements = new(driver);
    private readonly BrowserActions browser = new(driver);

    public void Open() => browser.Navigate(new Uri(TestSettings.BaseUri, "login"));

    public void SignIn(string user, string password)
    {
        elements.Type(By.Id("username"), user);
        elements.Type(By.Id("password"), password);
        elements.Click(By.CssSelector("#login button[type='submit']"));
    }

    public string SecureAreaHeading => elements.Text(By.CssSelector("#content h2"));
    public string SuccessMessage => elements.Text(By.CssSelector("#flash.success"));
    public string ErrorMessage => elements.Text(By.CssSelector("#flash.error"));
}
