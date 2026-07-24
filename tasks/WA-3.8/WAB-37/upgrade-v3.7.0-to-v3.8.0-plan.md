# Upgrade plan: Web Awesome 3.7.0 → 3.8.0

- **Ticket:** WAB-37 (epic WAB-36 "Web Awesome 3.8")
- **Train:** WA-3.8 (new subtrunk `/main/WA-3.8`, branched off `/main` at cs:195 after 3.7.0 release; task branch `/main/WA-3.8/WAB-37`)
- **Source tag:** https://github.com/shoelace-style/webawesome/tree/v3.8.0
- **Change report:** `temp\wa-api\changes_3.7.0_to_3.8.0.json` / `.md` — 6 added, 0 removed, 10 modified, 2 flagged breaking.
- **Component count:** 76 → 82.

## Summary of the diff

| Bucket | Count | Items |
|---|---|---|
| New components | 6 | wa-accordion, wa-accordion-item, wa-date-input (Pro), wa-date-picker, wa-known-date, wa-time-input |
| Modified (additive) | 8 | wa-card, wa-file-input, wa-qr-code, wa-rating, wa-tab, wa-tab-panel, wa-tree, wa-tree-item |
| Flagged breaking | 2 | wa-drawer (light-dismiss default), wa-video (timeupdate event type) — **neither is a wrapper-API break** (see Phase 1) |

## Phase 1 — Breaking changes (verify-before-breaking)

Both "breaking" entries are **default/type-annotation** changes, not surface removals. Applying the CEM-diff lesson (a diff entry is not automatically a wrapper break — verify upstream first), neither requires a wrapper API change:

1. **wa-drawer `light-dismiss` default `true` → `false`.** The wrapper (`WaDrawer.LightDismiss`, a `bool`) already defaults to `false` and emits the attribute only when `true` (Blazor omits a false bool attribute). Previously the wrapper's `false` default silently disagreed with WA's `true` default; the 3.8.0 change **aligns** them. No code change. Behavioral note goes to CHANGELOG/migration.
2. **wa-video `timeupdate` event `type` `null` → `Event`.** Annotation-only; the event name is unchanged and already bound via `parity-config` `eventOverrides` (`timeupdate` → `OnTimeUpdate`). No code change.

No components removed → no wrapper/enum/event-arg/test deletions.

## Phase 2 — New components

Delegated to `wa-wrapper-engineer` in two parallel groups.

### Group A — Accordion family (free, experimental)
- **WaAccordion** (`wa-accordion`) — `ComponentBase` container.
  - Attributes: `appearance` (reuse `WaAppearance`, default `Outlined`; covers filled/outlined/filled-outlined/plain), `heading-level` (`string? HeadingLevel`, default "3"), `icon-placement` (reuse `WaIconPlacement`), `mode` (new enum `WaAccordionMode`: single / single-collapsible / multiple).
  - Events: `OnExpand`, `OnCollapse`, `OnAfterExpand`, `OnAfterCollapse` as `EventCallback<EventArgs>`. The `wa-expand/collapse/after-*` detail carries a live `{ item }` DOM node; consistent with the library's DOM-node-drop convention (tree/carousel) and `WaDetails`, the item is **not** projected — plain `EventArgs`. All four event names are already registered in the JS module.
  - Slot: default → `ChildContent`. Methods: `ExpandAllAsync`, `CollapseAllAsync`.
- **WaAccordionItem** (`wa-accordion-item`) — `ComponentBase`.
  - Attributes: `disabled`, `expanded`, `label` (`string?`).
  - Slots: default → `ChildContent`, `icon` → `IconContent`, `label` → `LabelContent`. No events. Methods: `ExpandAsync`, `CollapseAsync`, `ToggleAsync`, `FocusAsync`.

### Group B — Date/Time family
- **WaDateInput** (`wa-date-input`, **Pro**) — `WaInputBase<string?>`, modeled on `WaCombobox` (popup form control). Value is the ISO date/range string. New enums as needed (mode, first-day-of-week, page-by, weekday-format); reuse `WaSize`, `WaInputAppearance`, `WaPlacement`. `months` (1|2) → `int`. wa-* popup events (`OnShow/OnHide/OnAfterShow/OnAfterHide/OnClear/OnInvalid`); `blur/focus/input` inherited from base. Methods: `ShowAsync/HideAsync/ClearAsync/BlurAsync/FocusAsync` (+ inherited `SetCustomValidityAsync/ResetValidityAsync`).
- **WaTimeInput** (`wa-time-input`) — `WaInputBase<string?>`, sibling of WaDateInput. `hour-format` enum, `step` (`number|'any'`). Same event/method shape as WaDateInput.
- **WaKnownDate** (`wa-known-date`) — `WaInputBase<string?>`, simpler birthday-style 3-field control. `appearance`, `locale`, `min`, `max`, `size`. Events `OnInvalid` (+ inherited blur/focus/input); methods inherited validity only.
- **WaDatePicker** (`wa-date-picker`) — **not form-associated** (no `name`/`custom-error`/FACE in surface): plain `ComponentBase` with manual two-way `Value`/`ValueChanged` (native `change` folded into the binder). Events: `OnInput`, `OnFocusDay` (typed `WaDatePickerFocusDayEventArgs { Date }`), `OnViewChange` (typed `WaDatePickerViewChangeEventArgs { View, Date }`) — both details carry JS `Date`, projected to ISO strings via new JS `specialArgs`. Methods: `ClearAsync/FocusAsync/GoToDateAsync/GoToTodayAsync`.

## Phase 3 — Modified components (additive)

