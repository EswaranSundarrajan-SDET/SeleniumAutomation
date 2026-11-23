using System.Collections.ObjectModel;
using System.Diagnostics;
using OpenQA.Selenium;

namespace AutomationFramework.UI;

/// <summary>Finds a single intended element by trying named alternatives when the primary locator is absent.</summary>
public sealed class FallbackBy : By
{
    private readonly By primary;
    private readonly By[] alternatives;

    /// <summary>Creates an ordered set of alternatives for the same element.</summary>
    /// <param name="primary">The preferred locator, checked on every lookup.</param>
    /// <param name="alternatives">Ordered, narrowly scoped selectors that each identify the same element.</param>
    public FallbackBy(By primary, params By[] alternatives)
    {
        ArgumentNullException.ThrowIfNull(primary);
        ArgumentNullException.ThrowIfNull(alternatives);
        if (alternatives.Length == 0 || alternatives.Any(locator => locator is null))
            throw new ArgumentException("Provide at least one valid fallback locator.", nameof(alternatives));

        this.primary = primary;
        this.alternatives = (By[])alternatives.Clone();
        Description = $"{primary} with fallbacks [{string.Join(", ", this.alternatives.Select(locator => locator.ToString()))}]";
    }

    /// <summary>Returns the first result or throws when neither the primary nor an alternative matches.</summary>
    public override IWebElement FindElement(ISearchContext context) =>
        FindElements(context).FirstOrDefault()
        ?? throw new NoSuchElementException($"No element matched {this}.");

    /// <summary>Preserves primary matches and accepts an alternative only when it identifies exactly one element.</summary>
    /// <exception cref="InvalidOperationException">A fallback locator identifies more than one element.</exception>
    public override ReadOnlyCollection<IWebElement> FindElements(ISearchContext context)
    {
        var matches = context.FindElements(primary);
        if (matches.Count > 0)
            return matches;

        foreach (var alternative in alternatives)
        {
            matches = context.FindElements(alternative);
            if (matches.Count == 0)
                continue;

            if (matches.Count > 1)
                throw new InvalidOperationException($"Fallback locator {alternative} for {primary} matched {matches.Count} elements.");

            Trace.TraceWarning("Selenium used fallback locator {0} because {1} was absent.", alternative, primary);
            return matches;
        }

        return matches;
    }
}
