# WebAwesome.Blazor.Demo end-to-end tests

Playwright tests that drive the actual running demo app in a real browser. This exists because
several real bugs were invisible to both `dotnet build` and the bUnit test suite — they were
runtime JS-interop failures and DOM event semantics that only show up when a real browser
actually renders the page and a real user interaction fires a real DOM event. See
`docs\CHANGELOG.md` (`## [Unreleased]` → `### Fixed`) for what these tests exist to catch and why.

## What's covered

- `tests\sweep.spec.js` — visits **every** demo route (every component page from
  `api-surface.json`, plus the layout pages and the home page) and asserts no unhandled
  console/page error was logged. This is the first line of defense against the "wrapper throws
  on first render" class of bug (e.g. the `wa-resize-observer` "initialize" crash).
- `tests\checkbox-switch-binding.spec.js` — regression test for the `WaCheckbox`/`WaSwitch`
  two-way binding bug (state never propagated back to Blazor).
- `tests\custom-event-payload.spec.js` — regression test for the custom-event delivery bug
  (no `wa-*` EventCallback ever fired: bindings lacked the `on` attribute prefix and no event
  type was registered with `Blazor.registerCustomEventType`); asserts a real tab click delivers
  the typed `e.Name` payload into the Blazor handler.
- `tests\theme-and-dark-mode.spec.js` — regression test for the dark-mode switch and theme
  selector doing nothing visible.
- `tests\event-dispatch.spec.js` — real-interaction dispatch proof for the wrappers' EventCallbacks.
  Each case in `tests\helpers\event-cases.js` drives a component with the mouse or keyboard and lists
  the callbacks it proves; afterwards every listed callback must appear in the event log of the
  harness pages (`/testing/events-forms`, `-overlays`, `-content`, `-pro`, see
  `src\WebAwesome.Blazor.Demo\Pages\Testing`), which renders what reached .NET. Known defects run as
  expected failures (`KNOWN_DEFECT_CASES`, `test.fail`): they start failing once the defect is fixed.
  The list is empty since 3.12.0: the focus cases prove `OnFocus`/`OnBlur` through `focusin`/`focusout`,
  and the color picker, select, combobox and intersection observer cases prove the events the JS
  initializer relays (non-bubbling, or stopped in the shadow root).
- `tests\event-payload.spec.js` — the payloads the JS initializer builds by hand (`specialArgs`):
  split panel, observers, random content, date picker, video playlist and data grid events, each
  with its non-default field values as .NET received them (`/testing/event-payloads`).
- `tests\event-coverage.spec.js` — no browser: checks `data\event-callbacks.json` (every wrapper
  callback, exported from the rendered bindings by the bUnit test `EventCallbackManifestTests`)
  against the dispatch and payload cases, `EXTERNAL_COVERAGE` in `tests\helpers\event-coverage.js`
  and the reasoned exemptions in `data\event-coverage-exemptions.json`. A new callback fails the
  bUnit test until the manifest is refreshed, then this spec until it is covered or exempted. An
  exemption for a known defect is flagged `"knownDefect": true` (with a reason starting `KNOWN DEFECT`)
  and needs a `KNOWN_DEFECT_CASES` case; the spec prints how many there are.
- Pro components (combobox, date input/picker, file input, video, video playlist, data grid) upgrade
  only with a Pro asset override; their tests skip visibly on the free CDN (`skipUnlessProUpgrades`).
  To run them locally, point `WA_PRO_DIST` at the release zip's extracted package (e.g.
  `temp\wa-src\<version>`) and run `tools\demo\Set-WaProAssets.ps1`; clear it with `-Clear`. Build the
  demo only with the override cleared: a build made while it is active serves a stale
  `appsettings.Local.json` after clearing, until the next build.

## Running

Prerequisite: **the demo app must already be running** (Playwright does not build or start it
by default — see `playwright.config.js` if you want to change that):

```powershell
dotnet build src\WebAwesome.slnx -p:Configuration=Debug
dotnet run --project src\WebAwesome.Blazor.Demo\WebAwesome.Blazor.Demo.csproj --configuration Debug --no-build
```

Then, from this directory (first run only needs `npm install` + browser download):

```powershell
cd tools\e2e
npm install
npm run install-browsers   # downloads Chromium once; not needed on subsequent runs
npm test
```

Set `DEMO_BASE_URL` if the demo is running on a different port than `http://localhost:5000`.

The same suite also runs against the server-hosted demo variant (interactive server render
mode over a SignalR circuit) — both hosting models should stay green:

```powershell
dotnet run --project src\WebAwesome.Blazor.Demo.Server --no-build --launch-profile http
cd tools\e2e
$env:DEMO_BASE_URL = 'http://localhost:5100'; npm test
```

## Release gate

`tools\release\Test-WaReleasePreflight.ps1` runs this suite itself: it starts the WebAssembly demo on a
free loopback port, checks that the server answering there is the process it started, and runs
`npx playwright test --forbid-only --reporter=list,json` with `CI=1` (so `test.only` fails and retries
are armed; flaky tests are listed). It then checks the JSON report against
`data\expected-skips.json`: every skipped test must be listed with a reason for the asset mode
(`free-cdn`, or `pro` for the opt-in `-ProDist <path>` pass against a self-hosted Pro dist), a
listed test that ran, is listed twice or no longer exists fails, and the pass must run at least `minimumTests`
tests. So a self-skipping test (a Pro component on the free CDN) must be added to that file, and
`minimumTests` must be raised when tests are added.

## Adding tests

- New component demo pages are picked up automatically by `sweep.spec.js` (it reads routes
  straight from `api-surface.json`, the same document that drives the demo's own nav) — no
  maintenance needed there.
- Layout routes (`LAYOUT_ROUTES` in `tests\helpers\routes.js`) are hand-authored components
  (`WaCluster`, `WaFlank`, ...) with no generated manifest; keep that list in sync with
  `MainLayout.razor`'s `LayoutLinks` array by hand if a layout component is added or removed.
- When you find and fix another bug this way, add a targeted regression test alongside the
  existing ones and log the root cause + workaround in `docs\CHANGELOG.md` so the `/wa-upgrade`
  pipeline knows to re-verify it against future Web Awesome releases (see
  `docs\UPGRADE-PROCESS.md` → "Pending workarounds to re-verify every upgrade").
