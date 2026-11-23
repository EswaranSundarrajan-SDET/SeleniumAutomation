using AutomationFramework.Hooks;
using NUnit.Framework;
using Reqnroll;

namespace AutomationFramework.StepDefinitions;

[Binding]
public class LoginSteps(ScenarioSession session)
{
    [Given("I open the login page")]
    public void Open() => session.Login.Open(session.BaseUri);

    [When("I sign in with the demo account")]
    public void SignIn() => session.Login.SignIn(session.Data.User, session.Data.Password);

    [When("I sign in with an incorrect password")]
    public void SignInWithIncorrectPassword() => session.Login.SignIn(session.Data.User, "incorrect");

    [Then("I see the settlement form")]
    public void CheckForm() => Assert.That(session.Settlement.FormVisible, Is.True);

    [Then("I see the invalid credentials message")]
    public void CheckError() => Assert.That(session.Login.Error, Is.EqualTo("Invalid credentials"));
}
