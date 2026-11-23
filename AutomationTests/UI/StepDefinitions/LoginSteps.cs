using AutomationTests.Hooks;
using AutomationTests.UI.Pages;
using AutomationTests.Configuration;
using NUnit.Framework;
using Reqnroll;

namespace AutomationTests.UI.StepDefinitions;

/// <summary>Connects the authentication scenarios to the scenario-scoped login page.</summary>
[Binding]
public class LoginSteps(ScenarioSession session)
{
    private LoginPage? page;
    /// <summary>Creates the page object lazily with Reqnroll's scenario browser.</summary>
    private LoginPage Page => page ??= new(session.Driver);
    private readonly TestData data = TestSettings.LoadData();

    /// <summary>Navigates to the login form.</summary>
    [Given("I open the login page")]
    public void Open() => Page.Open();

    /// <summary>Submits the public test account credentials from the test data file.</summary>
    [When("I sign in with the public demo account")]
    public void SignIn() => Page.SignIn(data.User, data.Password);

    /// <summary>Submits the known username with the intentionally invalid password.</summary>
    [When("I sign in with an incorrect password")]
    public void SignInWithIncorrectPassword() => Page.SignIn(data.User, data.InvalidPassword);

    /// <summary>Verifies the secure heading and successful login banner.</summary>
    [Then("I see the secure area and a success message")]
    public void CheckSecureArea()
    {
        Assert.That(Page.SecureAreaHeading, Is.EqualTo("Secure Area"));
        Assert.That(Page.SuccessMessage, Does.Contain("You logged into a secure area!"));
    }

    /// <summary>Verifies the invalid-password banner.</summary>
    [Then("I see the invalid password message")]
    public void CheckError() => Assert.That(Page.ErrorMessage, Does.Contain("Your password is invalid!"));
}
