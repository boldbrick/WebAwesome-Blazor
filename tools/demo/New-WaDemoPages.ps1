<#
.SYNOPSIS
Generates skeleton demo pages for Web Awesome components that do not have one yet.

.DESCRIPTION
Reads an API surface JSON (produced by tools\upgrade\Export-WaApiSurface.ps1) and, for every
component without an existing page under src\WebAwesome.Blazor.Demo\Pages\Components\, emits a
skeleton Razor page: title, CEM description, one minimal live example, a TODO marker for curated
examples, and the ApiTable API reference. Existing pages are never overwritten (idempotent).

With -PruneRemoved, pages whose component no longer exists in the surface are deleted.

The demo navigation is data-driven from wwwroot\data\api-surface.json, so no nav updates are needed.

.PARAMETER SurfacePath
API surface JSON to generate from. Defaults to the demo's own wwwroot\data\api-surface.json.

.PARAMETER RepoRoot
Repository root. Defaults to two levels above this script.

.PARAMETER PruneRemoved
Delete pages for components not present in the surface.

.EXAMPLE
.\New-WaDemoPages.ps1
#>
[CmdletBinding()]
param(
    [string]$SurfacePath,

    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,

    [switch]$PruneRemoved
)

$ErrorActionPreference = 'Stop'

$pagesDir = Join-Path $RepoRoot 'src\WebAwesome.Blazor.Demo\Pages\Components'
if (-not $SurfacePath) { $SurfacePath = Join-Path $RepoRoot 'src\WebAwesome.Blazor.Demo\wwwroot\data\api-surface.json' }
if (-not (Test-Path $SurfacePath)) { throw "Surface JSON not found: $SurfacePath" }
if (-not (Test-Path $pagesDir)) { New-Item -ItemType Directory -Force $pagesDir | Out-Null }

$surface = Get-Content $SurfacePath -Raw | ConvertFrom-Json

function Get-PascalName([string]$tag) {
    $name = $tag -replace '^wa-', ''
    return ($name -split '-' | ForEach-Object { $_.Substring(0,1).ToUpperInvariant() + $_.Substring(1) }) -join ''
}

function Get-HumanName([string]$tag) {
    $name = $tag -replace '^wa-', ''
    return ($name -split '-' | ForEach-Object { $_.Substring(0,1).ToUpperInvariant() + $_.Substring(1) }) -join ' '
}

$created = @()
$existing = @()

foreach ($tag in $surface.components.PSObject.Properties.Name) {
    $component = $surface.components.$tag
    $pascal = Get-PascalName $tag
    $human = Get-HumanName $tag
    $route = $tag -replace '^wa-', ''
    $pagePath = Join-Path $pagesDir "$($pascal)Page.razor"

    if (Test-Path $pagePath) {
        $existing += $tag
        continue
    }

    $className = "Wa$pascal"

    # components without a wrapper yet get a native-element example so the page still compiles
    $wrapperPath = Join-Path $RepoRoot "src\WebAwesome.Blazor\Components\$className.cs"
    $wrapperExists = Test-Path $wrapperPath

    # detect whether the wrapper derives from WaInputBase<T>/InputBase<T>, which requires @bind-Value
    # (a bare Value= attribute or no binding at all throws at runtime because ValueExpression is unset)
    $isInputBase = $false
    $valueType = $null
    if ($wrapperExists) {
        $wrapperSource = Get-Content $wrapperPath -Raw
        if ($wrapperSource -match ':\s*(?:WaInputBase|InputBase)<([^>]+)>') {
            $isInputBase = $true
            $valueType = $matches[1].Trim()
        }
    }

    # detect whether the component exposes a default (unnamed) content slot
    $hasDefaultSlot = $false
    if ($component.slots) {
        $hasDefaultSlot = [bool]($component.slots.PSObject.Properties.Name -contains '(default)')
    }

    $codeField = $null
    if ($wrapperExists) {
        if ($isInputBase) {
            # bound example: give the control a @bind-Value and a matching backing field
            $example = "<$className @bind-Value=`"value`">$human</$className>"
            $codeField = "private $valueType value;"
        }
        elseif ($hasDefaultSlot) {
            # text child content instead of a self-closing tag, since the component renders a default slot
            $example = "<$className>$className example</$className>"
        }
        else {
            $example = "<$className />"
        }
        $subtitle = "<p><code>&lt;$tag&gt;</code> wrapped by <code>$className</code>.</p>"
        $todo = "@* TODO: curated examples - translate from the Web Awesome docs (inputs\WebAwesome\components\$route.md) *@"
    }
    else {
        $example = "<$tag></$tag>"
        $subtitle = "<p><code>&lt;$tag&gt;</code> - <strong>wrapper not yet implemented</strong>; the example below uses the native element.</p>"
        $todo = "@* TODO: implement the $className wrapper, then replace the native element example *@"
    }

    $defaultExampleLine = "private const string DefaultExample = `"$($example -replace '"', '\"')`";"
    if ($codeField) {
        $codeBody = "    $codeField`r`n`r`n    $defaultExampleLine"
    }
    else {
        $codeBody = "    $defaultExampleLine"
    }

    $content = @"
@page "/components/$route"

<PageTitle>$human</PageTitle>

<h1>$human <ComponentBadges Tag="$tag" /></h1>
$subtitle

<ExampleSection Title="Default" Code="@DefaultExample">
    $example
</ExampleSection>

$todo

<ApiTable Tag="$tag" />

@code {
$codeBody
}
"@

    [System.IO.File]::WriteAllText($pagePath, $content, (New-Object System.Text.UTF8Encoding $true))
    $created += $tag
}

# prune pages whose component disappeared from the surface
$pruned = @()
if ($PruneRemoved) {
    $knownPages = $surface.components.PSObject.Properties.Name | ForEach-Object { "$(Get-PascalName $_)Page.razor" }
    foreach ($page in (Get-ChildItem $pagesDir -Filter '*Page.razor')) {
        if ($knownPages -notcontains $page.Name) {
            Remove-Item $page.FullName
            $pruned += $page.Name
        }
    }
}

Write-Output "Demo pages: $($created.Count) created, $($existing.Count) already present, $($pruned.Count) pruned"
if ($created.Count -gt 0) { Write-Output "Created: $($created -join ', ')" }
if ($pruned.Count -gt 0) { Write-Output "Pruned: $($pruned -join ', ')" }
