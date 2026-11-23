# Selenium Automation

The .NET 8 solution uses Selenium WebDriver, Reqnroll, NUnit, RestSharp, and ExtentReports. Browser scenarios run against [The Internet](https://the-internet.herokuapp.com/). API scenarios use The Internet, [HTTPBingo](https://httpbingo.org/), and a controlled in-process HTTP transport.

| Feature | Coverage |
| --- | --- |
| Form authentication | Successful login with the site's public demo account and rejection of an incorrect password |
| Dynamic loading | Waits for the message displayed after clicking Start |
| Browser controls | Dropdowns, checkboxes, dynamic elements, alerts, context menus, hovers, sliders, drag and drop, nested frames, new windows, shadow DOM, tables, upload, and download |
| Advanced controls | Radio buttons, native multi-select, custom dropdowns, autocomplete, calendar, dynamically enabled inputs, stale elements, swipe, and long press |
| HTTP status codes | Checks 200 and 404 responses from The Internet with RestSharp |
| HTTP methods | Live GET, POST, PUT, PATCH, DELETE, HEAD, and OPTIONS using the HTTPBingo echo service. TRACE, CONNECT, COPY, MERGE, and SEARCH are checked in process at the transport boundary |

## Projects

| Project | Contents |
| --- | --- |
| `AutomationFramework` | Reusable Selenium class library with driver creation, browser configuration, waits, and UI helpers |
| `AutomationTests` | UI and API scenarios under `UI/` and `API/`, with shared Reqnroll hooks in `Hooks/` |
| `ApiAutomation` | RestSharp client library |
| `AutomationReporting` | Extent HTML reporting shared by the UI and API projects |

`AutomationTests` references `AutomationFramework` for Selenium, `ApiAutomation` for RestSharp, and `AutomationReporting` for Extent HTML reporting.

## Run locally

Install the .NET 8 SDK and Chrome or Firefox. Run from the repository root.

```powershell
dotnet test SeleniumAutomation.sln
```

Run one suite with its NUnit category filter.

```powershell
dotnet test AutomationTests/AutomationTests.csproj --filter "TestCategory=http"
dotnet test AutomationTests/AutomationTests.csproj --filter "TestCategory=ui"
```

Chrome runs headless by default. Open `AutomationTests/Reports/ExtentReport.html` after the run. A full run includes both suites in the same report. Reports include scenario outcomes and executed steps. The browser report embeds screenshots on failure. To watch the UI scenarios in Firefox and write reports to the Reports folder, run these commands from the repository root.

```powershell
$env:BROWSER = "firefox"
$env:HEADLESS = "false"
$env:REPORTS_DIR = Join-Path $PWD "AutomationTests/Reports"
dotnet test AutomationTests/AutomationTests.csproj --filter "TestCategory=ui" --logger "trx;LogFileName=selenium.trx" --results-directory $env:REPORTS_DIR
```

The first restore requires NuGet access. Selenium Manager can download a matching driver when one is unavailable locally.

## Extend the suite

UI features, steps, page objects, and input data live under `AutomationTests/UI`. API features and steps live under `AutomationTests/API`. Both suites use `AutomationTests/Hooks/TestHooks.cs` for reporting. The `@ui` tag enables scenario-scoped browser startup, and screenshots are captured when a browser scenario fails. Shared test configuration lives in `AutomationTests/Configuration/TestSettings.cs`.

## Object-oriented design

| Principle | Where it is used |
| --- | --- |
| Encapsulation | `LoginPage.SignIn` owns its selectors and interaction sequence. `ScenarioSession` owns browser and download resources. |
| Abstraction | `PageObject` exposes shared navigation and UI helpers to derived pages. `IBrowserDriverProvider` defines the browser creation contract. |
| Inheritance | The login, dynamic loading, browser controls, and advanced controls page objects inherit common behavior from `PageObject`. |
| Polymorphism | `DriverFactory` calls `IBrowserDriverProvider.Create` and selects the Chrome or Firefox implementation at runtime. The Selenium helpers operate on the returned `IWebDriver`. |

`ApiClient.ExecuteAsync` can take a configured RestSharp request or a resource and `HttpMethod`. The latter keeps method selection and custom-verb handling inside the API library.

## API tests

The RestSharp client in `ApiAutomation/Clients/ApiClient.cs` executes requests built with `RestRequest`. The live requests check HTTP status, query encoding, echoed JSON, HEAD metadata, and OPTIONS preflight headers. HTTPBingo echoes synthetic data. For TRACE and CONNECT, a RestSharp interceptor sets the verb before an in-process handler records it. These cases check request construction without opening a network connection or establishing a tunnel. Run the transport cases alone with `dotnet test AutomationTests/AutomationTests.csproj --filter "TestCategory=transport"`.

## UI helpers

| Helper | Operations |
| --- | --- |
| `ElementActions` | Find elements, wait for visibility or disappearance, click, click by text, type, clear, send keys, checkboxes and radio buttons, select and deselect options, inspect text and DOM state, read tables, access shadow roots, and upload files |
| `InputActions` | Click, double-click, context-click, hover, move the pointer, hold and release, long press, drag and drop, send keys, hold modifiers, and scroll with the wheel |
| `BrowserActions` | Navigate, wait for URLs, switch frames and windows, operate alerts, manage cookies, run JavaScript, dispatch HTML5 drag events, scroll into view, and take screenshots |
| `DownloadActions` | Wait for a named download in the browser's download directory |
| `WaitHelper` | Build explicit waits and wait for elements to be present, visible, enabled, hidden, or stale |

Use these helpers in page objects. For a sequence that needs the full Selenium Actions builder, use `InputActions.Perform`. Use `PerformSequences` to submit low-level sequences for devices such as touch and pen.

```csharp
var input = new InputActions(driver);
var item = driver.FindElement(By.Id("item"));
input.Perform(actions => actions.MoveToElement(item).KeyDown(Keys.Shift).Click().KeyUp(Keys.Shift));
```

For a native dropdown, use `SelectByText`, `SelectByValue`, or `SelectByIndex`. For a custom dropdown, click the trigger and option through `ElementActions`. The `@download` scenario uses a temporary directory for each browser. The HTML5 drag and drop example checks the page's drop handler with DOM events. Swipe and long press use native WebDriver pointer actions.

## GitHub Actions

Selenium CI runs on pull requests, pushes to main, and manual dispatch. The Chrome and Firefox jobs build the solution, run the test project, and publish `ExtentReport.html`, TRX results, and any failure screenshots in the `selenium-chrome` and `selenium-firefox` artifacts. The RestSharp job selects the API scenarios from the same test project and publishes an HTML report and TRX results in the `restsharp-api` artifact.
