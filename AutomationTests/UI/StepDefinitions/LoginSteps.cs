using AutomationTests.Hooks;
using AutomationTests.UI.Pages;
using AutomationTests.Configuration;
using NUnit.Framework;
using Reqnroll;

namespace AutomationTests.UI.StepDefinitions;

[Binding]
public class LoginSteps(ScenarioSession session)
{
    private LoginPage Page => new(session.Driver);
    private readonly TestData data = TestSettings.LoadData();

    [Given("I open the login page")]
    public void Open() => Page.Open();

    [When("I sign in with the public demo account")]
    public void SignIn() => Page.SignIn(data.User, data.Password);

    [When("I sign in with an incorrect password")]
    public void SignInWithIncorrectPassword() => Page.SignIn(data.User, data.InvalidPassword);

    [Then("I see the secure area and a success message")]
    public void CheckSecureArea()
    {
        Assert.That(Page.SecureAreaHeading, Is.EqualTo("Secure Area"));
        Assert.That(Page.SuccessMessage, Does.Contain("You logged into a secure area!"));
    }

    [Then("I see the invalid password message")]
    public void CheckError() => Assert.That(Page.ErrorMessage, Does.Contain("Your password is invalid!"));
}
