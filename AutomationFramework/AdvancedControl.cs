using AutomationFramework.Utilities;
using OpenQA.Selenium;

namespace AutomationFramework.UI;

/// <summary>Provides Selenium actions for controls that need multiple browsing contexts or browser APIs.</summary>
public sealed class AdvancedControl(IWebDriver driver)
{
    private readonly BrowserActions browser = new(driver);

    /// <summary>Finds an element through a chain of shadow hosts, ordered from outside to inside.</summary>
    /// <remarks>Use CSS selectors for locators inside a shadow root.</remarks>
    public IWebElement FindInShadowRoots(By locator, params By[] hosts)
    {
        var wait = WaitHelper.Create(driver);
        wait.Message = $"Element did not appear in the shadow DOM: {locator}";
        wait.IgnoreExceptionTypes(typeof(NoSuchShadowRootException), typeof(DetachedShadowRootException));

        return wait.Until(currentDriver =>
        {
            ISearchContext context = currentDriver;
            foreach (var host in hosts)
                context = context.FindElement(host).GetShadowRoot();

            return context.FindElement(locator);
        })!;
    }

    /// <summary>Runs an action inside a located frame and returns to its parent context.</summary>
    public T InFrame<T>(By frame, Func<T> action) => InFrame(WaitHelper.Present(driver, frame), action);

    /// <summary>Runs an action inside a frame element, including one found in a shadow root.</summary>
    public T InFrame<T>(IWebElement frame, Func<T> action)
    {
        driver.SwitchTo().Frame(frame);
        try
        {
            return action();
        }
        finally
        {
            browser.SwitchToParentFrame();
        }
    }

    /// <summary>Reads the requested HTML form validation flag without relying on translated messages.</summary>
    public bool HasValidationError(By field, string validityFlag)
    {
        var element = WaitHelper.Present(driver, field);
        var result = browser.ExecuteScript("return arguments[0].validity?.[arguments[1]];", element, validityFlag);
        return result is bool flag
            ? flag
            : throw new ArgumentException($"Validation flag '{validityFlag}' is unavailable on {field}.", nameof(validityFlag));
    }

    /// <summary>Waits for a cookie to be written and returns its value.</summary>
    public string WaitForCookieValue(string name)
    {
        var wait = WaitHelper.Create(driver);
        wait.Message = $"Cookie '{name}' was not written.";
        return wait.Until(currentDriver => currentDriver.Manage().Cookies.GetCookieNamed(name))!.Value;
    }

    /// <summary>Scrolls to the end of the page and waits for new items to appear.</summary>
    public int ScrollForMoreItems(By items)
    {
        var initialCount = driver.FindElements(items).Count;
        browser.ExecuteScript("window.scrollTo(0, document.scrollingElement.scrollHeight);");

        var wait = WaitHelper.Create(driver, TimeSpan.FromSeconds(30));
        wait.Message = $"Scrolling did not load more items matching {items}.";
        return wait.Until(currentDriver =>
        {
            var matches = currentDriver.FindElements(items);
            return matches.Count > initialCount ? matches : null;
        })!.Count;
    }

    /// <summary>Reads a browser permission state for the active page without requesting access.</summary>
    public string ReadPermissionState(string permission)
    {
        var state = browser.ExecuteAsyncScript("""
            const done = arguments[arguments.length - 1];
            navigator.permissions.query({ name: arguments[0] }).then(
                permission => done(permission.state),
                error => done(error.message));
            """, permission) as string;

        return state is "granted" or "denied" or "prompt"
            ? state
            : throw new InvalidOperationException($"Could not read browser permission '{permission}': {state}");
    }
}
