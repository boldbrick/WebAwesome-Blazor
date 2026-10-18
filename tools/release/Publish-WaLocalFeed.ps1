# Publish-WaLocalFeed.ps1
# Publishes the two packages of a release (WebAwesome.Blazor and WebAwesome.Blazor.Extensions) with their docs
# bundle to the internal NuGet feed. Run it before the release tag goes to the GitHub mirror.
# The docs bundle (DocPack) is made by local Release builds only: packages built on GitHub (nuget.org) have none.
# Windows PowerShell 5.1 compatible; ASCII only (5.1 misparses BOM-less non-ASCII scripts).
#
# Usage (from anywhere):
#   & tools\release\Publish-WaLocalFeed.ps1 -Source <name> [-SkipBuild] [-ConfigFile <path>] [-AllowDirty]
# -Source is the name of a NuGet source (dotnet nuget add source <feed> --name <name>, user level, never in the
# repository). This script contains no feed path.
# Exit code 0 = both packages pushed (or already there); 1 = a check or a push failed.
#
# Steps: clean workspace, dotnet tool restore, Release build of src\WebAwesome.slnx (which packs), both nupkgs
# at the Version.props version with docs/, the main package with docs/content/ (migration guides, web-awesome/,
# no ai/), dotnet nuget push --skip-duplicate of both nupkgs (the CLI also pushes a .snupkg beside a .nupkg
# when the source accepts symbols).

param(
    [Parameter(Mandatory = $true)]
    [string]$Source,     # name of the NuGet source to push to (never a path in this script)
    [switch]$SkipBuild,  # use the packages of an earlier Release build
    [string]$ConfigFile, # NuGet config file that defines the source (default: the user-level config)
    [switch]$AllowDirty  # for tests only: skip the clean-workspace check
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
Set-Location $repoRoot

function Stop-Publish([string]$message) {
    Write-Host ("[FAIL] {0}" -f $message)
    exit 1
}

Write-Host "=== WebAwesome.Blazor local-feed publish ==="
Write-Host ("Repo root: {0}" -f $repoRoot)

# --- clean workspace ----------------------------------------------------------
$pending = @(cm status --short 2>&1 | Where-Object { $_ -and $_.ToString().Trim() -ne '' })
if ($pending.Count -gt 0) {
    if (-not $AllowDirty) { Stop-Publish ("workspace not clean: {0} pending items (check in first)" -f $pending.Count) }
    Write-Warning ("-AllowDirty: {0} pending items; the packages do not match a changeset" -f $pending.Count)
} else {
    Write-Host '[ OK ] workspace clean'
}

# --- version ------------------------------------------------------------------
[xml]$props = Get-Content src\Version.props
$version = ($props.Project.PropertyGroup | Where-Object { $_.Version } | Select-Object -First 1).Version
if ([string]::IsNullOrEmpty($version)) { Stop-Publish 'no Version in src\Version.props' }
Write-Host ("Version.props version: {0}" -f $version)

# --- tool restore and Release build -------------------------------------------
if (-not $SkipBuild) {
    # the Release build bundles the package docs with the docpack tool pinned in .config\dotnet-tools.json
    $restore = dotnet tool restore 2>&1
    if ($LASTEXITCODE -ne 0) {
        $restore | Select-Object -Last 10 | ForEach-Object { Write-Host ("    {0}" -f $_) }
        Stop-Publish ("dotnet tool restore exit {0}" -f $LASTEXITCODE)
    }
    Write-Host '[ OK ] dotnet tool restore'

    $build = dotnet build src\WebAwesome.slnx -p:Configuration=Release 2>&1
    if ($LASTEXITCODE -ne 0) {
        $build | Select-String -Pattern 'error' | Select-Object -First 20 | ForEach-Object { Write-Host ("    {0}" -f $_.Line) }
        Stop-Publish ("Release build exit {0}" -f $LASTEXITCODE)
    }
    Write-Host '[ OK ] Release build (packs both packages)'
} else {
    Write-Host 'Build skipped (-SkipBuild): the packages of an earlier Release build are used.'
}

# --- package checks -----------------------------------------------------------
$mainId = 'WebAwesome.Blazor'
$packageIds = @($mainId, 'WebAwesome.Blazor.Extensions')
$packageDir = 'src\output\packages\Release'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$nupkgs = @()
foreach ($id in $packageIds) {
    $nupkgPath = Join-Path $packageDir ("{0}.{1}.nupkg" -f $id, $version)
    if (-not (Test-Path $nupkgPath)) { Stop-Publish ("missing {0}" -f $nupkgPath) }
    $zip = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path $nupkgPath).Path)
    try { $docs = @($zip.Entries | ForEach-Object { $_.FullName } | Where-Object { $_.StartsWith('docs/') }) } finally { $zip.Dispose() }
    if ($docs.Count -eq 0) { Stop-Publish ("{0}: no docs/ folder (DocPack bundles in local Release builds only)" -f $id) }

    if ($id -eq $mainId) {
        $content = @($docs | Where-Object { $_.StartsWith('docs/content/') })
        $guides = @(Get-ChildItem docs\migration -File -Filter 'MIGRATION-*.md' | ForEach-Object { 'docs/content/migration/' + $_.Name })
        $missing = @($guides | Where-Object { $content -notcontains $_ })
        if ($missing.Count -gt 0) { Stop-Publish ("{0}: missing migration guides {1}" -f $id, ($missing -join ', ')) }
        $upstream = @($content | Where-Object { $_.StartsWith('docs/content/web-awesome/') })
        if ($upstream.Count -eq 0) { Stop-Publish ("{0}: no docs/content/web-awesome/ folder" -f $id) }
        $aiFiles = @($content | Where-Object { $_ -match '/ai/' })
        if ($aiFiles.Count -gt 0) { Stop-Publish ("{0}: must not bundle {1}" -f $id, ($aiFiles -join ', ')) }
        Write-Host ("[ OK ] {0}: {1} docs files, {2} migration guides, {3} upstream files" -f $id, $docs.Count, $guides.Count, $upstream.Count)
    } else {
        Write-Host ("[ OK ] {0}: {1} docs files" -f $id, $docs.Count)
    }
    $nupkgs += (Resolve-Path $nupkgPath).Path
}

# --- push ---------------------------------------------------------------------
foreach ($nupkg in $nupkgs) {
    $pushArgs = @('nuget', 'push', $nupkg, '--source', $Source, '--skip-duplicate')
    if ($ConfigFile) { $pushArgs += @('--configfile', $ConfigFile) }
    $push = & dotnet @pushArgs 2>&1
    $push | ForEach-Object { Write-Host ("    {0}" -f $_) }
    if ($LASTEXITCODE -ne 0) { Stop-Publish ("push of {0} to source '{1}' exit {2}" -f (Split-Path $nupkg -Leaf), $Source, $LASTEXITCODE) }
    Write-Host ("[ OK ] pushed {0} to source '{1}'" -f (Split-Path $nupkg -Leaf), $Source)
}

Write-Host ("Both packages {0} are on source '{1}'. Push the release tag next." -f $version, $Source)
exit 0
