# Web Awesome 3.10.0 to 3.11.0 Upgrade Implementation Plan

JIRA: WAB-44 (epic WAB-43 "Web Awesome 3.11")
Branch: `/main/WA-3.11/WAB-44` (new train subtrunk `/main/WA-3.11`, created off `/main` cs:227)
Source tag: https://github.com/shoelace-style/webawesome/tree/v3.11.0

## Overview

Upgrade the Blazor bindings from Web Awesome 3.10.0 to 3.11.0. The release adds three components
(one Pro), normalizes the chart axis-label attribute names, adds two carousel slide-management
methods, and otherwise consists almost entirely of newly documented CSS parts.

Change report: `temp\wa-api\changes_3.10.0_to_3.11.0.json` / `.md`
(3 added, 0 removed, 45 modified, 18 breaking).

## Analysis Summary

| Metric | Count |
|---|---|
| New components | 3 |
| Removed components | 0 |
| Modified components | 45 |
| Breaking changes (report) | 18 |
| Components in CEM | 87 (was 84) |

**The 18 reported breaking changes are all one upstream rename**, repeated across the nine chart
components: the CEM previously declared the axis-label attributes as `xLabel`/`yLabel` and now
declares them as `x-label`/`y-label`. This is an *attribute-name* change only; the underlying
property and its semantics are unchanged, and the wrapper's public parameter names (`XLabel`,
`YLabel`) are unaffected because both spellings map to the same PascalCase name. The only real
work is the attribute string the wrapper renders. **There is no consumer-visible breaking change
in this upgrade, so no migration document is required.**

**41 of the 45 modified components changed only in `cssParts`.** The parity harness enforces
components, attributes, events and methods — not CSS parts — so these require no wrapper work.
Two are worth noting explicitly:

- `wa-page`: cssParts `skip-link`/`skip-links` removed, `skip-to-content` added. `WaPage` already
  renders the `skip-to-content` slot, so it is already aligned; no source references the removed
  part names.
- `wa-card`: new `actions` cssPart; no wrapper impact.

### Documentation ingest deviation (recorded per the failure-handling rule)

Web Awesome 3.11.0 is published to npm/jsdelivr but **has no `v3.11.0` tag in the public GitHub
repository** (newest public tag is `v3.10.0`), so `Sync-WaDocs.ps1` could not fetch a 3.11.0 docs
tree. `tools\upgrade\Sync-WaDocs.ps1` gained two documented parameters to handle this class of
release:

- `-DocsTagVersion` — takes the non-component documentation from an older tag that does exist.
- `-PreferBundledRefs` (implied by `-DocsTagVersion`) — lets the release zip's version-exact
  component references win over the older tree's component docs.

Run: `Sync-WaDocs.ps1 -Version 3.11.0 -DocsTagVersion 3.10.0`. Result: all **87** component docs
come from the 3.11.0 release zip's bundled references (21 filled, 66 overridden), 0 needing manual
capture; the 23 non-component doc files were byte-identical to the already-synced 3.10.0 tree.
Checked in as cs:230 (`inputs\WebAwesome`), the tool change as cs:229.

## Phase 1 — Breaking changes

### Chart axis-label attribute rename (`xLabel`/`yLabel` → `x-label`/`y-label`)

Affects all nine chart components through their shared base.

- `src\WebAwesome.Blazor\Components\WaChartBase.cs` — change the two rendered attribute names at
  sequences 16/17 from `"xLabel"`/`"yLabel"` to `"x-label"`/`"y-label"`. Parameters `XLabel` and
  `YLabel` are unchanged.
- `src\WebAwesome.Blazor.Tests\Components\WaChartIntegrationTests.cs` — update the three
  assertions that reference the literal `xLabel` attribute (lines ~28, ~80) to the new spelling.

No public API change, no enum change, no event-args change, no component deletions.

## Phase 2 — New components

All three are `status: experimental`, `since: 3.11`. Delegated to **wa-wrapper-engineer**
(one group; `WaDataGrid` gets its own agent because of its size).

### 1. WaPagination (`wa-pagination`) — NEW, free

17 attributes (2 globally ignored: `did-ssr`, `dir`, `lang`), 2 events, 4 slots, 0 methods.

- New file `src\WebAwesome.Blazor\Components\WaPagination.cs`.
- Enums: `WaPaginationAppearance` (`outlined`/`filled`/`plain`), `WaPaginationFormat`
  (`standard`/`compact`) with `ToHtmlValue()`.
- Events `wa-before-page-change` (cancelable) and `wa-page-change` → `OnBeforePageChange`,
  `OnPageChange`; register both in `WebAwesome.Blazor.lib.module.js` with `onwa-` prefix.
- Event args carrying the target page derive from `System.EventArgs`.
- `href-template` has a union type `string | ((page: number) => string)`; expose the **string**
  form only and record the function form as an intentional deviation.
- Slots `first-icon`, `last-icon`, `next-icon`, `previous-icon` as `RenderFragment?`.

