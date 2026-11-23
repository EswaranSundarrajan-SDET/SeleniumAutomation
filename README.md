# Selenium Automation

The .NET 8 solution uses Selenium WebDriver, Reqnroll, NUnit, and RestSharp to test [The Internet](https://the-internet.herokuapp.com/). Running the suite requires access to that site.

| Feature | Coverage |
| --- | --- |
| Form authentication | Successful login with the site's public demo account and rejection of an incorrect password |
| Dynamic loading | Waits for the message displayed after clicking Start |
| HTTP status codes | Checks the site's 200 and 404 responses with RestSharp |

## Projects

| Project | Contents |
| --- | --- |
| `AutomationFramework` | Reusable class library with Selenium driver creation, browser configuration, explicit waits, and a RestSharp client |
| `AutomationTests` | Reqnroll and NUnit tests, including feature files, step definitions, page objects, hooks, test settings, and test data |

`AutomationTests` references `AutomationFramework` and owns the configuration and locators for The Internet.

## Run locally

Install the .NET 8 SDK and Chrome or Firefox. Run from the repository root.

```powershell
dotnet test SeleniumAutomation.sln
```

Chrome runs headless by default. To watch the UI scenarios in Firefox and write reports to the Reports folder, run these commands from the repository root.

```powershell
$env:BROWSER = "firefox"
$env:HEADLESS = "false"
$env:REPORTS_DIR = Join-Path $PWD "AutomationTests/Reports"
dotnet test AutomationTests/AutomationTests.csproj --filter "TestCategory=ui" --logger "trx;LogFileName=selenium.trx" --results-directory $env:REPORTS_DIR
```

The first restore requires NuGet access. Selenium Manager can download a matching driver when one is unavailable locally.

## Extend the suite

In `AutomationTests`, add scenarios in `Features`, bind their steps in `StepDefinitions`, and keep locators and browser actions in `Pages`. The `@ui` tag enables the scenario-scoped browser, cleanup, and screenshot hooks. The `@http` scenarios use the RestSharp client in `AutomationFramework/API/ApiClient.cs`. The site URL lives in `AutomationTests/Configuration/TestSettings.cs`, and the public login credentials are in `AutomationTests/TestData/testdata.json`.

## GitHub Actions

Selenium CI runs on pull requests, pushes to main, and manual dispatch. The Chrome and Firefox jobs build the solution, run the test project, and publish browser-specific TRX results and any failure screenshots as artifacts.
