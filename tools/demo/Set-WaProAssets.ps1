<#
.SYNOPSIS
Configures the demo apps to use Web Awesome Pro assets from environment variables - without
touching any version-controlled file.

.DESCRIPTION
Generates src\WebAwesome.Blazor.Demo\wwwroot\appsettings.Local.json (ignored by both PlasticSCM
and the GitHub mirror) from one of two supply channels:

  1. Pro CDN / kit URLs:
       WA_PRO_STYLESHEET_URL - absolute URL of the Pro webawesome.css
       WA_PRO_LOADER_URL     - absolute URL of the Pro webawesome.loader.js
  2. Self-hosted Pro dist (works offline, nothing secret leaves the machine):
       WA_PRO_DIST           - path to an extracted Pro package root or its dist-cdn folder;
                               dist-cdn is copied to the ignored folder wwwroot\webawesome and
                               served by the app itself. Note it must be the dist-cdn build -
                               the npm-style dist build's loader only re-exports and never
                               starts the autoloader, so components would not upgrade.

If both are set, WA_PRO_DIST wins. With -Clear, the generated file and the copied dist folder are
removed and the demo falls back to the committed default (free CDN, library version).

The generated file is consumed by both demo hosts: the WebAssembly host fetches it at startup,
the server host loads it as an optional configuration file. NEVER check the generated file in.

.PARAMETER Clear
Remove the generated override and copied dist folder.

.PARAMETER RepoRoot
Repository root. Defaults to two levels above this script.

.EXAMPLE
$env:WA_PRO_DIST = 'D:\wa-pro\dist'; .\Set-WaProAssets.ps1

.EXAMPLE
.\Set-WaProAssets.ps1 -Clear
#>
[CmdletBinding()]
param(
    [switch]$Clear,

    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
)

$ErrorActionPreference = 'Stop'

$wwwroot = Join-Path $RepoRoot 'src\WebAwesome.Blazor.Demo\wwwroot'
$settingsPath = Join-Path $wwwroot 'appsettings.Local.json'
$distTarget = Join-Path $wwwroot 'webawesome'

if ($Clear) {
    if (Test-Path $settingsPath) { Remove-Item $settingsPath -Force }
    if (Test-Path $distTarget) { Remove-Item $distTarget -Recurse -Force }
    Write-Output 'Pro asset override cleared - demo uses the committed default (free CDN).'
    return
}

$proDist = $env:WA_PRO_DIST
$proStylesheet = $env:WA_PRO_STYLESHEET_URL
$proLoader = $env:WA_PRO_LOADER_URL

if ($proDist) {
    # resolve to the dist-cdn build: only that build's loader starts the autoloader
    if (Test-Path (Join-Path $proDist 'dist-cdn\webawesome.loader.js')) {
        $proDist = Join-Path $proDist 'dist-cdn'
    }
    elseif ((Split-Path $proDist -Leaf) -eq 'dist' -and (Test-Path (Join-Path $proDist '..\dist-cdn\webawesome.loader.js'))) {
        Write-Warning 'WA_PRO_DIST points at the npm-style dist build (loader does not autoload); using the sibling dist-cdn instead.'
        $proDist = Resolve-Path (Join-Path $proDist '..\dist-cdn')
    }
    if ((Split-Path $proDist -Leaf) -ne 'dist-cdn' -or -not (Test-Path (Join-Path $proDist 'webawesome.loader.js'))) {
        throw "WA_PRO_DIST must point at a Web Awesome package root or its dist-cdn folder (dist-cdn\webawesome.loader.js not found from: $proDist)"
    }
    if (Test-Path $distTarget) { Remove-Item $distTarget -Recurse -Force }
    Copy-Item $proDist $distTarget -Recurse
    $settings = [ordered]@{
        WebAwesome = [ordered]@{
            AssetSource = 'SelfHosted'
            BasePath    = 'webawesome'
        }
    }
    $mode = "self-hosted Pro dist copied from $proDist"
}
elseif ($proStylesheet -and $proLoader) {
    $settings = [ordered]@{
        WebAwesome = [ordered]@{
            StylesheetUrl = $proStylesheet
            LoaderUrl     = $proLoader
        }
    }
    $mode = 'Pro CDN/kit URLs'
}
else {
    throw 'Set WA_PRO_DIST, or both WA_PRO_STYLESHEET_URL and WA_PRO_LOADER_URL, before running this script (or use -Clear).'
}

$settings | ConvertTo-Json -Depth 5 | Out-File $settingsPath -Encoding utf8
Write-Output "Wrote $settingsPath ($mode). This file is ignored by version control - never check it in."
