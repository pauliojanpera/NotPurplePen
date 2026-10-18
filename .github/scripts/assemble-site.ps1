# assemble-site.ps1
#
# Builds the published site whole, from what gh-pages already holds
# (published/) plus what this run packaged (packages/), into site/.
#
# Called from build.yml's publish job, once per attempt: a run that loses
# the race to push re-clones gh-pages and calls this again, so it must be
# able to run repeatedly in the same workspace.

$pagesUrl = "$env:GithubPagesUrl"
$kept = @{ main = 3; beta = 3; base = 3; trial = 4 }

# catalog.json is the site's own record of what it holds. Names and
# directories are not parsed to work it out, because a package's name
# is the packaging tool's business and changes with it.
$catalog = @()
if (Test-Path published/catalog.json) {
    $catalog = @((Get-Content published/catalog.json -Raw | ConvertFrom-Json).entries)
}

$added = @()
foreach ($descriptorFile in (Get-ChildItem packages -Recurse -Filter descriptor.json)) {
    $descriptor = Get-Content $descriptorFile.FullName -Raw | ConvertFrom-Json
    foreach ($entry in $descriptor.files) {
        $directory = if ($descriptor.channel -eq 'trial') { "trial/$($descriptor.slug)" }
                     else { "downloads/$($descriptor.channel)" }
        $added += [pscustomobject]@{
            path = "$directory/$($entry.file)"; file = $entry.file
            channel = $descriptor.channel; slug = $descriptor.slug
            platform = $entry.platform; version = $entry.version
            offered = $entry.offered; sha256 = $entry.sha256
            source = (Join-Path $descriptorFile.DirectoryName $entry.file)
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
# than by file, so a version's platforms stay together.
$surviving = @()
foreach ($group in ($catalog | Group-Object { if ($_.channel -eq 'trial') { "trial/$($_.slug)" } else { $_.channel } })) {
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

New-Item site -ItemType Directory -Force | Out-Null
foreach ($entry in $surviving) {
    $target = Join-Path 'site' $entry.path
    New-Item (Split-Path $target) -ItemType Directory -Force | Out-Null
    $source = if ($entry.PSObject.Properties['source'] -and $entry.source) { $entry.source }
              else { Join-Path 'published' $entry.path }
    if (-not (Test-Path $source)) { continue }
    Copy-Item $source $target -Force
}
$surviving = @($surviving | Where-Object { Test-Path (Join-Path 'site' $_.path) })

[ordered]@{ entries = @($surviving | Select-Object path, file, channel, slug, platform, version, offered, sha256) } |
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
        url = $pagesUrl + $newest.path; sha256 = $newest.sha256; message = ""
    }
}
[ordered]@{ updates = @($updates) } | ConvertTo-Json -Depth 5 |
    Set-Content site/manifest.json -Encoding UTF8

function Section($title, $note, $rows) {
    "<h2>$title</h2>" + $(if ($note) { "<p>$note</p>" }) +
    $(if ($rows) { "<ul>" + ($rows -join "`n") + "</ul>" } else { "<p>None yet.</p>" })
}
function Rows($entries) {
    $entries | Sort-Object @{ Expression = { [version]$_.version }; Descending = $true },
                           @{ Expression = 'platform' }, @{ Expression = 'file' } | ForEach-Object {
        "<li><a href=""$($_.path)"">$($_.file)</a> &mdash; $($_.platform)</li>"
    }
}

$body = @()
$body += Section 'Releases' 'Offered to installed copies as updates.' (Rows ($surviving | Where-Object { $_.channel -eq 'main' }))
$body += Section 'Prereleases' 'Offered only to copies that are themselves prereleases.' (Rows ($surviving | Where-Object { $_.channel -eq 'beta' }))
$body += Section 'Upstream with this fork&rsquo;s build' 'Purple Pen as it stands upstream, built and named so it can be installed beside it. Never offered as an update.' (Rows ($surviving | Where-Object { $_.channel -eq 'base' }))
$body += Section 'Trial builds' 'Built from a branch against the current upstream. Installed from here, never offered as an update.' (Rows ($surviving | Where-Object { $_.channel -eq 'trial' }))

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
        <li>The .deb and .rpm packages are installed and removed by the system package manager and
            are not offered as updates from inside the application.</li>
        <li>The macOS disk image is not signed, so the first launch needs the usual confirmation.</li>
    </ul>
</body>
</html>
"@ | Set-Content site/index.html -Encoding UTF8

"published:"
Get-ChildItem site -Recurse -File | ForEach-Object { "  $($_.FullName.Substring((Resolve-Path 'site').Path.Length + 1))" }