### 2. WaOtpInput (`wa-otp-input`) — NEW, free, form control

22 attributes, 7 events, 2 slots (`hint`, `label`), 7 methods.

- New file `src\WebAwesome.Blazor\Components\WaOtpInput.cs`, deriving from `WaInputBase<string>`
  (string value, `length` default 6). Reuse the base's `Value`, `Name`, `Required`, `Disabled`,
  `Readonly`, `Hint`, `Label`, `CustomError`, `Size`, `Appearance`, `Autofocus`, `Autocomplete`
  members wherever the base already provides them — do not shadow base properties.
- Enums: `WaOtpInputType` (`numeric`/`alpha`/`alphanumeric`), `WaOtpInputCase`
  (`preserve`/`upper`/`lower`). `appearance` and `size` reuse the existing shared enums if their
  value sets match, otherwise add component-specific ones.
- Both `mask` and `with-mask` are declared and are **distinct features** (verified in
  `otp-input.d.ts`): `mask` displays entered characters as `--mask-char` (obscured input), while
  `with-mask` shows `--mask-char` as a hint in *empty* segments. Bind both as separate booleans
  (`Mask`, `WithMask`); neither is an alias, so no deviation entry is needed.
- Events: `blur`, `focus`, `change`, `input`, `wa-clear`, `wa-complete`, `wa-invalid` — the base
  already covers the standard form-control ones; add `OnClear` and `OnComplete`.
- Methods: `blur`, `focus` are native (allowlisted globally); `clear`, `select`, `resetValidity`,
  `setCustomValidity` → wrapper methods (base may already provide the validity ones);
  `formStateRestoreCallback` is a browser form-association callback, not consumer API → ignore
  with a reason.

### 3. WaDataGrid (`wa-data-grid`) — NEW, **Pro**

28 attributes, 15 events, 3 slots (`empty`, `loading`, `no-results`), 26 methods. The largest
single wrapper in the library; its own wa-wrapper-engineer agent.

