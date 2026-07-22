<#
.SYNOPSIS
Configures the demo apps to use Web Awesome Pro assets from environment variables - without
touching any version-controlled file.

.DESCRIPTION
Generates src\WebAwesome.Blazor.Demo\wwwroot\appsettings.Local.json (ignored by both PlasticSCM
and the GitHub mirror) from one of two supply channels:

  1. Pro CDN base URL (preferred - version-independent, set once):
       WA_PRO_CDN_BASE       - kit base URL with a literal {version} placeholder, e.g.
                               https://ka-p.webawesome.com/kit/<kit>/webawesome@{version}
                               Maps to WebAwesomeOptions.CdnBaseUrl: {version} resolves to the
                               library's own bound WA version at runtime, and the stylesheet
                               (styles/webawesome.css) and loader (webawesome.loader.js) URLs
                               are derived - so releases never require touching the variable.
  2. Explicit Pro asset URLs (fallback when the kit CDN layout ever diverges):
       WA_PRO_STYLESHEET_URL - absolute URL of the Pro webawesome.css
       WA_PRO_LOADER_URL     - absolute URL of the Pro webawesome.loader.js
  3. Self-hosted Pro dist (works offline, nothing secret leaves the machine):
       WA_PRO_DIST           - path to an extracted Pro package root or its dist-cdn folder;
                               dist-cdn is copied to the ignored folder wwwroot\webawesome and
                               served by the app itself. Note it must be the dist-cdn build -
                               the npm-style dist build's loader only re-exports and never
                               starts the autoloader, so components would not upgrade.

Precedence when several are set: WA_PRO_DIST, then WA_PRO_CDN_BASE, then the explicit URL pair.
With -Clear, the generated file and the copied dist folder are removed and the demo falls back
to the committed default (free CDN, library version).

The override is generated in two places, one per demo host: the WebAssembly host fetches
wwwroot\appsettings.Local.json over HTTP at startup, while the server host
(WebAwesome.Blazor.Demo.Server) loads appsettings.Local.json from its own content root as an
optional configuration file. NEVER check the generated files in.

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
$distTarget = Join-Path $wwwroot 'webawesome'

# two consumers, two locations: the WebAssembly host fetches the file from its wwwroot over HTTP,
# the server host (WebAwesome.Blazor.Demo.Server) loads it from its own content root
$settingsPaths = @(
    (Join-Path $wwwroot 'appsettings.Local.json'),
    (Join-Path $RepoRoot 'src\WebAwesome.Blazor.Demo.Server\appsettings.Local.json')
)

if ($Clear) {
    foreach ($path in $settingsPaths) {
        if (Test-Path $path) { Remove-Item $path -Force }
    }
    if (Test-Path $distTarget) { Remove-Item $distTarget -Recurse -Force }
    Write-Output 'Pro asset override cleared - demo uses the committed default (free CDN).'
    return
}

$proDist = $env:WA_PRO_DIST
$proCdnBase = $env:WA_PRO_CDN_BASE
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
elseif ($proCdnBase) {
    if ($proCdnBase -notmatch '\{version\}') {
        Write-Warning 'WA_PRO_CDN_BASE has no {version} placeholder - the URL is pinned and will need manual updates on every release.'
    }
    $settings = [ordered]@{
        WebAwesome = [ordered]@{
            CdnBaseUrl = $proCdnBase
        }
    }
    $mode = 'Pro CDN base URL, version resolved by the library'
}
elseif ($proStylesheet -and $proLoader) {
    $settings = [ordered]@{
        WebAwesome = [ordered]@{
            StylesheetUrl = $proStylesheet
            LoaderUrl     = $proLoader
        }
    }
    $mode = 'explicit Pro asset URLs'
}
else {
    throw 'Set WA_PRO_DIST, WA_PRO_CDN_BASE, or both WA_PRO_STYLESHEET_URL and WA_PRO_LOADER_URL, before running this script (or use -Clear).'
}

$json = $settings | ConvertTo-Json -Depth 5
foreach ($path in $settingsPaths) {
    $json | Out-File $path -Encoding utf8
    Write-Output "Wrote $path ($mode)."
}
Write-Output 'These files are ignored by version control - never check them in.'
