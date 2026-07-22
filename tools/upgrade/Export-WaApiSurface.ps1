<#
.SYNOPSIS
Exports a simplified, diffable API surface from a Web Awesome custom-elements.json manifest.

.DESCRIPTION
Reads the Custom Elements Manifest (CEM) of a Web Awesome release - either from an already
extracted source tree, an explicit manifest path, or directly out of the release zip - and
produces a deterministic, sorted JSON document describing every custom element: attributes
(with type and default), named events, slots, documented public methods, and CSS parts.

The output drives two consumers:
  * Compare-WaApiSurface.ps1 - diffing two versions to plan an upgrade
  * WebAwesome.Blazor.Tests API parity tests - verifying wrappers cover the surface

.PARAMETER Version
Web Awesome version, e.g. 3.1.0. Used to locate the manifest when -CemPath is not given
and stamped into the output document.

.PARAMETER CemPath
Explicit path to a custom-elements.json. Optional; when omitted the manifest is read from
temp\wa-src\<version>\dist\custom-elements.json if extracted, otherwise directly from
temp\download\webawesome_<version>.zip.

.PARAMETER OutputPath
Where to write the surface JSON. Defaults to temp\wa-api\surface_<version>.json.

.EXAMPLE
.\Export-WaApiSurface.ps1 -Version 3.1.0
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+(-[0-9A-Za-z\.\-]+)?$')]
    [string]$Version,

    [string]$CemPath,

    [string]$OutputPath,

    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
)

$ErrorActionPreference = 'Stop'

# ------ locate and load the manifest ------

$cemJson = $null
if ($CemPath) {
    if (-not (Test-Path $CemPath)) { throw "Manifest not found: $CemPath" }
    $cemJson = Get-Content $CemPath -Raw
}
else {
    $extracted = Join-Path $RepoRoot "temp\wa-src\$Version\dist\custom-elements.json"
    if (Test-Path $extracted) {
        $cemJson = Get-Content $extracted -Raw
    }
    else {
        # read the manifest straight out of the release zip without full extraction
        $zipPath = Join-Path $RepoRoot "temp\download\webawesome_$Version.zip"
        if (-not (Test-Path $zipPath)) { throw "Neither extracted source nor zip found for $Version ($zipPath)" }
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $zip = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
        try {
            $entry = $zip.Entries | Where-Object { $_.FullName -eq 'webawesome-zip/dist/custom-elements.json' } | Select-Object -First 1
            if ($null -eq $entry) { throw "custom-elements.json not found inside $zipPath" }
            $reader = New-Object System.IO.StreamReader($entry.Open())
            try { $cemJson = $reader.ReadToEnd() } finally { $reader.Dispose() }
        }
        finally {
            $zip.Dispose()
        }
    }
}

$cem = $cemJson | ConvertFrom-Json

# ------ Pro component list ------
# the CEM carries no Pro marker; tools\upgrade\pro-components.json is the curated source of truth
# (maintained per upgrade - see the comment inside that file)
$proTags = @()
$proListPath = Join-Path $PSScriptRoot 'pro-components.json'
if (Test-Path $proListPath) {
    $proTags = @((Get-Content $proListPath -Raw | ConvertFrom-Json).proComponents)
}

# ------ helpers ------

function Get-TypeText($typeObj) {
    if ($null -eq $typeObj) { return $null }
    return $typeObj.text
}

# ------ build the surface ------

$components = [ordered]@{}

foreach ($module in $cem.modules) {
    foreach ($decl in @($module.declarations)) {
        if ($null -eq $decl) { continue }
        if (-not $decl.customElement) { continue }
        if ([string]::IsNullOrEmpty($decl.tagName)) { continue }

        $attributes = [ordered]@{}
        foreach ($attr in (@($decl.attributes) | Where-Object { $_ -and $_.name } | Sort-Object name)) {
            $attributes[$attr.name] = [ordered]@{
                type        = Get-TypeText $attr.type
                default     = $attr.default
                description = $attr.description
            }
        }

        # only named event entries are part of the public surface
        $events = [ordered]@{}
        foreach ($evt in (@($decl.events) | Where-Object { $_ -and $_.name } | Sort-Object name)) {
            $events[$evt.name] = [ordered]@{
                type        = Get-TypeText $evt.type
                eventName   = $evt.eventName
                description = $evt.description
            }
        }

        # the default (unnamed) slot is keyed "(default)" - PowerShell 5.1 JSON cannot round-trip empty property names
        $slots = [ordered]@{}
        foreach ($slot in (@($decl.slots) | Where-Object { $null -ne $_ } | Sort-Object name)) {
            $slotName = $slot.name
            if ([string]::IsNullOrEmpty($slotName)) { $slotName = '(default)' }
            $slots[$slotName] = $slot.description
        }

        # documented public methods only - undocumented members are internals
        $methods = [ordered]@{}
        $methodCandidates = @($decl.members) | Where-Object {
            $_ -and $_.kind -eq 'method' -and $_.description -and
            $_.privacy -ne 'private' -and $_.privacy -ne 'protected'
        } | Sort-Object name
        foreach ($method in $methodCandidates) {
            $methods[$method.name] = [ordered]@{
                signature   = Get-TypeText $method.type
                description = $method.description
            }
        }

        $cssParts = @(@($decl.cssParts) | Where-Object { $_ -and $_.name } | Sort-Object name | ForEach-Object { $_.name })

        $components[$decl.tagName] = [ordered]@{
            className  = $decl.name
            since      = $decl.since
            status     = $decl.status
            pro        = ($proTags -contains $decl.tagName)
            attributes = $attributes
            events     = $events
            slots      = $slots
            methods    = $methods
            cssParts   = $cssParts
        }
    }
}

# sort components by tag name for deterministic output
$sorted = [ordered]@{}
foreach ($tag in ($components.Keys | Sort-Object)) { $sorted[$tag] = $components[$tag] }

$surface = [ordered]@{
    version    = $Version
    generated  = 'Export-WaApiSurface.ps1'
    components = $sorted
}

if (-not $OutputPath) {
    $outDir = Join-Path $RepoRoot 'temp\wa-api'
    if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Force $outDir | Out-Null }
    $OutputPath = Join-Path $outDir "surface_$Version.json"
}

$surface | ConvertTo-Json -Depth 10 | Out-File $OutputPath -Encoding utf8
Write-Output "API surface for $Version ($($sorted.Count) components) written to $OutputPath"
return $OutputPath
