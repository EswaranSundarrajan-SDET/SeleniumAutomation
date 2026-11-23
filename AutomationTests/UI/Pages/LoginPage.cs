using AutomationFramework.UI;
using AutomationTests.Configuration;
using OpenQA.Selenium;

namespace AutomationTests.UI.Pages;

/// <summary>Encapsulates the demo authentication form and its outcome messages.</summary>
public sealed class LoginPage(IWebDriver driver) : PageObject(driver, TestSettings.BaseUri)
{
    private static readonly By Username = By.Id("username");
    private static readonly By Password = By.Id("password");
    private static readonly By SubmitButton = By.CssSelector("#login button[type='submit']");
    private static readonly By SecureHeading = By.CssSelector("#content h2");
    private static readonly By SuccessBanner = By.CssSelector("#flash.success");
    private static readonly By ErrorBanner = By.CssSelector("#flash.error");

    /// <summary>Opens the login form using the scenario's existing browser.</summary>
    public void Open() => NavigateTo("login");

    /// <summary>Enters the supplied credentials and submits the form.</summary>
    public void SignIn(string user, string password)
    {
        Elements.Type(Username, user);
        Elements.Type(Password, password);
        Elements.Click(SubmitButton);
    }

    /// <summary>Waits for the secure page before reading its heading.</summary>
    public string SecureAreaHeading
    {
        get
        {
            Browser.WaitForUrl(url => new Uri(url).AbsolutePath == "/secure");
            return Elements.Text(SecureHeading);
        }
    }
    /// <summary>Reads the displayed success banner.</summary>
    public string SuccessMessage => Elements.Text(SuccessBanner);
    /// <summary>Reads the displayed login error banner.</summary>
    public string ErrorMessage => Elements.Text(ErrorBanner);
}
