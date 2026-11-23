using AutomationTests.Hooks;
using AutomationTests.Pages;
using NUnit.Framework;
using Reqnroll;

namespace AutomationTests.StepDefinitions;

[Binding]
public sealed class AdvancedControlsSteps(ScenarioSession session)
{
    private AdvancedControlsPage? page;
    private AdvancedControlsPage Page => page ??= new(session.Driver);

    [Given("I open the radio buttons example")]
    public void OpenRadioButtons() => Page.Open("radio_buttons");
    [When("I choose the red color")]
    public void SelectRed() => Page.SelectRed();
    [Then("only red is selected in the color group")]
    public void VerifyRed()
    {
        Assert.That(Page.RedSelected, Is.True);
        Assert.That(Page.BlueSelected, Is.False);
        Assert.That(Page.GreenSelected, Is.False);
    }

    [Given("I open the native multiple select example")]
    public void OpenMultipleSelect() => Page.Open("multiple_dropdown");
    [When("I select options 1 and 3 and deselect option 1")]
    public void ChangeMultipleSelection() => Page.UpdateMultipleSelection();
    [Then("only Option 3 remains selected")]
    public void VerifyMultipleSelection() => Assert.That(Page.SelectedOptions, Is.EqualTo(new[] { "Option 3" }));

    [Given("I open the custom dropdown example")]
    public void OpenCustomDropdown() => Page.Open("custom_dropdown");
    [When("I choose Banana from the custom dropdown")]
    public void SelectBanana() => Page.ChooseBanana();
    [Then("the dropdown reports that banana was selected")]
    public void VerifyCustomSelection() => Assert.That(Page.CustomSelection, Is.EqualTo("Selected: banana"));

    [Given("I open the autocomplete example")]
    public void OpenAutocomplete() => Page.Open("autocomplete");
    [When("I type {string} and choose {string}")]
    public void ChooseSuggestion(string query, string suggestion) => Page.ChooseSuggestion(query, suggestion);
    [Then("the autocomplete field contains {string}")]
    public void VerifyAutocomplete(string expected) => Assert.That(Page.AutocompleteValue, Is.EqualTo(expected));

    [Given("I open the calendar for April 2026")]
    public void OpenCalendar() => Page.Open("date_pickers?month=2026-04");
    [When("I pick {string}")]
    public void PickDate(string value) => Page.PickDate(value);
    [Then("the appointment field contains {string}")]
    public void VerifyAppointment(string expected) => Assert.That(Page.AppointmentDate, Is.EqualTo(expected));

    [Given("I open the dynamic controls example")]
    public void OpenDynamicControls() => Page.Open("dynamic_controls");
    [Then("the dynamic input is disabled")]
    public void VerifyDisabled() => Assert.That(Page.DynamicInputEnabled, Is.False);
    [When("I enable the dynamic input and type {string}")]
    public void EnableAndType(string value) => Page.EnterDynamicInput(value);
    [Then("the dynamic input contains {string}")]
    public void VerifyDynamicInput(string expected) => Assert.That(Page.DynamicInputValue, Is.EqualTo(expected));

    [Given("I open the stale element example")]
    public void OpenStaleElement() => Page.Open("stale_element");
    [When("I rebuild the list")]
    public void RebuildList() => Page.RebuildList();
    [Then("the replacement list starts with {string}")]
    public void VerifyReplacement(string expected) => Assert.That(Page.FirstItemText, Is.EqualTo(expected));

    [Given("I open the pointer events example")]
    public void OpenPointerEvents() => Page.Open("pointer_events");
    [When("I swipe the area to the right")]
    public void SwipeRight() => Page.SwipeRight();
    [Then("the swipe result is {string}")]
    public void VerifySwipe(string expected) => Assert.That(Page.SwipeResult, Is.EqualTo(expected));
    [When("I press and hold the long press button")]
    public void LongPress() => Page.LongPress();
    [Then("the long press result is {string}")]
    public void VerifyLongPress(string expected) => Assert.That(Page.LongPressResult, Is.EqualTo(expected));
}
