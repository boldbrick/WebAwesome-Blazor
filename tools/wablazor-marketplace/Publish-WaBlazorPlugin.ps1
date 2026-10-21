<#
.SYNOPSIS
    Publishes the plugins of this marketplace into another Claude Code marketplace folder.

.DESCRIPTION
    The source is this folder: .claude-plugin\marketplace.json, one plugin folder per plugin
    under plugins\, and the optional bundle manifest bundles.json. The target is the root folder
    of a file-based Claude Code marketplace, for example a folder that a team shares. The target
    folder must exist.

    The bundle manifest adds files from outside the plugin folder to the published plugin, for
    example a tool that has its own source folder in the repository. Its format:

      { "<plugin name>": [ { "source": "<path>", "target": "<path>" } ] }

    'source' is a file or a folder, relative to this marketplace folder. 'target' is relative to
    the plugin root and must not exist in the plugin folder. A missing source is an error.

    For each selected plugin, the script:

      1. Optionally raises the version in plugins\<name>\.claude-plugin\plugin.json
         (-BumpVersion). Only the version value changes; the format of the file stays.
      2. Stages the plugin in a temporary folder: a copy of the plugin folder plus the bundles
         of the plugin from bundles.json. The script always makes the staged copy, also with
         -WhatIf, and deletes it at the end.
      3. Validates the staged copy with 'claude plugin validate <folder> --strict'
         (-SkipValidation skips this step).
      4. Copies the staged copy into a versioned subfolder of the target, <name>_v<version>. The
         version comes from the plugin.json of the plugin. If the subfolder exists, the script
         deletes it first, so no old file stays. If the subfolder holds the same files as the
         staged copy already, the script does nothing for that plugin.
      5. Registers the version in the .claude-plugin\marketplace.json of the target. The entry
         gets name, source ('./<name>_v<version>') and version, and description and category from
         the entry of the plugin in the source marketplace.json.

    The target marketplace.json holds exactly one entry per plugin: a new version replaces the
    entry in place, and other entries of the same plugin are removed. 'claude plugin validate'
    refuses a marketplace with two entries of the same name. The folders of the older versions
    stay in the target.

    If the target has no marketplace.json, the script creates one with the name, owner and
    description of the source marketplace.json.

    Claude Code updates an installed plugin only when its version changes. When the target holds
    the same version with other files (a bundle source counts too), the script replaces the files
    and gives a warning: raise the version (-BumpVersion) so that the users get the change.

    At the end, the script validates the target marketplace with
    'claude plugin validate <target>'. This check is not strict, because the target can hold
    other plugins with warnings.

    The script supports -WhatIf and -Confirm. -WhatIf stages and validates the plugins and shows
    the bundle copies, the copies to the target, the version changes and the marketplace.json
    writes, but changes no file in this repository or in the target.

    The script runs in Windows PowerShell 5.1 and in PowerShell 7. Keep it ASCII-only.

.PARAMETER Destination
    The root folder of the target marketplace (the folder that holds .claude-plugin\). Required.
    When the parameter is not given, the script uses the environment variable
    WABLAZOR_PLUGIN_MARKETPLACE. There is no default path.

.PARAMETER PluginName
    The plugin to publish (folder name under plugins\). 'all' (the default) publishes every
    plugin of this marketplace.

.PARAMETER BumpVersion
    Raise one part of the version before the publication: Major, Minor or Patch. The lower parts
    become 0, and a pre-release suffix is removed. The script writes the new version to the
    plugin.json of the plugin in this repository.

.PARAMETER SkipValidation
    Do not run 'claude plugin validate'. Use it only when the Claude Code CLI is not installed.

.EXAMPLE
    .\tools\wablazor-marketplace\Publish-WaBlazorPlugin.ps1 -Destination <marketplace folder> -WhatIf
    Shows what the script would publish, and changes no file.

.EXAMPLE
    .\tools\wablazor-marketplace\Publish-WaBlazorPlugin.ps1 -Destination <marketplace folder> -PluginName wablazor -BumpVersion Patch
    Raises the patch version of the plugin wablazor and publishes it.

