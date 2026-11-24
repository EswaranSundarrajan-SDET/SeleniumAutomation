using System.Globalization;
using System.Net;
using System.Text;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace AutomationReporting;

/// <summary>Serializes ExtentReports updates from concurrent scenario workers.</summary>
public sealed class ExtentReport
{
    private const string ReportTitle = "Automation Execution Results";
    private readonly object reportLock = new();
    private readonly ExtentReports report = new();

    /// <summary>Creates the output directory and configures the combined HTML report.</summary>
    public ExtentReport(string directory)
    {
        Directory.CreateDirectory(directory);
        var reporter = new ExtentSparkReporter(Path.Combine(directory, ReportTitle + ".html"));
        reporter.Config.DocumentTitle = ReportTitle;
        reporter.Config.ReportName = ReportTitle;
        reporter.Config.CSS = CategoryHeadingStyles;
        reporter.Config.JS = TestViewScript;
        report.AttachReporter(reporter);
    }

    /// <summary>Creates a scenario with its feature name and report category.</summary>
    public ExtentTest StartScenario(string feature, string name, string? category = null)
    {
        lock (reportLock)
            return report.CreateTest(WebUtility.HtmlEncode(name), WebUtility.HtmlEncode(feature))
                .AssignCategory(WebUtility.HtmlEncode(category ?? feature));
    }

    /// <summary>Writes an HTML-encoded message and status to the scenario's entry.</summary>
    public void Log(ExtentTest scenario, Status status, string message)
    {
        lock (reportLock)
            scenario.Log(status, WebUtility.HtmlEncode(message));
    }

    /// <summary>Writes a BDD step with its timing, binding, and expandable evidence.</summary>
    public void LogStep(ExtentTest scenario, Status status, string step, TimeSpan? duration, string? binding,
        string? dataTable, string? docString, string? diagnostic, string? screenshotBase64 = null,
        string? screenshotNote = null, string? apiDetails = null)
    {
        var html = new StringBuilder("<strong>").Append(WebUtility.HtmlEncode(step)).Append("</strong>");
        if (duration is TimeSpan elapsed)
        {
            html.Append("<div class='small'>Duration: ")
                .Append(elapsed.TotalMilliseconds.ToString("0.##", CultureInfo.InvariantCulture))
                .Append(" ms</div>");
        }
        if (binding != null)
            html.Append("<div class='small'>Step definition: ").Append(WebUtility.HtmlEncode(binding)).Append("</div>");

        AppendStepDetail(html, "Data table", dataTable);
        AppendStepDetail(html, "Doc string", docString);
        AppendStepDetail(html, "Result details", diagnostic);
        AppendStepDetail(html, "HTTP details", apiDetails);
        if (screenshotBase64 != null || screenshotNote != null)
        {
            html.Append("<details class='mt-1'><summary>Screenshot</summary>");
            if (screenshotNote != null)
                html.Append("<p class='small mb-1'>").Append(WebUtility.HtmlEncode(screenshotNote)).Append("</p>");
            if (screenshotBase64 != null)
            {
                html.Append("<img class='img-fluid' loading='lazy' alt='Browser viewport' src='data:image/png;base64,")
                    .Append(WebUtility.HtmlEncode(screenshotBase64)).Append("'>");
            }
            html.Append("</details>");
        }

        lock (reportLock)
            scenario.Log(status, html.ToString());
    }

    private static void AppendStepDetail(StringBuilder html, string label, string? content)
    {
        if (content != null)
        {
            html.Append("<details class='mt-1'><summary>").Append(label).Append("</summary><pre class='mb-1'>")
                .Append(WebUtility.HtmlEncode(content)).Append("</pre></details>");
        }
    }

    /// <summary>Embeds a Base64-encoded failure screenshot in the scenario entry.</summary>
    public void AttachScreenshot(ExtentTest scenario, string base64)
    {
        lock (reportLock)
            scenario.AddScreenCaptureFromBase64String(base64, "Failure screenshot");
    }

    /// <summary>Writes pending report updates to the HTML file.</summary>
    public void Flush()
    {
        lock (reportLock)
            report.Flush();
    }

    private const string CategoryHeadingStyles = """
        .test-view .test-list-item > .report-category-heading {
            border-bottom: 1px solid rgba(127, 127, 127, .3);
        }
        .report-category-heading h2 {
            margin: 0;
            font-size: 16px;
            font-weight: 700;
        }
        .report-category-heading button {
            display: flex;
            align-items: center;
            gap: 10px;
            width: 100%;
            padding: 12px 16px;
            border: 0;
            background: transparent;
            color: inherit;
            font: inherit;
            text-align: left;
            cursor: pointer;
        }
        .report-category-heading button:hover {
            background: rgba(127, 127, 127, .1);
        }
        .report-category-heading button:focus-visible {
            outline: 2px solid currentColor;
            outline-offset: -2px;
        }
        .report-category-heading button::before {
            content: '';
            width: 8px;
            height: 8px;
            border: solid currentColor;
            border-width: 1px 1px 0 0;
            transform: rotate(45deg);
        }
        .report-category-heading button[aria-expanded='true']::before {
            transform: rotate(135deg);
        }
        .test-view .test-list-item > .test-item[hidden] {
            display: none;
        }
        """;

    private const string TestViewScript = """
        (() => {
            const list = document.querySelector('.test-view .test-list-item');
            if (!list) return;

            // Spark's default search scans markup, which includes encoded screenshot bytes.
            const jquery = window.jQuery;
            if (jquery) {
                jquery.fn.dynamicTestSearch = function (selector) {
                    const rows = this;
                    jquery(selector).off('keyup').on('keyup', function () {
                        const query = this.value.toLowerCase();
                        for (const row of rows) {
                            row.classList.toggle('d-none', !row.textContent.toLowerCase().includes(query));
                        }
                    });
                    return rows;
                };
            }

            const groups = new Map([['UI', []], ['API', []]]);
            for (const row of list.querySelectorAll(':scope > .test-item')) {
                const category = row.getAttribute('tag')?.trim() || 'Other';
                if (!groups.has(category)) groups.set(category, []);
                groups.get(category).push(row);
            }

            for (const [category, rows] of groups) {
                if (rows.length === 0) continue;

                const heading = document.createElement('li');
                heading.className = 'report-category-heading';
                const title = document.createElement('h2');
                const toggle = document.createElement('button');
                toggle.type = 'button';
                toggle.textContent = category;
                const rowIds = rows.map(row => row.id ||= `report-scenario-${row.getAttribute('test-id')}`);
                toggle.setAttribute('aria-controls', rowIds.join(' '));
                title.append(toggle);
                heading.append(title);

                const setExpanded = (expanded) => {
                    toggle.setAttribute('aria-expanded', String(expanded));
                    for (const row of rows) {
                        row.hidden = !expanded;
                    }
                };
                setExpanded(true);
                toggle.addEventListener('click', () => setExpanded(toggle.getAttribute('aria-expanded') === 'false'));

                // Spark can select a scenario through a filter, a deep link, or the arrow keys.
                jquery?.(rows).on('click', () => {
                    if (toggle.getAttribute('aria-expanded') === 'false') setExpanded(true);
                });

                // Keep Spark's scenario rows and their click handlers in the original list.
                list.append(heading, ...rows);

                const updateHeading = () => {
                    heading.hidden = rows.every(row => row.classList.contains('d-none'));
                };
                const observer = new MutationObserver(updateHeading);
                for (const row of rows) {
                    observer.observe(row, { attributes: true, attributeFilter: ['class'] });
                }
                updateHeading();
            }
        })();
        """;
}
