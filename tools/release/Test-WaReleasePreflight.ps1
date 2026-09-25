# Test-WaReleasePreflight.ps1
# Automated release-preflight gates for WebAwesome.Blazor (see .claude\skills\wa-release-preflight\SKILL.md).
# Verifies everything that can be checked without VCS/ticketing context:
#   version alignment, changelog/migration doc shape, builds, tests, nupkg dependency floors, browser e2e.
# Windows PowerShell 5.1 compatible; ASCII only (5.1 misparses BOM-less non-ASCII scripts).
#
# Usage (from anywhere):
#   & tools\release\Test-WaReleasePreflight.ps1 [-SkipE2E] [-SkipBuild] [-ProDist <path>] [-E2EPort <port>]
# Exit code 0 = all executed gates passed; 1 = at least one gate failed.
#
# The e2e gate starts the WebAssembly demo on a free loopback port (or -E2EPort, which must be free),
# checks that the server answering there is the demo process it started, and runs Playwright with
# CI=1 and --forbid-only. It then reads the JSON report and fails on any failed test, on any skip
# not listed for the asset mode in tools\e2e\data\expected-skips.json, on a listed skip that ran or
# no longer exists, and on fewer tests than the mode's minimumTests.
# -ProDist runs a second, opt-in pass against a self-hosted Pro dist (the extracted Pro package root
# or its dist-cdn folder, e.g. temp\wa-src\<version> from the release zip): it generates the ignored
# asset override with tools\demo\Set-WaProAssets.ps1, runs the "pro" mode, and clears the override
# again. No Pro URL or token is needed or committed; WA_PRO_DIST is used when -ProDist is omitted
# and -ProE2E is given.

