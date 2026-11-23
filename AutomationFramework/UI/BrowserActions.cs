using AutomationFramework.Utilities;
using OpenQA.Selenium;

namespace AutomationFramework.UI;

/// <summary>Controls the current WebDriver session, browsing context, and JavaScript execution.</summary>
public sealed class BrowserActions(IWebDriver driver)
{
    /// <summary>Gets the URL of the active page.</summary>
    public string Url => driver.Url;
    /// <summary>Gets the title of the active page.</summary>
    public string Title => driver.Title;
    /// <summary>Gets the active window or tab handle.</summary>
    public string CurrentWindow => driver.CurrentWindowHandle;
    /// <summary>Gets all window and tab handles in this session.</summary>
    public IReadOnlyList<string> WindowHandles => driver.WindowHandles;

    /// <summary>Opens the supplied URL in the active window.</summary>
    public void Navigate(Uri url) => driver.Navigate().GoToUrl(url);
    /// <summary>Moves one entry back in browser history.</summary>
    public void Back() => driver.Navigate().Back();
    /// <summary>Moves one entry forward in browser history.</summary>
    public void Forward() => driver.Navigate().Forward();
    /// <summary>Reloads the current page.</summary>
    public void Refresh() => driver.Navigate().Refresh();
    /// <summary>Polls the page URL until the supplied predicate succeeds.</summary>
    public void WaitForUrl(Func<string, bool> matches) =>
        WaitHelper.Create(driver).Until(d => matches(d.Url));

    /// <summary>Waits for a located frame element and enters its browsing context.</summary>
    public void SwitchToFrame(By locator) => driver.SwitchTo().Frame(WaitHelper.Present(driver, locator));
    /// <summary>Enters a frame identified by name or ID in the current context.</summary>
    public void SwitchToFrame(string nameOrId) => driver.SwitchTo().Frame(nameOrId);
    /// <summary>Enters a frame by zero-based index in the current context.</summary>
    public void SwitchToFrame(int index) => driver.SwitchTo().Frame(index);
    /// <summary>Returns to the parent of the current frame.</summary>
    public void SwitchToParentFrame() => driver.SwitchTo().ParentFrame();
    /// <summary>Returns to the window's top-level document.</summary>
    public void SwitchToMainDocument() => driver.SwitchTo().DefaultContent();

    /// <summary>Changes the active window or tab to the supplied handle.</summary>
    public void SwitchToWindow(string handle) => driver.SwitchTo().Window(handle);
    /// <summary>Opens and switches to a new tab or window of the requested type.</summary>
    public void OpenWindow(WindowType type) => driver.SwitchTo().NewWindow(type);
    /// <summary>Closes the active tab or window.</summary>
    public void CloseWindow() => driver.Close();

    /// <summary>Waits for a handle outside the supplied snapshot and switches to it.</summary>
    public void SwitchToNewWindow(IReadOnlyCollection<string> existingHandles)
    {
        var wait = WaitHelper.Create(driver);
        wait.Message = "A new browser window did not open.";
        var handle = wait.Until(d => d.WindowHandles.FirstOrDefault(window => !existingHandles.Contains(window)));
        SwitchToWindow(handle!);
    }

    /// <summary>Waits for a JavaScript dialog and returns its displayed message.</summary>
    public string AlertText() => WaitForAlert().Text;
    /// <summary>Waits for a JavaScript dialog and accepts it.</summary>
    public void AcceptAlert() => WaitForAlert().Accept();
    /// <summary>Waits for a JavaScript dialog and dismisses it.</summary>
    public void DismissAlert() => WaitForAlert().Dismiss();

    /// <summary>Enters text into a JavaScript prompt and accepts it.</summary>
    public void SubmitPrompt(string text)
    {
        var alert = WaitForAlert();
        alert.SendKeys(text);
        alert.Accept();
    }

    /// <summary>Runs a synchronous script in the current context using positional arguments.</summary>
    public object? ExecuteScript(string script, params object[] arguments) =>
        ((IJavaScriptExecutor)driver).ExecuteScript(script, arguments);

    /// <summary>Runs a script whose completion is signaled by Selenium's callback.</summary>
    public object? ExecuteAsyncScript(string script, params object[] arguments) =>
        ((IJavaScriptExecutor)driver).ExecuteAsyncScript(script, arguments);

    /// <summary>Gets the browser cookie API for the current session.</summary>
    public ICookieJar Cookies => driver.Manage().Cookies;

    /// <summary>Centers the located element in the viewport with JavaScript.</summary>
    public void ScrollIntoView(By locator) =>
        ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", WaitHelper.Present(driver, locator));

    /// <summary>Dispatches HTML5 drag events with a shared DataTransfer for DOM drop handlers.</summary>
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

    /// <summary>Captures the active browser viewport when the driver supports screenshots.</summary>
    public Screenshot CaptureScreenshot() => ((ITakesScreenshot)driver).GetScreenshot();

    /// <summary>Polls for the dialog while the browser reports no alert present.</summary>
    private IAlert WaitForAlert()
    {
        var wait = WaitHelper.Create(driver);
        wait.Message = "The JavaScript alert did not appear.";
        wait.IgnoreExceptionTypes(typeof(NoAlertPresentException));
        return wait.Until(d => d.SwitchTo().Alert());
    }
}
