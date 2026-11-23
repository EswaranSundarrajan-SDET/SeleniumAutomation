using AutomationFramework.UI;
using AutomationTests.Hooks;
using AutomationTests.UI.Pages;
using NUnit.Framework;
using Reqnroll;

namespace AutomationTests.UI.StepDefinitions;

/// <summary>Binds the browser controls feature to its page actions and assertions.</summary>
[Binding]
public sealed class BrowserControlSteps(ScenarioSession session)
{
    private BrowserControlsPage? page;
    /// <summary>Creates the controls page only when a step first uses the scenario browser.</summary>
    private BrowserControlsPage Page => page ??= new(session.Driver);

    /// <summary>Opens the native dropdown example.</summary>
    [Given("I open the dropdown example")]
    public void OpenDropdown() => Page.Open("dropdown");
    /// <summary>Selects the demo option with the label Option 2.</summary>
    [When("I select Option 2")]
    public void SelectOption() => Page.SelectOption("Option 2");
    /// <summary>Verifies Option 2 is the only selected label.</summary>
    [Then("the dropdown shows Option 2 as selected")]
    public void VerifyDropdown() => Assert.That(Page.SelectedOptions, Is.EqualTo(new[] { "Option 2" }));

    /// <summary>Opens the checkbox example.</summary>
    [Given("I open the checkboxes example")]
    public void OpenCheckboxes() => Page.Open("checkboxes");
    /// <summary>Sets the first checkbox to checked.</summary>
    [When("I select the first checkbox")]
    public void SetCheckbox() => Page.SetFirstCheckbox(true);
    /// <summary>Sets the first checkbox to unchecked.</summary>
    [When("I clear the first checkbox")]
    public void ClearCheckbox() => Page.SetFirstCheckbox(false);
    /// <summary>Asserts the first checkbox is selected.</summary>
    [Then("the first checkbox is checked")]
    public void VerifyChecked() => Assert.That(Page.IsFirstCheckboxSelected, Is.True);
    /// <summary>Asserts the first checkbox is cleared.</summary>
    [Then("the first checkbox is unchecked")]
    public void VerifyUnchecked() => Assert.That(Page.IsFirstCheckboxSelected, Is.False);

    /// <summary>Opens the example that adds and removes elements.</summary>
    [Given("I open the add and remove elements example")]
    public void OpenDynamicElements() => Page.Open("add_remove_elements/");
    /// <summary>Triggers insertion of the demo Delete button.</summary>
    [When("I add an element")]
    public void AddElement() => Page.AddElement();
    /// <summary>Verifies the new Delete button is visible.</summary>
    [Then("its Delete button is visible")]
    public void VerifyElementAdded() => Assert.That(Page.DeleteButtonVisible, Is.True);
    /// <summary>Uses the Delete button to remove the dynamic element.</summary>
    [When("I remove the element")]
    public void RemoveElement() => Page.RemoveElement();
    /// <summary>Waits until the Delete button disappears.</summary>
    [Then("its Delete button is hidden")]
    public void VerifyElementRemoved() => Page.WaitForDeleteButtonToDisappear();

    /// <summary>Opens the JavaScript dialog examples.</summary>
    [Given("I open the JavaScript alerts example")]
    public void OpenAlerts() => Page.Open("javascript_alerts");
    /// <summary>Opens and cancels the confirmation dialog.</summary>
    [When("I dismiss the confirmation")]
    public void DismissConfirmation() => Page.DismissConfirmation();
    /// <summary>Enters the scenario text in the prompt and submits it.</summary>
    [When("I enter {string} in the prompt")]
    public void EnterPrompt(string text) => Page.SubmitPrompt(text);
    /// <summary>Checks the result text displayed after handling the dialog.</summary>
    [Then("the dialog result is {string}")]
    public void VerifyDialogResult(string expected) => Assert.That(Page.DialogResult, Is.EqualTo(expected));

    /// <summary>Opens the example with a right-click target.</summary>
    [Given("I open the context menu example")]
    public void OpenContextMenu() => Page.Open("context_menu");
    /// <summary>Opens the context menu on the demo target.</summary>
    [When("I right click the context menu area")]
    public void RightClick() => Page.RightClickContextMenu();
    /// <summary>Reads and accepts the resulting alert, then checks its text.</summary>
    [Then("the alert says {string}")]
    public void VerifyAlert(string expected) => Assert.That(Page.ReadAndAcceptAlert(), Is.EqualTo(expected));

    /// <summary>Opens the profile hover example.</summary>
    [Given("I open the hovers example")]
    public void OpenHovers() => Page.Open("hovers");
    /// <summary>Moves the pointer over the first profile.</summary>
    [When("I hover over the first profile")]
    public void HoverOverProfile() => Page.HoverOverFirstProfile();
    /// <summary>Verifies the hover reveals the expected profile name.</summary>
    [Then("the first profile displays {string}")]
    public void VerifyProfile(string expected) => Assert.That(Page.FirstProfileName, Is.EqualTo(expected));

