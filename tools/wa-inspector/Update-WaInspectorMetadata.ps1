<#
.SYNOPSIS
    Generates the element metadata of the WA Inspector extension (metadata\wa-metadata.json in the extension).

.DESCRIPTION
    The inspector builds its forms from one JSON file:
    - the attributes of every wa- element (type, options, default, description) from the committed API surface
      (src\WebAwesome.Blazor.Tests\ApiParity\expected-api-surface.json), which already resolves the type aliases;
    - the CSS custom properties and the summaries from the custom elements manifest of the release dist;
    - the utility classes from dist\styles\utilities\*.css;
    - the theme tokens from dist\styles\themes\default.css.
    Run it after each Web Awesome upgrade, when the API surface has the new version.
    The extension is in the assets of the Claude Code plugin wablazor:
    tools\wablazor-marketplace\plugins\wablazor\assets\wa-inspector-extension. Publish a new plugin version after
    a change of the metadata, so that the users get it.
    Keep this script ASCII-only: Windows PowerShell 5.1 misparses BOM-less non-ASCII scripts.

.PARAMETER DistPath
    The dist folder of the Web Awesome release (custom-elements.json and styles\). Default: the extracted release
    under temp\wa-src\<version>\dist of the workspace.

.PARAMETER SurfacePath
    The API surface JSON. Default: the committed expected-api-surface.json.

.PARAMETER OutputPath
    The metadata file to write. Default: metadata\wa-metadata.json in the extension folder of the plugin wablazor.

.EXAMPLE
    .\tools\wa-inspector\Update-WaInspectorMetadata.ps1
