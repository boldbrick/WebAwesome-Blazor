<#
.SYNOPSIS
    Installs or updates the WA Inspector browser extension for Chrome and Edge on Windows.

.DESCRIPTION
    The script copies the WA Inspector extension to a stable folder, by default
    %LOCALAPPDATA%\WABlazor\wa-inspector-extension. The source is the extension that the published plugin wablazor
    bundles (assets\wa-inspector-extension in the plugin folder), or the folder of -Source. A plugin installed from
    the marketplace of a repository checkout has no bundled extension: give -Source then, for example
    <repository>\tools\wa-inspector\extension of a WebAwesome.Blazor checkout. The stable folder keeps the id of the unpacked
    extension, and so the saved review sessions, across updates. On an update, the script replaces the content of
    the folder, so removed files go away.

    Then the script opens a new window of the browser and puts the folder path on the clipboard. The browsers do not
    open their internal pages (chrome://extensions, edge://extensions) from the command line, and they let only the
    user load an unpacked extension. So the script writes the next steps: open the extensions page, then on the
    first installation Developer mode and Load unpacked, on an update the reload button of the extension.

    The script supports -WhatIf: it shows the state and the actions, and changes nothing.
    Keep this script ASCII-only and compatible with Windows PowerShell 5.1.

.PARAMETER Browser
    Chrome, Edge or Both. When the parameter is not given and only one of the browsers is installed, the script uses
    that browser. When both are installed, give the parameter.

.PARAMETER ListBrowsers
    Write the installed browsers and the state of the extension folder, and change nothing.

.PARAMETER NoBrowser
    Copy the extension only. Do not open a browser and do not change the clipboard.

.PARAMETER Source
    The extension folder to install. It must hold the manifest.json of the WA Inspector. Default: the bundled
    extension, assets\wa-inspector-extension of the plugin that holds this script (only in a published plugin).

.PARAMETER Destination
    The stable extension folder. Default: %LOCALAPPDATA%\WABlazor\wa-inspector-extension.

.EXAMPLE
    .\Install-WaInspector.ps1 -ListBrowsers

.EXAMPLE
    .\Install-WaInspector.ps1 -Browser Edge -WhatIf

.EXAMPLE
    .\Install-WaInspector.ps1 -Browser Chrome -Source <repository>\tools\wa-inspector\extension
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [ValidateSet('Chrome', 'Edge', 'Both')]
    [string] $Browser,

    [switch] $ListBrowsers,

    [switch] $NoBrowser,

    [string] $Source,

    [string] $Destination
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# the name in the manifest.json of the extension
$extensionName = 'WA Inspector'

# defaults here: Windows PowerShell 5.1 has no $PSScriptRoot in the default values of parameters
$sourceGiven = [bool]$Source
if (-not $Source) { $Source = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\assets\wa-inspector-extension')) }
if (-not $Destination) { $Destination = Join-Path $env:LOCALAPPDATA 'WABlazor\wa-inspector-extension' }

# the browsers: executable name, the extensions page and the usual install folders below the program folders
$browserInfo = [ordered]@{
    Chrome = @{ Exe = 'chrome.exe'; Page = 'chrome://extensions'; SubPath = 'Google\Chrome\Application\chrome.exe' }
    Edge   = @{ Exe = 'msedge.exe'; Page = 'edge://extensions'; SubPath = 'Microsoft\Edge\Application\msedge.exe' }
}

# --- Helpers ---------------------------------------------------------------------

function Find-BrowserPath([string] $Exe, [string] $SubPath) {
    # App Paths of the machine and of the user, then the usual folders
    $keys = @(
        "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\$Exe",
        "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\$Exe",
        "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\$Exe"
    )
    foreach ($key in $keys) {
        $item = Get-ItemProperty -LiteralPath $key -ErrorAction SilentlyContinue
        if ($item -and $item.PSObject.Properties['(default)']) {
            $path = ([string]$item.'(default)').Trim('"')
            if ($path -and (Test-Path -LiteralPath $path -PathType Leaf)) { return $path }
        }
    }
    $roots = @($env:ProgramFiles, ${env:ProgramFiles(x86)}, $env:LOCALAPPDATA) | Where-Object { $_ }
    foreach ($root in $roots) {
        $path = Join-Path $root $SubPath
        if (Test-Path -LiteralPath $path -PathType Leaf) { return $path }
    }
    return $null
}

function Get-ExtensionVersion([string] $Folder) {
    $manifestPath = Join-Path $Folder 'manifest.json'
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) { return $null }
    return [string](([System.IO.File]::ReadAllText($manifestPath) | ConvertFrom-Json).version)
}

function Get-Sha256([string] $Path) {
    # .NET instead of Get-FileHash: Get-FileHash of Windows PowerShell 5.1 obeys -WhatIf and returns nothing then
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $stream = [System.IO.File]::OpenRead($Path)
    try { return [System.BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-', '') }
    finally { $stream.Dispose(); $sha.Dispose() }
}

function Get-TreeHash([string] $Folder) {
    # one line per file: the relative path and the SHA-256; names that start with '_' are reserved for the browser
    $root = (Resolve-Path -LiteralPath $Folder).ProviderPath.TrimEnd('\')
    $lines = Get-ChildItem -LiteralPath $root -Recurse -Force -File | ForEach-Object {
        $relative = $_.FullName.Substring($root.Length + 1)
        if (-not $relative.StartsWith('_')) {
            '{0}|{1}' -f $relative.ToLowerInvariant(), (Get-Sha256 $_.FullName)
        }
    } | Sort-Object
    return (@($lines) -join "`n")
}

# --- Source and state ------------------------------------------------------------

if (-not $sourceGiven -and -not (Test-Path -LiteralPath $Source -PathType Container)) {
    throw ('This plugin has no bundled extension ({0}). Only a published plugin bundles it. Give the extension ' +
        'folder with -Source, for example <repository>\tools\wa-inspector\extension of a WebAwesome.Blazor checkout.') -f $Source
}
if (-not (Test-Path -LiteralPath $Source -PathType Container)) {
    throw "The extension folder $Source does not exist."
}
$manifestFile = Join-Path $Source 'manifest.json'
if (-not (Test-Path -LiteralPath $manifestFile -PathType Leaf)) {
    throw "The extension folder $Source has no manifest.json."
}
$manifestName = [string](([System.IO.File]::ReadAllText($manifestFile) | ConvertFrom-Json).name)
if ($manifestName -ne $extensionName) {
    throw "The manifest.json in $Source is not the manifest of the $extensionName extension (name: '$manifestName')."
}
$Source = (Resolve-Path -LiteralPath $Source).ProviderPath.TrimEnd('\')
$newVersion = Get-ExtensionVersion $Source

$oldVersion = $null
if (-not (Test-Path -LiteralPath $Destination -PathType Container)) {
    $state = 'FirstInstall'
}
else {
    $oldVersion = Get-ExtensionVersion $Destination
    if ($null -eq $oldVersion) { $state = 'FirstInstall' }
    elseif ((Get-TreeHash $Destination) -eq (Get-TreeHash $Source)) { $state = 'UpToDate' }
    else { $state = 'Update' }
}

$installed = [ordered]@{}
foreach ($name in $browserInfo.Keys) {
    $path = Find-BrowserPath $browserInfo[$name].Exe $browserInfo[$name].SubPath
    if ($path) { $installed[$name] = $path }
}

if ($ListBrowsers) {
    if ($installed.Count -eq 0) { Write-Output 'Installed browsers: none (Chrome or Edge not found)' }
    else { Write-Output ('Installed browsers: {0}' -f (@($installed.Keys) -join ', ')) }
    Write-Output "Extension source: $Source"
    Write-Output "Extension folder: $Destination"
    Write-Output ('State: {0} (installed version: {1}, source version: {2})' -f $state, $(if ($oldVersion) { $oldVersion } else { 'none' }), $newVersion)
    return
}

# --- Browsers ----------------------------------------------------------------------

$targets = @()
if (-not $NoBrowser) {
    if ($Browser -eq 'Both') { $targets = @('Chrome', 'Edge') }
    elseif ($Browser) { $targets = @($Browser) }
    elseif ($installed.Count -eq 1) { $targets = @($installed.Keys) }
    elseif ($installed.Count -gt 1) { throw 'Chrome and Edge are both installed. Give -Browser Chrome, Edge or Both.' }
    else { throw 'Chrome or Edge is not installed. Install one of them, or use -NoBrowser.' }

    foreach ($name in $targets) {
        if (-not $installed.Contains($name)) { throw "$name is not installed." }
    }
}

# --- Copy --------------------------------------------------------------------------

if ($state -eq 'UpToDate') {
    Write-Output "The extension folder is up to date: $Destination"
}
elseif ($PSCmdlet.ShouldProcess($Destination, "Copy the WA Inspector extension $newVersion")) {
    if (Test-Path -LiteralPath $Destination) {
        # keep the folder itself, so the path and the extension id stay
        Get-ChildItem -LiteralPath $Destination -Force | Remove-Item -Recurse -Force -Confirm:$false
    }
    else {
        New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    }
    Copy-Item -Path (Join-Path $Source '*') -Destination $Destination -Recurse -Force
    if ((Get-TreeHash $Destination) -ne (Get-TreeHash $Source)) { throw "The copy to $Destination is not complete." }
    Write-Output "Copied the extension to $Destination"
}

# --- Browser window and clipboard --------------------------------------------------

# the browsers ignore their internal pages on the command line, so the script opens a new window only
foreach ($name in $targets) {
    if ($PSCmdlet.ShouldProcess($name, 'Open a new window')) {
        Start-Process -FilePath $installed[$name] -ArgumentList '--new-window'
        Write-Output "Opened a new $name window"
    }
}
if ($targets.Count -gt 0 -and $PSCmdlet.ShouldProcess('clipboard', "Set the text '$Destination'")) {
    Set-Clipboard -Value $Destination
    Write-Output 'The folder path is on the clipboard.'
}

# --- Next steps --------------------------------------------------------------------

$pages = (@($targets | ForEach-Object { $browserInfo[$_].Page }) -join ' and ')
if ($targets.Count -eq 2) { $pages = 'chrome://extensions (Chrome) and edge://extensions (Edge)' }
elseif ($targets.Count -eq 0) { $pages = 'chrome://extensions (Chrome) or edge://extensions (Edge)' }

Write-Output ''
switch ($state) {
    'FirstInstall' {
        Write-Output "Result: first installation of WA Inspector $newVersion"
        Write-Output 'Next steps:'
        Write-Output "  1. Type $pages in the address bar."
        Write-Output '  2. Turn on Developer mode.'
        Write-Output '  3. Click Load unpacked.'
        Write-Output "  4. Paste the folder path ($Destination) and select the folder."
    }
    'Update' {
        if ($oldVersion -eq $newVersion) { Write-Output "Result: update of WA Inspector $newVersion (other files, same version number)" }
        else { Write-Output "Result: update of WA Inspector from $oldVersion to $newVersion" }
        Write-Output "Next step: type $pages in the address bar, then click the reload button on the WA Inspector card."
        Write-Output 'If the extension is not loaded yet, use Developer mode, Load unpacked and the folder path.'
    }
    'UpToDate' {
        Write-Output "Result: WA Inspector $newVersion is up to date."
        Write-Output "If the extension is not loaded yet, type $pages in the address bar, then use Developer mode, Load unpacked and the folder path."
    }
}
Write-Output 'Then reload the pages that were open before: the extension loads with the page.'
Write-Output ''
Write-Output 'Notes:'
Write-Output '  - Chrome or Edge can warn about extensions in developer mode. The warning is expected for an unpacked extension.'
Write-Output "  - To remove the extension: click Remove on the WA Inspector card, then delete the folder $Destination."
if ($WhatIfPreference) { Write-Output 'WhatIf: no file, browser or clipboard was changed.' }
