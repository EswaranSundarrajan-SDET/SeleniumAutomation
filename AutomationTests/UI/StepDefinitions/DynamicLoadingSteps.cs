using AutomationTests.Hooks;
using AutomationTests.UI.Pages;
using NUnit.Framework;
using Reqnroll;

namespace AutomationTests.UI.StepDefinitions;

/// <summary>Connects the dynamic loading scenario to its page and assertions.</summary>
[Binding]
public class DynamicLoadingSteps(ScenarioSession session)
{
    private DynamicLoadingPage? page;
    /// <summary>Creates the page object with the browser owned by the current scenario.</summary>
    private DynamicLoadingPage Page => page ??= new(session.Driver);

    /// <summary>Opens the dynamic loading example.</summary>
    [Given("I open the dynamic loading example")]
    public void Open() => Page.Open();

    /// <summary>Starts loading the delayed content.</summary>
    [When("I start loading the message")]
    public void Start() => Page.Start();

    /// <summary>Verifies the loaded message matches the scenario's expected text.</summary>
    [Then("I see the loaded message {string}")]
    public void CheckMessage(string expected) => Assert.That(Page.Message, Is.EqualTo(expected));
}
