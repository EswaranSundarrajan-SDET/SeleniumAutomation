$reportsDirectory = $env:REPORTS_DIR
if ([string]::IsNullOrWhiteSpace($reportsDirectory)) {
    $reportsDirectory = Join-Path -Path $PSScriptRoot -ChildPath 'AutomationTests/Reports'
}

$reportPath = Join-Path -Path $reportsDirectory -ChildPath 'Automation Execution Results.html'
if (-not (Test-Path -LiteralPath $reportPath -PathType Leaf)) {
    throw "Report not found at '$reportPath'. Run the automation tests to generate it."
}

Invoke-Item -LiteralPath $reportPath -ErrorAction Stop
