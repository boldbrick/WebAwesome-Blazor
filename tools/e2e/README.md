# WebAwesome.Blazor.Demo end-to-end tests

Playwright tests that drive the actual running demo app in a real browser. This exists because
several real bugs were invisible to both `dotnet build` and the bUnit test suite — they were
runtime JS-interop failures and DOM event semantics that only show up when a real browser
actually renders the page and a real user interaction fires a real DOM event. See
`docs\CHANGELOG.md` (`## [Unreleased]` → `### Fixed`) for what these tests exist to catch and why.

## What's covered

- **Every spec fails when Blazor crashes.** Specs take `test` and `expect` from the common test base
  `tests\helpers\test.js` instead of `@playwright/test`. Its page fixture watches Blazor's error UI
  (`#blazor-error-ui`, "An unhandled error has occurred") from the first document on and fails the test when the
  error UI was shown at any point, even if the test itself passed or navigated away; the failure quotes the console
  errors logged before it (the .NET exception). `tests\test-base.spec.js` checks that every spec uses the base and,
  as an expected failure, that the fixture fails a test whose page showed the error UI.
- `tests\interaction-sweep.spec.js` — drives every demo route except the harness pages the way a curious user
  would: each enabled control in the page's content area (`main.demo-content`, not the sidebar) once, in document
  order, with tabs and disclosures after the other visible controls so each panel's content gets its turn. Buttons,
  switches, checkboxes, radios, tabs, details and accordion items, selects and comboboxes (an option picked), inputs
  and text areas (a value typed), one-time-code and known-date fields, sliders, ratings, color and date pickers,
  dropdowns (an item that is not a link picked), tree items, removable tags, pagination, carousels, split panels and
  comparisons; every dialog, drawer, popover or menu an interaction opens is closed again. Links are never followed,
  and a button next to a Pro component that did not upgrade (the free CDN) is passed over, because its handler calls
  the missing element's methods; the Pro pass drives it. The page clock (`page.clock`, installed before the app
  starts) jumps 6 s after each interaction, twice, so toasts, delays and autoplay finish without real waiting. After
  each interaction the page must be healthy: no error UI, no page error, no console problem (the sweep's filter,
  `tests\helpers\page-health.js`), no overlay left open, no navigation away. Pages whose interactions legitimately
  leave the page list the element in `ROUTE_EXCLUSIONS` with the reason (none today). Each page has a 30 s
  interaction budget; every test is annotated with how many elements it drove and how long it took, and
  `E2E_SWEEP_DETAIL=1` lists them.
