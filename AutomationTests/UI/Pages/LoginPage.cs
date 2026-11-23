using AutomationFramework.UI;
using AutomationTests.Configuration;
using OpenQA.Selenium;

namespace AutomationTests.UI.Pages;

public sealed class LoginPage(IWebDriver driver) : PageObject(driver, TestSettings.BaseUri)
{
    private static readonly By Username = By.Id("username");
    private static readonly By Password = By.Id("password");
    private static readonly By SubmitButton = By.CssSelector("#login button[type='submit']");
    private static readonly By SecureHeading = By.CssSelector("#content h2");
    private static readonly By SuccessBanner = By.CssSelector("#flash.success");
    private static readonly By ErrorBanner = By.CssSelector("#flash.error");

    public void Open() => NavigateTo("login");

    public void SignIn(string user, string password)
    {
        Elements.Type(Username, user);
        Elements.Type(Password, password);
        Elements.Click(SubmitButton);
    }

    public string SecureAreaHeading
    {
        get
        {
            Browser.WaitForUrl(url => new Uri(url).AbsolutePath == "/secure");
            return Elements.Text(SecureHeading);
        }
    }
    public string SuccessMessage => Elements.Text(SuccessBanner);
    public string ErrorMessage => Elements.Text(ErrorBanner);
}
