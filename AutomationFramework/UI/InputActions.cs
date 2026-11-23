using AutomationFramework.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace AutomationFramework.UI;

/// <summary>Builds native WebDriver keyboard, pointer, and wheel input for the active session.</summary>
public sealed class InputActions(IWebDriver driver)
{
    /// <summary>Builds a fresh Actions sequence and sends it to the driver once.</summary>
    public void Perform(Action<Actions> sequence)
    {
        var actions = new Actions(driver);
        sequence(actions);
        actions.Perform();
    }

    /// <summary>Sends low-level device sequences for synchronized WebDriver input.</summary>
    public void PerformSequences(params ActionSequence[] sequences) =>
        ((IActionExecutor)driver).PerformActions(sequences);

    /// <summary>Clicks at the pointer's current position.</summary>
    public void Click() => Perform(actions => actions.Click());
    /// <summary>Moves to a displayed, enabled element and clicks it.</summary>
    public void Click(By locator) => Perform(actions => actions.Click(Ready(locator)));
    /// <summary>Double-clicks at the pointer's current position.</summary>
    public void DoubleClick() => Perform(actions => actions.DoubleClick());
    /// <summary>Moves to a displayed, enabled element and double-clicks it.</summary>
    public void DoubleClick(By locator) => Perform(actions => actions.DoubleClick(Ready(locator)));
    /// <summary>Opens the context menu at the pointer's current position.</summary>
    public void RightClick() => Perform(actions => actions.ContextClick());
    /// <summary>Moves to a displayed element and performs a context click.</summary>
    public void RightClick(By locator) => Perform(actions => actions.ContextClick(Visible(locator)));
    /// <summary>Moves the pointer to a displayed element.</summary>
    public void Hover(By locator) => Perform(actions => actions.MoveToElement(Visible(locator)));
    /// <summary>Moves the pointer to a pixel offset from the element's in-view center.</summary>
    public void MoveToElement(By locator, int xOffset, int yOffset) =>
        Perform(actions => actions.MoveToElement(Visible(locator), xOffset, yOffset));
    /// <summary>Moves the pointer relative to its current position.</summary>
    public void MoveByOffset(int x, int y) => Perform(actions => actions.MoveByOffset(x, y));
    /// <summary>Moves the pointer to absolute viewport coordinates.</summary>
    public void MoveToLocation(int x, int y) => Perform(actions => actions.MoveToLocation(x, y));

    /// <summary>Presses the primary pointer button at its current position.</summary>
    public void ClickAndHold() => Perform(actions => actions.ClickAndHold());
    /// <summary>Moves to a displayed, enabled element and holds the primary pointer button.</summary>
    public void ClickAndHold(By locator) => Perform(actions => actions.ClickAndHold(Ready(locator)));
    /// <summary>Scrolls to the element, holds the primary button for the duration, and releases it.</summary>
    public void LongPress(By locator, TimeSpan duration)
    {
        new BrowserActions(driver).ScrollIntoView(locator);
        Perform(actions => actions.ClickAndHold(Ready(locator)).Pause(duration).Release());
    }

    /// <summary>Releases the primary pointer button at its current position.</summary>
    public void Release() => Perform(actions => actions.Release());
    /// <summary>Moves to the displayed element and releases the primary pointer button.</summary>
    public void Release(By locator) => Perform(actions => actions.Release(Visible(locator)));
    /// <summary>Uses native pointer actions to drag the source to the target.</summary>
    public void DragAndDrop(By source, By target) =>
        Perform(actions => actions.DragAndDrop(Visible(source), Visible(target)));
    /// <summary>Uses native pointer actions to drag the source by a pixel delta.</summary>
    public void DragByOffset(By source, int x, int y) =>
        Perform(actions => actions.DragAndDropToOffset(Visible(source), x, y));

    /// <summary>Sends keys to the currently focused element.</summary>
    public void SendKeys(string keys) => Perform(actions => actions.SendKeys(keys));
    /// <summary>Targets a displayed, enabled element and sends the requested keys.</summary>
    public void SendKeys(By locator, string keys) => Perform(actions => actions.SendKeys(Ready(locator), keys));
    /// <summary>Starts holding a modifier key in the current focus context.</summary>
    public void KeyDown(string modifier) => Perform(actions => actions.KeyDown(modifier));
    /// <summary>Targets an element and starts holding the given modifier.</summary>
    public void KeyDown(By locator, string modifier) =>
        Perform(actions => actions.KeyDown(Ready(locator), modifier));
    /// <summary>Targets an element and releases the given modifier.</summary>
    public void KeyUp(By locator, string modifier) =>
        Perform(actions => actions.KeyUp(Ready(locator), modifier));
    /// <summary>Releases a modifier key in the current focus context.</summary>
    public void KeyUp(string modifier) => Perform(actions => actions.KeyUp(modifier));
    /// <summary>Holds the modifier, sends the keys, and releases the modifier in one sequence.</summary>
    public void Chord(string modifier, string keys) =>
        Perform(actions => actions.KeyDown(modifier).SendKeys(keys).KeyUp(modifier));
    /// <summary>Uses wheel input to bring a displayed element into the viewport.</summary>
    public void ScrollToElement(By locator) => Perform(actions => actions.ScrollToElement(Visible(locator)));
    /// <summary>Scrolls by the requested horizontal and vertical wheel deltas.</summary>
    public void ScrollByAmount(int x, int y) => Perform(actions => actions.ScrollByAmount(x, y));
    /// <summary>Uses the displayed element as the wheel origin and scrolls by the given deltas.</summary>
    public void ScrollFromElement(By locator, int x, int y) =>
        Perform(actions => actions.ScrollFromOrigin(new WheelInputDevice.ScrollOrigin
        {
            Element = Visible(locator)
        }, x, y));

    /// <summary>Clears device input state, including held keys and buttons.</summary>
    public void ResetInputState() => ((IActionExecutor)driver).ResetInputState();

    /// <summary>Waits for a displayed target before building a pointer or wheel action.</summary>
    private IWebElement Visible(By locator) => WaitHelper.Visible(driver, locator);
    /// <summary>Waits for a displayed, enabled target before building an input action.</summary>
    private IWebElement Ready(By locator) => WaitHelper.Enabled(driver, locator);
}
