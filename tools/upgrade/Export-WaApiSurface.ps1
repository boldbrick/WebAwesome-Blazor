<#
.SYNOPSIS
Exports a simplified, diffable API surface from a Web Awesome custom-elements.json manifest.

.DESCRIPTION
Reads the Custom Elements Manifest (CEM) of a Web Awesome release - either from an already
extracted source tree, an explicit manifest path, or directly out of the release zip - and
produces a deterministic, sorted JSON document describing every custom element: attributes
(with type and default), named events, slots, documented public methods, and CSS parts.
Because the CEM also lists events the element never dispatches (e.g. the wa-data-grid 'request'
extraction artifact), each component also records the event names of its own @event JSDoc
('jsDocEvents', from dist\components\<name>\<name>.d.ts), and the document records every event
name that has an event class in dist\events ('declaredEventTypes'). The parity tests use both to
corroborate the CEM events.

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
# the CEM carries no Pro marker. Since Web Awesome 3.3.0 the release zip bundles reference docs
# (dist/skills/webawesome/references/components/<name>.md) whose first heading marks Pro
# components with "[Pro]" - that is the authoritative, versioned source. For older releases
# (no bundled references) tools\upgrade\pro-components.json is the curated fallback.
$proTags = @()

# prefer the extracted source tree, then the release zip
$refsDir = Join-Path $RepoRoot "temp\wa-src\$Version\dist\skills\webawesome\references\components"
if (Test-Path $refsDir) {
    foreach ($file in Get-ChildItem $refsDir -Filter '*.md') {
        $firstLine = Get-Content $file.FullName -TotalCount 1
        if ($firstLine -match '\[Pro\]') { $proTags += "wa-$($file.BaseName)" }
    }
}
else {
    $zipPath = Join-Path $RepoRoot "temp\download\webawesome_$Version.zip"
    if (Test-Path $zipPath) {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $zip = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
        try {
            foreach ($entry in $zip.Entries) {
                if ($entry.FullName -match '(^|/)dist/skills/webawesome/references/components/([a-z0-9\-]+)\.md$') {
                    $componentName = $Matches[2]
                    $reader = New-Object System.IO.StreamReader($entry.Open())
                    try { $firstLine = $reader.ReadLine() } finally { $reader.Dispose() }
                    if ($firstLine -match '\[Pro\]') { $proTags += "wa-$componentName" }
                }
            }
        }
        finally {
            $zip.Dispose()
        }
    }
}

if ($proTags.Count -gt 0) {
    Write-Verbose "Pro components derived from the bundled reference docs: $($proTags -join ', ')"
}
else {
    # pre-3.3.0 release (or no zip available): fall back to the curated list
    $proListPath = Join-Path $PSScriptRoot 'pro-components.json'
    if (Test-Path $proListPath) {
        $proTags = @((Get-Content $proListPath -Raw | ConvertFrom-Json).proComponents)
        Write-Verbose "Pro components taken from the curated fallback list: $($proTags -join ', ')"
    }
}

# ------ helpers ------

function Get-TypeText($typeObj) {
    if ($null -eq $typeObj) { return $null }
    return $typeObj.text
}

# reads a text file of the release's dist folder (path relative to dist, '/'-separated) from the
# extracted source tree, else from the release zip; $null when neither has it
$distRoot = Join-Path $RepoRoot "temp\wa-src\$Version\dist"
$distZipPath = Join-Path $RepoRoot "temp\download\webawesome_$Version.zip"
$distZip = $null
function Get-DistText([string]$relativePath) {
    $extractedPath = Join-Path $distRoot ($relativePath -replace '/', '\')
    if (Test-Path $extractedPath) { return Get-Content $extractedPath -Raw }
    if (-not (Test-Path $distZipPath)) { return $null }
    if ($null -eq $script:distZip) {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $script:distZip = [System.IO.Compression.ZipFile]::OpenRead($distZipPath)
    }
    $entry = $script:distZip.GetEntry("webawesome-zip/dist/$relativePath")
    if ($null -eq $entry) { return $null }
    $reader = New-Object System.IO.StreamReader($entry.Open())
    try { return $reader.ReadToEnd() } finally { $reader.Dispose() }
}

# the event names a component's own @event JSDoc declares (the component .d.ts next to the CEM module),
# e.g. " * @event {{ item: WaAccordionItem }} wa-expand - ..." -> wa-expand; $null when the .d.ts is missing
function Get-JsDocEvents([string]$modulePath) {
    $dtsPath = ($modulePath -replace '^_bundle_/src/', '') -replace '\.js$', '.d.ts'
    $text = Get-DistText $dtsPath
    if ($null -eq $text) { return $null }
    $names = foreach ($match in [regex]::Matches($text, '(?m)^\s*\*\s*@event\s+(?:\{.*\}\s+)?([A-Za-z][\w-]*)')) { $match.Groups[1].Value }
    # the comma keeps an empty result an (empty) array instead of unrolling it to $null
    return ,@($names | Sort-Object -Unique)
}

# the event names that have an event class in dist\events: every file events.d.ts re-exports maps its
# event name onto the class in GlobalEventHandlersEventMap ("'wa-show': WaShowEvent;")
function Get-DeclaredEventTypes {
    $index = Get-DistText 'events/events.d.ts'
    if ($null -eq $index) { return $null }
    $names = foreach ($export in [regex]::Matches($index, "from '\./([\w-]+)\.js'")) {
        $text = Get-DistText "events/$($export.Groups[1].Value).d.ts"
        if ($null -eq $text) { throw "events/$($export.Groups[1].Value).d.ts is exported by events.d.ts but missing" }
        foreach ($entry in [regex]::Matches($text, "'([^']+)'\s*:\s*\w+\s*;")) { $entry.Groups[1].Value }
    }
    # the comma keeps an empty result an (empty) array instead of unrolling it to $null
    return ,@($names | Sort-Object -Unique)
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
            jsDocEvents = Get-JsDocEvents $module.path
            slots      = $slots
            methods    = $methods
            cssParts   = $cssParts
        }
    }
}

# sort components by tag name for deterministic output
$sorted = [ordered]@{}
foreach ($tag in ($components.Keys | Sort-Object)) { $sorted[$tag] = $components[$tag] }

$declaredEventTypes = Get-DeclaredEventTypes
if ($null -ne $distZip) { $distZip.Dispose() }

$surface = [ordered]@{
    version            = $Version
    generated          = 'Export-WaApiSurface.ps1'
    declaredEventTypes = $declaredEventTypes
    components         = $sorted
}

if (-not $OutputPath) {
    $outDir = Join-Path $RepoRoot 'temp\wa-api'
    if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Force $outDir | Out-Null }
    $OutputPath = Join-Path $outDir "surface_$Version.json"
}

$surface | ConvertTo-Json -Depth 10 | Out-File $OutputPath -Encoding utf8
Write-Output "API surface for $Version ($($sorted.Count) components) written to $OutputPath"
return $OutputPath
