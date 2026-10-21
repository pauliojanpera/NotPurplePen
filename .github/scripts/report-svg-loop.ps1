# report-svg-loop.ps1
#
# Turns what one run of the SVG loop produced into the verdict a Claude session
# reads: the summary files of the results branch, the job summary, and the
# check run "SVG loop report", whose title carries the counts and whose
# annotations are the failures. A session reaches git and the GitHub API but
# not this run's log or artifacts, so what it needs must arrive by those two.
#
# Called from svg.yml after the tests, and after a failed build too, when there
# is no test report and the build's errors are the verdict.
#
# Environment:
#   RESULTS         the results directory, which the tests write into as well
#   RESULTS_BRANCH  the branch the results are published to
#   HEAD_SHA        the commit the check run is attached to
#   BUILD_OUTCOME   the outcome of the build step
#   TEST_EXIT       the exit code of the test run, empty if it did not run
#   GH_TOKEN        a token that may create check runs and read this run's jobs

$ErrorActionPreference = 'Stop'
$results = $env:RESULTS
$workspace = $env:GITHUB_WORKSPACE.TrimEnd('/') + '/'
$runUrl = "$env:GITHUB_SERVER_URL/$env:GITHUB_REPOSITORY/actions/runs/$env:GITHUB_RUN_ID"

# The repository path and line of the first source location in a stack trace
# or a compiler message that lies in this repository; $null if none does.
#
# Parameters:
#   text - the stack trace or message.
function Find-SourceLocation([string] $text) {
    foreach ($match in [regex]::Matches($text, '(?<path>/[^\s:(]+\.cs)(?::line |\()(?<line>\d+)')) {
        $path = $match.Groups['path'].Value
        if ($path.StartsWith($workspace)) {
            return [pscustomobject]@{ Path = $path.Substring($workspace.Length); Line = [int]$match.Groups['line'].Value }
        }
    }
    return $null
}

# A text cut to at most the given number of characters, which the check run
# API allows a field.
#
# Parameters:
#   text - the text.
#   length - the most characters to keep.
function Limit-Text([string] $text, [int] $length) {
    if ($text.Length -le $length) { return $text }
    return $text.Substring(0, $length - 1) + [char]0x2026
}

# The steps of this job so far, and how long each took.
$stepRows = @(& gh api "repos/$env:GITHUB_REPOSITORY/actions/runs/$env:GITHUB_RUN_ID/jobs" `
                  --jq '.jobs[] | select(.name == "SVG loop") | .steps[] | [.name, (.conclusion // .status), (.started_at // ""), (.completed_at // "")] | @tsv')
$steps = @(foreach ($row in $stepRows) {
    $field = $row -split "`t"
    $seconds = if ($field[2] -and $field[3]) { [int]([datetimeoffset]$field[3] - [datetimeoffset]$field[2]).TotalSeconds } else { '' }
    [pscustomobject]@{ Name = $field[0]; Outcome = $field[1]; Seconds = $seconds }
})

# The tests, from the test report.
$tests = @()
$reportPath = Join-Path $results 'test-report.trx'
if (Test-Path $reportPath) {
    [xml] $report = Get-Content $reportPath -Raw
    $tests = @(foreach ($result in $report.TestRun.Results.UnitTestResult) {
        [pscustomobject]@{
            Name = $result.testName
            Outcome = $result.outcome
            Message = [string]$result.Output.ErrorInfo.Message
            StackTrace = [string]$result.Output.ErrorInfo.StackTrace
        }
    })
}
$passed = @($tests | Where-Object { $_.Outcome -eq 'Passed' })
$failed = @($tests | Where-Object { $_.Outcome -eq 'Failed' })
$notRun = @($tests | Where-Object { $_.Outcome -ne 'Passed' -and $_.Outcome -ne 'Failed' })

# The pending cases, each with the calls it still needs, as the tests saved them.
$pendingDirectory = Join-Path $results 'pending'
$pending = @(if (Test-Path $pendingDirectory) {
    foreach ($file in (Get-ChildItem $pendingDirectory -Filter '*.txt' | Sort-Object Name)) {
        [pscustomobject]@{ Case = $file.BaseName; Calls = @(Get-Content $file.FullName | Where-Object { $_ }) }
    }
})

# For each call that pending cases need, how many need it, and how many need
# nothing else: the cases its implementation would bring to comparison.
$byCall = @($pending | ForEach-Object { $_.Calls } | Group-Object | ForEach-Object {
    $call = $_.Name
    [pscustomobject]@{
        Call = $call
        Cases = $_.Count
        Alone = @($pending | Where-Object { $_.Calls.Count -eq 1 -and $_.Calls[0] -eq $call }).Count
    }
} | Sort-Object -Property @{ Expression = 'Cases'; Descending = $true }, @{ Expression = 'Call'; Descending = $false })

# The build's errors, when there was no build to test.
$buildFailed = $env:BUILD_OUTCOME -ne 'success'
$buildErrors = @()
$buildLog = Join-Path $results 'build.log'
if ($buildFailed -and (Test-Path $buildLog)) {
    $buildErrors = @(Get-Content $buildLog | Where-Object { $_ -match ': error ' } | ForEach-Object { $_.Trim().Replace($workspace, '') } | Select-Object -Unique)
}

