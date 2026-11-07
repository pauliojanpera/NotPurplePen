# assemble-site.ps1
#
# Builds the published site whole, from the catalog gh-pages already holds
# (published/catalog.json) plus what this run packaged (packages/), into
# site/.
#
# The packages themselves are release assets and are not part of the site: a
# published Pages site may hold no more than a gigabyte, and one version of
# this program across five platforms is a quarter of that. What the site
# carries is the index, the catalog and the manifest, which name the assets
# by URL.
#
# Called from build.yml's publish job, once per attempt: a run that loses
# the race to push re-reads gh-pages and calls this again, so it must be
# able to run repeatedly in the same workspace.

$repository = "$env:GITHUB_REPOSITORY"
$kept = @{ main = 3; beta = 3; base = 3; trial = 4 }

# Where a release serves the asset it was given. The tag comes from the
# descriptor, so the rule that picks it is written once, where the packages
# are collected.
function AssetUrl($tag, $file) {
    "https://github.com/$repository/releases/download/$tag/$file"
}

# catalog.json is the site's own record of what it holds. Names and
# directories are not parsed to work it out, because a package's name
# is the packaging tool's business and changes with it. Each entry carries
# the commit its build was made from and the commit's message; an entry
# that came without them is given them below, from the run that built it.
$catalog = @()
if (Test-Path published/catalog.json) {
    $catalog = @((Get-Content published/catalog.json -Raw | ConvertFrom-Json).entries)
}

# An entry written before the packages moved into releases names a file on
# the site, which is not there to link to any more.
$catalog = @($catalog | Where-Object { $_.PSObject.Properties['url'] -and $_.url })

$added = @()
foreach ($descriptorFile in (Get-ChildItem packages -Recurse -Filter descriptor.json)) {
    $descriptor = Get-Content $descriptorFile.FullName -Raw | ConvertFrom-Json
    foreach ($entry in $descriptor.files) {
        $added += [pscustomobject]@{
            tag = $descriptor.tag; url = (AssetUrl $descriptor.tag $entry.file)
            file = $entry.file
            channel = $descriptor.channel; slug = $descriptor.slug
            platform = $entry.platform; version = $entry.version
            offered = $entry.offered; sha256 = $entry.sha256
            commit = $descriptor.commit; message = $descriptor.message
        }
    }
}

# A build replaces what the same channel, slug and platform published
# before; the rest of the catalog is carried forward.
$replaced = $added | ForEach-Object { "$($_.channel)|$($_.slug)|$($_.platform)|$($_.version)" }
$catalog = @($catalog | Where-Object {
    "$($_.channel)|$($_.slug)|$($_.platform)|$($_.version)" -notin $replaced
})
$catalog += $added

# Keep the few most recent of each channel, counting by version rather
# than by file, so a version's platforms stay together. A trial is not
# packaged for every platform at once -- each run packages the ones its
# branch names -- so it keeps the newest build of each platform instead,
# and the platform in each file's name tells them apart.
$surviving = @()
foreach ($group in ($catalog | Group-Object { if ($_.channel -eq 'trial') { "trial/$($_.slug)/$($_.platform)" } else { $_.channel } })) {
    $limit = if ($group.Name -like 'trial/*') { 1 } else { $kept[$group.Name] }
    if (-not $limit) { $limit = 3 }
    $versions = @($group.Group | Sort-Object { [version]$_.version } -Descending |
                  ForEach-Object { $_.version } | Select-Object -Unique | Select-Object -First $limit)
    $surviving += $group.Group | Where-Object { $_.version -in $versions }
}
# And only the newest few trials, whole.
$trialSlugs = @($surviving | Where-Object { $_.channel -eq 'trial' } |
                Sort-Object { [version]$_.version } -Descending |
                ForEach-Object { $_.slug } | Select-Object -Unique | Select-Object -First $kept['trial'])
$surviving = @($surviving | Where-Object { $_.channel -ne 'trial' -or $_.slug -in $trialSlugs })

