# Selenium Automation

The .NET 8 solution uses Selenium WebDriver, Reqnroll, NUnit, and RestSharp to test [The Internet](https://the-internet.herokuapp.com/). Running the suite requires access to that site.

| Feature | Coverage |
| --- | --- |
| Form authentication | Successful login with the site's public demo account and rejection of an incorrect password |
| Dynamic loading | Waits for the message displayed after clicking Start |
| Browser controls | Dropdowns, checkboxes, dynamic elements, alerts, context menus, hovers, sliders, drag and drop, nested frames, new windows, shadow DOM, tables, upload, and download |
| HTTP status codes | Checks the site's 200 and 404 responses with RestSharp |

## Projects

| Project | Contents |
| --- | --- |
| `AutomationFramework` | Reusable class library with Selenium driver creation, browser configuration, explicit waits, UI helpers, Extent reporting, and a RestSharp client |
| `AutomationTests` | Reqnroll and NUnit tests, including feature files, step definitions, page objects, hooks, test settings, and test data |

`AutomationTests` references `AutomationFramework` and owns the configuration and locators for The Internet.

## Run locally

Install the .NET 8 SDK and Chrome or Firefox. Run from the repository root.

```powershell
dotnet test SeleniumAutomation.sln
```

Chrome runs headless by default. Open `AutomationTests/Reports/ExtentReport.html` after the run. The report shows scenario outcomes and executed steps and embeds screenshots for browser failures. To watch the UI scenarios in Firefox and write reports to the Reports folder, run these commands from the repository root.

```powershell
$env:BROWSER = "firefox"
$env:HEADLESS = "false"
$env:REPORTS_DIR = Join-Path $PWD "AutomationTests/Reports"
dotnet test AutomationTests/AutomationTests.csproj --filter "TestCategory=ui" --logger "trx;LogFileName=selenium.trx" --results-directory $env:REPORTS_DIR
```

The first restore requires NuGet access. Selenium Manager can download a matching driver when one is unavailable locally.

## Extend the suite

In `AutomationTests`, add scenarios in `Features`, bind their steps in `StepDefinitions`, and keep locators and browser actions in `Pages`. The `@ui` tag enables the scenario-scoped browser, cleanup, and screenshot hooks. The `@http` scenarios use the RestSharp client in `AutomationFramework/API/ApiClient.cs`. The site URL lives in `AutomationTests/Configuration/TestSettings.cs`, and the public login credentials are in `AutomationTests/TestData/testdata.json`.

## UI helpers

| Helper | Operations |
| --- | --- |
| `ElementActions` | Find elements, wait for visibility or disappearance, click, type, clear, send keys, checkboxes and radio buttons, select and deselect options, inspect text and DOM state, read tables, access shadow roots, and upload files |
| `InputActions` | Click, double-click, context-click, hover, move the pointer, hold and release, drag and drop, send keys, hold modifiers, and scroll with the wheel |
| `BrowserActions` | Navigate, wait for URLs, switch frames and windows, operate alerts, run JavaScript, bring an element into view, and take screenshots |
| `DownloadActions` | Wait for a named download in the browser's download directory |
| `WaitHelper` | Build explicit waits and wait for elements to be present, visible, enabled, or hidden |

Use these helpers in page objects. For a sequence that needs the full Selenium Actions builder, use `InputActions.Perform`.

```csharp
var input = new InputActions(driver);
var item = driver.FindElement(By.Id("item"));
input.Perform(actions => actions.MoveToElement(item).KeyDown(Keys.Shift).Click().KeyUp(Keys.Shift));
```

For a native dropdown, use `SelectByText`, `SelectByValue`, or `SelectByIndex`. For a custom dropdown, click the trigger and option through `ElementActions`. The `@download` scenarios use a temporary directory for each browser.

## GitHub Actions

Selenium CI runs on pull requests, pushes to main, and manual dispatch. The Chrome and Firefox jobs build the solution, run the test project, and publish `ExtentReport.html`, TRX results, and any failure screenshots in the `selenium-chrome` and `selenium-firefox` artifacts.
