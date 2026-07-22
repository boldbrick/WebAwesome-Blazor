# Test-WaReleasePreflight.ps1
# Automated release-preflight gates for WebAwesome.Blazor (see .claude\skills\wa-release-preflight\SKILL.md).
# Verifies everything that can be checked without VCS/ticketing context:
#   version alignment, changelog/migration doc shape, builds, tests, nupkg dependency floors, browser e2e.
# Windows PowerShell 5.1 compatible; ASCII only (5.1 misparses BOM-less non-ASCII scripts).
#
# Usage (from anywhere):
#   powershell -File tools\release\Test-WaReleasePreflight.ps1 [-SkipE2E] [-SkipBuild]
# Exit code 0 = all executed gates passed; 1 = at least one gate failed.

param(
    [switch]$SkipE2E,   # skip the Playwright sweep (demo server lifecycle)
    [switch]$SkipBuild  # skip builds/tests/nuspec gates (docs-only quick check)
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
Set-Location $repoRoot

$script:results = @()
function Add-Gate([string]$name, [bool]$ok, [string]$detail) {
    $script:results += New-Object PSObject -Property @{ Name = $name; Ok = $ok; Detail = $detail }
    $tag = 'FAIL'
    if ($ok) { $tag = ' OK ' }
    Write-Host ("[{0}] {1} - {2}" -f $tag, $name, $detail)
}

Write-Host "=== WebAwesome.Blazor release preflight ==="
Write-Host ("Repo root: {0}" -f $repoRoot)

# --- gate: clean workspace -------------------------------------------------
$pending = @(cm status --short 2>&1 | Where-Object { $_ -and $_.ToString().Trim() -ne '' })
Add-Gate 'workspace-clean' ($pending.Count -eq 0) ("pending items: {0}" -f $pending.Count)
$statusHeader = (cm status --header 2>&1 | Select-Object -First 1)
Write-Host ("Workspace position: {0}" -f $statusHeader)

# --- gate: gitsync author mapping --------------------------------------------
# without an [email-mapping] entry for the current Plastic user, GitSync exports
# commits with an empty author email and GitHub cannot attribute them to an account
$plasticUser = (cm whoami 2>&1 | Select-Object -First 1)
if ($plasticUser) { $plasticUser = $plasticUser.ToString().Trim() }
$gitsyncConf = Join-Path $env:LOCALAPPDATA 'plastic4\gitsync.conf'
if ([string]::IsNullOrEmpty($plasticUser)) {
    Add-Gate 'gitsync-author-mapping' $false 'cm whoami returned no user'
} elseif (-not (Test-Path $gitsyncConf)) {
    Add-Gate 'gitsync-author-mapping' $false ("{0} not found (mapping for '{1}' required)" -f $gitsyncConf, $plasticUser)
} else {
    $mapped = @(Get-Content $gitsyncConf | Where-Object { $_.TrimStart().StartsWith($plasticUser) })
    Add-Gate 'gitsync-author-mapping' ($mapped.Count -gt 0) ("mapping for '{0}' in {1}" -f $plasticUser, $gitsyncConf)
}

# --- gate: version alignment ------------------------------------------------
[xml]$props = Get-Content src\Version.props
$version = ($props.Project.PropertyGroup | Where-Object { $_.Version } | Select-Object -First 1).Version
Write-Host ("Version.props version: {0}" -f $version)
Add-Gate 'version-props' (-not [string]::IsNullOrEmpty($version)) ("Version = {0}" -f $version)

$parity = Get-Content src\WebAwesome.Blazor.Tests\ApiParity\parity-config.json -Raw | ConvertFrom-Json
Add-Gate 'parity-armed' ($parity.enabled -eq $true) ("enabled = {0}" -f $parity.enabled)
Add-Gate 'parity-version' ($parity.targetWaVersion -eq $version) ("targetWaVersion = {0}" -f $parity.targetWaVersion)

$surfaceHead = (Get-Content src\WebAwesome.Blazor.Tests\ApiParity\expected-api-surface.json -TotalCount 5) -join ' '
$surfaceVersion = ''
if ($surfaceHead -match '"version"\s*:\s*"([^"]+)"') { $surfaceVersion = $Matches[1] }
Add-Gate 'surface-version' ($surfaceVersion -eq $version) ("expected-api-surface version = {0}" -f $surfaceVersion)

$readme = Get-Content README.md -Raw
Add-Gate 'readme-cdn-version' ($readme.Contains("webawesome@$version")) ("README CDN snippet references webawesome@{0}" -f $version)

$demoIndex = Get-Content src\WebAwesome.Blazor.Demo\wwwroot\index.html -Raw
Add-Gate 'demo-cdn-version' ($demoIndex.Contains("webawesome@$version")) ("demo index.html references webawesome@{0}" -f $version)

# --- gate: changelog and migration doc ---------------------------------------
$changelog = Get-Content docs\CHANGELOG.md -Raw
$hasEntry = $changelog -match [regex]::Escape("## [$version]")
Add-Gate 'changelog-entry' $hasEntry ("dated section '## [{0}]' present" -f $version)

$unreleasedFixes = $false
if ($changelog -match '(?s)## \[Unreleased\](.*?)(## \[|$)') {
    $unreleasedBody = $Matches[1]
    if ($unreleasedBody -match '\S') { $unreleasedFixes = $true }
}
Add-Gate 'changelog-unreleased-folded' (-not $unreleasedFixes) 'no leftover [Unreleased] content at release time'

$entryHasBreaking = $false
if ($changelog -match ('(?s)## \[' + [regex]::Escape($version) + '\](.*?)(\r?\n## \[|$)')) {
    if ($Matches[1] -match '### Breaking changes') { $entryHasBreaking = $true }
}
if ($entryHasBreaking) {
    Add-Gate 'migration-doc' (Test-Path ("docs\MIGRATION-{0}.md" -f $version)) ("breaking changes present, docs\MIGRATION-{0}.md required" -f $version)
} else {
    Add-Gate 'migration-doc' $true 'no breaking changes section, migration doc not required'
}

# --- gates: build, tests, package -------------------------------------------
if (-not $SkipBuild) {
    foreach ($config in 'Debug', 'Release') {
        $build = dotnet build src\WebAwesome.slnx -p:Configuration=$config 2>&1
        $buildOk = ($LASTEXITCODE -eq 0)
        $warnings = -1
        $m = ($build | Select-String -Pattern '(\d+) Warning\(s\)' | Select-Object -First 1)
        if ($m) { $warnings = [int]$m.Matches[0].Groups[1].Value }
        Add-Gate ("build-{0}" -f $config.ToLower()) ($buildOk -and $warnings -eq 0) ("exit {0}, warnings {1}" -f $LASTEXITCODE, $warnings)

        $test = dotnet test src\WebAwesome.slnx --no-build -p:Configuration=$config 2>&1
        $testOk = ($LASTEXITCODE -eq 0)
        $skipped = ($test | Select-String -Pattern 'Skipped:\s*[1-9]' | Measure-Object).Count
        $totals = ($test | Select-String -Pattern 'Passed!.*Total:\s*\d+' | ForEach-Object { $_.Line.Trim() }) -join ' | '
        Add-Gate ("test-{0}" -f $config.ToLower()) ($testOk -and $skipped -eq 0) ("exit {0}, skipped-frameworks {1}: {2}" -f $LASTEXITCODE, $skipped, $totals)
    }

    # nupkg dependency floors: every shipped dependency must sit on a base major (x.0.0)
    $nuspecPath = "src\output\obj\Release\WebAwesome.Blazor\WebAwesome.Blazor.$version.nuspec"
    if (Test-Path $nuspecPath) {
        [xml]$nuspec = Get-Content $nuspecPath
        $deps = @($nuspec.package.metadata.dependencies.group | ForEach-Object { $_.dependency })
        $offenders = @($deps | Where-Object { $_.version -notmatch '^\d+\.0\.0$' } | ForEach-Object { "{0} {1}" -f $_.id, $_.version })
        $detail = 'all shipped dependencies floored at x.0.0'
        if ($offenders.Count -gt 0) { $detail = 'non-floored: ' + ($offenders -join ', ') }
        Add-Gate 'nupkg-dependency-floors' ($offenders.Count -eq 0) $detail
    } else {
        Add-Gate 'nupkg-dependency-floors' $false ("nuspec not found at {0}" -f $nuspecPath)
    }
} else {
    Write-Host 'Builds/tests/nuspec gates skipped (-SkipBuild).'
}

# --- gate: browser e2e sweep --------------------------------------------------
if (-not $SkipE2E) {
    if (-not (Test-Path tools\e2e\node_modules)) {
        Add-Gate 'e2e-sweep' $false 'tools\e2e\node_modules missing - run npm install (and npm run install-browsers) first'
    } else {
        $demoArgs = 'run --project src\WebAwesome.Blazor.Demo --configuration Debug --no-build --urls http://localhost:5000'
        $demo = Start-Process -FilePath dotnet -ArgumentList $demoArgs -WorkingDirectory $repoRoot -PassThru -WindowStyle Hidden
        try {
            $up = $false
            $deadline = (Get-Date).AddSeconds(90)
            while ((Get-Date) -lt $deadline) {
                try {
                    $r = Invoke-WebRequest 'http://localhost:5000' -UseBasicParsing -TimeoutSec 3
                    if ($r.StatusCode -eq 200) { $up = $true; break }
                } catch { Start-Sleep -Seconds 2 }
            }
            if (-not $up) {
                Add-Gate 'e2e-sweep' $false 'demo server did not answer on http://localhost:5000 within 90s'
            } else {
                Push-Location tools\e2e
                try {
                    $e2e = cmd /c "npm test 2>&1"
                    $e2eOk = ($LASTEXITCODE -eq 0)
                    $summary = ($e2e | Select-String -Pattern '\d+ passed|\d+ failed' | ForEach-Object { $_.Line.Trim() }) -join ' | '
                    Add-Gate 'e2e-sweep' $e2eOk ("exit {0}: {1}" -f $LASTEXITCODE, $summary)
                } finally {
                    Pop-Location
                }
            }
        } finally {
            # dotnet run spawns the app as a child process - kill the whole tree
            cmd /c ("taskkill /PID {0} /T /F >nul 2>&1" -f $demo.Id) | Out-Null
        }
    }
} else {
    Write-Host 'E2E gate skipped (-SkipE2E).'
}

# --- summary ------------------------------------------------------------------
$failed = @($script:results | Where-Object { -not $_.Ok })
Write-Host ''
Write-Host ("=== Preflight result: {0} gates, {1} failed ===" -f $script:results.Count, $failed.Count)
if ($failed.Count -gt 0) {
    $failed | ForEach-Object { Write-Host ("  BLOCKER: {0} - {1}" -f $_.Name, $_.Detail) }
    exit 1
}
Write-Host 'All executed gates PASSED.'
exit 0
