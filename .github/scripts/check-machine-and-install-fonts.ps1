# check-machine-and-install-fonts.ps1
#
# Checks the machine of a test job against .github/ci/environment.json and the
# font store ci-fonts against .github/ci/fonts.json, installs the fonts of the
# store, and waits until DirectWrite gives each face that it installed to a
# program that starts. A difference in the store stops the job. A difference
# in the machine stops the job only when MACHINE_CHECK is 'stop'.
#
# Called from the test job of tests.yml, in the workspace, after the store is
# checked out into ci-fonts. The script is in this file and not in the step,
# because the runner writes the script of a step into the log before it runs
# it: the workflow commands of the script (::error and ::warning) were then
# text in the log of each test job, also when no error occurred.

# As the runner sets it for a step whose shell is pwsh.
$ErrorActionPreference = 'Stop'

"image: $env:ImageOS $env:ImageVersion"
$cv = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion'
$windows = "$($cv.ProductName) $($cv.DisplayVersion)"
"windows: $windows build $($cv.CurrentBuild).$($cv.UBR)"
"architecture: $env:PROCESSOR_ARCHITECTURE"
""

Add-Type -Namespace Native -Name Gdi -MemberDefinition @'
[DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
public static extern int AddFontResource(string fileName);
'@

Add-Type -Namespace Native -Name Sfnt -MemberDefinition @'
static int U16(byte[] d, int o) { return (d[o] << 8) | d[o + 1]; }
static int U32(byte[] d, int o) {
    return (d[o] << 24) | (d[o + 1] << 16) | (d[o + 2] << 8) | d[o + 3];
}

// One line per face in the file: family, style, and the version the font
// calls itself, tab separated. A collection holds several faces.
//
// Several of these faces carry localized names, and a font filed as
// "Arial Negreta" is a font nothing asks for; a Windows record is
// authoritative and the Macintosh one is a fallback, since in some older
// faces it holds mojibake.
public static string[] Faces(string path)
{
    byte[] d = System.IO.File.ReadAllBytes(path);
    int[] starts;
    if (d.Length > 16 && d[0] == 't' && d[1] == 't' && d[2] == 'c' && d[3] == 'f') {
        int count = U32(d, 8);
        starts = new int[count];
        for (int i = 0; i < count; i++) { starts[i] = U32(d, 12 + 4 * i); }
    }
    else {
        starts = new int[] { 0 };
    }

    System.Collections.Generic.List<string> faces =
        new System.Collections.Generic.List<string>();
    foreach (int start in starts) {
        if (start < 0 || start + 12 > d.Length) { continue; }

        int table = -1;
        int tableCount = U16(d, start + 4);
        for (int i = 0; i < tableCount; i++) {
            int o = start + 12 + 16 * i;
            if (o + 16 > d.Length) { break; }
            if (d[o] == 'n' && d[o + 1] == 'a' && d[o + 2] == 'm' && d[o + 3] == 'e')
                table = U32(d, o + 8);
        }
        if (table < 0 || table + 6 > d.Length) { continue; }

        int count = U16(d, table + 2), strings = U16(d, table + 4);
        string[] win = new string[6], mac = new string[6];
        for (int i = 0; i < count; i++) {
            int p = table + 6 + 12 * i;
            if (p + 12 > d.Length) { break; }
            int platform = U16(d, p), language = U16(d, p + 4), nameId = U16(d, p + 6);
            int len = U16(d, p + 8), offset = U16(d, p + 10);
            if (nameId != 1 && nameId != 2 && nameId != 5) { continue; }
            if (platform == 3 && language != 0x0409) { continue; }
            if (platform == 1 && language != 0) { continue; }

            int at = table + strings + offset;
            if (at < 0 || at + len > d.Length) { continue; }
            string s = (platform == 3)
                ? System.Text.Encoding.BigEndianUnicode.GetString(d, at, len)
                : System.Text.Encoding.ASCII.GetString(d, at, len);
            s = s.Replace("\0", "").Trim();
            if (s.Length == 0) { continue; }
            if (platform == 3) { if (win[nameId] == null) { win[nameId] = s; } }
            else { if (mac[nameId] == null) { mac[nameId] = s; } }
        }

        string family = win[1] ?? mac[1];
        if (family == null) { continue; }
        faces.Add(family + "\t" + (win[2] ?? mac[2] ?? "Regular") + "\t" + (win[5] ?? mac[5] ?? ""));
    }
    return faces.ToArray();
}
'@

# Parameters:
#   files - the font files to read.
#   label - where they came from, for the report.
function Read-Faces($files, $label) {
    $out = New-Object System.Collections.Generic.List[object]
    foreach ($file in $files) {
        try { $faces = [Native.Sfnt]::Faces($file.FullName) } catch { continue }
        foreach ($face in $faces) {
            $parts = $face -split "`t"
            $out.Add([pscustomobject]@{
                Source = $label; File = $file.Name; Path = $file.FullName
                Bytes = $file.Length; Sha256 = $null
                Family = $parts[0]; Style = $parts[1]; Version = $parts[2]
            })
        }
    }
    return $out
}

# The name Windows files a font under: the family alone when the style is
# the plain one, the two together otherwise.
function Get-FaceLabel($family, $style) {
    $lower = if ($style) { $style.ToLowerInvariant() } else { 'regular' }
    $plain = @('regular', 'normal', 'plain', 'book', 'roman', 'standard')
    if ($plain -contains $lower) { return $family }
    return "$family $style"
}

# Parameters:
#   differences - what differs, each named with how it differs.
#   heading     - the line that introduces them, naming what they differ from.
function Write-Differences($differences, $heading) {
    if ($differences.Count) {
        $heading
        $differences.GetEnumerator() | ForEach-Object { "  $($_.Key) $($_.Value)" }
        ""
    }
}

$required = Get-Content .github/ci/environment.json -Raw | ConvertFrom-Json
$declared = Get-Content .github/ci/fonts.json -Raw | ConvertFrom-Json
$fontDir = Join-Path $env:WINDIR 'Fonts'
$machineDifferences = [ordered]@{}
$storeDifferences = [ordered]@{}

# The machine first.
if ($windows -ne $required.windows) {
    $machineDifferences['windows'] = "is '$windows', required '$($required.windows)'"
}

# Then the faces the tests take from it, each as this machine has it: a
# file that matches its record is the recorded entry, and one that does
# not is read again.
$machineFonts = New-Object System.Collections.Generic.List[object]
foreach ($one in $required.systemFonts) {
    $path = Join-Path $fontDir $one.file
    if (-not (Test-Path $path)) {
        $machineDifferences[$one.file] = 'is not on this machine'
        continue
    }
    $hash = (Get-FileHash $path -Algorithm SHA256).Hash
    if ($hash -eq $one.sha256) {
        $machineFonts.Add($one)
        continue
    }
    $families = ($one.faces | ForEach-Object { $_.family }) -join ', '
    $machineDifferences[$one.file] = "($families) hashes to $hash, recorded as $($one.sha256)"
    $machineFonts.Add([pscustomobject]@{
        file = $one.file
        sha256 = $hash
        faces = @(Read-Faces (Get-Item $path) 'machine' | ForEach-Object {
            [pscustomobject]@{ family = $_.Family; style = $_.Style; version = $_.Version }
        })
    })
}

# And the store against what it is declared to hold.
$kinds = @('*.ttf', '*.otf', '*.ttc')
$store = Read-Faces (Get-ChildItem ci-fonts -Include $kinds -Recurse) 'store'
$byFile = @{}
foreach ($one in $store) {
    if (-not $byFile.ContainsKey($one.File)) {
        $one.Sha256 = (Get-FileHash $one.Path -Algorithm SHA256).Hash
        $byFile[$one.File] = $one
    }
}
foreach ($one in $declared.files) {
    $have = $byFile[$one.file]
    if (-not $have) { $storeDifferences[$one.file] = 'is missing from the store'; continue }
    if ($have.Sha256 -ne $one.sha256) {
        $storeDifferences[$one.file] = "hashes to $($have.Sha256), declared as $($one.sha256)"
    }
}
# Anything else the store carries is never installed, so it cannot reach
# a rendering; it is only dead weight worth saying out loud.
$undeclared = $byFile.Keys | Where-Object { $_ -notin ($declared.files | ForEach-Object { $_.file }) }
if ($undeclared.Count) {
    "The store carries $($undeclared.Count) file(s) nothing asks for: $(($undeclared | Sort-Object) -join ', ')"
}

Write-Differences $machineDifferences 'The machine is not what the tests require of it (ci/environment.json):'
Write-Differences $storeDifferences 'The store is not the one ci/fonts.json declares:'

# The record that replaces ci/environment.json once the baselines hold on
# this machine, folded away in the log.
if ($machineDifferences.Count) {
    "::group::ci/environment.json as this machine has it"
    [pscustomobject]@{ description = $required.description; windows = $windows; systemFonts = $machineFonts } |
        ConvertTo-Json -Depth 5
    "::endgroup::"
    ""
}

# A verdict goes on the run's page as an annotation, where it is read
# without opening the log; an exception would leave there only an exit
# code.
if ($storeDifferences.Count) {
    "::error title=The store is not the declared one::The store differs from ci/fonts.json in $($storeDifferences.Keys -join ', ')."
    exit 1
}
if ($machineDifferences.Count) {
    $verdict = "The machine differs from ci/environment.json in $($machineDifferences.Keys -join ', ')."
    if ($env:MACHINE_CHECK -eq 'stop') {
        "Rendering compares against images made on a machine that had all of these, and this"
        "run was started to stop at a machine that differs. To see which baselines the"
        "differences move, run it again with machine_check set to report."
        "::error title=Stopped on a machine the tests do not expect::$verdict"
        exit 1
    }
    "::warning title=Testing on a machine the tests do not expect::$verdict"
    "Testing anyway: the tests show whether the differences move any baseline. If they do,"
    "re-record the baselines they move and replace ci/environment.json with the record above."
}
else {
    "The machine is what the tests require: $windows, and the $($required.systemFonts.Count) font files the tests take from it."
}
"The store is the declared one: $($declared.files.Count) files."

# A font file the system already has open cannot be overwritten; stopping
# the cache service releases most of them. Nothing installed here replaces
# a face the image ships, but the service also caches the absences.
#
# The service has to stay down until every file is in: installing a font
# starts it again on demand, and it then builds a font set from the files
# in place so far. One such set, the image's fonts and the first file
# alone, still reached a test process minutes after the install, which
# drew Arial Narrow in Arial. So it is disabled for the copy, and given
# back its start mode and started again, as it was, once all the files
# are in place.
$stopped = @(foreach ($name in 'FontCache', 'FontCache3.0.0.0') {
    $service = Get-Service $name -ErrorAction SilentlyContinue
    if ($service) {
        [pscustomobject]@{ Name = $name; StartType = "$($service.StartType)"; Running = $service.Status -eq 'Running' }
        Set-Service $name -StartupType Disabled
        $service | Stop-Service -Force -ErrorAction SilentlyContinue
    }
})

# Copying the file and registering it is what installing a font amounts
# to. Handing each one to the shell namespace instead costs seconds
# apiece. The entry has to carry the name the font calls itself, which
# only the file knows; filed under anything else a face is not found by
# name.
$started = Get-Date
$registry = 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts'
$filedUnder = @{}
foreach ($one in $declared.files) {
    $source = $byFile[$one.file]
    $target = Join-Path $fontDir $one.file
    Copy-Item $source.Path $target -Force

    $label = Get-FaceLabel $source.Family $source.Style
    $suffix = if ([IO.Path]::GetExtension($one.file) -eq '.otf') { '(OpenType)' } else { '(TrueType)' }
    New-ItemProperty -Path $registry -Name "$label $suffix" `
                     -Value $one.file -PropertyType String -Force | Out-Null
    $filedUnder["$label $suffix"] = $one.file
    [Native.Gdi]::AddFontResource($target) | Out-Null
}
foreach ($one in $stopped) {
    Set-Service $one.Name -StartupType $one.StartType
    if ($one.Running -or $one.Name -eq 'FontCache') { Start-Service $one.Name -ErrorAction SilentlyContinue }
}
"Installed $($declared.files.Count) fonts in $([int]((Get-Date) - $started).TotalSeconds) s"

# A font in place is not yet a font a program finds. Skia draws with
# DirectWrite, which takes the system's fonts from the font cache service
# when a program first asks for them, and the service learns of new fonts
# in its own time; a test that asked before then would draw a face it
# lacks in another, and the run would put that down to the rendering. So
# each face the store installed is looked for among the fonts DirectWrite
# gives a program started now, and opened, until all of them are there;
# one still missing after two minutes stops the job, named. DirectWrite
# files faces under families of its own -- Arial Narrow under Arial, at a
# narrower width -- so a face is looked for by the family and style names
# Windows files it under and the version it carries, as the store
# declares them.
#
# A face is looked for only if the registry names its file. Two files
# whose faces carry the same names, as two versions of one font do, are
# filed under the same entries, and the later file takes them: the
# earlier is installed for GDI alone, and DirectWrite, which takes the
# system's fonts from the registry, never has it.
$registered = @($filedUnder.Values)
$shadowed = @($declared.files | Where-Object { $registered -notcontains $_.file })
if ($shadowed.Count) {
    "Filed under the names of later files, so installed for GDI alone: $(($shadowed | ForEach-Object { $_.file }) -join ', ')"
}
$facesSource = @'
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

// The faces of DirectWrite's system font collection.
public static class SystemFaces
{
    const int VersionStrings = 2, Win32FamilyNames = 11, Win32SubfamilyNames = 12;

    // One line per face of the collection that a program starting now is
    // given, leaving out faces DirectWrite simulates from another: its
    // family names, style names and version strings, each list joined by
    // '|', and "ok" or why it cannot be opened, tab separated.
    public static string[] List()
    {
        List<string> lines = new List<string>();
        IDWriteFontCollection collection = Collection(false);
        for (uint f = 0; f < collection.GetFontFamilyCount(); f++) {
            IDWriteFontFamily family;
            Check(collection.GetFontFamily(f, out family));
            for (uint i = 0; i < family.GetFontCount(); i++) {
                IDWriteFont font;
                Check(family.GetFont(i, out font));
                if (font.GetSimulations() != 0) { continue; }
                IntPtr face;
                int hr = font.CreateFontFace(out face);
                if (hr >= 0) { Marshal.Release(face); }
                lines.Add(Strings(font, Win32FamilyNames) + "\t" + Strings(font, Win32SubfamilyNames) + "\t" +
                          Strings(font, VersionStrings) + "\t" + (hr >= 0 ? "ok" : "cannot be opened (0x" + hr.ToString("X8") + ")"));
            }
        }
        return lines.ToArray();
    }

    // Has DirectWrite look for changes to the system's fonts at once,
    // rather than when the font cache service comes to them.
    public static void CheckForUpdates()
    {
        Collection(true);
    }

    // The system font collection of the shared factory, which is the one
    // Skia takes.
    static IDWriteFontCollection Collection(bool checkForUpdates)
    {
        Guid iid = typeof(IDWriteFactory).GUID;
        object factory;
        Check(DWriteCreateFactory(0, ref iid, out factory));
        IDWriteFontCollection collection;
        Check(((IDWriteFactory)factory).GetSystemFontCollection(out collection, checkForUpdates));
        return collection;
    }

    // Every string, in every language, of one of a face's informational
    // strings, joined by '|'.
    static string Strings(IDWriteFont font, int id)
    {
        IDWriteLocalizedStrings strings;
        bool exists;
        Check(font.GetInformationalStrings(id, out strings, out exists));
        if (!exists || strings == null) { return ""; }
        List<string> all = new List<string>();
        for (uint i = 0; i < strings.GetCount(); i++) {
            uint length;
            Check(strings.GetStringLength(i, out length));
            IntPtr buffer = Marshal.AllocHGlobal((int)(length + 1) * 2);
            try {
                Check(strings.GetString(i, buffer, length + 1));
                all.Add(Marshal.PtrToStringUni(buffer).Replace("\0", "").Trim());
            }
            finally {
                Marshal.FreeHGlobal(buffer);
            }
        }
        return string.Join("|", all.Distinct());
    }

    static void Check(int hr)
    {
        if (hr < 0) { Marshal.ThrowExceptionForHR(hr); }
    }

    [DllImport("dwrite.dll")]
    static extern int DWriteCreateFactory(int factoryType, ref Guid iid, [MarshalAs(UnmanagedType.IUnknown)] out object factory);

    [ComImport, Guid("b859ee5a-d838-4b5b-a2e8-1adc7d93db48"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IDWriteFactory
    {
        [PreserveSig] int GetSystemFontCollection(out IDWriteFontCollection collection, [MarshalAs(UnmanagedType.Bool)] bool checkForUpdates);
    }

    [ComImport, Guid("a84cee02-3eea-4eee-a827-87c1a02a0fcc"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IDWriteFontCollection
    {
        [PreserveSig] uint GetFontFamilyCount();
        [PreserveSig] int GetFontFamily(uint index, out IDWriteFontFamily family);
    }

    // IDWriteFontFamily, whose first three methods are IDWriteFontList's.
    [ComImport, Guid("da20d8ef-812a-4c43-9802-62ec4abd7add"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IDWriteFontFamily
    {
        [PreserveSig] int GetFontCollection(out IntPtr collection);
        [PreserveSig] uint GetFontCount();
        [PreserveSig] int GetFont(uint index, out IDWriteFont font);
    }

    [ComImport, Guid("acd16696-8c14-4f5d-877e-fe3fc1d32737"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IDWriteFont
    {
        [PreserveSig] int GetFontFamily(out IntPtr family);
        [PreserveSig] int GetWeight();
        [PreserveSig] int GetStretch();
        [PreserveSig] int GetStyle();
        [PreserveSig] int IsSymbolFont();
        [PreserveSig] int GetFaceNames(out IntPtr names);
        [PreserveSig] int GetInformationalStrings(int id, out IDWriteLocalizedStrings strings, [MarshalAs(UnmanagedType.Bool)] out bool exists);
        [PreserveSig] int GetSimulations();
        [PreserveSig] void GetMetrics(IntPtr metrics);
        [PreserveSig] int HasCharacter(uint codepoint, [MarshalAs(UnmanagedType.Bool)] out bool exists);
        [PreserveSig] int CreateFontFace(out IntPtr face);
    }

    [ComImport, Guid("08256209-099a-4b34-b86d-c22b110e7771"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IDWriteLocalizedStrings
    {
        [PreserveSig] uint GetCount();
        [PreserveSig] int FindLocaleName([MarshalAs(UnmanagedType.LPWStr)] string locale, out uint index, [MarshalAs(UnmanagedType.Bool)] out bool exists);
        [PreserveSig] int GetLocaleNameLength(uint index, out uint length);
        [PreserveSig] int GetLocaleName(uint index, IntPtr buffer, uint size);
        [PreserveSig] int GetStringLength(uint index, out uint length);
        [PreserveSig] int GetString(uint index, IntPtr buffer, uint size);
    }
}
'@
$facesHelper = Join-Path $env:RUNNER_TEMP 'SystemFaces.dll'
Add-Type -TypeDefinition $facesSource -OutputAssembly $facesHelper -OutputType Library
Add-Type -Path $facesHelper

# Parameters:
#   listed - the lines SystemFaces.List gave.
# Returns each face the store declares that is not among them, or not
# one DirectWrite can open, with why.
function Get-MissingFaces($listed) {
    $faces = @(foreach ($line in $listed) {
        $parts = "$line" -split "`t"
        if ($parts.Count -eq 4) {
            [pscustomobject]@{
                Families = $parts[0] -split '\|'; Styles = $parts[1] -split '\|'
                Versions = $parts[2] -split '\|'; Status = $parts[3]
            }
        }
    })
    foreach ($one in @($declared.files | Where-Object { $registered -contains $_.file })) {
        foreach ($face in $one.faces) {
            $found = @($faces | Where-Object {
                $_.Families -contains $face.family -and $_.Styles -contains $face.style -and
                (-not $face.version -or $_.Versions -contains $face.version)
            })
            $name = "$($face.family) $($face.style), $($face.version) ($($one.file))"
            if (-not $found.Count) { "$name, not found" }
            elseif (-not @($found | Where-Object { $_.Status -eq 'ok' }).Count) { "$name, $($found[0].Status)" }
        }
    }
}

