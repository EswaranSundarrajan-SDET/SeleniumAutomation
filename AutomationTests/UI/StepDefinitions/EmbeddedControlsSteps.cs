using AutomationTests.Hooks;
using AutomationTests.UI.Pages;
using NUnit.Framework;
using Reqnroll;

namespace AutomationTests.UI.StepDefinitions;

/// <summary>Binds frame and shadow root scenarios to browser interactions.</summary>
[Binding]
public sealed class EmbeddedControlsSteps(ScenarioSession session)
{
    private EmbeddedControlsPage? page;
    private string? sandboxButtonText;
    private string? editedText;
    private EmbeddedControlsPage Page => page ??= new(session.Driver);

    [Given("I open the advanced iframes example")]
    public void OpenAdvancedFrames() => Page.Open("iframes_advanced");

    [When("I send a message from the sandboxed iframe")]
    public void SendFromSandbox() => sandboxButtonText = Page.SendMessageFromSandbox();

    [Then("the parent page receives the sandboxed iframe message")]
    public void VerifySandboxMessage()
    {
        Assert.That(sandboxButtonText, Is.EqualTo("Message sent"));
        Assert.That(Page.SandboxMessage, Is.EqualTo("Message from the sandboxed frame: hello"));
        Assert.That(Page.SandboxAccessNotice, Is.EqualTo("The page can't read the sandboxed frame"));
    }

    [Then("I can enter {string} in the inline iframe")]
    public void VerifyInlineFrameInput(string value) => Assert.That(Page.EnterInlineText(value), Is.EqualTo(value));

    [Then("the iframe inside the shadow root displays {string}")]
    public void VerifyShadowFrameTitle(string expected) => Assert.That(Page.ReadShadowFrameTitle(), Is.EqualTo(expected));

    [Given("I open the advanced shadow DOM example")]
    public void OpenAdvancedShadowDom() => Page.Open("shadowdom_advanced");

    [When("I click the button inside the nested shadow roots")]
    public void ClickNestedButton() => Page.ClickNestedShadowButton();

    [Then("the page shows that the inner button was clicked")]
    public void VerifyInnerButton() =>
        Assert.That(Page.NestedButtonResult, Is.EqualTo("You clicked the button in the inner shadow root"));

    [Then("I can enter {string} in the closed shadow root input")]
    public void VerifyClosedRootInput(string value) => Assert.That(Page.EnterClosedRootText(value), Is.EqualTo(value));

    [Then("the title slot displays {string}")]
    public void VerifyTitleSlot(string expected) => Assert.That(Page.ReadProjectedTitle(), Is.EqualTo(expected));

    [When("I submit a {int}-star rating for {string}")]
    public void SubmitRating(int stars, string name) => Page.SubmitRating(name, stars);

    [Then("the form shows the submitted name and rating {string}")]
    public void VerifyRating(string expected) => Assert.That(Page.RatingResult, Is.EqualTo(expected));

    [Given("I open the TinyMCE editor example")]
    public void OpenEditor() => Page.Open("tinymce");

    [When("I replace the editor content with {string} in bold")]
    public void ReplaceEditorContent(string value) => editedText = Page.ReplaceEditorTextInBold(value);

    [Then("the editor shows {string} in bold")]
    public void VerifyEditorContent(string expected)
    {
        Assert.That(editedText, Is.EqualTo(expected));
        Assert.That(Page.ReadBoldEditorText(), Is.EqualTo(expected));
    }
}
