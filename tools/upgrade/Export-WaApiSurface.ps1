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
corroborate the CEM events. An attribute typed by a type alias also records the alias-resolved
union as 'resolvedType': aliases come from the release's .d.ts files (the component's own first) and,
for other packages' types (lib.dom, chart.js), from external-type-aliases.json next to this script.

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

# ------ type alias resolution ------
# the CEM keeps attribute types as written in the source, so an attribute typed by an alias
# ('IconAnimation | undefined', 'WaDateInputPlacement') carries no literal values. Aliases declared in
# the release's own .d.ts files as a plain union ("export type IconCanvas = 'fixed' | 'auto';") are
# resolved from there, preferring the component's own .d.ts; aliases of external packages (lib.dom,
# chart.js) come from the curated external-type-aliases.json next to this script. The resolved union
# is recorded as 'resolvedType'; 'type' stays as the CEM has it. A name declared with differing
# unions in several files is ambiguous and stays unresolved.

# a single- or double-quoted literal, a primitive keyword, or a bare identifier
$literalPartPattern = "^(?:'[^']*'|`"[^`"]*`"|null|undefined|string|number|boolean|-?\d+(?:\.\d+)?)$"
$identifierPattern = '^[A-Za-z_]\w*$'
# 'type X = <union>;' whose right-hand side has no object, function or generic syntax
$aliasDeclarationPattern = '(?ms)^\s*(?:export\s+)?(?:declare\s+)?type\s+([A-Za-z_]\w*)\s*=\s*([^;{}()<>\[\]]+?);'

function Split-UnionParts([string]$text) {
    return @($text -split '\|' | ForEach-Object { $_.Trim() } | Where-Object { $_.Length -gt 0 })
}

function Get-AliasDeclarations([string]$text) {
    $found = @{}
    if ($null -eq $text) { return $found }
    foreach ($match in [regex]::Matches($text, $aliasDeclarationPattern)) {
        $parts = Split-UnionParts $match.Groups[2].Value
        $simple = @($parts | Where-Object { $_ -notmatch $literalPartPattern -and $_ -notmatch $identifierPattern }).Count -eq 0
        if ($parts.Count -gt 0 -and $simple) { $found[$match.Groups[1].Value] = ($parts -join ' | ') }
    }
    return $found
}

# every plain-union alias of the release's .d.ts files; $null marks an ambiguous name
$releaseAliases = @{}
$dtsFiles = @()
if (Test-Path $distRoot) {
    $dtsFiles = @(Get-ChildItem $distRoot -Recurse -Filter '*.d.ts' | ForEach-Object { Get-Content $_.FullName -Raw })
}
elseif (Test-Path $distZipPath) {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $aliasZip = [System.IO.Compression.ZipFile]::OpenRead($distZipPath)
    try {
        foreach ($entry in $aliasZip.Entries) {
            if ($entry.FullName -notmatch '^webawesome-zip/dist/.*\.d\.ts$') { continue }
            $reader = New-Object System.IO.StreamReader($entry.Open())
            try { $dtsFiles += $reader.ReadToEnd() } finally { $reader.Dispose() }
        }
    }
    finally {
        $aliasZip.Dispose()
    }
}
foreach ($text in $dtsFiles) {
    $declared = Get-AliasDeclarations $text
    foreach ($name in $declared.Keys) {
        if (-not $releaseAliases.ContainsKey($name)) { $releaseAliases[$name] = $declared[$name] }
        elseif ($null -ne $releaseAliases[$name] -and $releaseAliases[$name] -ne $declared[$name]) { $releaseAliases[$name] = $null }
    }
}

$externalAliases = @{}
$externalAliasPath = Join-Path $PSScriptRoot 'external-type-aliases.json'
if (Test-Path $externalAliasPath) {
    $externalJson = Get-Content $externalAliasPath -Raw | ConvertFrom-Json
    foreach ($property in $externalJson.aliases.PSObject.Properties) { $externalAliases[$property.Name] = $property.Value.union }
}

# resolves the alias parts of a CEM type; $null when the type references no resolvable alias
function Resolve-TypeText([string]$typeText, [hashtable]$localAliases) {
    if ([string]::IsNullOrWhiteSpace($typeText)) { return $null }
    $parts = Split-UnionParts $typeText
    # only a flat union can be resolved part by part
    if (@($parts | Where-Object { $_ -notmatch $literalPartPattern -and $_ -notmatch $identifierPattern }).Count -gt 0) { return $null }

    $state = @{ resolvedAny = $false; result = (New-Object System.Collections.Generic.List[string]) }
    foreach ($part in $parts) { Expand-TypePart $part $localAliases 0 $state }
    if (-not $state.resolvedAny) { return $null }
    return ($state.result -join ' | ')
}

# appends one union part to $state.result, replacing a known alias by its members in place (source order kept)
function Expand-TypePart([string]$part, [hashtable]$localAliases, [int]$depth, [hashtable]$state) {
    $definition = $null
    if ($part -notmatch $literalPartPattern -and $depth -lt 5) {
        if ($localAliases.ContainsKey($part)) { $definition = $localAliases[$part] }
        elseif ($releaseAliases.ContainsKey($part)) { $definition = $releaseAliases[$part] }
        elseif ($externalAliases.ContainsKey($part)) { $definition = $externalAliases[$part] }
    }
    if ($null -eq $definition) {
        if (-not $state.result.Contains($part)) { $state.result.Add($part) }
        return
    }
    $state.resolvedAny = $true
    foreach ($inner in (Split-UnionParts $definition)) { Expand-TypePart $inner $localAliases ($depth + 1) $state }
}

# ------ build the surface ------

$components = [ordered]@{}

foreach ($module in $cem.modules) {
    foreach ($decl in @($module.declarations)) {
        if ($null -eq $decl) { continue }
        if (-not $decl.customElement) { continue }
        if ([string]::IsNullOrEmpty($decl.tagName)) { continue }

        # aliases of the component's own .d.ts win over same-named aliases elsewhere
        $moduleDtsPath = ($module.path -replace '^_bundle_/src/', '') -replace '\.js$', '.d.ts'
        $localAliases = Get-AliasDeclarations (Get-DistText $moduleDtsPath)

        $attributes = [ordered]@{}
        foreach ($attr in (@($decl.attributes) | Where-Object { $_ -and $_.name } | Sort-Object name)) {
            $typeText = Get-TypeText $attr.type
            $attributes[$attr.name] = [ordered]@{
                type        = $typeText
                default     = $attr.default
                description = $attr.description
            }
            $resolvedType = Resolve-TypeText $typeText $localAliases
            if ($null -ne $resolvedType) { $attributes[$attr.name]['resolvedType'] = $resolvedType }
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
