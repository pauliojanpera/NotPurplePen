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
# is the packaging tool's business and changes with it.
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

[ordered]@{ entries = @($surviving | Select-Object tag, url, file, channel, slug, platform, version, offered, sha256) } |
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
function Rows($entries) {
    $entries | Sort-Object @{ Expression = { [version]$_.version }; Descending = $true },
                           @{ Expression = 'platform' }, @{ Expression = 'file' } | ForEach-Object {
        "<li><a href=""$($_.url)"">$($_.file)</a> &mdash; $($_.platform)</li>"
    }
}

$body = @()
$body += Section 'Releases' 'Offered to installed copies as updates.' (Rows ($surviving | Where-Object { $_.channel -eq 'main' }))
$body += Section 'Prereleases' 'Offered only to copies that are themselves prereleases.' (Rows ($surviving | Where-Object { $_.channel -eq 'beta' }))
$body += Section 'Upstream with this fork&rsquo;s build' 'Purple Pen as this fork last took it from upstream, built and named so it can be installed beside it. Never offered as an update.' (Rows ($surviving | Where-Object { $_.channel -eq 'base' }))
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
