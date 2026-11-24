using AutomationFramework.UI;
using AutomationFramework.Utilities;
using AutomationTests.Configuration;
using OpenQA.Selenium;

namespace AutomationTests.UI.Pages;

/// <summary>Models the demo forms, cookie choices, scrolling content, and two-factor sign-in pages.</summary>
public sealed class AdvancedInteractionsPage(IWebDriver driver) : PageObject(driver, TestSettings.BaseUri)
{
    private static readonly By Username = By.Id("username");
    private static readonly By Password = By.Id("password");
    private static readonly By ValidationSubmit = By.CssSelector("#signup button[type='submit']");
    private static readonly By UsernameError = By.CssSelector("#username[aria-invalid='true'] + #username-error");
    private static readonly By CookieBanner = By.Id("cookie-banner");
    private static readonly By LoadedItems = By.CssSelector(".scroll .jscroll-added > br");
    private readonly AdvancedControl advanced = new(driver);
    private readonly IWebDriver browserDriver = driver;

    /// <summary>Opens the validation form.</summary>
    public void OpenValidationForm() => NavigateTo("form_validation");

    /// <summary>Tries to submit the validation form with its current values.</summary>
    public void SubmitValidationForm() => Elements.Click(ValidationSubmit);

    /// <summary>Checks that the browser flagged and focused the empty username field.</summary>
    public bool BrowserRequiresUsername => advanced.HasValidationError(Username, "valueMissing")
        && Elements.IsVisible(By.CssSelector("#username:focus"));

    /// <summary>Fills the form with valid values and a demo username the server marks as taken.</summary>
    public void SubmitTakenUsername(string username)
    {
        Elements.Type(Username, username);
        Elements.Type(By.Id("email"), "selenium@example.com");
        Elements.Type(Password, "automation-demo");
        Elements.SetChecked(By.Id("terms"), true);
        SubmitValidationForm();
    }

    /// <summary>Reads the error once the server rejects the username.</summary>
    public string UsernameErrorMessage
    {
        get
        {
            var wait = WaitHelper.Create(browserDriver, TimeSpan.FromSeconds(45));
            wait.Message = "The demo server did not display a username error.";
            return wait.Until(current => current.FindElements(UsernameError)
                .FirstOrDefault(element => element.Displayed))!.Text;
        }
    }

    /// <summary>Opens the cookie consent page and waits for its banner.</summary>
    public void OpenCookieConsent()
    {
        NavigateTo("cookie_consent");
        Elements.FindVisible(CookieBanner);
    }

    /// <summary>Saves the choice, waits for the banner to close, and reloads the current page.</summary>
    public void ChooseNecessaryCookiesAndReload()
    {
        Elements.Click(By.Id("reject-all"));
        Elements.WaitUntilHidden(CookieBanner);
        Browser.Refresh();
    }

    /// <summary>Checks whether the cookie banner is displayed.</summary>
    public bool CookieBannerVisible => Elements.IsVisible(CookieBanner);

    /// <summary>Reads the saved cookie choice.</summary>
    public string SavedCookieChoice => advanced.WaitForCookieValue("cookie_consent");

    /// <summary>Reads the site's analytics cookie status.</summary>
    public string ConsentStatus => Elements.Text(By.Id("consent-status"));

    /// <summary>Opens the scrolling page and waits for its first piece of loaded content.</summary>
    public void OpenInfiniteScroll()
    {
        NavigateTo("infinite_scroll");
        Elements.Find(LoadedItems);
    }

    /// <summary>Gets the current count of items that have finished loading.</summary>
    public int LoadedItemCount => Elements.FindAll(LoadedItems).Count;

    /// <summary>Scrolls to the end and returns the count after another item has loaded.</summary>
    public int LoadMoreItems() => advanced.ScrollForMoreItems(LoadedItems);

    /// <summary>Opens the two-factor sign-in page.</summary>
    public void OpenTwoFactorLogin() => NavigateTo("two_factor");

    /// <summary>Submits the public demo account credentials to open the second sign-in step.</summary>
    public void SubmitTwoFactorCredentials(TestData data)
    {
        Elements.Type(Username, data.User);
        Elements.Type(Password, data.Password);
        Elements.Click(By.CssSelector("#two-factor-login button[type='submit']"));
    }

    /// <summary>Enters the one-time code displayed by the demo on the challenge page.</summary>
    public void SubmitDisplayedCode()
    {
        var displayedCode = Elements.Text(By.Id("otp-code"));
        Elements.SendKeys(By.Id("code"), displayedCode);
        Elements.Click(By.CssSelector("#two-factor-code button[type='submit']"));
    }

    /// <summary>Waits for the two-factor secure page and reads its success banner.</summary>
    public string TwoFactorSuccessMessage
    {
        get
        {
            Browser.WaitForUrl(url => new Uri(url).AbsolutePath == "/two_factor/secure");
            return Elements.Text(By.CssSelector("#flash.success"));
        }
    }
}
