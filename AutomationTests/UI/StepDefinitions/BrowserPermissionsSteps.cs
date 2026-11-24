using AutomationFramework.UI;
using AutomationFramework.Utilities;
using AutomationTests.Configuration;
using AutomationTests.Hooks;
using NUnit.Framework;
using OpenQA.Selenium;
using Reqnroll;

namespace AutomationTests.UI.StepDefinitions;

/// <summary>Verifies a denied permission in an isolated browser profile.</summary>
[Binding]
public sealed class BrowserPermissionsSteps(ScenarioSession session)
{
    private static readonly By GeolocationState = By.Id("state-geolocation");

    [Given("I open the browser permissions example")]
    public void OpenPermissions() =>
        new BrowserActions(session.Driver).Navigate(new Uri(TestSettings.BaseUri, "permissions"));

    [Then("the browser and the page report geolocation as denied")]
    public void VerifyGeolocationDenied()
    {
        var driver = session.Driver;
        Assert.That(new AdvancedControl(driver).ReadPermissionState("geolocation"), Is.EqualTo("denied"));

        var wait = WaitHelper.Create(driver);
        wait.Message = "The permissions page did not display a geolocation state.";
        var displayedState = wait.Until(currentDriver =>
        {
            var state = currentDriver.FindElement(GeolocationState).Text;
            return state == "checking" ? null : state;
        });
        Assert.That(displayedState, Is.EqualTo("denied"));
    }
}
