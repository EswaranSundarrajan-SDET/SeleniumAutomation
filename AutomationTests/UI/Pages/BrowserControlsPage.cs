using AutomationFramework.UI;
using AutomationTests.Configuration;
using OpenQA.Selenium;

namespace AutomationTests.UI.Pages;

/// <summary>Models the browser and HTML control examples exercised by the UI suite.</summary>
public sealed class BrowserControlsPage(IWebDriver driver) : PageObject(driver, TestSettings.BaseUri)
{
    private static readonly By Dropdown = By.Id("dropdown");
    private static readonly By FirstCheckbox = By.CssSelector("#checkboxes input:first-of-type");
    private static readonly By DeleteButton = By.CssSelector("#elements button");
    private static readonly By FirstProfile = By.CssSelector(".figure");
    private readonly InputActions input = new(driver);
    private string? originalWindow;

    /// <summary>Opens the browser controls example at the supplied path.</summary>
    public void Open(string path) => NavigateTo(path);

    /// <summary>Selects the named option in the native dropdown.</summary>
    public void SelectOption(string label) => Elements.SelectByText(Dropdown, label);
    /// <summary>Returns the dropdown's selected labels.</summary>
    public IReadOnlyList<string> SelectedOptions => Elements.SelectedOptions(Dropdown);

    /// <summary>Sets the first checkbox to the requested state.</summary>
    public void SetFirstCheckbox(bool selected) => Elements.SetChecked(FirstCheckbox, selected);
    /// <summary>Gets the first checkbox's selection state.</summary>
    public bool IsFirstCheckboxSelected => Elements.IsSelected(FirstCheckbox);

    /// <summary>Clicks the demo control that inserts a Delete button.</summary>
    public void AddElement() => Elements.Click(By.CssSelector(".example > button"));
    /// <summary>Clicks the Delete button to remove the dynamic element.</summary>
    public void RemoveElement() => Elements.Click(DeleteButton);
    /// <summary>Waits for the dynamically added Delete button.</summary>
    public bool DeleteButtonVisible => Elements.FindVisible(DeleteButton).Displayed;
    /// <summary>Waits until no Delete button remains visible.</summary>
    public void WaitForDeleteButtonToDisappear() => Elements.WaitUntilHidden(DeleteButton);

    /// <summary>Opens the confirmation dialog and dismisses it.</summary>
    public void DismissConfirmation()
    {
        Elements.Click(By.CssSelector("button[onclick='jsConfirm()']"));
        Browser.DismissAlert();
    }

    /// <summary>Opens the demo prompt and accepts the entered text.</summary>
    public void SubmitPrompt(string text)
    {
        Elements.Click(By.CssSelector("button[onclick='jsPrompt()']"));
        Browser.SubmitPrompt(text);
    }

    /// <summary>Reads the text the demo page displays after the dialog closes.</summary>
    public string DialogResult => Elements.Text(By.Id("result"));

    /// <summary>Performs a native context click on the menu target.</summary>
    public void RightClickContextMenu() => input.RightClick(By.Id("hot-spot"));
    /// <summary>Reads the alert text before accepting the alert.</summary>
    public string ReadAndAcceptAlert()
    {
        var text = Browser.AlertText();
        Browser.AcceptAlert();
        return text;
    }

    /// <summary>Moves the pointer over the first profile to reveal its caption.</summary>
    public void HoverOverFirstProfile() => input.Hover(FirstProfile);
    /// <summary>Reads the first profile's revealed name.</summary>
    public string FirstProfileName => Elements.Text(By.CssSelector(".figure:first-of-type .figcaption h5"));

    /// <summary>Resets the range input with Home and increments it with Right Arrow.</summary>
    public void MoveSliderRight()
    {
        Elements.SendKeys(By.CssSelector("input[type='range']"), Keys.Home);
        input.SendKeys(Keys.ArrowRight);
    }
    /// <summary>Reads the value displayed by the range slider example.</summary>
    public string SliderValue => Elements.Text(By.Id("range"));

    /// <summary>Dispatches HTML5 drag events from column A to column B.</summary>
    public void DragFirstColumn() => Browser.DragHtml5(By.Id("column-a"), By.Id("column-b"));
    /// <summary>Reads the heading currently shown in the first draggable column.</summary>
    public string FirstColumn => Elements.Text(By.CssSelector("#column-a header"));

    /// <summary>Reads the left nested frame and returns to the top-level document.</summary>
    public string ReadLeftFrame()
    {
        Browser.SwitchToFrame(By.Name("frame-top"));
        Browser.SwitchToFrame(By.Name("frame-left"));
        var text = Elements.Text(By.TagName("body"));
        Browser.SwitchToMainDocument();
        return text;
    }

    /// <summary>Reads the bottom frame and returns to the top-level document.</summary>
    public string ReadBottomFrame()
    {
        Browser.SwitchToFrame(By.Name("frame-bottom"));
        var text = Elements.Text(By.TagName("body"));
        Browser.SwitchToMainDocument();
        return text;
    }

    /// <summary>Remembers the current tab, follows the link, and waits for the new tab.</summary>
    public void FollowNewWindowLink()
    {
        originalWindow = Browser.CurrentWindow;
        var before = Browser.WindowHandles;
        Elements.Click(By.CssSelector("a[href='/windows/new']"));
        Browser.SwitchToNewWindow(before);
    }

    /// <summary>Closes the new tab and restores focus to the original tab.</summary>
    public void CloseNewWindow()
    {
        Browser.CloseWindow();
        Browser.SwitchToWindow(originalWindow!);
    }

    /// <summary>Reads the heading in the currently active tab.</summary>
    public string WindowHeading => Elements.Text(By.CssSelector(".example h3"));

    /// <summary>Reads the name attribute of the slot within the first shadow host.</summary>
    public string? FirstShadowSlotName =>
        Elements.FindInShadowRoot(By.CssSelector("my-paragraph"), By.CssSelector("slot")).GetDomAttribute("name");

    /// <summary>Returns the header row of the first sortable table.</summary>
    public IReadOnlyList<string> TableHeaders => Elements.ReadTable(By.Id("table1"))[0];

    /// <summary>Sends the local file path to the upload input and submits the form.</summary>
    public void UploadFile(string path)
    {
        Elements.UploadFile(By.Id("file-upload"), path);
        Elements.Click(By.Id("file-submit"));
    }

    /// <summary>Reads the file name displayed by the upload confirmation.</summary>
    public string UploadedFile => Elements.Text(By.Id("uploaded-files"));

    /// <summary>Clicks the demo link that serves a text file via a data URL.</summary>
    public void DownloadFile() => Elements.Click(By.Id("data-url-download"));
}