- `tests\showcase-*.spec.js` — one flow spec per showcase, with real user tasks and assertions on what Blazor
  renders: the Overlays invite dialog feeds the typed e-mail and the picked role into the feedback, the drawer,
  popover and popup open and close (also by Escape), the dropdown items feed the feedback, and the programmatic and
  declarative toasts show, close and show again; the Registration Form lists each required field's message and follows
  each correction (completing it needs the Pro `WaDateInput`); the Settings controls update the footer's saved
  settings and the tabs the section being edited; the Dashboard refresh shows its skeletons and the pagination drives
  the page caption (and, with Pro, the grid's rows); the Media Gallery carousel reports its slide; the Content tree,
  tip rotator, split panel, resize, mutation and intersection observers report their payloads. Timers are
  fast-forwarded with the page clock, never waited for.
- `tests\carousel-slides.spec.js` — carousel slides rendered from a model with `@key` (`/testing/carousel-slides`,
  plain and looping): adding a slide, removing the active slide (the last one included) and removing the last slide
  keep one pagination dot per slide, the active dot, `OnSlideChange` and the navigation in step, without a Blazor
  error. Removing the active slide keeps the active index, so the next slide shows; removing the active last slide
  shows the new last slide, or the first one when the carousel loops.
- `tests\sweep.spec.js` — visits **every** demo route (every component page from
  `api-surface.json`, plus the layout, showcase and harness pages and the home page), waits until every
  free `wa-*` element on it has rendered, and fails on any page error and on every console error or
  warning except network `Failed to load resource` noise and the autoload warning of a Pro component.
  Every component page sets non-default enum, boolean and number values, so an invalid value a wrapper
  emits surfaces here (Web Awesome throws, e.g. a `RangeError` from `Intl`, or warns, e.g. a deprecated
  size). It is also the first line of defense against the "wrapper throws on first render" class of
  bug (e.g. the `wa-resize-observer` "initialize" crash).
- `tests\extensions-package.spec.js` — the demo loads the JS initializer of the optional `WebAwesome.Blazor.Extensions`
  package (`_content/WebAwesome.Blazor.Extensions/...lib.module.js` answers 200). It proves the static web asset chain
  from the package to the host, which fails silently otherwise. It passes against both demo hosts.
- `tests\data-grid-columns.spec.js` — the `WaDataGrid` column model as the element receives it (Pro): the region
  column's `FilterOptions` fill the set filter picker in their order, with their labels, and filter by their values;
  the pushed columns hold no null member; and a page render that leaves `Columns` unchanged does not push them again
  (the element keeps the same column array).
- `tests\js-functions.spec.js` — `WaJsFunction` references. Without a grid (free CDN and Pro): the resolver of
  `webawesome-functions.js` binds a function registered by the demo's JS initializer, a global function and a module
  export with a trailing `ctx` at a fixed position, waits for a late registration, applies a context decorator, and
  refuses a module of another origin; and the `Uses` declaration registers `wa-progress-bar`, which only a cell function
  renders. With a grid (Pro): the collocated module of the "Custom Cell Rendering with JavaScript" example renders
  upgraded badges and progress bars and its comparator sorts; on `/testing/js-functions` the formatters get `ctx.args`,
  `ctx.rowKey`, `ctx.column` and `ctx.grid`, `discover()` registers an element no column declared, an
  `AggregatedFormatter` renders group rows, a new list with the same column JSON is not pushed, and (with `?broken=true`)
  references that do not resolve leave raw values, a working grid and one console error each.
- `tests\element-functions.spec.js` — the function-valued members of the element wrappers and `ShadowStyles`
  (`/testing/element-functions`, functions from the collocated `ElementFunctionsHarness.razor.js`). Free CDN: the
  `WaSelect` tags from `GetTagFunction` with `ctx.args` and `ctx.element`, styled by `ShadowStyles` (changed in place),
  and the element's own tags again once the function is removed; the `WaRating` symbols; the `WaSlider` and `WaRange`
  value text; the `WaPagination` links; and the obsolete `SetGetTagFunctionAsync`/`SetSymbolFunctionAsync`, which no
  longer break the element (the test failed before the fix with `TypeError: this.getTag is not a function`). Pro: the
  `WaCombobox` tags and filter, the disabled days and day content of `WaDatePicker`, `WaDateInput` and
  `WaDateRangeInput` (in the shadow root of the input's own calendar too), and `ShadowStyles` on a `WaDataGrid` cell.
- `tests\icon-library-functions.spec.js` — `IconLibraryOptions.ResolverFunction` and `MutatorFunction`
  (`/testing/icon-library`, functions from the collocated `IconLibraryHarness.razor.js`, data URL icons and the local
  sprite `wwwroot\fragments\icon-sprite.svg`, so no network): a module resolver gets `(name, family, variant,
  autoWidth)` and `ctx` (`args`, `library`); a registered resolver returns a Promise; a sprite sheet resolver fills the
  `<use>` element; module and global mutators change the SVG; the obsolete `Mutator` path still works; a resolver that
  does not resolve fails the registration, and a mutator that does not resolve is left out. Free CDN.
- `tests\grid-functions.spec.js` — the function-valued members of `WaDataGrid` phase 2 (`/testing/grid-functions`,
  functions from the collocated `GridFunctionsHarness.razor.js`, and the "More JavaScript Members" example of
  `/components/data-grid`): a `ValueFunction` drives sort, search and the CSV export; a `FooterFunction` sums the
  filtered rows in an upgraded badge; `CellClassFunction` and `RowClassFunction` classes styled by `ShadowStyles`; a
  `FilterFunction`; `SelectableRowsFunction` locks a row for select-all; a `SearchFunction` ignores diacritics; a
  `RowDetailFunction` panel with an upgraded tag on expand; an `AggregationFunction` on a grouped grid; a
  `ChildRowsFunction` tree from `ctx.args` and `ctx.rowKey` on the first render (the data waits for the function, as
  WA 3.14.0 reads `childRows` only with the data). Pro only.
- `tests\cell-actions.spec.js` — the phase 2 extras of `WebAwesome.Blazor.Extensions` (`/testing/cell-actions` with the
  collocated `CellActionsHarness.razor.js`, `/testing/lit-cells`, and the "Extensions: Actions & Rules" example of
  `/components/data-grid`): the `ctx` helpers without a grid (free CDN); with Pro assets, the interactive presets
  (`Button`, `Actions`, `Menu`, `Switch`, `Checkbox`) raise `@onwablazor-cell-action` and not `OnCellClick` (D4), Tab,
  Enter and Space in an action cell (R4), a menu in a pinned column and in the last row (R6), `ctx.emit`, `ctx.h` and
  `data-wablazor-action` in a cell, a detail panel and a footer, a Lit template of the app's own Lit (R2), and the class
  rules, footers, collation, formatted search, filter options, detail fields and selectable field. `cell-presets.spec.js`
  covers the other presets (the DOM of all of them without a grid on the free CDN).
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
- `tests\date-typing.spec.js` — the strongly typed date and time wrappers (`/testing/date-typing`): a user picks a
  date, a range and a time and the typed model (`DateOnly`, `WaDateRange`, `TimeOnly`) shows up in the Blazor echo,
  a half-filled range binds as `From` only, `DisabledDates`/`DisabledDaysOfWeek` disable exactly those calendar cells,
  and `WaRelativeTime` reads a UTC `DateTimeOffset` as that instant in a browser at `Asia/Tokyo`. Model to UI after a
  user edit for the range wrappers is in `value-sync-binding.spec.js`.
- `tests\day-content.spec.js` — `WaDayContent` in all four date hosts (`/testing/day-content`): the initial content
  shows in its own day cell, every content for one date shows, content added and removed by a page render and toggled
  inside a component rendering on its own (no host render) is followed, and content of the next month shows after
  navigating there. A cell is read through its day slot's flattened assigned nodes, so wa-date-input's forwarding slot
  is followed. The add/remove/toggle rows prove the wrapper's forwarding nudge for wa-date-input (they fail without it).
- `tests\sticky-attributes.spec.js` — the sticky attribute rule (`/testing/sticky-attributes`): `WaSlider` Max 50 back
  to its default 100, `WaInput` Type Password back to Text and a nullable `WaTooltip` Distance 20 back to null each leave
  the element property at Web Awesome's default (100, `text`, 8). With the attribute removed instead, Lit sets the
  slider max and the tooltip distance to null (both cases fail then). A WaRelativeTime and a WaFormatDate Date set to 2020
  and back to null show the current time again, not the 1970 epoch a removed date attribute reads as.
- `tests\toast-items.spec.js` — declarative `WaToastItem`s removed from the model in `OnAfterHide` (`/testing/toast-items`
  and the Overlays showcase): closed by the close button, by `Duration` and by `HideAsync`, each leaves no Blazor error,
  its element is gone, the empty stack closes and a toast added afterwards shows. An item the model keeps stays hidden in
  place and its stack still closes. Without the JS initializer's ownership guard every case fails with
  `Cannot read properties of null (reading 'removeChild')`.
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
  The skip decides on the outcome, not on a grace period: the module next to the page's Web Awesome
  script answering 404 skips at once, and otherwise the test waits for the upgrade (up to
  `WA_READY_TIMEOUT_MS`, 45 s), so a slow page under the default 20 workers no longer skips a Pro test.
- Timing: a test may take 60 s (`playwright.config.js`), because booting the WASM demo and loading the
  CDN modules under a full worker load can take most of 30 s; `waitForWaReady` fails after 45 s naming
  the elements that never got ready, and every assertion keeps its own `expect` timeout.
- Readiness, not timing: a test acts only on elements that have upgraded (`waitForWaReady` with every tag it
  touches), waits for the element's own state where one exists (a tooltip's resolved `anchor`), hovers and clicks
  at action time rather than at coordinates read earlier, and asserts on a request or event issued after its own
  action (`expectFired` with the count it expects), never on whichever came last. Both readiness probes also survive
  the one-off page navigation a full worker load can cause during the boot.
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

Chromium runs with `--disable-audio-output` (`playwright.config.js`): a fake audio sink drives the media clock. A real
sink can stall: the "Remote Audio" device of a Windows RDP session keeps `currentTime` of a playing clip at 0, so the
`WaVideo` `OnEnded` case failed there.

The same suite also runs against the server-hosted demo variant (interactive server render
mode over a SignalR circuit) — both hosting models should stay green:

```powershell
dotnet run --project src\WebAwesome.Blazor.Demo.Server --no-build --launch-profile http
cd tools\e2e
$env:DEMO_BASE_URL = 'http://localhost:5100'; npm test
```

## WA Inspector tests

`inspector\*.spec.js` test the WA Inspector browser extension (`tools\wablazor-marketplace\plugins\wablazor\assets\wa-inspector-extension`) on the harness page
`/testing/inspector`. They have their own config, `playwright.inspector.config.js` (one worker, base URL
`http://localhost:5100`), and the standard suite and the release gate do not run them. Each test launches the full
Chromium build (`channel: 'chromium'`, headless; the headless shell loads no extensions) in a persistent context with
the extension loaded, opens the panel as a tab (`panel.html?tabId=<id>`) and drives it through its test hooks
(`window.__waInspectorPanel`) and its DOM. The page fixture keeps the Blazor crash guard of `tests\helpers\test.js`.
Covered: the element tree, the placement metadata, attribute overrides on a placement and on a source site that stay
applied after a render, a navigation and a reload, the remap of a data-driven value, CSS custom properties, utility
classes, theme tokens, comments, the numbered markers, the item status and the export files.

Run them against Demo.Server with the metadata on:

```powershell
$env:WebAwesome__EmitInspectorMetadata = 'true'
dotnet run --project src\WebAwesome.Blazor.Demo.Server --no-build --launch-profile http
cd tools\e2e
npm run test:inspector
```

## Release gate

`tools\release\Test-WaReleasePreflight.ps1` runs this suite itself: it starts the WebAssembly demo on a
free loopback port, checks that the server answering there is the process it started, and runs
`npx playwright test --forbid-only --reporter=list,json` with `CI=1` (so `test.only` fails and retries
are armed; flaky tests are listed). It then checks the JSON report against
`data\expected-skips.json`: every skipped test must be listed with a reason for the asset mode
(`free-cdn`, or `pro` for the second pass against a self-hosted Pro dist: by default the local Pro build of the target version at `temp\wa-src\<version>`, else `-ProDist <path>`; the gate warns, without failing, when neither exists, and `-SkipProE2E` turns the pass off), a
listed test that ran, is listed twice or no longer exists fails, and the pass must run at least `minimumTests`
tests. So a self-skipping test (a Pro component on the free CDN) must be added to that file, and
`minimumTests` must be raised when tests are added.

## Adding tests

- Take `test` and `expect` from `./helpers/test`, never from `@playwright/test` (`test-base.spec.js` fails
  otherwise), so the spec fails when Blazor crashes.
- New demo pages are driven by `interaction-sweep.spec.js` automatically. If one of their interactions legitimately
  leaves the page (a navigation, a download, a new window), list the element in its `ROUTE_EXCLUSIONS` with the
  reason; fix any other failure.
- A new showcase needs a flow spec (`showcase-<name>.spec.js`) with its real user tasks, and its route in
  `SHOWCASE_ROUTES`; a component worked into a showcase gets steps in that showcase's flow.
- Assert with polling `expect`, never fixed timeouts; drive timers with the page clock (`openShowcase` with
  `clock: true`, then `page.clock.fastForward`, or `pauseClock` to hold a state that ends on a timer).
- New component demo pages are picked up automatically by `sweep.spec.js` (it reads routes
  straight from `api-surface.json`, the same document that drives the demo's own nav) — no
  maintenance needed there.
- Wrapper pages that render another wrapper's element in one of its modes (the date range wrappers) are not in
  `api-surface.json`; list them in `WRAPPER_ROUTES` in `tests\helpers\routes.js` and in `ComponentCategoryMap.WrapperPages`.
- Layout routes (`LAYOUT_ROUTES` in `tests\helpers\routes.js`) are hand-authored components
  (`WaCluster`, `WaFlank`, ...) with no generated manifest; keep that list in sync with
  `MainLayout.razor`'s `LayoutLinks` array by hand if a layout component is added or removed.
- When you find and fix another bug this way, add a targeted regression test alongside the
  existing ones and log the root cause + workaround in `docs\CHANGELOG.md` so the `/wa-upgrade`
  pipeline knows to re-verify it against future Web Awesome releases (see
  `docs\contributing\UPGRADE-PROCESS.md` → "Pending workarounds to re-verify every upgrade").