# Whether a Build run is one that publishes to an entry's channel: a push to
# basewithfixes for base, to master or dev for main and beta, and for a trial
# a push to a wip branch or a pull request from a branch that the version
# step names as the trial is named.
function IsPublishedBy($entry, $run) {
    switch ($entry.channel) {
        'base'  { return $run.event -eq 'push' -and $run.branch -eq 'basewithfixes' }
        'trial' {
            $slug = ((($run.branch -split '/')[-1]) -replace '[^A-Za-z0-9._-]', '-').Trim('-').ToLowerInvariant()
            return ($run.event -eq 'pull_request' -or ($run.event -eq 'push' -and $run.branch -like 'wip/*')) -and
                   $slug -eq $entry.slug
        }
        default { return $run.event -eq 'push' -and $run.branch -in @('master', 'dev') }
    }
}

# Gives each entry without a commit, published before builds recorded their
# commits or by a branch whose copy of this script dropped them, the commit
# of the Build run that built it, with its message. The version step adds
# the run's number to the third component of the version, so the run is the
# one whose number is that component less $runNumberOffset. It is taken only
# when it publishes to the entry's channel, and its head is then the commit
# built: the one pushed, or a pull request's head. A release's tag is no
# guide, as it points at whatever the default branch held when the release
# was made. An entry whose run is gone, or was a dispatched ref's, which the
# run does not name, stays without a commit.
function AddMissingCommits($entries, [int] $runNumberOffset) {
    $lacking = @($entries | Where-Object { -not $_.commit })
    if (-not $lacking) { return }
    $oldest = ($lacking | ForEach-Object { ([version]$_.version).Build - $runNumberOffset } | Measure-Object -Minimum).Minimum

    # The workflow's runs come newest first, a hundred to a page.
    $workflow = (($env:GITHUB_WORKFLOW_REF -split '@')[0] -split '/')[-1]
    $runs = @{}
    for ($page = 1; $page -le 10; $page++) {
        $lines = @(& gh api "repos/$repository/actions/workflows/$workflow/runs?per_page=100&page=$page" `
                        --jq '.workflow_runs[] | {number: .run_number, event, branch: .head_branch, sha: .head_sha} | tojson')
        if ($LASTEXITCODE -ne 0) { "::warning::The runs of $workflow could not be read, so no build is given its commit."; return }
        $read = @($lines | ForEach-Object { $_ | ConvertFrom-Json })
        foreach ($run in $read) { $runs[[int]$run.number] = $run }
        if (-not $read -or ($read.number | Measure-Object -Minimum).Minimum -le $oldest) { break }
    }

    $messages = @{}
    foreach ($entry in $lacking) {
        $run = $runs[([version]$entry.version).Build - $runNumberOffset]
        if (-not $run -or -not (IsPublishedBy $entry $run)) { continue }
        if (-not $messages.ContainsKey($run.sha)) {
            $message = @(& gh api "repos/$repository/commits/$($run.sha)" --jq .commit.message) -join "`n"
            $messages[$run.sha] = if ($LASTEXITCODE -eq 0) { $message.TrimEnd() } else { '' }
        }
        $entry | Add-Member commit $run.sha -Force
        $entry | Add-Member message $messages[$run.sha] -Force
    }
}

# This run's own packages show what the version step adds to a run's number.
if ($added) {
    AddMissingCommits $surviving (([version]$added[0].version).Build - [int]$env:GITHUB_RUN_NUMBER)
}

New-Item site -ItemType Directory -Force | Out-Null

[ordered]@{ entries = @($surviving | Select-Object tag, url, file, channel, slug, platform, version, offered, sha256, commit, message) } |
    ConvertTo-Json -Depth 5 | Set-Content site/catalog.json -Encoding UTF8

