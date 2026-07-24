<#
.SYNOPSIS
Refreshes the versioned Web Awesome reference documentation from the public GitHub repository.

.DESCRIPTION
Downloads the public Web Awesome GitHub repository at tag v<Version> (single codeload zip
request, no API rate limits), extracts the packages/webawesome/docs/docs folder, and replaces
the destination folder's contents with it (default: inputs\WebAwesome).

Pro components (e.g. combobox, page) are absent from the public GitHub docs tree. For every
component present in the release CEM (temp\download\webawesome_<version>.zip ->
dist/custom-elements.json) but missing from the fetched docs, the script fills the gap from
the first available source:
  1. the reference doc bundled in the release zip itself (dist/skills/webawesome/references/
     components/<name>.md, present since Web Awesome 3.3.0) - versioned exactly and covering
     all components including Pro; a source header naming the zip origin is prepended;
  2. the existing doc carried forward from the carry-from folder (default: inputs\WebAwesome),
     re-stamping its source header with the target version and date;
  3. otherwise the component is recorded as NEEDS CAPTURE - its doc page must be fetched from
     https://webawesome.com/docs/components/<name> and converted to markdown manually (or by
     the upgrade pipeline), with the source URL noted at the top.

The downloaded GitHub zip is cached in temp\wa-docs\ and reused; pass -Force to re-download.

.PARAMETER Version
Web Awesome version whose docs to ingest, e.g. 3.6.0.

.PARAMETER Destination
Folder whose contents are replaced with the fetched docs tree. Defaults to inputs\WebAwesome
under the repository root. May be an arbitrary folder for testing.

.PARAMETER CarryFromPath
Folder holding the previous docs version, used as the carry-forward source for Pro component
docs. Defaults to inputs\WebAwesome under the repository root.

.PARAMETER RepoRoot
Repository root. Defaults to two levels above this script.

.PARAMETER Force
Re-download the GitHub zip even if a cached copy exists.

.EXAMPLE
.\Sync-WaDocs.ps1 -Version 3.6.0

.EXAMPLE
.\Sync-WaDocs.ps1 -Version 3.6.0 -Destination temp\wa-docs-test\3.6.0
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+(-[0-9A-Za-z\.\-]+)?$')]
    [string]$Version,

    [string]$Destination,

    [string]$CarryFromPath,

    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,

    [switch]$Force
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

if (-not $Destination) { $Destination = Join-Path $RepoRoot 'inputs\WebAwesome' }
elseif (-not [System.IO.Path]::IsPathRooted($Destination)) { $Destination = Join-Path $RepoRoot $Destination }
if (-not $CarryFromPath) { $CarryFromPath = Join-Path $RepoRoot 'inputs\WebAwesome' }
elseif (-not [System.IO.Path]::IsPathRooted($CarryFromPath)) { $CarryFromPath = Join-Path $RepoRoot $CarryFromPath }

$docsRepoPath = 'packages/webawesome/docs/docs/'
$zipUrl = "https://codeload.github.com/shoelace-style/webawesome/zip/refs/tags/v$Version"
$cacheDir = Join-Path $RepoRoot 'temp\wa-docs'
$zipPath = Join-Path $cacheDir "github_v$Version.zip"
$releaseZipPath = Join-Path $RepoRoot "temp\download\webawesome_$Version.zip"

# ------ download the GitHub repository zip at the tag (cached) ------

if (-not (Test-Path $cacheDir)) { New-Item -ItemType Directory -Force $cacheDir | Out-Null }
if ($Force -and (Test-Path $zipPath)) { Remove-Item $zipPath -Force }
if (Test-Path $zipPath) {
    Write-Output "Using cached GitHub zip: $zipPath"
}
else {
    Write-Output "Downloading $zipUrl ..."
    try {
        Invoke-WebRequest -Uri $zipUrl -OutFile $zipPath -MaximumRedirection 5
    }
    catch {
        if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
        throw "Download failed for tag v$Version ($zipUrl): $($_.Exception.Message)"
    }
}

# ------ extract the docs tree into a staging folder ------

$staging = Join-Path $cacheDir "staging_v$Version"
if (Test-Path $staging) { Remove-Item -Recurse -Force $staging }
New-Item -ItemType Directory -Force $staging | Out-Null

$extracted = 0
$zip = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    foreach ($entry in $zip.Entries) {
        if ($entry.FullName.EndsWith('/')) { continue }

        # the codeload zip root folder is variable (shoelace-style-webawesome-<sha>/) - strip the first segment
        $slash = $entry.FullName.IndexOf('/')
        if ($slash -lt 0) { continue }
        $relative = $entry.FullName.Substring($slash + 1)
        if (-not $relative.StartsWith($docsRepoPath)) { continue }

        $relative = $relative.Substring($docsRepoPath.Length)
        if ([string]::IsNullOrEmpty($relative)) { continue }

        $targetPath = Join-Path $staging ($relative -replace '/', '\')
        $targetDir = Split-Path $targetPath -Parent
        if (-not (Test-Path $targetDir)) { New-Item -ItemType Directory -Force $targetDir | Out-Null }
        [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $targetPath, $true)
        $extracted++
    }
}
finally {
    $zip.Dispose()
}