#>
[CmdletBinding()]
param(
    [string] $DistPath,
    [string] $SurfacePath,
    [string] $OutputPath
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
if (-not $SurfacePath) { $SurfacePath = Join-Path $repoRoot 'src\WebAwesome.Blazor.Tests\ApiParity\expected-api-surface.json' }
if (-not $OutputPath) {
    $OutputPath = Join-Path $repoRoot 'tools\wablazor-marketplace\plugins\wablazor\assets\wa-inspector-extension\metadata\wa-metadata.json'
}

$surface = Get-Content $SurfacePath -Raw | ConvertFrom-Json
$version = $surface.version
if (-not $DistPath) { $DistPath = Join-Path $repoRoot "temp\wa-src\$version\dist" }
if (-not (Test-Path (Join-Path $DistPath 'custom-elements.json'))) {
    throw "No custom-elements.json under '$DistPath'. Pass -DistPath (the dist folder of the $version release)."
}

# attributes that are no review settings: SSR bookkeeping, text direction and language
$skippedAttributes = @('did-ssr', 'dir', 'lang', 'custom-error')

# the long size names are deprecated aliases of the short ones; offer the short ones only
$deprecatedSizeAliases = @('small', 'medium', 'large')

function Get-Unquoted([string] $text) {
    if ($null -eq $text) { return $null }
    $trimmed = $text.Trim()
    if ($trimmed.Length -ge 2 -and (($trimmed.StartsWith("'") -and $trimmed.EndsWith("'")) -or ($trimmed.StartsWith('"') -and $trimmed.EndsWith('"')))) {
        return $trimmed.Substring(1, $trimmed.Length - 2)
    }
    return $trimmed
}

# turns a TypeScript type text into a form field kind: enum (with options), boolean, number, string, or none
function ConvertTo-FieldType([string] $typeText) {
    if (-not $typeText) { return @{ kind = 'string' } }
    $normalized = ($typeText -replace '\s+', ' ').Trim().TrimStart('|').Trim()
    if ($normalized -match '=>') { return @{ kind = 'none' } }

    $parts = @($normalized -split '\|' | ForEach-Object { $_.Trim() } | Where-Object { $_ -and $_ -notin @('undefined', 'null') })
    $literals = @($parts | Where-Object { $_ -match "^'[^']*'$" } | ForEach-Object { Get-Unquoted $_ })
    $numbers = @($parts | Where-Object { $_ -match '^-?\d+(\.\d+)?$' })
    $others = @($parts | Where-Object { $_ -notmatch "^'[^']*'$" -and $_ -notmatch '^-?\d+(\.\d+)?$' })

    if ($parts.Count -eq 1 -and $parts[0] -eq 'boolean') { return @{ kind = 'boolean' } }
    if ($literals.Count -gt 0 -or $numbers.Count -gt 1) {
        $options = @($literals + $numbers)
        if (($options | Where-Object { $_ -in @('s', 'm', 'l') }).Count -gt 0) {
            $options = @($options | Where-Object { $_ -notin $deprecatedSizeAliases })
        }
        return @{ kind = 'enum'; options = $options; custom = ($others -contains 'string') }
    }
    if ($others.Count -eq 1 -and $others[0] -eq 'number') { return @{ kind = 'number' } }
    if ($others -contains 'number' -and $others -contains 'string') { return @{ kind = 'string' } }
    if ($others.Count -ge 1 -and ($others | Where-Object { $_ -notin @('string', 'number', 'boolean') }).Count -gt 0) {
        # arrays, objects and elements cannot be set as an attribute by hand
        if ($others -contains 'string') { return @{ kind = 'string' } }
        return @{ kind = 'none' }
    }
    return @{ kind = 'string' }
}

function Get-ShortText([string] $text, [int] $limit = 280) {
    if (-not $text) { return $null }
    $flat = ($text -replace '\s+', ' ').Trim()
    if ($flat.Length -le $limit) { return $flat }
    return $flat.Substring(0, $limit - 3) + '...'
}

# the CEM: summaries and CSS custom properties per tag
$cem = Get-Content (Join-Path $DistPath 'custom-elements.json') -Raw | ConvertFrom-Json
$declarations = @{}
foreach ($module in $cem.modules) {
    foreach ($declaration in $module.declarations) {
        if ($declaration.tagName) { $declarations[$declaration.tagName] = $declaration }
    }
}

$elements = [ordered]@{}
foreach ($tag in ($surface.components.PSObject.Properties.Name | Sort-Object)) {
    $component = $surface.components.$tag
    $declaration = $declarations[$tag]

    $attributes = @()
    foreach ($property in ($component.attributes.PSObject.Properties | Sort-Object Name)) {
        $name = $property.Name
        if ($name -in $skippedAttributes) { continue }
        $attribute = $property.Value
        $typeText = if ($attribute.resolvedType) { $attribute.resolvedType } else { $attribute.type }
        $field = ConvertTo-FieldType $typeText
        if ($field.kind -eq 'none') { continue }

        $entry = [ordered]@{ name = $name; kind = $field.kind }
        if ($field.kind -eq 'enum') {
            $entry.options = $field.options
            if ($field.custom) { $entry.custom = $true }
        }
        $default = Get-Unquoted $attribute.default
        if ($null -ne $default -and $default -ne 'null' -and $default -ne 'undefined') { $entry.default = $default }
        $description = Get-ShortText $attribute.description
        if ($description) { $entry.description = $description }
        $attributes += $entry
    }

    $cssProperties = @()
    if ($declaration -and $declaration.cssProperties) {
        foreach ($cssProperty in ($declaration.cssProperties | Sort-Object name)) {
            if ($cssProperty.deprecated) { continue }
            $entry = [ordered]@{ name = $cssProperty.name }
            $description = Get-ShortText $cssProperty.description
            if ($description) { $entry.description = $description }
            if ($cssProperty.default) { $entry.default = $cssProperty.default }
            $cssProperties += $entry
        }
    }

    $cssParts = @()
    if ($declaration -and $declaration.cssParts) {
        foreach ($cssPart in ($declaration.cssParts | Sort-Object name)) {
            if ($cssPart.deprecated) { continue }
            $entry = [ordered]@{ name = $cssPart.name }
            $description = Get-ShortText $cssPart.description
            if ($description) { $entry.description = $description }
            $cssParts += $entry
        }
    }

    $slots = @()
    if ($declaration -and $declaration.slots) {
        foreach ($slot in $declaration.slots) {
            $entry = [ordered]@{ name = $slot.name }
            $description = Get-ShortText $slot.description
            if ($description) { $entry.description = $description }
            $slots += $entry
        }
    }

    $element = [ordered]@{}
    $summary = if ($declaration) { Get-ShortText ($(if ($declaration.summary) { $declaration.summary } else { $declaration.description })) } else { $null }
    if ($summary) { $element.summary = $summary }
    if ($component.pro) { $element.pro = $true }
    $element.attributes = $attributes
    $element.cssProperties = $cssProperties
    $element.cssParts = $cssParts
    $element.slots = $slots
    $elements[$tag] = $element
}

# utility classes: the class selectors of each utilities file, grouped by file name
$utilityClasses = [ordered]@{}
$utilitiesPath = Join-Path $DistPath 'styles\utilities'
foreach ($file in (Get-ChildItem $utilitiesPath -Filter '*.css' | Sort-Object Name)) {
    $css = Get-Content $file.FullName -Raw
    $names = [regex]::Matches($css, '\.(wa-[a-z0-9-]+)') | ForEach-Object { $_.Groups[1].Value }
    if ($file.BaseName -eq 'layout') {
        # the layout primitives match by substring ([class*='wa-flank']) and take modifiers ([class*='\:end'])
        $primitives = [regex]::Matches($css, "\[class\*='(wa-[a-z]+)'\]") | ForEach-Object { $_.Groups[1].Value }
        $modifiers = [regex]::Matches($css, "\[class\*='(wa-[a-z]+)'\]\[class\*='\\:([a-z]+)'\]") |
            ForEach-Object { '{0}:{1}' -f $_.Groups[1].Value, $_.Groups[2].Value }
        $names = @($primitives) + @($modifiers) + @($names)
    }
    $unique = @($names | Where-Object { $_ } | Sort-Object -Unique)
    if ($unique.Count -gt 0) { $utilityClasses[$file.BaseName] = $unique }
}

# theme tokens: the first declaration of each custom property in the default theme
$themeTokens = @()
$seen = @{}
$themeCss = Get-Content (Join-Path $DistPath 'styles\themes\default.css') -Raw
foreach ($match in [regex]::Matches($themeCss, '(--wa-[a-z0-9-]+)\s*:\s*([^;]+);')) {
    $name = $match.Groups[1].Value
    if ($seen.ContainsKey($name)) { continue }
    $seen[$name] = $true
    $value = ($match.Groups[2].Value -replace '\s+', ' ').Trim()
    $group = ($name -replace '^--wa-', '') -replace '-.*$', ''
    $themeTokens += [ordered]@{ name = $name; value = $value; group = $group }
}

$metadata = [ordered]@{
    version        = $version
    generated      = (Get-Date).ToUniversalTime().ToString('yyyy-MM-dd')
    elements       = $elements
    utilityClasses = $utilityClasses
    themeTokens    = $themeTokens
}

$directory = Split-Path $OutputPath -Parent
if (-not (Test-Path $directory)) { New-Item -ItemType Directory -Path $directory | Out-Null }
$json = $metadata | ConvertTo-Json -Depth 12 -Compress
[System.IO.File]::WriteAllText($OutputPath, $json, (New-Object System.Text.UTF8Encoding($false)))

Write-Host ('Wrote {0}: {1} elements, {2} utility class groups, {3} theme tokens, Web Awesome {4}' -f $OutputPath, $elements.Count, $utilityClasses.Count, $themeTokens.Count, $version)
