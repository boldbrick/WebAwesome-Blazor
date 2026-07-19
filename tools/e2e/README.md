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
- `tests\theme-and-dark-mode.spec.js` — regression test for the dark-mode switch and theme
  selector doing nothing visible.

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
