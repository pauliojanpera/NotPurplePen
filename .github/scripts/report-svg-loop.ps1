# report-svg-loop.ps1
#
# Turns what one run of the SVG loop produced into the verdict a Claude session
# reads: the summary files of the results branch, the job summary, and the
# check run "SVG loop report", whose title carries the counts and whose
# annotations are the failures. A session reaches git and the GitHub API but
# not this run's log or artifacts, so what it needs must arrive by those two.
#
# The summary also says what changed since the results the branch held before
# this run: tests that changed their outcome, and the drawings whose SVG or
# measurements changed. changes.md lists each change.
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
#   GH_TOKEN        a token that may create check runs, read this run's jobs
#                   and clone the results branch

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

# The tests of a test report, each with its name, outcome, message and stack
# trace; none when there is no report.
#
# Parameters:
#   path - the path of the .trx file.
function Read-TestReport([string] $path) {
    if (-not (Test-Path $path)) { return @() }
    [xml] $report = Get-Content $path -Raw
    return @(foreach ($result in $report.TestRun.Results.UnitTestResult) {
        [pscustomobject]@{
            Name = $result.testName
            Outcome = $result.outcome
            Message = [string]$result.Output.ErrorInfo.Message
            StackTrace = [string]$result.Output.ErrorInfo.StackTrace
        }
    })
}

# The drawings of a results directory: for each case, its measurements by
# column, and the size and SHA-256 hash of its SVG, either of which may be
# missing.
#
# Parameters:
#   directory - the results directory.
function Read-Drawings([string] $directory) {
    $drawings = New-Object 'System.Collections.Generic.Dictionary[string,object]' ([StringComparer]::Ordinal)
    $measurementDirectory = Join-Path $directory 'measurements'
    if (Test-Path $measurementDirectory) {
        foreach ($file in (Get-ChildItem $measurementDirectory -Filter '*.tsv')) {
            $lines = @(Get-Content $file.FullName)
            if ($lines.Count -lt 2) { continue }
            $values = [ordered]@{}
            $names = $lines[0] -split "`t"
            $fields = $lines[1] -split "`t"
            for ($i = 0; $i -lt [Math]::Min($names.Count, $fields.Count); $i++) { $values[$names[$i]] = $fields[$i] }
            $drawings[$file.BaseName] = [pscustomobject]@{ Measurements = $values; Bytes = $null; Hash = $null }
        }
    }
    $svgDirectory = Join-Path $directory 'svg'
    if (Test-Path $svgDirectory) {
        foreach ($file in (Get-ChildItem $svgDirectory -Filter '*.svg')) {
            if (-not $drawings.ContainsKey($file.BaseName)) {
                $drawings[$file.BaseName] = [pscustomobject]@{ Measurements = $null; Bytes = $null; Hash = $null }
            }
            $drawings[$file.BaseName].Bytes = $file.Length
            $drawings[$file.BaseName].Hash = (Get-FileHash $file.FullName -Algorithm SHA256).Hash
        }
    }
    return $drawings
}

# The results the branch held before this run, cloned into a directory of
# their own: the subject of their commit, which names the run and commit they
# came from, their tests and their drawings. $null when the branch does not
# exist yet or cannot be read.
function Read-PreviousResults {
    $directory = Join-Path ([IO.Path]::GetTempPath()) 'svg-loop-previous'
    if (Test-Path $directory) { Remove-Item $directory -Recurse -Force }
    $url = "https://x-access-token:$($env:GH_TOKEN)@github.com/$($env:GITHUB_REPOSITORY).git"
    & git clone --quiet --depth 1 --single-branch --branch $env:RESULTS_BRANCH $url $directory 2>$null
    if ($LASTEXITCODE -ne 0) { return $null }
    return [pscustomobject]@{
        Subject = (& git -C $directory log -1 --format=%s)
        Tests = Read-TestReport (Join-Path $directory 'test-report.trx')
        Drawings = Read-Drawings $directory
    }
}

# A number of bytes with thousands separated, as the summary writes sizes.
#
# Parameters:
#   bytes - the number of bytes.
function Format-Bytes([long] $bytes) {
    return $bytes.ToString('N0', [Globalization.CultureInfo]::InvariantCulture)
}

