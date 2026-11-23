using AutomationFramework.UI;
using AutomationTests.Hooks;
using AutomationTests.UI.Pages;
using NUnit.Framework;
using Reqnroll;

namespace AutomationTests.UI.StepDefinitions;

[Binding]
public sealed class BrowserControlSteps(ScenarioSession session)
{
    private BrowserControlsPage? page;
    private BrowserControlsPage Page => page ??= new(session.Driver);

    [Given("I open the dropdown example")]
    public void OpenDropdown() => Page.Open("dropdown");
    [When("I select Option 2")]
    public void SelectOption() => Page.SelectOption("Option 2");
    [Then("the dropdown shows Option 2 as selected")]
    public void VerifyDropdown() => Assert.That(Page.SelectedOptions, Is.EqualTo(new[] { "Option 2" }));

    [Given("I open the checkboxes example")]
    public void OpenCheckboxes() => Page.Open("checkboxes");
    [When("I select the first checkbox")]
    public void SetCheckbox() => Page.SetFirstCheckbox(true);
    [When("I clear the first checkbox")]
    public void ClearCheckbox() => Page.SetFirstCheckbox(false);
    [Then("the first checkbox is checked")]
    public void VerifyChecked() => Assert.That(Page.IsFirstCheckboxSelected, Is.True);
    [Then("the first checkbox is unchecked")]
    public void VerifyUnchecked() => Assert.That(Page.IsFirstCheckboxSelected, Is.False);

    [Given("I open the add and remove elements example")]
    public void OpenDynamicElements() => Page.Open("add_remove_elements/");
    [When("I add an element")]
    public void AddElement() => Page.AddElement();
    [Then("its Delete button is visible")]
    public void VerifyElementAdded() => Assert.That(Page.DeleteButtonVisible, Is.True);
    [When("I remove the element")]
    public void RemoveElement() => Page.RemoveElement();
    [Then("its Delete button is hidden")]
    public void VerifyElementRemoved() => Page.WaitForDeleteButtonToDisappear();

    [Given("I open the JavaScript alerts example")]
    public void OpenAlerts() => Page.Open("javascript_alerts");
    [When("I dismiss the confirmation")]
    public void DismissConfirmation() => Page.DismissConfirmation();
    [When("I enter {string} in the prompt")]
    public void EnterPrompt(string text) => Page.SubmitPrompt(text);
    [Then("the dialog result is {string}")]
    public void VerifyDialogResult(string expected) => Assert.That(Page.DialogResult, Is.EqualTo(expected));

    [Given("I open the context menu example")]
    public void OpenContextMenu() => Page.Open("context_menu");
    [When("I right click the context menu area")]
    public void RightClick() => Page.RightClickContextMenu();
    [Then("the alert says {string}")]
    public void VerifyAlert(string expected) => Assert.That(Page.ReadAndAcceptAlert(), Is.EqualTo(expected));

    [Given("I open the hovers example")]
    public void OpenHovers() => Page.Open("hovers");
    [When("I hover over the first profile")]
    public void HoverOverProfile() => Page.HoverOverFirstProfile();
    [Then("the first profile displays {string}")]
    public void VerifyProfile(string expected) => Assert.That(Page.FirstProfileName, Is.EqualTo(expected));

    [Given("I open the horizontal slider example")]
    public void OpenSlider() => Page.Open("horizontal_slider");
    [When("I move the slider one step right")]
    public void MoveSlider() => Page.MoveSliderRight();
    [Then("the slider shows {string}")]
    public void VerifySlider(string expected) => Assert.That(Page.SliderValue, Is.EqualTo(expected));

    [Given("I open the drag and drop example")]
    public void OpenDragAndDrop() => Page.Open("drag_and_drop");
    [When("I drag column A onto column B")]
    public void DragColumn() => Page.DragFirstColumn();
    [Then("the first column label is {string}")]
    public void VerifyFirstColumn(string expected) => Assert.That(Page.FirstColumn, Is.EqualTo(expected));

    [Given("I open the nested frames example")]
    public void OpenFrames() => Page.Open("nested_frames");
    [Then("the left frame contains {string}")]
    public void VerifyLeftFrame(string expected) => Assert.That(Page.ReadLeftFrame(), Is.EqualTo(expected));
    [Then("the bottom frame contains {string}")]
    public void VerifyBottomFrame(string expected) => Assert.That(Page.ReadBottomFrame(), Is.EqualTo(expected));

    [Given("I open the multiple windows example")]
    public void OpenWindows() => Page.Open("windows");
    [When("I follow the link to a new window")]
    public void FollowNewWindowLink() => Page.FollowNewWindowLink();
    [Then("the new window displays {string}")]
    [Then("the original window displays {string}")]
    public void VerifyWindowHeading(string expected) => Assert.That(Page.WindowHeading, Is.EqualTo(expected));
    [When("I close the new window")]
    public void CloseNewWindow() => Page.CloseNewWindow();

    [Given("I open the shadow DOM example")]
    public void OpenShadowDom() => Page.Open("shadowdom");
    [Then("the first shadow component contains a slot named {string}")]
    public void VerifyShadowSlot(string expected) => Assert.That(Page.FirstShadowSlotName, Is.EqualTo(expected));

    [Given("I open the sortable tables example")]
    public void OpenTables() => Page.Open("tables");
    [Then("the table has a column named {string}")]
    public void VerifyTable(string expected) => Assert.That(Page.TableHeaders, Does.Contain(expected));

    [Given("I open the file upload example")]
    public void OpenUpload() => Page.Open("upload");
    [When("I upload the sample text file")]
    public void UploadFile() =>
        Page.UploadFile(Path.Combine(AppContext.BaseDirectory, "UI", "TestData", "upload-example.txt"));
    [Then("the upload confirmation shows {string}")]
    public void VerifyUpload(string expected) => Assert.That(Page.UploadedFile, Is.EqualTo(expected));

    [Given("I open the modern file download example")]
    public void OpenDownload() => Page.Open("download_modern");
    [When("I download the example text file")]
    public void DownloadFile() => Page.DownloadFile();
    [Then("the downloaded file contains {string}")]
    public void VerifyDownload(string expected)
    {
        var path = DownloadActions.WaitForFile(session.Downloads!.FullName, "hello.txt");
        Assert.That(File.ReadAllText(path).Trim(), Is.EqualTo(expected));
    }
}