    /// <summary>Opens the horizontal range slider example.</summary>
    [Given("I open the horizontal slider example")]
    public void OpenSlider() => Page.Open("horizontal_slider");
    /// <summary>Moves the slider right by one keyboard step.</summary>
    [When("I move the slider one step right")]
    public void MoveSlider() => Page.MoveSliderRight();
    /// <summary>Verifies the example displays the expected slider value.</summary>
    [Then("the slider shows {string}")]
    public void VerifySlider(string expected) => Assert.That(Page.SliderValue, Is.EqualTo(expected));

    /// <summary>Opens the HTML5 draggable columns example.</summary>
    [Given("I open the drag and drop example")]
    public void OpenDragAndDrop() => Page.Open("drag_and_drop");
    /// <summary>Drags the first demo column into the second column.</summary>
    [When("I drag column A onto column B")]
    public void DragColumn() => Page.DragFirstColumn();
    /// <summary>Verifies the first column displays the expected heading after the drop.</summary>
    [Then("the first column label is {string}")]
    public void VerifyFirstColumn(string expected) => Assert.That(Page.FirstColumn, Is.EqualTo(expected));

    /// <summary>Opens the nested frames example.</summary>
    [Given("I open the nested frames example")]
    public void OpenFrames() => Page.Open("nested_frames");
    /// <summary>Verifies the text inside the left nested frame.</summary>
    [Then("the left frame contains {string}")]
    public void VerifyLeftFrame(string expected) => Assert.That(Page.ReadLeftFrame(), Is.EqualTo(expected));
    /// <summary>Verifies the text inside the bottom frame.</summary>
    [Then("the bottom frame contains {string}")]
    public void VerifyBottomFrame(string expected) => Assert.That(Page.ReadBottomFrame(), Is.EqualTo(expected));

    /// <summary>Opens the multiple windows example.</summary>
    [Given("I open the multiple windows example")]
    public void OpenWindows() => Page.Open("windows");
    /// <summary>Follows the link and switches to the newly opened window.</summary>
    [When("I follow the link to a new window")]
    public void FollowNewWindowLink() => Page.FollowNewWindowLink();
    /// <summary>Verifies the heading in the currently selected window.</summary>
    [Then("the new window displays {string}")]
    [Then("the original window displays {string}")]
    public void VerifyWindowHeading(string expected) => Assert.That(Page.WindowHeading, Is.EqualTo(expected));
    /// <summary>Closes the new window and switches back to the original.</summary>
    [When("I close the new window")]
    public void CloseNewWindow() => Page.CloseNewWindow();

    /// <summary>Opens the shadow DOM example.</summary>
    [Given("I open the shadow DOM example")]
    public void OpenShadowDom() => Page.Open("shadowdom");
    /// <summary>Verifies the named slot is found within the first shadow component.</summary>
    [Then("the first shadow component contains a slot named {string}")]
    public void VerifyShadowSlot(string expected) => Assert.That(Page.FirstShadowSlotName, Is.EqualTo(expected));

    /// <summary>Opens the sortable tables example.</summary>
    [Given("I open the sortable tables example")]
    public void OpenTables() => Page.Open("tables");
    /// <summary>Verifies the expected column appears in the first table's header row.</summary>
    [Then("the table has a column named {string}")]
    public void VerifyTable(string expected) => Assert.That(Page.TableHeaders, Does.Contain(expected));

    /// <summary>Opens the file upload example.</summary>
    [Given("I open the file upload example")]
    public void OpenUpload() => Page.Open("upload");
    /// <summary>Uploads the bundled text fixture from the test output directory.</summary>
    [When("I upload the sample text file")]
    public void UploadFile() =>
        Page.UploadFile(Path.Combine(AppContext.BaseDirectory, "UI", "TestData", "upload-example.txt"));
    /// <summary>Verifies the uploaded fixture's name appears on the confirmation page.</summary>
    [Then("the upload confirmation shows {string}")]
    public void VerifyUpload(string expected) => Assert.That(Page.UploadedFile, Is.EqualTo(expected));

    /// <summary>Opens the modern file download example.</summary>
    [Given("I open the modern file download example")]
    public void OpenDownload() => Page.Open("download_modern");
    /// <summary>Starts the sample text file download.</summary>
    [When("I download the example text file")]
    public void DownloadFile() => Page.DownloadFile();
    /// <summary>Waits for the file in the scenario's temporary directory and checks its contents.</summary>
    [Then("the downloaded file contains {string}")]
    public void VerifyDownload(string expected)
    {
        var path = DownloadActions.WaitForFile(session.DownloadDirectory, "hello.txt");
        Assert.That(File.ReadAllText(path).Trim(), Is.EqualTo(expected));
    }
}
