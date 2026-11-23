using System.Collections.ObjectModel;
using System.Reflection;
using AutomationFramework.UI;
using NUnit.Framework;
using OpenQA.Selenium;

namespace AutomationTests.UI;

/// <summary>Verifies locator fallback rules without starting a web browser.</summary>
[Category("ui")]
public sealed class FallbackByTests
{
    private static readonly By Primary = By.Id("original");
    private static readonly By Alternative = By.CssSelector("input[name='username']");

    /// <summary>Verifies the fallback is skipped when the primary locator returns an element.</summary>
    [Test]
    public void UsesPrimaryWhenPresent()
    {
        var original = DispatchProxy.Create<IWebElement, ElementProxy>();
        var context = new StubSearchContext(locator => locator == Primary
            ? [original]
            : throw new InvalidOperationException("The alternative should not be queried."));

        Assert.That(new FallbackBy(Primary, Alternative).FindElement(context), Is.SameAs(original));
    }

    /// <summary>Verifies a unique fallback is returned when the primary locator has no match.</summary>
    [Test]
    public void UsesUniqueFallbackWhenPrimaryIsAbsent()
    {
        var fallback = DispatchProxy.Create<IWebElement, ElementProxy>();
        var context = new StubSearchContext(locator => locator == Alternative ? [fallback] : []);

        Assert.That(new FallbackBy(Primary, Alternative).FindElement(context), Is.SameAs(fallback));
    }

    /// <summary>Verifies an ambiguous selector fails before any candidate can be used.</summary>
    [Test]
    public void RejectsAmbiguousFallback()
    {
        var first = DispatchProxy.Create<IWebElement, ElementProxy>();
        var second = DispatchProxy.Create<IWebElement, ElementProxy>();
        var context = new StubSearchContext(locator => locator == Alternative ? [first, second] : []);

        Assert.That(() => new FallbackBy(Primary, Alternative).FindElement(context),
            Throws.InvalidOperationException.With.Message.Contains("matched 2 elements"));
    }

    /// <summary>Verifies a missing locator preserves Selenium's missing-element exception.</summary>
    [Test]
    public void ReportsMissingElementWhenNoLocatorMatches()
    {
        var context = new StubSearchContext(_ => []);

        Assert.That(() => new FallbackBy(Primary, Alternative).FindElement(context),
            Throws.TypeOf<NoSuchElementException>());
    }

    /// <summary>Returns the scripted matches for each locator.</summary>
    private sealed class StubSearchContext(Func<By, IWebElement[]> lookup) : ISearchContext
    {
        /// <summary>Returns the first scripted match or reports that no element exists.</summary>
        public IWebElement FindElement(By by) =>
            lookup(by).FirstOrDefault() ?? throw new NoSuchElementException(by.ToString());

        /// <summary>Returns the scripted matches in a Selenium-compatible collection.</summary>
        public ReadOnlyCollection<IWebElement> FindElements(By by) => Array.AsReadOnly(lookup(by));
    }

    /// <summary>Represents an element whose identity can be checked without WebDriver commands.</summary>
    public class ElementProxy : DispatchProxy
    {
        /// <summary>Fails if lookup attempts to interact with the stub element.</summary>
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
            throw new NotSupportedException("The stub only supplies an element reference.");
    }
}
