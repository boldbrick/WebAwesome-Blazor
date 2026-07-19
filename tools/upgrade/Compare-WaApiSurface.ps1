<#
.SYNOPSIS
Compares two Web Awesome API surface documents and produces a structured change report.

.DESCRIPTION
Takes two surface JSON files produced by Export-WaApiSurface.ps1 (the currently bound
version and the upgrade target) and computes the API delta: components added/removed,
and per surviving component the added/removed/changed attributes, events, slots,
documented methods, and CSS parts.

Outputs a machine-readable JSON report (consumed by the wa-upgrade skill to plan
implementation work) and optionally a human-readable Markdown summary.

.PARAMETER FromPath
Surface JSON of the version currently bound (baseline).

.PARAMETER ToPath
Surface JSON of the upgrade target version.

.PARAMETER OutputPath
Where to write the JSON report. Defaults to temp\wa-api\changes_<from>_to_<to>.json.

.PARAMETER MarkdownPath
Optional path for a Markdown summary of the changes.

.EXAMPLE
.\Compare-WaApiSurface.ps1 -FromPath temp\wa-api\surface_3.0.0.json -ToPath temp\wa-api\surface_3.1.0.json -MarkdownPath temp\wa-api\changes.md
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$FromPath,

    [Parameter(Mandatory = $true)]
    [string]$ToPath,

    [string]$OutputPath,

    [string]$MarkdownPath,

    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $FromPath)) { throw "Baseline surface not found: $FromPath" }
if (-not (Test-Path $ToPath)) { throw "Target surface not found: $ToPath" }

$from = Get-Content $FromPath -Raw | ConvertFrom-Json
$to = Get-Content $ToPath -Raw | ConvertFrom-Json

# ------ helpers ------

function Get-Keys($obj) {
    # works for both deserialized PSCustomObjects and in-memory (ordered) hashtables
    if ($null -eq $obj) { return @() }
    if ($obj -is [System.Collections.IDictionary]) { return @($obj.Keys) }
    return @($obj.PSObject.Properties.Name)
}

function Compare-Map($fromMap, $toMap, [scriptblock]$isChanged) {
    # compares two name->detail maps; returns added/removed/changed name lists with details
    $fromKeys = Get-Keys $fromMap
    $toKeys = Get-Keys $toMap

    $added = [ordered]@{}
    foreach ($k in ($toKeys | Where-Object { $fromKeys -notcontains $_ })) { $added[$k] = $toMap.$k }

    $removed = @($fromKeys | Where-Object { $toKeys -notcontains $_ })

    $changed = [ordered]@{}
    foreach ($k in ($toKeys | Where-Object { $fromKeys -contains $_ })) {
        if (& $isChanged $fromMap.$k $toMap.$k) {
            $changed[$k] = [ordered]@{ from = $fromMap.$k; to = $toMap.$k }
        }
    }

    return [ordered]@{ added = $added; removed = $removed; changed = $changed }
}

function Test-HasContent($section) {
    return ($section.added.Count -gt 0) -or ($section.removed.Count -gt 0) -or ($section.changed.Count -gt 0)
}

$attrChanged = { param($a, $b) ($a.type -ne $b.type) -or ($a.default -ne $b.default) }
$eventChanged = { param($a, $b) ($a.type -ne $b.type) -or ($a.eventName -ne $b.eventName) }
$slotChanged = { param($a, $b) $false }   # slot identity is its name; description changes are not API changes
$methodChanged = { param($a, $b) $a.signature -ne $b.signature }

# ------ compute the delta ------

$fromTags = Get-Keys $from.components
$toTags = Get-Keys $to.components

$addedComponents = [ordered]@{}
foreach ($tag in ($toTags | Where-Object { $fromTags -notcontains $_ } | Sort-Object)) {
    $addedComponents[$tag] = $to.components.$tag
}

$removedComponents = @($fromTags | Where-Object { $toTags -notcontains $_ } | Sort-Object)

$modifiedComponents = [ordered]@{}
foreach ($tag in ($toTags | Where-Object { $fromTags -contains $_ } | Sort-Object)) {
    $f = $from.components.$tag
    $t = $to.components.$tag

    $attributes = Compare-Map $f.attributes $t.attributes $attrChanged
    $events = Compare-Map $f.events $t.events $eventChanged
    $slots = Compare-Map $f.slots $t.slots $slotChanged
    $methods = Compare-Map $f.methods $t.methods $methodChanged

    $fromParts = @($f.cssParts)
    $toParts = @($t.cssParts)
    $cssParts = [ordered]@{
        added   = @($toParts | Where-Object { $fromParts -notcontains $_ })
        removed = @($fromParts | Where-Object { $toParts -notcontains $_ })
        changed = @()
    }

    $delta = [ordered]@{}
    if (Test-HasContent $attributes) { $delta['attributes'] = $attributes }
    if (Test-HasContent $events) { $delta['events'] = $events }
    if (Test-HasContent $slots) { $delta['slots'] = $slots }
    if (Test-HasContent $methods) { $delta['methods'] = $methods }
    if (($cssParts.added.Count -gt 0) -or ($cssParts.removed.Count -gt 0)) { $delta['cssParts'] = $cssParts }
    if ($f.status -ne $t.status) { $delta['status'] = [ordered]@{ from = $f.status; to = $t.status } }

    if ($delta.Count -gt 0) { $modifiedComponents[$tag] = $delta }
}

