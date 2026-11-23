# Selenium automation framework

The .NET 8 test project uses Selenium WebDriver, Reqnroll and NUnit. The feature files cover successful login, invalid login and settlement submission with API verification. Each scenario gets a fresh browser, a local ASP.NET Core application on an available loopback port, and its own in-memory data. Demo credentials and amounts are synthetic.

## Run locally

Install the .NET 8 SDK and Chrome or Firefox. Run from the repository root:

```powershell
dotnet test AutomationFramework/AutomationFramework.csproj
```

Chrome runs headless by default. For a visible Firefox run with reports in the repository:

```powershell
$env:BROWSER = "firefox"
$env:HEADLESS = "false"
$env:REPORTS_DIR = Join-Path $PWD "AutomationFramework/Reports"
dotnet test AutomationFramework/AutomationFramework.csproj --logger "trx;LogFileName=selenium.trx" --results-directory $env:REPORTS_DIR
```

The first restore requires NuGet access. Selenium Manager can download a matching driver when one is unavailable locally.

## Extend the suite

Add a scenario in `Features`, bind its steps in `StepDefinitions`, and keep selectors and browser actions in `Pages`. `Hooks/TestHooks.cs` owns scenario setup, failure screenshots and cleanup. `TestSupport/DemoApp.cs` implements the demo application's UI and API. To target your own application, adapt the hook's base URL, page objects, API paths and test data to its contract.

## GitHub Actions

Selenium CI runs on pull requests, pushes to main and manual dispatch. Chrome and Firefox run independently. Each job uploads its TRX result and any failure screenshots as a browser-specific artifact in the Actions run.
