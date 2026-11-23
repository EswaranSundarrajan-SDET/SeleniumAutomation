using AutomationFramework.Utilities;
using OpenQA.Selenium;

namespace AutomationFramework.UI;

public sealed class BrowserActions(IWebDriver driver)
{
    public string Url => driver.Url;
    public string Title => driver.Title;
    public string CurrentWindow => driver.CurrentWindowHandle;
    public IReadOnlyList<string> WindowHandles => driver.WindowHandles;

    public void Navigate(Uri url) => driver.Navigate().GoToUrl(url);
    public void Back() => driver.Navigate().Back();
    public void Forward() => driver.Navigate().Forward();
    public void Refresh() => driver.Navigate().Refresh();
    public void WaitForUrl(Func<string, bool> matches) =>
        WaitHelper.Create(driver).Until(d => matches(d.Url));

    public void SwitchToFrame(By locator) => driver.SwitchTo().Frame(WaitHelper.Present(driver, locator));
    public void SwitchToFrame(string nameOrId) => driver.SwitchTo().Frame(nameOrId);
    public void SwitchToFrame(int index) => driver.SwitchTo().Frame(index);
    public void SwitchToParentFrame() => driver.SwitchTo().ParentFrame();
    public void SwitchToMainDocument() => driver.SwitchTo().DefaultContent();

    public void SwitchToWindow(string handle) => driver.SwitchTo().Window(handle);
    public void OpenWindow(WindowType type) => driver.SwitchTo().NewWindow(type);
    public void CloseWindow() => driver.Close();

    public void SwitchToNewWindow(IReadOnlyCollection<string> existingHandles)
    {
        var wait = WaitHelper.Create(driver);
        wait.Message = "A new browser window did not open.";
        var handle = wait.Until(d => d.WindowHandles.FirstOrDefault(window => !existingHandles.Contains(window)));
        SwitchToWindow(handle!);
    }

    public string AlertText() => WaitForAlert().Text;
    public void AcceptAlert() => WaitForAlert().Accept();
    public void DismissAlert() => WaitForAlert().Dismiss();

    public void SubmitPrompt(string text)
    {
        var alert = WaitForAlert();
        alert.SendKeys(text);
        alert.Accept();
    }

    public object? ExecuteScript(string script, params object[] arguments) =>
        ((IJavaScriptExecutor)driver).ExecuteScript(script, arguments);

    public object? ExecuteAsyncScript(string script, params object[] arguments) =>
        ((IJavaScriptExecutor)driver).ExecuteAsyncScript(script, arguments);

    public ICookieJar Cookies => driver.Manage().Cookies;

    public void ScrollIntoView(By locator) =>
        ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", WaitHelper.Present(driver, locator));

    // Dispatch DOM drag events when testing an HTML5 drop handler across browsers.
    public void DragHtml5(By source, By target)
    {
        ExecuteScript("""
            const source = arguments[0], target = arguments[1];
            const dataTransfer = new DataTransfer();
            const send = (element, type) => element.dispatchEvent(new DragEvent(type, {
                bubbles: true, cancelable: true, dataTransfer
            }));
            send(source, 'dragstart');
            send(target, 'dragenter');
            if (!send(target, 'dragover')) send(target, 'drop');
            send(source, 'dragend');
            """, WaitHelper.Visible(driver, source), WaitHelper.Visible(driver, target));
    }

    public Screenshot CaptureScreenshot() => ((ITakesScreenshot)driver).GetScreenshot();

    private IAlert WaitForAlert()
    {
        var wait = WaitHelper.Create(driver);
        wait.Message = "The JavaScript alert did not appear.";
        wait.IgnoreExceptionTypes(typeof(NoAlertPresentException));
        return wait.Until(d => d.SwitchTo().Alert());
    }
}