Data-binding design (the key decision, per the reference doc: *"Like the chart components, data
grids are driven by JavaScript properties. Set `data` and `columns` on the element"*):

- `data` and `columns` are **JS properties, not attributes**, and there is no declarative JSON
  child form (unlike `wa-chart`). Expose them as typed C# parameters and push them with the
  existing `WebAwesomeJSInterop.SetPropertyAsync` on first render and on parameter change:
  - `[Parameter] public IReadOnlyList<TItem>? Data` — serialized to JS.
  - `[Parameter] public IReadOnlyList<WaDataGridColumn>? Columns` — a new serializable record in
    `src\WebAwesome.Blazor\Models\` covering the JSON-expressible column members (`field`,
    `label`, `width`, `flex`, `minWidth`, `align`, `sortable`, `filterable`, `resizable`,
    `pinned`, `footer`, …) as read from `data-grid.d.ts`.
  - The column `formatter` member is a JS function / Lit template and **cannot cross the interop
    boundary** — omitted, recorded as an intentional deviation.
  - `dataSource` (a request→Promise function) likewise cannot be marshaled. The Blazor-idiomatic
    server path is `Server="true"` + the `wa-data-request` event + `Total`/`Loading` + reassigning
    `Data`; record `dataSource` as a deviation pointing at that pattern.
  - `child-rows` union `string | ((row) => Row[])`: expose the string form only.
- Non-generic wrapper preferred (`Data` as `IReadOnlyList<object>`) unless a type parameter is
  needed; a generic `WaDataGrid<TItem>` is acceptable — parity strips generic arity when matching
  the class name.
- Enums with `ToHtmlValue()`: `WaDataGridAppearance` (`outlined`/`plain`), `WaDataGridSelectable`
  (`single`/`multiple`/`none`, plus the empty string meaning "multiple"). `size` reuses the shared
  size enum.
- 15 events, all `wa-*` except `request`: register every one in
  `WebAwesome.Blazor.lib.module.js` with the `onwa-` prefix, and add `specialArgs` payload
  mappings where the detail carries DOM nodes (`wa-cell-click`, `wa-cell-contextmenu`,
  `wa-column-*`, `wa-row-*` carry row/cell/column references — map to serializable fields such as
  index, field name, row key, and `detail.finished` for the live drag events). Event args classes
  inherit `System.EventArgs`.
- 26 methods: expose as `…Async` JS-interop methods. Ones returning DOM nodes or non-marshalable
  objects (`getProcessedRows`, `getVisibleRows`, `getColumnFacets`, `getState`/`setState` if the
  state object is not JSON-round-trippable) must either return a serializable projection or be
  ignored with a reason. `handleColumnsChange`, `handlePageChange`, `handleSearchTermChange` are
  internal event handlers, not consumer API → ignore with reasons. `focus` is native.

## Phase 3 — Modified components (additive)

### WaCarousel (`wa-carousel`) — two new methods

- `addSlide(slide: WaCarouselItem)` → `AddSlideAsync(ElementReference slide)`. The parameter is a
  DOM element; `ElementReference` marshals correctly through the existing `invokeMethod` interop.
  Document that the declarative `ChildContent` route is the normal way to add slides in Blazor.
- `removeSlide(index: number)` → `RemoveSlideAsync(int index)`.

No other component needs wrapper changes (the remaining modifications are cssParts only).

## Phase 4 — Intentional deviations (parity-config.json)

`targetWaVersion` → `3.11.0`, `enabled` stays true. Additions, each with an `ignoreReasons` entry:

- `wa-pagination`: `hrefTemplate` function form (attribute bound as string).
- `wa-otp-input`: `formStateRestoreCallback` (browser form-association callback).
- `wa-data-grid`: `columns`, `data`, `dataSource` if they surface as attributes; the internal
  `handleColumnsChange` / `handlePageChange` / `handleSearchTermChange` methods; any method
  returning non-marshalable objects.
- Re-stamp the existing `extraElementMethods` reasons for 3.11.0 (see below).

### Element method audit (re-verified against the 3.11.0 sources)

| Component | Allowlisted | 3.11.0 verdict |
|---|---|---|
| `wa-mutation-observer` | `stopObserver`, `startObserver` | present as private methods in `mutation-observer.d.ts` — still valid |
| `wa-resize-observer` | `stopObserver`, `startObserver` | present as private methods in `resize-observer.d.ts` — still valid |
| `wa-relative-time` | `update` | still the inherited Lit `ReactiveElement.update()`; `WebAwesomeElement extends LitElement` confirmed in `internal\webawesome-element.d.ts` — still valid |

All three entries re-stamped with the 3.11.0 verification date; none became CEM-documented.

## Phase 5 — Tests and docs

Delegated to **wa-test-engineer**.

- Integration tests for `WaPagination`, `WaOtpInput`, `WaDataGrid` following the existing
  `Wa*IntegrationTests.cs` pattern.
- `WaOtpInput` additionally gets bUnit **EditForm** coverage (binding, change propagation,
  validation lifecycle, custom validity) per the form-control tests in `…\Tests\Base\`.
- `WaCarouselIntegrationTests` — cover `AddSlideAsync`/`RemoveSlideAsync`.
- `WaChartIntegrationTests` — assert the new `x-label`/`y-label` attribute spelling.
- No `docs\MIGRATION-3.11.0.md`: there is no consumer-visible breaking change.
- `docs\CHANGELOG.md` — new `## [3.11.0]` section; `### Breaking changes` records the upstream
  attribute-name normalization as internal-only (no consumer action).
- Demo: `New-WaDemoPages.ps1 -PruneRemoved`, then curate `PaginationPage`, `OtpInputPage`,
  `DataGridPage` from `inputs\WebAwesome\components\*.md`; work the new components into the
  showcases (OTP input + pagination → form showcase; data grid → dashboard showcase).
- Promote `PublicApiSnapshotTests` baseline once every diff is explained by the change report.

## Validation checklist

- [ ] `dotnet build src/WebAwesome.slnx -p:Configuration=Debug` — 0 warnings, 0 errors
- [ ] `dotnet build src/WebAwesome.slnx -p:Configuration=Release` — 0 warnings, 0 errors
- [ ] `dotnet test src/WebAwesome.slnx` green on net9.0 and net10.0 (baseline was 562 per TFM)
- [ ] `ApiSurfaceParityTests` green (4 gaps at arming: 3 wrappers + 2 carousel methods)
- [ ] `EventBindingRegistrationTests` green — all new `wa-*` events registered in the JS module
- [ ] `ElementMethodInvocationTests` green — allowlist re-verified
- [ ] `PublicApiSnapshotTests` baseline promoted, every diff explained
- [ ] Demo builds; new component pages curated (no TODO markers); showcases extended
- [ ] Playwright sweep from `tools\e2e` green (3.11.0 confirmed available on jsdelivr)

## Risks

- **WaDataGrid scope.** 28 attributes / 15 events / 26 methods and a JS-property data contract
  make it by far the largest wrapper. Its rich-cell story (Lit `formatter` templates) has no
  Blazor equivalent; the wrapper will cover data, columns, and the whole attribute/event/method
  surface, but not custom cell renderers. This is a documented capability gap, not a defect.
- **Experimental upstream status.** All three new components are `status: experimental`, so their
  API may shift in 3.12; the CHANGELOG entry should say so.
- **`wa-data-grid` event payloads.** Several details carry DOM nodes; without correct `specialArgs`
  mappings the typed event args would be empty at runtime — a class of bug bUnit cannot see, so
  the Playwright sweep must exercise the data grid page.
- **Docs provenance.** Component docs come from the release zip's bundled references rather than
  the public GitHub tree this cycle; the non-component docs are pinned to `v3.10.0` and will
  refresh normally once `v3.11.0` is tagged publicly.
- ~~**`wa-otp-input` duplicate mask attributes.**~~ Resolved during analysis: `mask` and
  `with-mask` are independent features, both bound as separate booleans.