# The verdict.
$testExit = $env:TEST_EXIT
$counts = "$($passed.Count) passed, $($failed.Count) failed, $($pending.Count) pending"
if ($buildFailed) {
    $title = "The build failed with $($buildErrors.Count) errors"
    $conclusion = 'failure'
}
elseif (-not $testExit) {
    $title = 'The tests did not run'
    $conclusion = 'failure'
}
elseif ($testExit -ne '0' -and $failed.Count -eq 0) {
    $title = "$counts; the test run ended with exit code $testExit"
    $conclusion = 'failure'
}
else {
    $title = $counts
    $conclusion = if ($failed.Count) { 'failure' } else { 'success' }
}

$summary = New-Object System.Collections.Generic.List[string]
$summary.Add("SVG loop: $title")
$summary.Add('')
$summary.Add("Commit $env:HEAD_SHA, run $runUrl. Results are on $env:RESULTS_BRANCH.")
if ($buildErrors.Count) {
    $summary.Add('')
    $summary.Add('## Build errors')
    $summary.Add('')
    foreach ($line in ($buildErrors | Select-Object -First 40)) { $summary.Add("    $line") }
}
if ($failed.Count) {
    $summary.Add('')
    $summary.Add("## Failures ($($failed.Count); failures.md has each in full)")
    $summary.Add('')
    foreach ($test in ($failed | Select-Object -First 20)) { $summary.Add("- **$($test.Name)**: $(($test.Message -split "`n")[0])") }
}
if ($byCall.Count) {
    $summary.Add('')
    $summary.Add("## Pending cases by the calls they need ($($pending.Count) cases; pending.tsv lists each)")
    $summary.Add('')
    $summary.Add('| Call | Cases that need it | Cases that need only it |')
    $summary.Add('| --- | ---: | ---: |')
    foreach ($row in $byCall) { $summary.Add("| $($row.Call) | $($row.Cases) | $($row.Alone) |") }
}
if ($notRun.Count -ne $pending.Count) {
    $summary.Add('')
    $summary.Add("$($notRun.Count) tests did not run, $($pending.Count) of them as pending cases.")
}
if ($steps.Count) {
    $summary.Add('')
    $summary.Add('## Steps')
    $summary.Add('')
    $summary.Add('| Step | Outcome | Seconds |')
    $summary.Add('| --- | --- | ---: |')
    foreach ($step in $steps) { $summary.Add("| $($step.Name) | $($step.Outcome) | $($step.Seconds) |") }
}
$summaryText = $summary -join "`n"

Set-Content (Join-Path $results 'summary.md') $summaryText -Encoding utf8NoBOM
Set-Content (Join-Path $results 'pending.tsv') (@("case`tcalls") + @($pending | ForEach-Object { "$($_.Case)`t$($_.Calls -join ', ')" })) -Encoding utf8NoBOM
Set-Content (Join-Path $results 'steps.tsv') (@("step`toutcome`tseconds") + @($steps | ForEach-Object { "$($_.Name)`t$($_.Outcome)`t$($_.Seconds)" })) -Encoding utf8NoBOM
$fence = '```'
Set-Content (Join-Path $results 'failures.md') (@($failed | ForEach-Object {
    "## $($_.Name)`n`n$($_.Message)`n`n$fence`n$($_.StackTrace.Replace($workspace, ''))`n$fence`n"
}) -join "`n") -Encoding utf8NoBOM
Add-Content $env:GITHUB_STEP_SUMMARY $summaryText

# Each build error that names a source line, and each failed test, as an
# annotation of the check run. A failure whose stack trace leaves this
# repository is placed on the workflow, which an annotation needs some file of.
$annotations = New-Object System.Collections.Generic.List[object]
foreach ($line in $buildErrors) {
    $location = Find-SourceLocation ($workspace + $line)
    if ($location) {
        $annotations.Add([ordered]@{
            path = $location.Path; start_line = $location.Line; end_line = $location.Line
            annotation_level = 'failure'; title = 'Build error'; message = Limit-Text $line 60000
        })
    }
}
foreach ($test in $failed) {
    $location = Find-SourceLocation $test.StackTrace
    if (-not $location) { $location = [pscustomobject]@{ Path = '.github/workflows/svg.yml'; Line = 1 } }
    $message = if ($test.Message) { $test.Message } else { 'failed' }
    $annotations.Add([ordered]@{
        path = $location.Path; start_line = $location.Line; end_line = $location.Line
        annotation_level = 'failure'; title = $test.Name; message = Limit-Text $message 60000
    })
}

$output = [ordered]@{
    title = $title
    summary = Limit-Text $summaryText 65000
    annotations = @($annotations | Select-Object -First 50)
}
if ($pending.Count) {
    $output.text = Limit-Text ((@($pending | ForEach-Object { "$($_.Case): $($_.Calls -join ', ')" })) -join "`n") 65000
}
$payload = [ordered]@{
    name = 'SVG loop report'
    head_sha = $env:HEAD_SHA
    status = 'completed'
    conclusion = $conclusion
    details_url = $runUrl
    output = $output
}
$payloadPath = Join-Path ([IO.Path]::GetTempPath()) 'svg-loop-report.json'
$payload | ConvertTo-Json -Depth 8 | Set-Content $payloadPath -Encoding utf8NoBOM
& gh api "repos/$env:GITHUB_REPOSITORY/check-runs" --method POST --input $payloadPath | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'The check run could not be created.' }
"SVG loop: $title"