.EXAMPLE
    $env:WABLAZOR_PLUGIN_MARKETPLACE = '<marketplace folder>'
    .\tools\wablazor-marketplace\Publish-WaBlazorPlugin.ps1
    Publishes all plugins into the folder of the environment variable.
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Position = 0)]
    [string]$Destination = $env:WABLAZOR_PLUGIN_MARKETPLACE,

    [Parameter(Position = 1)]
    [ArgumentCompleter({
        param($commandName, $parameterName, $wordToComplete)
        Get-ChildItem -Path (Join-Path $PSScriptRoot 'plugins') -Directory |
            Where-Object { $_.Name -like "$wordToComplete*" -and (Test-Path (Join-Path $_.FullName '.claude-plugin\plugin.json')) } |
            ForEach-Object Name
    })]
    [string]$PluginName = 'all',

    [ValidateSet('Major', 'Minor', 'Patch')]
    [string]$BumpVersion,

    [switch]$SkipValidation
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# paths from $PSScriptRoot here: Windows PowerShell 5.1 has no $PSScriptRoot in the default values of parameters
$sourceRoot            = (Resolve-Path -LiteralPath $PSScriptRoot).ProviderPath.TrimEnd('\')
$pluginsRoot           = Join-Path $sourceRoot 'plugins'
$sourceMarketplaceFile = Join-Path $sourceRoot '.claude-plugin\marketplace.json'
$bundleManifestFile    = Join-Path $sourceRoot 'bundles.json'
$utf8NoBom             = New-Object System.Text.UTF8Encoding($false)
$semVerPattern         = '^(\d+)\.(\d+)\.(\d+)(-[0-9A-Za-z.-]+)?$'
$stagingRoot           = Join-Path ([System.IO.Path]::GetTempPath()) ('plugin-publish-' + [guid]::NewGuid().ToString('N'))

# --- Helpers ---------------------------------------------------------------------

function Read-JsonFile([string]$Path) {
    # ReadAllText detects UTF-8 with and without BOM; Get-Content of Windows PowerShell does not
    return [System.IO.File]::ReadAllText($Path) | ConvertFrom-Json
}

function Get-NextVersion([string]$Current, [string]$Part) {
    if ($Current -notmatch $semVerPattern) { throw "The version '$Current' is not a semantic version (x.y.z)." }
    $major = [int]$Matches[1]; $minor = [int]$Matches[2]; $patch = [int]$Matches[3]
    switch ($Part) {
        'Major' { return "$($major + 1).0.0" }
        'Minor' { return "$major.$($minor + 1).0" }
        'Patch' { return "$major.$minor.$($patch + 1)" }
    }
}

function Set-PluginVersion([string]$ManifestPath, [string]$Current, [string]$New) {
    # replace only the first "version" value, so the format, the key order and a BOM stay
    $bytes  = [System.IO.File]::ReadAllBytes($ManifestPath)
    $hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
    $skip   = if ($hasBom) { 3 } else { 0 }
    $text   = $utf8NoBom.GetString($bytes, $skip, $bytes.Length - $skip)
    $regex  = New-Object System.Text.RegularExpressions.Regex ('("version"\s*:\s*")' + [regex]::Escape($Current) + '(")')
    if (-not $regex.IsMatch($text)) { throw "The version value '$Current' is not in $ManifestPath." }
    $updated = $regex.Replace($text, "`${1}$New`${2}", 1)
    if (($updated | ConvertFrom-Json).version -ne $New) { throw "The version check of $ManifestPath failed; the file was not written." }
    [System.IO.File]::WriteAllText($ManifestPath, $updated, (New-Object System.Text.UTF8Encoding($hasBom)))
}

function Get-Sha256([string]$Path) {
    # .NET instead of Get-FileHash: Get-FileHash of Windows PowerShell 5.1 obeys -WhatIf and returns nothing then
    $sha    = [System.Security.Cryptography.SHA256]::Create()
    $stream = [System.IO.File]::OpenRead($Path)
    try { return [System.BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-', '') }
    finally { $stream.Dispose(); $sha.Dispose() }
}

function Get-TreeHash([string]$Path) {
    # one line per file: the relative path and the SHA-256, in a fixed order
    $root  = (Resolve-Path -LiteralPath $Path).ProviderPath.TrimEnd('\')
    $lines = Get-ChildItem -LiteralPath $root -Recurse -Force -File | ForEach-Object {
        $relative = $_.FullName.Substring($root.Length + 1)
        "$($relative.ToLowerInvariant())|$(Get-Sha256 $_.FullName)"
    } | Sort-Object
    return (@($lines) -join "`n")
}

function Get-EntryVersion($Entry) {
    # an entry of a relative-path plugin can have no version
    $property = $Entry.PSObject.Properties['version']
    if ($property) { return [string]$property.Value }
    return $null
}

function Invoke-ClaudeValidate([string]$Path, [string]$Label, [switch]$Strict) {
    $arguments = @('plugin', 'validate', $Path)
    if ($Strict) { $arguments += '--strict' }
    $output = & claude @arguments 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0) { throw "claude $($arguments -join ' ') failed:`n$output" }
    Write-Verbose $output
    Write-Host "Validated $Label"
}

function Get-StringProperty($Object, [string]$Name, [string]$Context) {
    $property = $Object.PSObject.Properties[$Name]
    if (-not $property -or -not ($property.Value -is [string]) -or [string]::IsNullOrWhiteSpace($property.Value)) {
        throw "$Context has no text value '$Name'."
    }
    return [string]$property.Value
}

function Test-IsInside([string]$Path, [string]$Root) {
    return $Path.StartsWith("$Root\", [StringComparison]::OrdinalIgnoreCase)
}

# --- Target ------------------------------------------------------------------------

if ([string]::IsNullOrWhiteSpace($Destination)) {
    throw 'Give the target marketplace folder with -Destination or with the environment variable WABLAZOR_PLUGIN_MARKETPLACE.'
}
if (-not (Test-Path -LiteralPath $Destination -PathType Container)) {
    throw "The target marketplace folder $Destination does not exist. Create it first."
}
$Destination = (Resolve-Path -LiteralPath $Destination).ProviderPath.TrimEnd('\')
if ($Destination -eq $sourceRoot -or (Test-IsInside $Destination $sourceRoot) -or (Test-IsInside $sourceRoot $Destination)) {
    throw "The target marketplace folder $Destination must not contain or be inside the source folder $sourceRoot."
}

if (-not $SkipValidation -and -not (Get-Command claude -ErrorAction SilentlyContinue)) {
    throw 'The Claude Code CLI (claude) is not on the PATH. Install it, or use -SkipValidation.'
}

# --- Plugins -----------------------------------------------------------------------

$allPluginDirs = @(Get-ChildItem -Path $pluginsRoot -Directory |
    Where-Object { Test-Path (Join-Path $_.FullName '.claude-plugin\plugin.json') })
if ($PluginName -eq 'all') {
    $pluginDirs = $allPluginDirs
}
else {
    $pluginDirs = @($allPluginDirs | Where-Object { $_.Name -eq $PluginName })
    if ($pluginDirs.Count -eq 0) {
        $known = (@($allPluginDirs | ForEach-Object { $_.Name }) | Sort-Object) -join ', '
        throw "The plugin '$PluginName' is not in $pluginsRoot. Known plugins: $known"
    }
}
if ($pluginDirs.Count -eq 0) { throw "$pluginsRoot holds no plugin." }

# --- Bundle manifest ---------------------------------------------------------------

$bundles = @{}
if (Test-Path -LiteralPath $bundleManifestFile -PathType Leaf) {
    $bundleManifest = Read-JsonFile $bundleManifestFile
    $knownNames     = @($allPluginDirs | ForEach-Object { (Read-JsonFile (Join-Path $_.FullName '.claude-plugin\plugin.json')).name })
    foreach ($property in $bundleManifest.PSObject.Properties) {
        if ($knownNames -notcontains $property.Name) {
            Write-Warning "$bundleManifestFile names the plugin '$($property.Name)', which is not in $pluginsRoot."
        }
        $bundles[$property.Name] = @($property.Value)
    }
}

# --- Marketplace files -------------------------------------------------------------

$sourceMarketplace = Read-JsonFile $sourceMarketplaceFile
$sourceEntries = @{}
foreach ($sourceEntry in @($sourceMarketplace.plugins)) { $sourceEntries[$sourceEntry.name] = $sourceEntry }

$destMarketplaceDir  = Join-Path $Destination '.claude-plugin'
$destMarketplaceFile = Join-Path $destMarketplaceDir 'marketplace.json'
if (Test-Path -LiteralPath $destMarketplaceFile) {
    $marketplace = Read-JsonFile $destMarketplaceFile
    if (-not ($marketplace.PSObject.Properties.Name -contains 'plugins') -or $null -eq $marketplace.plugins) {
        $marketplace | Add-Member -NotePropertyName plugins -NotePropertyValue @() -Force
    }
}
else {
    $marketplace = [pscustomobject][ordered]@{
        name        = $sourceMarketplace.name
        owner       = $sourceMarketplace.owner
        description = $sourceMarketplace.description
        plugins     = @()
    }
    Write-Host "The target has no marketplace.json; the script creates one with the name '$($sourceMarketplace.name)'."
}

function Save-Marketplace {
    if ($PSCmdlet.ShouldProcess($destMarketplaceFile, 'Write marketplace.json')) {
        if (-not (Test-Path -LiteralPath $destMarketplaceDir)) { New-Item -ItemType Directory -Path $destMarketplaceDir | Out-Null }
        $json = $marketplace | ConvertTo-Json -Depth 10
        [System.IO.File]::WriteAllText($destMarketplaceFile, $json + "`r`n", $utf8NoBom)
    }
}

# --- Stage one plugin --------------------------------------------------------------

function New-StagedPlugin($Dir, [string]$Name) {
    # the staged copy is temporary, so -WhatIf and -Confirm do not apply to it
    $stageDir = Join-Path $stagingRoot $Dir.Name
    if (-not (Test-Path -LiteralPath $stagingRoot)) {
        New-Item -ItemType Directory -Path $stagingRoot -WhatIf:$false -Confirm:$false | Out-Null
    }
    Copy-Item -LiteralPath $Dir.FullName -Destination $stageDir -Recurse -WhatIf:$false -Confirm:$false

    if (-not $bundles.ContainsKey($Name)) { return $stageDir }
    foreach ($bundle in $bundles[$Name]) {
        $context = "A bundle of the plugin '$Name' in $bundleManifestFile"
        $source  = Get-StringProperty $bundle 'source' $context
        $target  = Get-StringProperty $bundle 'target' $context
        if ([System.IO.Path]::IsPathRooted($target)) { throw "$context has the absolute target '$target'. Give a path relative to the plugin root." }

        $sourceFull = [System.IO.Path]::GetFullPath((Join-Path $sourceRoot $source)).TrimEnd('\')
        if (-not (Test-Path -LiteralPath $sourceFull)) { throw "The bundle source '$source' of the plugin '$Name' ($sourceFull) does not exist." }

        $targetFull = [System.IO.Path]::GetFullPath((Join-Path $stageDir $target)).TrimEnd('\')
        if (-not (Test-IsInside $targetFull $stageDir)) { throw "The bundle target '$target' of the plugin '$Name' is outside the plugin root." }
        if (Test-Path -LiteralPath $targetFull) { throw "The bundle target '$target' of the plugin '$Name' exists in the plugin folder already." }

        $targetParent = Split-Path -Parent $targetFull
        if (-not (Test-Path -LiteralPath $targetParent)) {
            New-Item -ItemType Directory -Path $targetParent -WhatIf:$false -Confirm:$false | Out-Null
        }
        Copy-Item -LiteralPath $sourceFull -Destination $targetFull -Recurse -WhatIf:$false -Confirm:$false
        Write-Host "Bundled $source -> $Name/$($target.Replace('\', '/'))"
    }
    return $stageDir
}

# --- Publish one plugin ----------------------------------------------------------

function Publish-Plugin($Dir) {
    $manifestPath = Join-Path $Dir.FullName '.claude-plugin\plugin.json'
    $manifest     = Read-JsonFile $manifestPath
    $name         = $manifest.name
    if (-not ($manifest.PSObject.Properties.Name -contains 'version') -or -not $manifest.version) {
        throw "The plugin '$name' has no version in its plugin.json."
    }
    $version = [string]$manifest.version
    if ($version -notmatch $semVerPattern) { throw "The version '$version' of the plugin '$name' is not a semantic version (x.y.z)." }

    $stageDir = New-StagedPlugin $Dir $name

    if ($BumpVersion) {
        $newVersion = Get-NextVersion $version $BumpVersion
        # the staged copy always gets the new version, so -WhatIf compares and validates what a real run publishes
        Set-PluginVersion (Join-Path $stageDir '.claude-plugin\plugin.json') $version $newVersion
        if ($PSCmdlet.ShouldProcess($manifestPath, "Set the version $version -> $newVersion")) {
            Set-PluginVersion $manifestPath $version $newVersion
            Write-Host "$name $version -> $newVersion"
        }
        $version = $newVersion
    }

    # validate before the target changes
    if (-not $SkipValidation) { Invoke-ClaudeValidate $stageDir "the staged plugin '$name' $version" -Strict }

    $destSubfolder = "${name}_v$version"
    $destPluginDir = Join-Path $Destination $destSubfolder

    if (Test-Path -LiteralPath $destPluginDir) {
        if ((Get-TreeHash $destPluginDir) -eq (Get-TreeHash $stageDir)) {
            Write-Host "$destSubfolder is up to date"
        }
        else {
            Write-Warning "$destSubfolder exists with other files. Installed copies of $name $version do not update. Raise the version (-BumpVersion) so that the users get the change."
            if ($PSCmdlet.ShouldProcess($destPluginDir, "Delete the folder and copy the plugin '$name' $version again")) {
                Remove-Item -LiteralPath $destPluginDir -Recurse -Force -Confirm:$false
                Copy-Item -LiteralPath $stageDir -Destination $destPluginDir -Recurse
                Write-Host "Replaced $destSubfolder"
            }
        }
    }
    elseif ($PSCmdlet.ShouldProcess($destPluginDir, "Copy the plugin '$name' $version")) {
        Copy-Item -LiteralPath $stageDir -Destination $destPluginDir -Recurse
        Write-Host "Published $destSubfolder"
    }

    $entry = [ordered]@{
        name    = $name
        source  = "./$destSubfolder"
        version = $version
    }
    if ($sourceEntries.ContainsKey($name)) {
        $sourceEntry = $sourceEntries[$name]
        if ($sourceEntry.PSObject.Properties.Name -contains 'description') { $entry.description = $sourceEntry.description }
        if ($sourceEntry.PSObject.Properties.Name -contains 'category')    { $entry.category    = $sourceEntry.category }
    }

    # one entry per plugin, at the position of the first old entry
    $prior    = @(@($marketplace.plugins) | Where-Object { $_.name -eq $name })
    $plugins  = New-Object System.Collections.ArrayList
    $replaced = $false
    foreach ($item in @($marketplace.plugins)) {
        if ($item.name -ne $name) { [void]$plugins.Add($item); continue }
        if (-not $replaced) { [void]$plugins.Add([pscustomobject]$entry); $replaced = $true }
    }
    if (-not $replaced) { [void]$plugins.Add([pscustomobject]$entry) }
    $marketplace.plugins = $plugins.ToArray()
    if ($prior.Count -eq 1 -and (Get-EntryVersion $prior[0]) -eq $version) {
        Write-Host "$name $version is registered already"
    }
    elseif ($prior.Count -gt 0) {
        $priorVersions = (@($prior | ForEach-Object { Get-EntryVersion $_ } | Where-Object { $_ }) | Sort-Object -Unique) -join ', '
        Write-Host "Updated $name to $version in marketplace.json (was: $priorVersions)"
    }
    else {
        Write-Host "Registered $name $version in marketplace.json"
    }
    Save-Marketplace
}

# --- Publish -----------------------------------------------------------------------

try {
    foreach ($dir in $pluginDirs) { Publish-Plugin $dir }
}
finally {
    if (Test-Path -LiteralPath $stagingRoot) {
        Remove-Item -LiteralPath $stagingRoot -Recurse -Force -WhatIf:$false -Confirm:$false
    }
}

# not strict: the warnings of the other plugins in the target are not a reason to fail
if (-not $SkipValidation -and -not $WhatIfPreference) { Invoke-ClaudeValidate $Destination "the target marketplace $Destination" }

Write-Host "Done. The users run 'claude plugin marketplace update $($marketplace.name)', then 'claude plugin install <plugin>@$($marketplace.name)' or 'claude plugin update <plugin>@$($marketplace.name)'."
