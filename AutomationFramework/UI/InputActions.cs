using AutomationFramework.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace AutomationFramework.UI;

public sealed class InputActions(IWebDriver driver)
{
    // The full Selenium Actions builder is available for composed input sequences.
    public void Perform(Action<Actions> sequence)
    {
        var actions = new Actions(driver);
        sequence(actions);
        actions.Perform();
    }

    public void PerformSequences(params ActionSequence[] sequences) =>
        ((IActionExecutor)driver).PerformActions(sequences);

    public void Click() => Perform(actions => actions.Click());
    public void Click(By locator) => Perform(actions => actions.Click(Ready(locator)));
    public void DoubleClick() => Perform(actions => actions.DoubleClick());
    public void DoubleClick(By locator) => Perform(actions => actions.DoubleClick(Ready(locator)));
    public void RightClick() => Perform(actions => actions.ContextClick());
    public void RightClick(By locator) => Perform(actions => actions.ContextClick(Visible(locator)));
    public void Hover(By locator) => Perform(actions => actions.MoveToElement(Visible(locator)));
    public void MoveToElement(By locator, int xOffset, int yOffset) =>
        Perform(actions => actions.MoveToElement(Visible(locator), xOffset, yOffset));
    public void MoveByOffset(int x, int y) => Perform(actions => actions.MoveByOffset(x, y));
    public void MoveToLocation(int x, int y) => Perform(actions => actions.MoveToLocation(x, y));

    public void ClickAndHold() => Perform(actions => actions.ClickAndHold());
    public void ClickAndHold(By locator) => Perform(actions => actions.ClickAndHold(Ready(locator)));
    public void LongPress(By locator, TimeSpan duration)
    {
        new BrowserActions(driver).ScrollIntoView(locator);
        Perform(actions => actions.ClickAndHold(Ready(locator)).Pause(duration).Release());
    }

    public void Release() => Perform(actions => actions.Release());
    public void Release(By locator) => Perform(actions => actions.Release(Visible(locator)));
    public void DragAndDrop(By source, By target) =>
        Perform(actions => actions.DragAndDrop(Visible(source), Visible(target)));
    public void DragByOffset(By source, int x, int y) =>
        Perform(actions => actions.DragAndDropToOffset(Visible(source), x, y));

    public void SendKeys(string keys) => Perform(actions => actions.SendKeys(keys));
    public void SendKeys(By locator, string keys) => Perform(actions => actions.SendKeys(Ready(locator), keys));
    public void KeyDown(string modifier) => Perform(actions => actions.KeyDown(modifier));
    public void KeyDown(By locator, string modifier) =>
        Perform(actions => actions.KeyDown(Ready(locator), modifier));
    public void KeyUp(By locator, string modifier) =>
        Perform(actions => actions.KeyUp(Ready(locator), modifier));
    public void KeyUp(string modifier) => Perform(actions => actions.KeyUp(modifier));
    public void Chord(string modifier, string keys) =>
        Perform(actions => actions.KeyDown(modifier).SendKeys(keys).KeyUp(modifier));
    public void ScrollToElement(By locator) => Perform(actions => actions.ScrollToElement(Visible(locator)));
    public void ScrollByAmount(int x, int y) => Perform(actions => actions.ScrollByAmount(x, y));
    public void ScrollFromElement(By locator, int x, int y) =>
        Perform(actions => actions.ScrollFromOrigin(new WheelInputDevice.ScrollOrigin
        {
            Element = Visible(locator)
        }, x, y));

    public void ResetInputState() => ((IActionExecutor)driver).ResetInputState();

    private IWebElement Visible(By locator) => WaitHelper.Visible(driver, locator);
    private IWebElement Ready(By locator) => WaitHelper.Enabled(driver, locator);
}