# breaking = anything removed or with a changed contract; used for phase planning
$breaking = @()
$breaking += @($removedComponents | ForEach-Object { "component removed: $_" })
foreach ($tag in $modifiedComponents.Keys) {
    $d = $modifiedComponents[$tag]
    foreach ($section in 'attributes', 'events', 'slots', 'methods') {
        if ($d.Contains($section)) {
            $breaking += @($d[$section].removed | ForEach-Object { "$tag - $section removed: $_" })
            $breaking += @($d[$section].changed.Keys | ForEach-Object { "$tag - $section changed: $_" })
        }
    }
}

$report = [ordered]@{
    fromVersion        = $from.version
    toVersion          = $to.version
    summary            = [ordered]@{
        addedComponents    = $addedComponents.Count
        removedComponents  = $removedComponents.Count
        modifiedComponents = $modifiedComponents.Count
        breakingChanges    = $breaking.Count
    }
    breakingChanges    = $breaking
    addedComponents    = $addedComponents
    removedComponents  = $removedComponents
    modifiedComponents = $modifiedComponents
}

if (-not $OutputPath) {
    $outDir = Join-Path $RepoRoot 'temp\wa-api'
    if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Force $outDir | Out-Null }
    $OutputPath = Join-Path $outDir "changes_$($from.version)_to_$($to.version).json"
}

$report | ConvertTo-Json -Depth 12 | Out-File $OutputPath -Encoding utf8

# ------ optional Markdown summary ------

if ($MarkdownPath) {
    $md = New-Object System.Text.StringBuilder
    [void]$md.AppendLine("# Web Awesome API changes: $($from.version) -> $($to.version)")
    [void]$md.AppendLine()
    [void]$md.AppendLine("| Metric | Count |")
    [void]$md.AppendLine("|---|---|")
    [void]$md.AppendLine("| New components | $($addedComponents.Count) |")
    [void]$md.AppendLine("| Removed components | $($removedComponents.Count) |")
    [void]$md.AppendLine("| Modified components | $($modifiedComponents.Count) |")
    [void]$md.AppendLine("| Breaking changes | $($breaking.Count) |")
    [void]$md.AppendLine()

    if ($breaking.Count -gt 0) {
        [void]$md.AppendLine("## Breaking changes")
        [void]$md.AppendLine()
        foreach ($b in $breaking) { [void]$md.AppendLine("- $b") }
        [void]$md.AppendLine()
    }

    if ($addedComponents.Count -gt 0) {
        [void]$md.AppendLine("## New components")
        [void]$md.AppendLine()
        foreach ($tag in $addedComponents.Keys) {
            $c = $addedComponents[$tag]
            [void]$md.AppendLine("- ``$tag`` ($($c.className)) - $((Get-Keys $c.attributes).Count) attributes, $((Get-Keys $c.events).Count) events, $((Get-Keys $c.slots).Count) slots, $((Get-Keys $c.methods).Count) methods")
        }
        [void]$md.AppendLine()
    }

    if ($removedComponents.Count -gt 0) {
        [void]$md.AppendLine("## Removed components")
        [void]$md.AppendLine()
        foreach ($tag in $removedComponents) { [void]$md.AppendLine("- ``$tag``") }
        [void]$md.AppendLine()
    }

    if ($modifiedComponents.Count -gt 0) {
        [void]$md.AppendLine("## Modified components")
        [void]$md.AppendLine()
        foreach ($tag in $modifiedComponents.Keys) {
            [void]$md.AppendLine("### ``$tag``")
            [void]$md.AppendLine()
            $d = $modifiedComponents[$tag]
            foreach ($section in $d.Keys) {
                if ($section -eq 'status') {
                    [void]$md.AppendLine("- status: $($d.status.from) -> $($d.status.to)")
                    continue
                }
                $s = $d[$section]
                foreach ($name in (Get-Keys $s.added)) { [void]$md.AppendLine("- ${section}: added ``$name``") }
                foreach ($name in $s.removed) { [void]$md.AppendLine("- ${section}: removed ``$name``") }
                foreach ($name in (Get-Keys $s.changed)) { [void]$md.AppendLine("- ${section}: changed ``$name``") }
            }
            [void]$md.AppendLine()
        }
    }

    $md.ToString() | Out-File $MarkdownPath -Encoding utf8
    Write-Output "Markdown summary written to $MarkdownPath"
}

Write-Output "Change report written to $OutputPath ($($addedComponents.Count) added, $($removedComponents.Count) removed, $($modifiedComponents.Count) modified, $($breaking.Count) breaking)"
return $OutputPath
