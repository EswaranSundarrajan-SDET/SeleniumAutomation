using AutomationTests.Hooks;
using AutomationTests.UI.Pages;
using NUnit.Framework;
using Reqnroll;

namespace AutomationTests.UI.StepDefinitions;

/// <summary>Binds advanced widget scenarios to their shared page object and assertions.</summary>
[Binding]
public sealed class AdvancedControlsSteps(ScenarioSession session)
{
    private AdvancedControlsPage? page;
    /// <summary>Creates the page object lazily using the current scenario browser.</summary>
    private AdvancedControlsPage Page => page ??= new(session.Driver);

    /// <summary>Opens the radio-button example.</summary>
    [Given("I open the radio buttons example")]
    public void OpenRadioButtons() => Page.Open("radio_buttons");
    /// <summary>Selects red in the radio group.</summary>
    [When("I choose the red color")]
    public void SelectRed() => Page.SelectRed();
    /// <summary>Verifies red is selected and the other choices are cleared.</summary>
    [Then("only red is selected in the color group")]
    public void VerifyRed()
    {
        Assert.That(Page.RedSelected, Is.True);
        Assert.That(Page.BlueSelected, Is.False);
        Assert.That(Page.GreenSelected, Is.False);
    }

    /// <summary>Opens the native multiple-select example.</summary>
    [Given("I open the native multiple select example")]
    public void OpenMultipleSelect() => Page.Open("multiple_dropdown");
    /// <summary>Selects two options and removes the first choice.</summary>
    [When("I select options 1 and 3 and deselect option 1")]
    public void ChangeMultipleSelection() => Page.UpdateMultipleSelection();
    /// <summary>Verifies only the third option remains selected.</summary>
    [Then("only Option 3 remains selected")]
    public void VerifyMultipleSelection() => Assert.That(Page.SelectedOptions, Is.EqualTo(new[] { "Option 3" }));

    /// <summary>Opens the custom dropdown example.</summary>
    [Given("I open the custom dropdown example")]
    public void OpenCustomDropdown() => Page.Open("custom_dropdown");
    /// <summary>Picks Banana from the custom dropdown.</summary>
    [When("I choose Banana from the custom dropdown")]
    public void SelectBanana() => Page.ChooseBanana();
    /// <summary>Verifies the custom dropdown reports banana as its selection.</summary>
    [Then("the dropdown reports that banana was selected")]
    public void VerifyCustomSelection() => Assert.That(Page.CustomSelection, Is.EqualTo("Selected: banana"));

    /// <summary>Opens the asynchronous autocomplete example.</summary>
    [Given("I open the autocomplete example")]
    public void OpenAutocomplete() => Page.Open("autocomplete");
    /// <summary>Enters the query and picks the requested autocomplete result.</summary>
    [When("I type {string} and choose {string}")]
    public void ChooseSuggestion(string query, string suggestion) => Page.ChooseSuggestion(query, suggestion);
    /// <summary>Compares the final field value to the expected suggestion.</summary>
    [Then("the autocomplete field contains {string}")]
    public void VerifyAutocomplete(string expected) => Assert.That(Page.AutocompleteValue, Is.EqualTo(expected));

    /// <summary>Opens the date picker at April 2026.</summary>
    [Given("I open the calendar for April 2026")]
    public void OpenCalendar() => Page.Open("date_pickers?month=2026-04");
    /// <summary>Chooses the scenario's date in the calendar.</summary>
    [When("I pick {string}")]
    public void PickDate(string value) => Page.PickDate(value);
    /// <summary>Verifies the appointment field contains the expected date.</summary>
    [Then("the appointment field contains {string}")]
    public void VerifyAppointment(string expected) => Assert.That(Page.AppointmentDate, Is.EqualTo(expected));

    /// <summary>Opens the example with an initially disabled input.</summary>
    [Given("I open the dynamic controls example")]
    public void OpenDynamicControls() => Page.Open("dynamic_controls");
    /// <summary>Verifies the input is initially disabled.</summary>
    [Then("the dynamic input is disabled")]
    public void VerifyDisabled() => Assert.That(Page.DynamicInputEnabled, Is.False);
    /// <summary>Enables the field and enters the supplied text.</summary>
    [When("I enable the dynamic input and type {string}")]
    public void EnableAndType(string value) => Page.EnterDynamicInput(value);
    /// <summary>Verifies the now-enabled input contains the expected text.</summary>
    [Then("the dynamic input contains {string}")]
    public void VerifyDynamicInput(string expected) => Assert.That(Page.DynamicInputValue, Is.EqualTo(expected));

    /// <summary>Opens the example that can replace elements in place.</summary>
    [Given("I open the stale element example")]
    public void OpenStaleElement() => Page.Open("stale_element");
    /// <summary>Triggers the replacement and waits for the original element to detach.</summary>
    [When("I rebuild the list")]
    public void RebuildList() => Page.RebuildList();
    /// <summary>Verifies the first item can be located in the rebuilt list.</summary>
    [Then("the replacement list starts with {string}")]
    public void VerifyReplacement(string expected) => Assert.That(Page.FirstItemText, Is.EqualTo(expected));

    /// <summary>Opens the pointer gesture example.</summary>
    [Given("I open the pointer events example")]
    public void OpenPointerEvents() => Page.Open("pointer_events");
    /// <summary>Performs a rightward pointer drag in the demo area.</summary>
    [When("I swipe the area to the right")]
    public void SwipeRight() => Page.SwipeRight();
    /// <summary>Checks the pointer example recognized the expected gesture.</summary>
    [Then("the swipe result is {string}")]
    public void VerifySwipe(string expected) => Assert.That(Page.SwipeResult, Is.EqualTo(expected));
    /// <summary>Performs the sustained pointer-button press.</summary>
    [When("I press and hold the long press button")]
    public void LongPress() => Page.LongPress();
    /// <summary>Checks that the demo recognized the sustained press.</summary>
    [Then("the long press result is {string}")]
    public void VerifyLongPress(string expected) => Assert.That(Page.LongPressResult, Is.EqualTo(expected));
}