# The manifest is the updater's view: the newest build of each platform
# on each channel it will ever ask for. Schema per
# PurplePenCore/CoreUpdater.cs -- a single "updates" array, read
# case-insensitively, with platform names as the application reports
# them. Nothing installed asks for "base" or "trial", so they are not
# in it.
$updates = @()
foreach ($entry in ($surviving | Where-Object { $_.offered -and $_.channel -in @('main', 'beta') } |
                    Group-Object channel, platform)) {
    $newest = $entry.Group | Sort-Object { [version]$_.version } -Descending | Select-Object -First 1
    $updates += [ordered]@{
        title = "NotPurplePen $($newest.version)"; version = $newest.version
        platform = $newest.platform; channel = $newest.channel
        url = $newest.url; sha256 = $newest.sha256; message = ""
    }
}
[ordered]@{ updates = @($updates) } | ConvertTo-Json -Depth 5 |
    Set-Content site/manifest.json -Encoding UTF8

function Section($title, $note, $rows) {
    "<h2>$title</h2>" + $(if ($note) { "<p>$note</p>" }) +
    $(if ($rows) { "<ul>" + ($rows -join "`n") + "</ul>" } else { "<p>None yet.</p>" })
}
# Text escaped for the page.
function Escape([string] $text) {
    [System.Net.WebUtility]::HtmlEncode($text)
}
# A section's rows, one for each build: the files one run published on the
# section's channel, newest build first, which are those of one version and,
# for a trial, of one trial. Each row names the build and lists its files,
# and then gives the commit it was made from, when the catalog records one,
# as a <details> whose summary is the commit's hash and whose body its whole
# message.
function Rows($entries) {
    $entries | Group-Object { "$($_.slug)|$($_.version)" } |
        Sort-Object @{ Expression = { [version]$_.Group[0].version }; Descending = $true },
                    @{ Expression = { $_.Group[0].slug } } | ForEach-Object {
        $build = $_.Group[0]
        $name = if ($build.slug) { "$(Escape $build.slug), $($build.version)" } else { $build.version }
        $files = $_.Group | Sort-Object platform, file | ForEach-Object {
            "<li><a href=""$($_.url)"">$($_.file)</a> &mdash; $($_.platform)</li>"
        }
        $commit = $_.Group | Where-Object { $_.commit } | Select-Object -First 1
        $details = if ($commit) {
            "<details><summary>Commit <code>$(Escape $commit.commit)</code></summary>" +
            "<pre style=""white-space: pre-wrap"">$(Escape $commit.message)</pre></details>"
        }
        "<li>$name<ul>$($files -join "`n")</ul>$details</li>"
    }
}

$body = @()
$body += Section 'Releases' 'Offered to installed copies as updates.' (Rows ($surviving | Where-Object { $_.channel -eq 'main' }))
$body += Section 'Prereleases' 'Offered only to copies that are themselves prereleases.' (Rows ($surviving | Where-Object { $_.channel -eq 'beta' }))
$body += Section 'Upstream with this fork&rsquo;s build and bug fixes' 'Purple Pen as this fork last took it from upstream, with the bug fixes this fork offers it, built and named so it can be installed beside it. Never offered as an update.' (Rows ($surviving | Where-Object { $_.channel -eq 'base' }))
$body += Section 'Trial builds' 'A branch as it stands, packaged so it can be installed and tried. Never offered as an update.' (Rows ($surviving | Where-Object { $_.channel -eq 'trial' }))

$timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss UTC'
@"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>NotPurplePen downloads</title>
</head>
<body>
    <h1>NotPurplePen</h1>
    <p>A fork of <a href="https://purple-pen.org">Purple Pen</a>, built for Windows, Linux and macOS.
       Updated $timestamp.</p>
    $($body -join "`n    ")
    <h2>Notes</h2>
    <ul>
        <li>The downloads are served from this repository's releases, not from this site.</li>
        <li>The .deb and .rpm packages are installed and removed by the system package manager and
            are not offered as updates from inside the application.</li>
        <li>The macOS disk image is not signed, so the first launch needs the usual confirmation.</li>
    </ul>
</body>
</html>
"@ | Set-Content site/index.html -Encoding UTF8

"published:"
Get-ChildItem site -Recurse -File | ForEach-Object { "  $($_.FullName.Substring((Resolve-Path 'site').Path.Length + 1))" }
"catalogued:"
$surviving | Sort-Object tag, file | ForEach-Object { "  $($_.tag)/$($_.file)" }