Delegated to `wa-wrapper-engineer` (Group C):
- **WaCard**: add `WithHeaderActions`, `WithFooterActions` (bool, SSR flags); emit `with-header-actions` / `with-footer-actions`, OR'd with the presence of the actions slots (mirrors existing `with-header`/`with-footer`).
- **WaFileInput**: add `Capture` (new enum `WaCaptureMode`: user / environment, nullable); emit `capture`.
- **WaQrCode**: add `Image` (`string?`), `ImageBackground` (`string?`), `ImageCoverage` (`decimal?`, 0–1 fraction), `ImagePadding` (`int?`, px). Types confirmed against `inputs\WebAwesome\components\qr-code.md`.

ARIA/global reflected attributes added to `wa-rating/tab/tab-panel/tree/tree-item` (`role`, `tabindex`): these are native global HTML attributes surfaced by WA's internal ARIA management, passed through via `AdditionalAttributes`. **No parameters added** — recorded as intentional deviations in `parity-config.json` `globalIgnoredAttributes` (`role`, `tabindex`) with reasons (same treatment as `title`/`dir`/`lang`).

## Phase 4 — parity harness / config

- `expected-api-surface.json` ← `surface_3.8.0.json`; `targetWaVersion` → `3.8.0`; `enabled` stays true.
- `globalIgnoredAttributes` += `role`, `tabindex` (+ `ignoreReasons`).
- New form-control config: `wa-date-input`, `wa-time-input`, `wa-known-date` → `ignoredAttributes: [name, custom-error]`, `ignoredEvents: [change]`, `ignoredMethods: [formStateRestoreCallback]`. `wa-date-picker` → `ignoredEvents: [change]`.
- `extraElementMethods` re-verified against 3.8.0 source (2026-07-24): `startObserver`/`stopObserver` still private on mutation-observer.d.ts + resize-observer.d.ts; relative-time `update` still the inherited Lit lifecycle method (only `updateTimeout` in relative-time.d.ts). Append 3.8.0 to the reason strings; no allowlist changes.
- Version bump `Version.props` 3.7.0 → 3.8.0 (Assembly/File 3.8.0.0); README version refs; demo `index.html` CDN + `wwwroot/data/api-surface.json`.
- Event delivery: register `wa-focus-day`, `wa-view-change` in `WebAwesome.Blazor.lib.module.js` with `specialArgs` projecting the JS `Date` to an ISO string.

## Per-file actions

| File | Action |
|---|---|
| `Components\WaAccordion.cs` | new |
| `Components\WaAccordionItem.cs` | new |
| `Components\WaDateInput.cs` | new (Pro) |
| `Components\WaDatePicker.cs` | new |
| `Components\WaKnownDate.cs` | new |
| `Components\WaTimeInput.cs` | new |
| `Components\Enums.cs` | add WaAccordionMode, WaCaptureMode, + date/time enums (mode/first-day-of-week/page-by/weekday-format/hour-format as needed) |
| `Components\EventArgs.cs` | add WaDatePickerFocusDayEventArgs, WaDatePickerViewChangeEventArgs |
| `Components\WaCard.cs` | + WithHeaderActions/WithFooterActions |
| `Components\WaFileInput.cs` | + Capture |
| `Components\WaQrCode.cs` | + Image/ImageBackground/ImageCoverage/ImagePadding |
| `wwwroot\WebAwesome.Blazor.lib.module.js` | register wa-focus-day, wa-view-change (+specialArgs) |
| `ApiParity\expected-api-surface.json` | replace with 3.8.0 surface |
| `ApiParity\parity-config.json` | targetWaVersion, globalIgnored role/tabindex, new form-control entries, extraElementMethods reason stamp |
| `Version.props`, `README.md`, demo `index.html`, demo `data\api-surface.json` | version bump |
| Tests (`Wa*IntegrationTests.cs`, EditForm coverage, breaking-change tests) | Phase 5 (wa-test-engineer) |
| `docs\MIGRATION-3.8.0.md`, `docs\CHANGELOG.md` | Phase 5 |
| Demo pages + showcases | Phase 5 |

## Validation checklist

- [ ] `dotnet build` Debug + Release green (incl. demo)
- [ ] `dotnet test` green both TFMs — ApiSurfaceParityTests, ElementMethodInvocationTests, EventBindingRegistrationTests, PublicApiSnapshotTests
- [ ] PublicApi baseline promoted; every diff explained by the change report
- [ ] New form controls (WaDateInput/WaTimeInput/WaKnownDate) have bUnit EditForm coverage
- [ ] Demo pages curated for all 6 new components; showcases updated (form controls → form showcase)
- [ ] Playwright e2e sweep green
- [ ] Check-ins per phase; JIRA → In Review

## Risks

- **Date/time family complexity.** wa-date-input has 37 attributes and several opaque named union types (`WaDateInputMode`, `WaDateInputFirstDayOfWeek`, …). Engineer resolves concrete values from `inputs\WebAwesome\components\*.md` (refreshed) / `temp\wa-src\3.8.0\dist\components\*.d.ts`; ambiguities recorded here.
- **wa-date-picker binding shape.** Not form-associated; using plain `ComponentBase` + manual `Value`/`ValueChanged` rather than `InputBase` (which requires a cascading EditContext). Its `change` event is folded into the binder and ignored in parity with the standard reason.
- **JS Date marshaling** for wa-focus-day / wa-view-change requires new `specialArgs`; verified by the Playwright sweep (bUnit cannot see the JS interop path).
- **Pro-source rule:** all 3.8.0 extraction stays under `temp\`; only the surface JSON and bundled reference docs are checked in.
