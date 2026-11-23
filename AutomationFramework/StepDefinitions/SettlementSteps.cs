using AutomationFramework.Hooks;
using NUnit.Framework;
using Reqnroll;

namespace AutomationFramework.StepDefinitions;

[Binding]
public class SettlementSteps(ScenarioSession session)
{
    [When("I submit the settlement from the test data")]
    public void Submit() => session.Settlement.Submit(session.Reference, session.Data.Amount);

    [Then("the confirmation shows my settlement reference")]
    public void CheckConfirmation() =>
        Assert.That(session.Settlement.Confirmation, Is.EqualTo($"Submitted {session.Reference}"));

    [Then("the API returns the submitted settlement amount")]
    public async Task CheckRecordAsync()
    {
        var record = await session.Api.GetSettlementAsync(session.Reference);
        Assert.That(record.Reference, Is.EqualTo(session.Reference));
        Assert.That(record.Amount, Is.EqualTo(session.Data.Amount));
    }
}