param(
    [switch]$SkipE2E,   # skip the Playwright sweep (demo server lifecycle)
    [switch]$SkipBuild, # skip builds/tests/nuspec gates (docs-only quick check)
    [string]$ProDist,   # opt-in: also run the e2e suite against this self-hosted Pro dist
    [switch]$ProE2E,    # opt-in: like -ProDist, with the path taken from $env:WA_PRO_DIST
    [int]$E2EPort = 0   # demo port for the e2e passes; 0 picks a free loopback port
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

# demo asset tags are emitted by WebAwesomeAssets from configuration and default to the library
# version, so demo/version alignment is structural; the gate instead verifies nothing reintroduced
# a hard-coded Web Awesome CDN pin into either demo host
$demoIndex = Get-Content src\WebAwesome.Blazor.Demo\wwwroot\index.html -Raw
$serverApp = Get-Content src\WebAwesome.Blazor.Demo.Server\App.razor -Raw
$noPins = (-not ($demoIndex -match 'webawesome@\d')) -and (-not ($serverApp -match 'webawesome@\d'))
Add-Gate 'demo-no-hardcoded-cdn' $noPins 'demo hosts must not hard-code Web Awesome CDN URLs (WebAwesomeAssets emits them from configuration)'

# --- gate: default asset state -------------------------------------------------
# release verification must run against the committed default (free CDN): an active Pro
# override would make builds/e2e exercise a different asset source than what ships
$overrideArtifacts = @(
    'src\WebAwesome.Blazor.Demo\wwwroot\appsettings.Local.json',
    'src\WebAwesome.Blazor.Demo.Server\appsettings.Local.json',
    'src\WebAwesome.Blazor.Demo\wwwroot\webawesome'
) | Where-Object { Test-Path $_ }
Add-Gate 'default-asset-state' (@($overrideArtifacts).Count -eq 0) `
    ($(if (@($overrideArtifacts).Count -eq 0) { 'no Pro asset override active' }
       else { 'Pro asset override active ({0}) - run tools\demo\Set-WaProAssets.ps1 -Clear and re-run preflight' -f ($overrideArtifacts -join ', ') }))

# --- gate: no Pro asset leakage ----------------------------------------------
# Pro kit URLs / dist overrides are supplied via env vars and the generated (ignored)
# appsettings.Local.json only - fail if the override artifacts are version-controlled
# (cm fileinfo: 'controlled' vs 'private'/error) or a kit-like URL sneaked into
# sources/workflows (inputs\ docs legitimately mention the public ka-f host)
$versionedOverrides = @(
    'src\WebAwesome.Blazor.Demo\wwwroot\appsettings.Local.json',
    'src\WebAwesome.Blazor.Demo.Server\appsettings.Local.json',
    'src\WebAwesome.Blazor.Demo\wwwroot\webawesome'
) | Where-Object { (Test-Path $_) -and ((cm fileinfo $_ --format='{status}' 2>$null) -eq 'controlled') }
$kitLeak = Get-ChildItem src, .github -Recurse -File -Include *.cs, *.razor, *.html, *.json, *.yml, *.props |
    Where-Object { $_.Name -ne 'appsettings.Local.json' } |
    Where-Object { (Get-Content $_.FullName -Raw) -match 'ka-f\.webawesome\.com/[A-Za-z0-9]{8,}|ka-p\.webawesome\.com/kit/[A-Za-z0-9]|_authToken' }
$ignoreConf = Get-Content ignore.conf -Raw
$gitIgnore = Get-Content .gitignore -Raw
$ignoresPresent = $ignoreConf.Contains('appsettings.Local.json') -and $gitIgnore.Contains('appsettings.Local.json')
Add-Gate 'pro-asset-leak' ((@($versionedOverrides).Count -eq 0) -and (@($kitLeak).Count -eq 0) -and $ignoresPresent) `
    ('leak check: versioned overrides={0}, kit-like URLs in sources={1}, ignore rules present={2}' -f @($versionedOverrides).Count, @($kitLeak).Count, $ignoresPresent)

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
    $entryBody = $Matches[1]
    if ($entryBody -match '(?s)### Breaking changes\s*(.*?)(\r?\n### |$)') {
        # a section counts as breaking only if it declares something beyond a lone "None" bullet
        $breakingBody = ($Matches[1] -replace '(?m)^\s*[-*]\s*', '').Trim()
        if ($breakingBody -and $breakingBody -notmatch '^[Nn]one\b') { $entryHasBreaking = $true }
    }
}
if ($entryHasBreaking) {
    Add-Gate 'migration-doc' (Test-Path ("docs\MIGRATION-{0}.md" -f $version)) ("breaking changes present, docs\MIGRATION-{0}.md required" -f $version)
} else {
    Add-Gate 'migration-doc' $true 'no breaking changes declared, migration doc not required'
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
# the demo title identifies the WebAssembly demo's index.html (a foreign server on the port lacks it)
$demoMarker = '<title>Web Awesome Blazor Bindings</title>'
$skipPolicyPath = Join-Path $repoRoot 'tools\e2e\data\expected-skips.json'

function Get-FreeLoopbackPort {
    $listener = New-Object System.Net.Sockets.TcpListener([System.Net.IPAddress]::Loopback, 0)
    $listener.Start()
    try { return ([System.Net.IPEndPoint]$listener.LocalEndpoint).Port } finally { $listener.Stop() }
}

function Get-ListeningProcessIds([int]$port) {
    return @(Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty OwningProcess -Unique)
}

# the process and all its descendants: dotnet run hosts the app in a child process
function Get-ProcessTreeIds([int]$rootId) {
    $all = @(Get-CimInstance Win32_Process | Select-Object ProcessId, ParentProcessId)
    $ids = New-Object 'System.Collections.Generic.List[int]'
    $ids.Add($rootId)
    for ($i = 0; $i -lt $ids.Count; $i++) {
        foreach ($p in $all) {
            if ($p.ParentProcessId -eq $ids[$i] -and -not $ids.Contains([int]$p.ProcessId)) { $ids.Add([int]$p.ProcessId) }
        }
    }
    return ,$ids
}

# flattens the Playwright JSON report into one record per test: '<file> > <describe...> > <title>',
# final status (expected/unexpected/flaky/skipped) and the skip reason when there is one
function Get-ReportTests($suite, [string[]]$titles) {
    $path = @($titles)
    if ($suite.title) { $path = @($titles) + $suite.title }
    foreach ($spec in @($suite.specs | Where-Object { $_ })) {
        foreach ($test in @($spec.tests | Where-Object { $_ })) {
            $reason = @($test.annotations | Where-Object { $_ -and $_.type -eq 'skip' } | ForEach-Object { $_.description }) -join '; '
            New-Object PSObject -Property @{ Key = ((@($path) + $spec.title) -join ' > '); Status = $test.status; Reason = $reason }
        }
    }
    foreach ($child in @($suite.suites | Where-Object { $_ })) { Get-ReportTests $child $path }
}

# checks one pass's report against the mode's policy; returns the problems (empty = pass)
function Test-E2eReport($report, $policy) {
    $problems = @()
    $expected = @{}
    foreach ($entry in @($policy.expectedSkips | Where-Object { $_ })) {
        if ([string]::IsNullOrWhiteSpace($entry.reason)) { $problems += ("expected skip without a reason: {0}" -f $entry.test) }
        $expected[$entry.test] = $entry.reason
    }

    $tests = @()
    foreach ($suite in @($report.suites | Where-Object { $_ })) { $tests += @(Get-ReportTests $suite @()) }
    foreach ($runError in @($report.errors | Where-Object { $_ })) { $problems += ("run error: {0}" -f (($runError.message -split "`n")[0])) }

    foreach ($test in $tests) {
        if ($test.Status -eq 'unexpected') { $problems += ("failed: {0}" -f $test.Key) }
        elseif ($test.Status -eq 'skipped' -and -not $expected.ContainsKey($test.Key)) {
            $problems += ("unexpected skip: {0} ({1})" -f $test.Key, $(if ($test.Reason) { $test.Reason } else { 'no reason given' }))
        }
        elseif ($test.Status -ne 'skipped' -and $expected.ContainsKey($test.Key)) {
            $problems += ("listed as an expected skip but ran ({0}): {1}" -f $test.Status, $test.Key)
        }
    }

    $keys = @{}
    foreach ($test in $tests) { $keys[$test.Key] = $true }
    foreach ($key in $expected.Keys) {
        if (-not $keys.ContainsKey($key)) { $problems += ("stale expected skip, no such test: {0}" -f $key) }
    }

    if ($tests.Count -lt [int]$policy.minimumTests) {
        $problems += ("only {0} tests ran, the minimum is {1}" -f $tests.Count, $policy.minimumTests)
    }
    return ,$problems
}

# one Playwright pass against a freshly started demo on a free port; $mode selects the skip policy
function Invoke-E2ePass([string]$mode) {
    $gate = "e2e-$mode"
    $port = $E2EPort
    if ($port -le 0) { $port = Get-FreeLoopbackPort }
    if (@(Get-ListeningProcessIds $port).Count -gt 0) {
        Add-Gate $gate $false ("port {0} is already in use (pid {1}) - stop that server or pass a free -E2EPort" -f $port, ((Get-ListeningProcessIds $port) -join ', '))
        return
    }

    $baseUrl = "http://localhost:$port"
    $demoArgs = "run --project src\WebAwesome.Blazor.Demo --configuration Debug --no-build --urls $baseUrl"
    $demo = Start-Process -FilePath dotnet -ArgumentList $demoArgs -WorkingDirectory $repoRoot -PassThru -WindowStyle Hidden
    $reportPath = Join-Path ([System.IO.Path]::GetTempPath()) ("wa-e2e-{0}-{1}.json" -f $mode, [guid]::NewGuid().ToString('N'))
    try {
        $ready = $null
        $deadline = (Get-Date).AddSeconds(90)
        while ((Get-Date) -lt $deadline -and -not $ready) {
            if ($demo.HasExited) { $ready = 'exited'; break }
            try {
                $r = Invoke-WebRequest $baseUrl -UseBasicParsing -TimeoutSec 3
                if ($r.StatusCode -eq 200) { $ready = 'up' }
            } catch { Start-Sleep -Seconds 2 }
        }
        if ($ready -ne 'up') {
            $why = 'did not answer within 90s'
            if ($ready -eq 'exited') { $why = ('exited with code {0} (port taken or build missing?)' -f $demo.ExitCode) }
            Add-Gate $gate $false ("demo on {0} {1}" -f $baseUrl, $why)
            return
        }

        # the answer must come from the demo started here: a stale or foreign server would pass a plain probe
        $tree = Get-ProcessTreeIds $demo.Id
        $owners = @(Get-ListeningProcessIds $port)
        $foreign = @($owners | Where-Object { -not $tree.Contains([int]$_) })
        if ($owners.Count -eq 0 -or $foreign.Count -gt 0 -or -not $r.Content.Contains($demoMarker)) {
            Add-Gate $gate $false ("{0} is not served by the demo started here (listening pids: {1}; demo tree: {2}; demo marker present: {3})" -f `
                $baseUrl, ($owners -join ', '), ($tree -join ', '), $r.Content.Contains($demoMarker))
            return
        }

        # CI=1 arms forbidOnly and retries in playwright.config.js; --forbid-only makes the former explicit
        $saved = @{ CI = $env:CI; DEMO_BASE_URL = $env:DEMO_BASE_URL; PLAYWRIGHT_JSON_OUTPUT_FILE = $env:PLAYWRIGHT_JSON_OUTPUT_FILE }
        $env:CI = '1'
        $env:DEMO_BASE_URL = $baseUrl
        $env:PLAYWRIGHT_JSON_OUTPUT_FILE = $reportPath
        Push-Location tools\e2e
        try {
            $output = cmd /c "npx playwright test --forbid-only --reporter=list,json 2>&1"
            $exitCode = $LASTEXITCODE
        } finally {
            Pop-Location
            foreach ($name in @($saved.Keys)) {
                if ($null -eq $saved[$name]) { Remove-Item ("env:{0}" -f $name) -ErrorAction SilentlyContinue }
                else { Set-Item -Path ("env:{0}" -f $name) -Value $saved[$name] }
            }
        }

        if (-not (Test-Path $reportPath)) {
            $output | Select-Object -Last 20 | ForEach-Object { Write-Host ("    {0}" -f $_) }
            Add-Gate $gate $false ("exit {0}, no JSON report written" -f $exitCode)
            return
        }

        $report = Get-Content $reportPath -Raw | ConvertFrom-Json
        $policy = (Get-Content $skipPolicyPath -Raw | ConvertFrom-Json).modes.$mode
        # the function returns its array unrolled-proof (",$problems"), so no @() here
        $problems = Test-E2eReport $report $policy
        if ($exitCode -ne 0 -and $problems.Count -eq 0) { $problems += ("playwright exit {0}" -f $exitCode) }
        foreach ($problem in $problems) { Write-Host ("    {0}" -f $problem) }

        $stats = $report.stats
        $flaky = @()
        foreach ($suite in @($report.suites | Where-Object { $_ })) { $flaky += @(Get-ReportTests $suite @() | Where-Object { $_.Status -eq 'flaky' } | ForEach-Object { $_.Key }) }
        foreach ($name in $flaky) { Write-Host ("    flaky (passed on retry): {0}" -f $name) }
        $detail = ("{0}: {1} passed, {2} skipped (all expected), {3} flaky, {4} failed" -f $baseUrl, $stats.expected, $stats.skipped, $stats.flaky, $stats.unexpected)
        if ($problems.Count -gt 0) { $detail = ("{0} problem(s): {1}" -f $problems.Count, ($problems -join ' | ')) }
        Add-Gate $gate ($problems.Count -eq 0) $detail
    } finally {
        # dotnet run spawns the app as a child process - kill the whole tree
        cmd /c ("taskkill /PID {0} /T /F >nul 2>&1" -f $demo.Id) | Out-Null
        if (Test-Path $reportPath) { Remove-Item $reportPath -Force }
    }
}

if (-not $SkipE2E) {
    if ($ProE2E -and -not $ProDist) { $ProDist = $env:WA_PRO_DIST }
    if (-not (Test-Path tools\e2e\node_modules)) {
        Add-Gate 'e2e-free-cdn' $false 'tools\e2e\node_modules missing - run npm install (and npm run install-browsers) first'
    } else {
        Invoke-E2ePass 'free-cdn'

        if ($ProE2E -and -not $ProDist) {
            Add-Gate 'e2e-pro' $false '-ProE2E given but WA_PRO_DIST is not set'
        } elseif ($ProDist) {
            # self-hosted Pro dist through the ignored override files; always restored to the free default
            $savedProDist = $env:WA_PRO_DIST
            try {
                $env:WA_PRO_DIST = $ProDist
                & (Join-Path $repoRoot 'tools\demo\Set-WaProAssets.ps1') | Out-Null
                Invoke-E2ePass 'pro'
            } catch {
                Add-Gate 'e2e-pro' $false ("Pro asset override failed: {0}" -f $_.Exception.Message)
            } finally {
                & (Join-Path $repoRoot 'tools\demo\Set-WaProAssets.ps1') -Clear | Out-Null
                $env:WA_PRO_DIST = $savedProDist
                if ($null -eq $savedProDist) { Remove-Item env:WA_PRO_DIST -ErrorAction SilentlyContinue }
            }
        } else {
            Write-Host 'Pro e2e pass not requested (opt-in: -ProDist <Pro dist path> or -ProE2E with WA_PRO_DIST).'
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