if ($extracted -eq 0) { throw "No files found under $docsRepoPath in the v$Version GitHub zip - tag layout changed?" }
Write-Output "Extracted $extracted doc files from GitHub tag v$Version."

# ------ determine the component list from the release CEM ------

if (-not (Test-Path $releaseZipPath)) { throw "Release zip not found: $releaseZipPath (needed for the CEM component list)" }

$cemJson = $null
$zip = [System.IO.Compression.ZipFile]::OpenRead($releaseZipPath)
try {
    $cemEntry = $zip.Entries | Where-Object { $_.FullName -match '(^|/)dist/custom-elements\.json$' } | Select-Object -First 1
    if (-not $cemEntry) { throw "dist/custom-elements.json not found in $releaseZipPath" }
    $reader = New-Object System.IO.StreamReader($cemEntry.Open())
    try { $cemJson = $reader.ReadToEnd() } finally { $reader.Dispose() }
}
finally {
    $zip.Dispose()
}

$cem = $cemJson | ConvertFrom-Json
$componentNames = @(
    foreach ($module in $cem.modules) {
        foreach ($declaration in $module.declarations) {
            if ($declaration.tagName -and $declaration.tagName -like 'wa-*') {
                $declaration.tagName.Substring(3)
            }
        }
    }
) | Sort-Object -Unique
Write-Output "CEM lists $($componentNames.Count) components."

# ------ index the reference docs bundled in the release zip (present since WA 3.3.0) ------

$bundledRefs = @{}
$zip = [System.IO.Compression.ZipFile]::OpenRead($releaseZipPath)
try {
    foreach ($entry in $zip.Entries) {
        if ($entry.FullName -match '(^|/)dist/skills/webawesome/references/components/([a-z0-9\-]+)\.md$') {
            $reader = New-Object System.IO.StreamReader($entry.Open())
            try { $bundledRefs[$Matches[2]] = $reader.ReadToEnd() } finally { $reader.Dispose() }
        }
    }
}
finally {
    $zip.Dispose()
}
if ($bundledRefs.Count -gt 0) { Write-Output "Release zip bundles $($bundledRefs.Count) component reference docs." }

# ------ fill doc gaps: bundled reference, then carry-forward, then NEEDS CAPTURE ------

$today = Get-Date -Format 'yyyy-MM-dd'
$filledFromZip = @()
$carriedForward = @()
$needsCapture = @()

foreach ($name in $componentNames) {
    $stagingDoc = Join-Path $staging "components\$name.md"
    if (Test-Path $stagingDoc) { continue }

    $content = $null
    if ($bundledRefs.ContainsKey($name)) {
        $header = "<!-- Source: reference doc bundled in the Web Awesome $Version release zip (dist/skills/webawesome/references/components/$name.md) -- component absent from the public GitHub docs tree. Full documentation: https://webawesome.com/docs/components/$name -->"
        $content = "$header`r`n`r`n$($bundledRefs[$name])"
        $filledFromZip += $name
    }
    else {
        $existingDoc = Join-Path $CarryFromPath "components\$name.md"
        if (Test-Path $existingDoc) {
            $content = Get-Content $existingDoc -Raw
            # re-stamp the source header comment (first line) with the target version and date
            $header = "<!-- Source: https://webawesome.com/docs/components/$name (public web docs -- component absent from the public GitHub docs tree; carried forward $today for Web Awesome $Version - verify against the target CEM for API changes). -->"
            if ($content -match '^\s*<!--[^\r\n]*-->') {
                $content = $content -replace '^\s*<!--[^\r\n]*-->', $header
            }
            else {
                $content = "$header`r`n`r`n$content"
            }
            $carriedForward += $name
        }
        else {
            $needsCapture += $name
            continue
        }
    }

    $targetDir = Split-Path $stagingDoc -Parent
    if (-not (Test-Path $targetDir)) { New-Item -ItemType Directory -Force $targetDir | Out-Null }
    Set-Content -Path $stagingDoc -Value $content -Encoding utf8 -NoNewline
}

if ($filledFromZip) { Write-Output "Filled from the release zip's bundled references: $($filledFromZip -join ', ')" }
if ($carriedForward) { Write-Output "Carried forward (re-stamped for $Version): $($carriedForward -join ', ')" }
if ($needsCapture) {
    Write-Warning "NEEDS CAPTURE - no doc in GitHub tree or carry-from folder; fetch from https://webawesome.com/docs/components/<name> and convert to markdown:"
    foreach ($name in $needsCapture) { Write-Warning "  $name -> https://webawesome.com/docs/components/$name" }
}

# ------ replace destination contents with the staged tree ------

if (Test-Path $Destination) { Remove-Item -Recurse -Force $Destination }
New-Item -ItemType Directory -Force (Split-Path $Destination -Parent) | Out-Null
Move-Item $staging $Destination

$total = (Get-ChildItem $Destination -Recurse -File).Count
Write-Output "Docs for Web Awesome $Version written to $Destination ($total files; $($filledFromZip.Count) filled from the zip, $($carriedForward.Count) carried forward, $($needsCapture.Count) need capture)."

return [pscustomobject]@{
    Destination    = $Destination
    FileCount      = $total
    FilledFromZip  = $filledFromZip
    CarriedForward = $carriedForward
    NeedsCapture   = $needsCapture
}
