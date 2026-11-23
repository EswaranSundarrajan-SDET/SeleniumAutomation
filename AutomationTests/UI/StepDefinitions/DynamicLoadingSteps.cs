using AutomationTests.Hooks;
using AutomationTests.UI.Pages;
using NUnit.Framework;
using Reqnroll;

namespace AutomationTests.UI.StepDefinitions;

[Binding]
public class DynamicLoadingSteps(ScenarioSession session)
{
    private DynamicLoadingPage Page => new(session.Driver);

    [Given("I open the dynamic loading example")]
    public void Open() => Page.Open();

    [When("I start loading the message")]
    public void Start() => Page.Start();

    [Then("I see the loaded message {string}")]
    public void CheckMessage(string expected) => Assert.That(Page.Message, Is.EqualTo(expected));
}
