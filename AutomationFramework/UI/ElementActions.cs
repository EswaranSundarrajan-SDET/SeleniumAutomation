using AutomationFramework.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AutomationFramework.UI;

/// <summary>Locates, reads, and operates on DOM elements in the current browser context.</summary>
public sealed class ElementActions(IWebDriver driver)
{
    /// <summary>Waits until the locator has a DOM match.</summary>
    public IWebElement Find(By locator) => WaitHelper.Present(driver, locator);
    /// <summary>Waits until the locator has a displayed match.</summary>
    public IWebElement FindVisible(By locator) => WaitHelper.Visible(driver, locator);
    /// <summary>Returns the current matches immediately, including hidden elements.</summary>
    public IReadOnlyList<IWebElement> FindAll(By locator) => driver.FindElements(locator);
    /// <summary>Waits for all matching elements to become hidden or disappear.</summary>
    public void WaitUntilHidden(By locator) => WaitHelper.UntilInvisible(driver, locator);
    /// <summary>Waits for a previously returned element reference to detach.</summary>
    public void WaitUntilStale(IWebElement element) => WaitHelper.UntilStale(driver, element);

    /// <summary>Waits for a displayed, enabled element and sends one WebDriver click.</summary>
    public void Click(By locator) => WaitHelper.Enabled(driver, locator).Click();
    /// <summary>Clicks the first displayed, enabled match with the exact rendered text.</summary>
    public void ClickByText(By locator, string text)
    {
        var wait = WaitHelper.Create(driver);
        wait.Message = $"No enabled element matching {locator} had the text '{text}'.";
        var element = wait.Until(d => d.FindElements(locator).FirstOrDefault(candidate =>
            candidate.Displayed && candidate.Enabled && candidate.Text == text));
        element!.Click();
    }

    /// <summary>Waits for an enabled control and clears its current value.</summary>
    public void Clear(By locator) => WaitHelper.Enabled(driver, locator).Clear();
    /// <summary>Sends the given text or Selenium key sequence without clearing the field.</summary>
    public void SendKeys(By locator, string keys) => WaitHelper.Enabled(driver, locator).SendKeys(keys);

    /// <summary>Clears an enabled field and enters replacement text.</summary>
    public void Type(By locator, string text)
    {
        var element = WaitHelper.Enabled(driver, locator);
        element.Clear();
        element.SendKeys(text);
    }

    /// <summary>Returns the rendered text of a displayed match.</summary>
    public string Text(By locator) => FindVisible(locator).Text;
    /// <summary>Reads the named DOM attribute of the first match.</summary>
    public string? Attribute(By locator, string name) => Find(locator).GetDomAttribute(name);
    /// <summary>Reads the named live DOM property, such as the current input value.</summary>
    public string? Property(By locator, string name) => Find(locator).GetDomProperty(name);
    /// <summary>Reads a computed CSS property from the first match.</summary>
    public string Css(By locator, string propertyName) => Find(locator).GetCssValue(propertyName);
    /// <summary>Checks immediately whether any current match is displayed.</summary>
    public bool IsVisible(By locator) => FindAll(locator).Any(element => element.Displayed);
    /// <summary>Waits for presence and then returns the element's current enabled state.</summary>
    public bool IsEnabled(By locator) => Find(locator).Enabled;
    /// <summary>Waits for presence and then returns the element's current selection state.</summary>
    public bool IsSelected(By locator) => Find(locator).Selected;

    /// <summary>Clicks once when the checkbox selection differs from the requested state.</summary>
    public void SetChecked(By locator, bool selected)
    {
        var checkbox = WaitHelper.Enabled(driver, locator);
        if (checkbox.Selected != selected)
            checkbox.Click();
    }

    /// <summary>Selects the radio button if it is currently unselected.</summary>
    public void SelectRadio(By locator) => SetChecked(locator, true);

    /// <summary>Selects a native dropdown option by its displayed label.</summary>
    public void SelectByText(By locator, string text) => Select(locator).SelectByText(text);
    /// <summary>Selects a native dropdown option by its HTML value.</summary>
    public void SelectByValue(By locator, string value) => Select(locator).SelectByValue(value);
    /// <summary>Selects a native dropdown option by its index.</summary>
    public void SelectByIndex(By locator, int index) => Select(locator).SelectByIndex(index);
    /// <summary>Clears all selections in a native multiple-select control.</summary>
    public void DeselectAll(By locator) => Select(locator).DeselectAll();
    /// <summary>Clears the native multiple-select option with the given label.</summary>
    public void DeselectByText(By locator, string text) => Select(locator).DeselectByText(text);
    /// <summary>Clears the native multiple-select option with the given HTML value.</summary>
    public void DeselectByValue(By locator, string value) => Select(locator).DeselectByValue(value);
    /// <summary>Clears the native multiple-select option at the supplied index.</summary>
    public void DeselectByIndex(By locator, int index) => Select(locator).DeselectByIndex(index);
    /// <summary>Reads the labels of selected options from a native select element.</summary>
    public IReadOnlyList<string> SelectedOptions(By locator) =>
        new SelectElement(Find(locator)).AllSelectedOptions.Select(option => option.Text).ToArray();

    /// <summary>Resolves a local path and sends it to the file input.</summary>
    public void UploadFile(By locator, string path) =>
        Find(locator).SendKeys(Path.GetFullPath(path));

    /// <summary>Polls for an element within the located shadow root and tolerates roots that reattach.</summary>
    /// <remarks>Use CSS selectors for elements inside the shadow root.</remarks>
    public IWebElement FindInShadowRoot(By host, By locator)
    {
        var wait = WaitHelper.Create(driver);
        wait.Message = $"Shadow element did not become available: {host} > {locator}";
        wait.IgnoreExceptionTypes(typeof(NoSuchShadowRootException), typeof(DetachedShadowRootException));
        return wait.Until(d =>
        {
            var root = d.FindElement(host).GetShadowRoot();
            // Selenium 4.28 sends a shadow locator directly to the driver without calling its custom lookup.
            return locator is FallbackBy fallback ? fallback.FindElement(root) : root.FindElement(locator);
        })!;
    }

    /// <summary>Reads header and data cell text grouped by table row.</summary>
    public IReadOnlyList<IReadOnlyList<string>> ReadTable(By table) =>
        FindVisible(table).FindElements(By.CssSelector("tr"))
            .Select(row => (IReadOnlyList<string>)row.FindElements(By.CssSelector("th, td"))
                .Select(cell => cell.Text).ToArray())
            .ToArray();

    /// <summary>Waits for an enabled control and wraps it as a native select element.</summary>
    private SelectElement Select(By locator) => new(WaitHelper.Enabled(driver, locator));
}
