<#
.SYNOPSIS
Extracts a downloaded Web Awesome release zip into a working source tree.

.DESCRIPTION
Takes a Web Awesome version (e.g. 3.1.0), locates temp\download\webawesome_<version>.zip
and extracts it into temp\wa-src\<version>, stripping the zip's internal "webawesome-zip/"
root folder so the destination directly contains package.json, dist\, dist-cdn\ etc.

.PARAMETER Version
Web Awesome version to extract, e.g. 3.1.0.

.PARAMETER RepoRoot
Repository root. Defaults to two levels above this script.

.PARAMETER Force
Re-extract even if the destination already exists (destination is removed first).

.EXAMPLE
.\Expand-WaRelease.ps1 -Version 3.1.0
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+(-[0-9A-Za-z\.\-]+)?$')]
    [string]$Version,

    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,

    [switch]$Force
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$zipPath = Join-Path $RepoRoot "temp\download\webawesome_$Version.zip"
$destination = Join-Path $RepoRoot "temp\wa-src\$Version"
$zipRootPrefix = 'webawesome-zip/'

if (-not (Test-Path $zipPath)) { throw "Release zip not found: $zipPath" }

if (Test-Path $destination) {
    if (-not $Force) {
        Write-Output "Already extracted: $destination (use -Force to re-extract)"
        return $destination
    }
    Remove-Item -Recurse -Force $destination
}

New-Item -ItemType Directory -Force $destination | Out-Null

$zip = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    foreach ($entry in $zip.Entries) {
        # skip directory entries
        if ($entry.FullName.EndsWith('/')) { continue }

        # strip the zip's internal root folder
        $relative = $entry.FullName
        if ($relative.StartsWith($zipRootPrefix)) { $relative = $relative.Substring($zipRootPrefix.Length) }
        if ([string]::IsNullOrEmpty($relative)) { continue }

        $targetPath = Join-Path $destination ($relative -replace '/', '\')
        $targetDir = Split-Path $targetPath -Parent
        if (-not (Test-Path $targetDir)) { New-Item -ItemType Directory -Force $targetDir | Out-Null }
        [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $targetPath, $true)
    }
}
finally {
    $zip.Dispose()
}

$manifest = Join-Path $destination 'dist\custom-elements.json'
if (-not (Test-Path $manifest)) { throw "Extraction incomplete: $manifest not found" }

Write-Output "Extracted Web Awesome $Version to $destination"
return $destination
