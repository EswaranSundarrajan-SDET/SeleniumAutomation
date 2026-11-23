using AutomationFramework.UI;
using AutomationTests.Configuration;
using OpenQA.Selenium;

namespace AutomationTests.Pages;

public sealed class BrowserControlsPage(IWebDriver driver)
{
    private static readonly By Dropdown = By.Id("dropdown");
    private static readonly By FirstCheckbox = By.CssSelector("#checkboxes input:first-of-type");
    private static readonly By DeleteButton = By.CssSelector("#elements button");
    private static readonly By FirstProfile = By.CssSelector(".figure");
    private readonly BrowserActions browser = new(driver);
    private readonly ElementActions elements = new(driver);
    private readonly InputActions input = new(driver);
    private string? originalWindow;

    public void Open(string path) => browser.Navigate(new Uri(TestSettings.BaseUri, path));

    public void SelectOption(string label) => elements.SelectByText(Dropdown, label);
    public IReadOnlyList<string> SelectedOptions => elements.SelectedOptions(Dropdown);

    public void SetFirstCheckbox(bool selected) => elements.SetChecked(FirstCheckbox, selected);
    public bool IsFirstCheckboxSelected => elements.IsSelected(FirstCheckbox);

    public void AddElement() => elements.Click(By.CssSelector(".example > button"));
    public void RemoveElement() => elements.Click(DeleteButton);
    public bool DeleteButtonVisible => elements.FindVisible(DeleteButton).Displayed;
    public void WaitForDeleteButtonToDisappear() => elements.WaitUntilHidden(DeleteButton);

    public void DismissConfirmation()
    {
        elements.Click(By.CssSelector("button[onclick='jsConfirm()']"));
        browser.DismissAlert();
    }

    public void SubmitPrompt(string text)
    {
        elements.Click(By.CssSelector("button[onclick='jsPrompt()']"));
        browser.SubmitPrompt(text);
    }

    public string DialogResult => elements.Text(By.Id("result"));

    public void RightClickContextMenu() => input.RightClick(By.Id("hot-spot"));
    public string ReadAndAcceptAlert()
    {
        var text = browser.AlertText();
        browser.AcceptAlert();
        return text;
    }

    public void HoverOverFirstProfile() => input.Hover(FirstProfile);
    public string FirstProfileName => elements.Text(By.CssSelector(".figure:first-of-type .figcaption h5"));

    public void MoveSliderRight()
    {
        elements.SendKeys(By.CssSelector("input[type='range']"), Keys.Home);
        input.SendKeys(Keys.ArrowRight);
    }
    public string SliderValue => elements.Text(By.Id("range"));

    public void DragFirstColumn()
    {
        var source = elements.FindVisible(By.Id("column-a"));
        var target = elements.FindVisible(By.Id("column-b"));
        var eventDelay = TimeSpan.FromMilliseconds(150);
        // HTML5 dragging needs pointer movement after crossing the drag threshold.
        input.Perform(actions => actions.MoveToElement(source)
            .ClickAndHold()
            .MoveByOffset(8, 0)
            .Pause(eventDelay)
            .MoveToElement(target)
            .MoveByOffset(4, 0)
            .Pause(eventDelay)
            .Release());
    }
    public string FirstColumn => elements.Text(By.CssSelector("#column-a header"));

    public string ReadLeftFrame()
    {
        browser.SwitchToFrame(By.Name("frame-top"));
        browser.SwitchToFrame(By.Name("frame-left"));
        var text = elements.Text(By.TagName("body"));
        browser.SwitchToMainDocument();
        return text;
    }

    public string ReadBottomFrame()
    {
        browser.SwitchToFrame(By.Name("frame-bottom"));
        var text = elements.Text(By.TagName("body"));
        browser.SwitchToMainDocument();
        return text;
    }

    public void FollowNewWindowLink()
    {
        originalWindow = browser.CurrentWindow;
        var before = browser.WindowHandles;
        elements.Click(By.CssSelector("a[href='/windows/new']"));
        browser.SwitchToNewWindow(before);
    }

    public void CloseNewWindow()
    {
        browser.CloseWindow();
        browser.SwitchToWindow(originalWindow!);
    }

    public string WindowHeading => elements.Text(By.CssSelector(".example h3"));

    public string? FirstShadowSlotName =>
        elements.FindInShadowRoot(By.CssSelector("my-paragraph"), By.CssSelector("slot")).GetDomAttribute("name");

    public IReadOnlyList<string> TableHeaders => elements.ReadTable(By.Id("table1"))[0];

    public void UploadFile(string path)
    {
        elements.UploadFile(By.Id("file-upload"), path);
        elements.Click(By.Id("file-submit"));
    }

    public string UploadedFile => elements.Text(By.Id("uploaded-files"));

    public void DownloadFile() => elements.Click(By.Id("data-url-download"));
}
