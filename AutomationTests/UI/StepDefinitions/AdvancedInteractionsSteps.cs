using AutomationTests.Configuration;
using AutomationTests.Hooks;
using AutomationTests.UI.Pages;
using NUnit.Framework;
using Reqnroll;

namespace AutomationTests.UI.StepDefinitions;

/// <summary>Binds the advanced form, cookie, scroll, and sign-in scenarios to their demo pages.</summary>
[Binding]
public sealed class AdvancedInteractionsSteps(ScenarioSession session)
{
    private AdvancedInteractionsPage? page;
    private AdvancedInteractionsPage Page => page ??= new(session.Driver);
    private int itemCountBeforeScroll;
    private int itemCountAfterScroll;

    [Given("I open the form validation example")]
    public void OpenValidationForm() => Page.OpenValidationForm();

    [When("I try to submit the empty validation form")]
    public void SubmitEmptyValidationForm() => Page.SubmitValidationForm();

    [Then("the browser requires a username")]
    public void VerifyUsernameRequired() => Assert.That(Page.BrowserRequiresUsername, Is.True);

    [When("I submit the validation form with the public demo username")]
    public void SubmitTakenUsername() => Page.SubmitTakenUsername(TestSettings.LoadData().User);

    [Then("the page reports that the username is already taken")]
    public void VerifyUsernameTaken() => Assert.That(Page.UsernameErrorMessage, Is.EqualTo("Username is already taken"));

    [Given("I open the cookie consent example with no saved choice")]
    public void OpenCookieConsent() => Page.OpenCookieConsent();

    [When("I choose only necessary cookies and reload the page")]
    public void ChooseNecessaryCookies() => Page.ChooseNecessaryCookiesAndReload();

    [Then("the cookie banner stays hidden and analytics cookies stay blocked")]
    public void VerifySavedConsent()
    {
        Assert.That(Page.CookieBannerVisible, Is.False);
        Assert.That(Page.SavedCookieChoice, Is.EqualTo("necessary"));
        Assert.That(Page.ConsentStatus, Is.EqualTo("Analytics cookies: blocked"));
    }

    [Given("I open the infinite scroll example and wait for its first item")]
    public void OpenInfiniteScroll() => Page.OpenInfiniteScroll();

    [When("I scroll to load more items")]
    public void ScrollForMoreItems()
    {
        itemCountBeforeScroll = Page.LoadedItemCount;
        itemCountAfterScroll = Page.LoadMoreItems();
    }

    [Then("the existing page contains more items")]
    public void VerifyMoreItems() => Assert.That(itemCountAfterScroll, Is.GreaterThan(itemCountBeforeScroll));

    [Given("I open the two-factor login example")]
    public void OpenTwoFactorLogin() => Page.OpenTwoFactorLogin();

    [When("I enter the public demo credentials for two-factor login")]
    public void SubmitTwoFactorCredentials() => Page.SubmitTwoFactorCredentials(TestSettings.LoadData());

    [When("I submit the one-time code shown on the page")]
    public void SubmitDisplayedCode() => Page.SubmitDisplayedCode();

    [Then("I see the two-factor secure area")]
    public void VerifyTwoFactorLogin() => Assert.That(Page.TwoFactorSuccessMessage,
        Does.Contain("You logged in with two-factor authentication!"));
}
