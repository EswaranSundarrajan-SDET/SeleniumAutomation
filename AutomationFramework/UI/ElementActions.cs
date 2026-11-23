using AutomationFramework.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AutomationFramework.UI;

public sealed class ElementActions(IWebDriver driver)
{
    public IWebElement Find(By locator) => WaitHelper.Present(driver, locator);
    public IWebElement FindVisible(By locator) => WaitHelper.Visible(driver, locator);
    public IReadOnlyList<IWebElement> FindAll(By locator) => driver.FindElements(locator);
    public void WaitUntilHidden(By locator) => WaitHelper.UntilInvisible(driver, locator);
    public void WaitUntilStale(IWebElement element) => WaitHelper.UntilStale(driver, element);

    public void Click(By locator) => WaitHelper.Enabled(driver, locator).Click();
    public void ClickByText(By locator, string text)
    {
        var wait = WaitHelper.Create(driver);
        wait.Message = $"No enabled element matching {locator} had the text '{text}'.";
        var element = wait.Until(d => d.FindElements(locator).FirstOrDefault(candidate =>
            candidate.Displayed && candidate.Enabled && candidate.Text == text));
        element!.Click();
    }

    public void Clear(By locator) => WaitHelper.Enabled(driver, locator).Clear();
    public void SendKeys(By locator, string keys) => WaitHelper.Enabled(driver, locator).SendKeys(keys);

    public void Type(By locator, string text)
    {
        var element = WaitHelper.Enabled(driver, locator);
        element.Clear();
        element.SendKeys(text);
    }

    public string Text(By locator) => FindVisible(locator).Text;
    public string? Attribute(By locator, string name) => Find(locator).GetDomAttribute(name);
    public string? Property(By locator, string name) => Find(locator).GetDomProperty(name);
    public string Css(By locator, string propertyName) => Find(locator).GetCssValue(propertyName);
    public bool IsVisible(By locator) => FindAll(locator).Any(element => element.Displayed);
    public bool IsEnabled(By locator) => Find(locator).Enabled;
    public bool IsSelected(By locator) => Find(locator).Selected;

    public void SetChecked(By locator, bool selected)
    {
        var checkbox = WaitHelper.Enabled(driver, locator);
        if (checkbox.Selected != selected)
            checkbox.Click();
    }

    public void SelectRadio(By locator) => SetChecked(locator, true);

    public void SelectByText(By locator, string text) => Select(locator).SelectByText(text);
    public void SelectByValue(By locator, string value) => Select(locator).SelectByValue(value);
    public void SelectByIndex(By locator, int index) => Select(locator).SelectByIndex(index);
    public void DeselectAll(By locator) => Select(locator).DeselectAll();
    public void DeselectByText(By locator, string text) => Select(locator).DeselectByText(text);
    public void DeselectByValue(By locator, string value) => Select(locator).DeselectByValue(value);
    public void DeselectByIndex(By locator, int index) => Select(locator).DeselectByIndex(index);
    public IReadOnlyList<string> SelectedOptions(By locator) =>
        new SelectElement(Find(locator)).AllSelectedOptions.Select(option => option.Text).ToArray();

    public void UploadFile(By locator, string path) =>
        Find(locator).SendKeys(Path.GetFullPath(path));

    public IWebElement FindInShadowRoot(By host, By locator)
    {
        var wait = WaitHelper.Create(driver);
        wait.Message = $"Shadow element did not become available: {host} > {locator}";
        wait.IgnoreExceptionTypes(typeof(NoSuchShadowRootException), typeof(DetachedShadowRootException));
        return wait.Until(d => d.FindElement(host).GetShadowRoot().FindElement(locator))!;
    }

    public IReadOnlyList<IReadOnlyList<string>> ReadTable(By table) =>
        FindVisible(table).FindElements(By.CssSelector("tr"))
            .Select(row => (IReadOnlyList<string>)row.FindElements(By.CssSelector("th, td"))
                .Select(cell => cell.Text).ToArray())
            .ToArray();

    private SelectElement Select(By locator) => new(WaitHelper.Enabled(driver, locator));
}
