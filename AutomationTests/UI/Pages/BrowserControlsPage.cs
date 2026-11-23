using AutomationFramework.UI;
using AutomationTests.Configuration;
using OpenQA.Selenium;

namespace AutomationTests.UI.Pages;

public sealed class BrowserControlsPage(IWebDriver driver) : PageObject(driver, TestSettings.BaseUri)
{
    private static readonly By Dropdown = By.Id("dropdown");
    private static readonly By FirstCheckbox = By.CssSelector("#checkboxes input:first-of-type");
    private static readonly By DeleteButton = By.CssSelector("#elements button");
    private static readonly By FirstProfile = By.CssSelector(".figure");
    private readonly InputActions input = new(driver);
    private string? originalWindow;

    public void Open(string path) => NavigateTo(path);

    public void SelectOption(string label) => Elements.SelectByText(Dropdown, label);
    public IReadOnlyList<string> SelectedOptions => Elements.SelectedOptions(Dropdown);

    public void SetFirstCheckbox(bool selected) => Elements.SetChecked(FirstCheckbox, selected);
    public bool IsFirstCheckboxSelected => Elements.IsSelected(FirstCheckbox);

    public void AddElement() => Elements.Click(By.CssSelector(".example > button"));
    public void RemoveElement() => Elements.Click(DeleteButton);
    public bool DeleteButtonVisible => Elements.FindVisible(DeleteButton).Displayed;
    public void WaitForDeleteButtonToDisappear() => Elements.WaitUntilHidden(DeleteButton);

    public void DismissConfirmation()
    {
        Elements.Click(By.CssSelector("button[onclick='jsConfirm()']"));
        Browser.DismissAlert();
    }

    public void SubmitPrompt(string text)
    {
        Elements.Click(By.CssSelector("button[onclick='jsPrompt()']"));
        Browser.SubmitPrompt(text);
    }

    public string DialogResult => Elements.Text(By.Id("result"));

    public void RightClickContextMenu() => input.RightClick(By.Id("hot-spot"));
    public string ReadAndAcceptAlert()
    {
        var text = Browser.AlertText();
        Browser.AcceptAlert();
        return text;
    }

    public void HoverOverFirstProfile() => input.Hover(FirstProfile);
    public string FirstProfileName => Elements.Text(By.CssSelector(".figure:first-of-type .figcaption h5"));

    public void MoveSliderRight()
    {
        Elements.SendKeys(By.CssSelector("input[type='range']"), Keys.Home);
        input.SendKeys(Keys.ArrowRight);
    }
    public string SliderValue => Elements.Text(By.Id("range"));

    public void DragFirstColumn() => Browser.DragHtml5(By.Id("column-a"), By.Id("column-b"));
    public string FirstColumn => Elements.Text(By.CssSelector("#column-a header"));

    public string ReadLeftFrame()
    {
        Browser.SwitchToFrame(By.Name("frame-top"));
        Browser.SwitchToFrame(By.Name("frame-left"));
        var text = Elements.Text(By.TagName("body"));
        Browser.SwitchToMainDocument();
        return text;
    }

    public string ReadBottomFrame()
    {
        Browser.SwitchToFrame(By.Name("frame-bottom"));
        var text = Elements.Text(By.TagName("body"));
        Browser.SwitchToMainDocument();
        return text;
    }

    public void FollowNewWindowLink()
    {
        originalWindow = Browser.CurrentWindow;
        var before = Browser.WindowHandles;
        Elements.Click(By.CssSelector("a[href='/windows/new']"));
        Browser.SwitchToNewWindow(before);
    }

    public void CloseNewWindow()
    {
        Browser.CloseWindow();
        Browser.SwitchToWindow(originalWindow!);
    }

    public string WindowHeading => Elements.Text(By.CssSelector(".example h3"));

    public string? FirstShadowSlotName =>
        Elements.FindInShadowRoot(By.CssSelector("my-paragraph"), By.CssSelector("slot")).GetDomAttribute("name");

    public IReadOnlyList<string> TableHeaders => Elements.ReadTable(By.Id("table1"))[0];

    public void UploadFile(string path)
    {
        Elements.UploadFile(By.Id("file-upload"), path);
        Elements.Click(By.Id("file-submit"));
    }

    public string UploadedFile => Elements.Text(By.Id("uploaded-files"));

    public void DownloadFile() => Elements.Click(By.Id("data-url-download"));
}