$wanted = @($declared.files | Where-Object { $registered -contains $_.file } | ForEach-Object { $_.faces }).Count
$looking = Get-Date
while ($true) {
    # A program started now: a new process, which takes the collection as
    # DirectWrite finds it.
    $listed = & pwsh -NoProfile -NonInteractive -Command "Add-Type -Path '$facesHelper'; [SystemFaces]::List()"
    if ($LASTEXITCODE -ne 0) { throw "DirectWrite's fonts could not be listed: $listed" }
    $missing = @(Get-MissingFaces $listed)
    $waited = [int]((Get-Date) - $looking).TotalSeconds
    if (-not $missing.Count) {
        "DirectWrite has the $wanted faces the store installed that the registry names, $waited s after they were installed."
        break
    }
    if ($waited -ge 120) {
        "DirectWrite still lacks $($missing.Count) of the $wanted faces the store installed after $waited s:"
        $missing | ForEach-Object { "  $_" }
        "::error title=Installed fonts not found::DirectWrite does not have $($missing.Count) of the faces the store installed: $($missing -join '; ')."
        exit 1
    }
    "after $waited s, DirectWrite lacks $($missing.Count) of the $wanted faces the store installed: $($missing -join '; ')"
    [SystemFaces]::CheckForUpdates()
    Start-Sleep -Seconds 2
}