# The changes from the previous results to this run's, one row for each test
# or drawing that changed: its name and what changed, the change of a test's
# outcome first, then of the drawing's SVG and measurements. A comparison case
# is named by its drawing, as MapArea(irregular1) is irregular1.
#
# Parameters:
#   previous - the previous results, from Read-PreviousResults.
#   tests - this run's tests.
#   drawings - this run's drawings, from Read-Drawings.
function Compare-Results($previous, $tests, $drawings) {
    $rows = New-Object System.Collections.Specialized.OrderedDictionary ([StringComparer]::Ordinal)
    $addChange = {
        param([string] $name, [string] $change)
        if (-not $rows.Contains($name)) { $rows[$name] = New-Object System.Collections.Generic.List[string] }
        $rows[$name].Add($change)
    }
    $nameOf = { param([string] $testName) if ($testName -match '^\w+\((.+)\)$') { $Matches[1] } else { $testName } }

    $before = New-Object 'System.Collections.Generic.Dictionary[string,string]' ([StringComparer]::Ordinal)
    foreach ($test in $previous.Tests) { $before[$test.Name] = $test.Outcome }
    $now = New-Object 'System.Collections.Generic.Dictionary[string,string]' ([StringComparer]::Ordinal)
    foreach ($test in $tests) { $now[$test.Name] = $test.Outcome }
    foreach ($test in $tests) {
        if (-not $before.ContainsKey($test.Name)) { & $addChange (& $nameOf $test.Name) "new test, $($test.Outcome)" }
        elseif ($before[$test.Name] -ne $test.Outcome) { & $addChange (& $nameOf $test.Name) "$($before[$test.Name]) -> $($test.Outcome)" }
    }
    foreach ($test in $previous.Tests) {
        if (-not $now.ContainsKey($test.Name)) { & $addChange (& $nameOf $test.Name) "test gone, was $($test.Outcome)" }
    }

    foreach ($name in (@($drawings.Keys) + @($previous.Drawings.Keys) | Sort-Object -Unique -CaseSensitive)) {
        $old = $previous.Drawings[$name]
        $new = $drawings[$name]
        $oldBytes = if ($old) { $old.Bytes } else { $null }
        $newBytes = if ($new) { $new.Bytes } else { $null }
        if ($null -eq $oldBytes -and $null -ne $newBytes) { & $addChange $name "new SVG, $(Format-Bytes $newBytes) bytes" }
        elseif ($null -ne $oldBytes -and $null -eq $newBytes) { & $addChange $name "SVG gone, was $(Format-Bytes $oldBytes) bytes" }
        elseif ($null -ne $oldBytes -and $old.Hash -ne $new.Hash) {
            & $addChange $name $(if ($oldBytes -eq $newBytes) { "SVG changed, $(Format-Bytes $newBytes) bytes as before" } else { "SVG $(Format-Bytes $oldBytes) -> $(Format-Bytes $newBytes) bytes" })
        }

        $oldValues = if ($old) { $old.Measurements } else { $null }
        $newValues = if ($new) { $new.Measurements } else { $null }
        if ($null -eq $oldValues -and $null -ne $newValues) { & $addChange $name 'new measurements' }
        elseif ($null -ne $oldValues -and $null -eq $newValues) { & $addChange $name 'measurements gone' }
        elseif ($null -ne $oldValues) {
            $changed = @(foreach ($column in (@($newValues.Keys) + @($oldValues.Keys) | Select-Object -Unique)) {
                if ($oldValues[$column] -ne $newValues[$column]) { "$column $($oldValues[$column]) -> $($newValues[$column])" }
            })
            if ($changed.Count) { & $addChange $name ($changed -join ', ') }
        }
    }
    return @(foreach ($name in $rows.Keys) { [pscustomobject]@{ Name = $name; Change = ($rows[$name] -join '; ') } })
}

# The steps of this job that have finished, and how long each took.
$stepRows = @(& gh api "repos/$env:GITHUB_REPOSITORY/actions/runs/$env:GITHUB_RUN_ID/jobs" `
                  --jq '.jobs[] | select(.name == "SVG loop") | .steps[] | select(.completed_at != null) | [.name, .conclusion, .started_at, .completed_at] | @tsv')
$steps = @(foreach ($row in $stepRows) {
    $field = $row -split "`t"
    [pscustomobject]@{ Name = $field[0]; Outcome = $field[1]; Seconds = [int]([datetimeoffset]$field[3] - [datetimeoffset]$field[2]).TotalSeconds }
})

# The tests, from the test report.
$tests = Read-TestReport (Join-Path $results 'test-report.trx')
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

# What changed since the results the branch held before this run, once the
# tests have run: without them there is nothing of this run to compare.
$previous = $null
$changes = @()
if ($tests.Count) { $previous = Read-PreviousResults }
if ($previous) {
    $drawings = Read-Drawings $results
    $changes = Compare-Results $previous $tests $drawings
    $svgBytes = ($drawings.Values | Where-Object { $null -ne $_.Bytes } | Measure-Object -Property Bytes -Sum).Sum
    $previousSvgBytes = ($previous.Drawings.Values | Where-Object { $null -ne $_.Bytes } | Measure-Object -Property Bytes -Sum).Sum
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
if ($tests.Count) {
    $summary.Add('')
    $summary.Add('## Since the previous run')
    $summary.Add('')
}
if ($tests.Count -and -not $previous) {
    $summary.Add("$env:RESULTS_BRANCH held no results before this run.")
}
elseif ($previous) {
    $summary.Add("Compared with the results $env:RESULTS_BRANCH held before: $($previous.Subject). " +
                 "The SVGs come to $(Format-Bytes $svgBytes) bytes, against $(Format-Bytes $previousSvgBytes) before.")
    $summary.Add('')
    if ($changes.Count -eq 0) {
        $summary.Add('No test changed its outcome, and no SVG or measurement changed.')
    }
    else {
        $summary.Add("$($changes.Count) tests or drawings changed$(if ($changes.Count -gt 30) { '; the first 30 are below, and changes.md lists each' } else { '' }).")
        $summary.Add('')
        $summary.Add('| Test or drawing | What changed |')
        $summary.Add('| --- | --- |')
        foreach ($row in ($changes | Select-Object -First 30)) { $summary.Add("| $($row.Name) | $($row.Change) |") }
    }
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
Set-Content (Join-Path $results 'changes.md') $(if ($previous) {
    @("Changes since the results $env:RESULTS_BRANCH held before: $($previous.Subject).", '',
      '| Test or drawing | What changed |', '| --- | --- |') + @($changes | ForEach-Object { "| $($_.Name) | $($_.Change) |" })
} elseif ($tests.Count) { "$env:RESULTS_BRANCH held no results before this run." }
  else { 'The tests did not run, so nothing was compared with the previous results.' }) -Encoding utf8NoBOM
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
