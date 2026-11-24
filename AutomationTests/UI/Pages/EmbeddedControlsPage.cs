using AutomationFramework.UI;
using AutomationTests.Configuration;
using OpenQA.Selenium;

namespace AutomationTests.UI.Pages;

/// <summary>Models the demos that place controls inside iframes and shadow roots.</summary>
public sealed class EmbeddedControlsPage(IWebDriver driver) : PageObject(driver, TestSettings.BaseUri)
{
    private static readonly By SandboxFrame = By.Id("sandboxed-frame");
    private static readonly By SendFrameMessage = By.Id("send-message");
    private static readonly By FrameMessage = By.XPath("//*[@id='message-log' and starts-with(normalize-space(), 'Message from the ')]");
    private static readonly By InlineFrame = By.Id("srcdoc-frame");
    private static readonly By InlineInput = By.Id("srcdoc-input");
    private static readonly By ShadowFrameHost = By.Id("frame-host");
    private static readonly By ShadowFrame = By.CssSelector("#shadow-frame");
    private static readonly By FrameTitle = By.Id("frame-title");
    private static readonly By OuterCard = By.Id("outer-card");
    private static readonly By InnerCard = By.CssSelector("#inner-card");
    private static readonly By InnerButton = By.CssSelector("#inner-button");
    private static readonly By ClosedCard = By.Id("closed-card");
    private static readonly By ClosedInput = By.CssSelector("#closed-input");
    private static readonly By SlotCard = By.Id("slot-card");
    private static readonly By TitleSlot = By.CssSelector("slot[name='title']");
    private static readonly By Rating = By.Id("rating");
    private static readonly By EditorFrame = By.CssSelector(".tox-edit-area iframe");
    private static readonly By EditorBody = By.CssSelector("body[contenteditable='true']");
    private static readonly By EditorBoldButton = By.CssSelector(".tox-tbtn[aria-label^='Bold']");
    private static readonly By EditorBoldText = By.CssSelector("body[contenteditable='true'] strong");
    private readonly AdvancedControl advanced = new(driver);
    private readonly InputActions inputActions = new(driver);

    public void Open(string path) => NavigateTo(path);

    public string SendMessageFromSandbox() => advanced.InFrame(SandboxFrame, () =>
    {
        Elements.Click(SendFrameMessage);
        return Elements.Text(SendFrameMessage);
    });

    public string SandboxAccessNotice => Elements.Text(By.Id("parent-access"));

    public string SandboxMessage => Elements.Text(FrameMessage);

    public string? EnterInlineText(string text) => advanced.InFrame(InlineFrame, () =>
    {
        Elements.Type(InlineInput, text);
        return Elements.Property(InlineInput, "value");
    });

    public string ReadShadowFrameTitle()
    {
        var frame = advanced.FindInShadowRoots(ShadowFrame, ShadowFrameHost);
        return advanced.InFrame(frame, () => Elements.Text(FrameTitle));
    }

    public void ClickNestedShadowButton() =>
        advanced.FindInShadowRoots(InnerButton, OuterCard, InnerCard).Click();

    public string NestedButtonResult => Elements.Text(By.Id("nested-result"));

    public string? EnterClosedRootText(string text)
    {
        var input = advanced.FindInShadowRoots(ClosedInput, ClosedCard);
        inputActions.Perform(actions => actions.Click(input).SendKeys(text));
        return input.GetDomProperty("value");
    }

    public string? ReadProjectedTitle()
    {
        var slot = advanced.FindInShadowRoots(TitleSlot, SlotCard);
        return Browser.ExecuteScript("return arguments[0].assignedElements()[0]?.textContent;", slot) as string;
    }

    public void SubmitRating(string name, int stars)
    {
        Elements.Type(By.Id("name"), name);
        advanced.FindInShadowRoots(By.CssSelector($"#star-{stars}"), Rating).Click();
        Elements.Click(By.CssSelector("#rating-form button[type='submit']"));
    }

    public string RatingResult => Elements.Text(By.Id("form-result"));

    public string ReplaceEditorTextInBold(string text)
    {
        var editedText = advanced.InFrame(EditorFrame, () =>
        {
            var editor = Elements.FindVisible(EditorBody);
            editor.Click();
            inputActions.Chord(Keys.Control, "a");
            inputActions.SendKeys(text);
            inputActions.Chord(Keys.Control, "a");
            return editor.Text;
        });
        Elements.Click(EditorBoldButton);
        return editedText;
    }

    public string ReadBoldEditorText() => advanced.InFrame(EditorFrame, () => Elements.Text(EditorBoldText));
}
