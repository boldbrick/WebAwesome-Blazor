# Web Awesome 3.13.0 to 3.14.0 Upgrade Implementation Plan

JIRA: WAB-50 (epic WAB-49 "Web Awesome 3.14")
Branch: `/main/WA-3.14/WAB-50` (new train subtrunk `/main/WA-3.14`, created off `/main` cs:321; epic folder cs:322)
Source tag: https://github.com/shoelace-style/webawesome/tree/v3.14.0

## Overview

Upgrade the Blazor bindings from Web Awesome 3.13.0 to 3.14.0. Upstream this is an additive release: two new
free components, `wa-stepper` and `wa-step`, a server (async options) mode for `wa-combobox`, a label for
`wa-divider`, and a handful of new attributes on `wa-dialog`, `wa-drawer`, `wa-page` and `wa-zoomable-frame`.
Nothing is removed or renamed upstream, and no wrapper member is removed or renamed either, so **no migration
guide** is needed.

Release gate: `/main` head cs:321 carries `Version.props` 3.13.0 (the WA-3.13 promotion of cs:320), so the
WA-3.14 train could start. No patch work is pending on `/main/WA-3.13`.

Change report: `temp\wa-api\changes_3.13.0_to_3.14.0.json` / `.md`
(2 added, 0 removed, 6 modified, 0 breaking).

## Analysis Summary

| Metric | Count |
|---|---|
| New components | 2 (`wa-stepper`, `wa-step`; free, experimental, since 3.14) |
| Removed components | 0 |
| Modified components | 6 (all additive) |
| Breaking changes (report) | 0 |
| Components in CEM | 90 (was 88) |
| Wrapper breaking changes | 0 |

### Change report tooling fix

The first run of `Compare-WaApiSurface.ps1` reported one breaking change, `wa-divider - slots removed: ''`.
The divider had **no** slots in 3.13.0 and gains a default slot in 3.14.0. Under PowerShell 7,
`@($o.PSObject.Properties.Name)` over an empty object yields a single `$null`, which the diff read as a
removed slot named `''` whenever a map went from empty to non-empty. `Get-Keys` now enumerates the
properties explicitly; the regenerated report has 0 breaking changes. Earlier reports were generated from
maps that were never empty on the "from" side of a non-empty "to" map, so they are unaffected.

### Decision: `wa-step` is wrapped as `WaStepperStep`

The CEM class name of `wa-step` is `WaStep`, but `WebAwesome.Blazor.Components.WaStep` is already the public
`number | 'any'` step value of `WaInput`, `WaNumberInput`, `WaTimeInput` and the sliders (since 3.12.0,
`Step="WaStep.Any"`). Renaming that struct would break every consumer that spells `WaStep` for a type
released two versions ago. The wrapper is therefore named **`WaStepperStep`**, recorded as a
`componentClassOverrides` entry (`wa-step` → `WaStepperStep`) with its reason, like `wa-textarea` →
`WaTextArea`. It reads naturally inside its only valid parent (`<WaStepper><WaStepperStep …>`).
Owner review point: if the struct should rather be renamed (e.g. `WaStepValue`) and the component take
`WaStep`, that is a breaking change for a later, deliberate release.

### Upstream source changes outside the CEM

The compiled chunks carry their `// _bundle_/src/...` module markers, so the 3.13.0 and 3.14.0 builds were
compared module by module (chunk hashes normalized): 10 modules are new (`step`, `stepper`, their styles and
React wrappers, and the events `before-step-change`, `step-change`, `options-request`, `options-error`), none
removed, and 23 changed:

| Module | Change | Wrapper impact |
|---|---|---|
| `combobox` (+ styles, React) | server mode: `server`, `loading`, `filter-debounce`, the JS-only `dataSource` callback property, `reload()`, `wa-options-request`/`wa-options-error`, status slots `empty`/`error`/`loading`/`no-results` | Phase 3 |
| `option` | notifies the closest `wa-select`/`wa-combobox` of slot changes in one lookup | none |
| `dialog`, `drawer` | `with-label` (SSR hint); light dismiss now requires the press to land outside the dialog's rectangle (`internal/offset` `isEventInsideRect`); `aria-label` from `label` when there is no header | `WithLabel` parameter |
| `divider` (+ styles) | renders a default slot as a label (`label` part), `label-placement`, `with-label` (reflected, recomputed from the slot) | `ChildContent`, `LabelPlacement`, `WithLabel` |
| `page` (+ styles) | `nonce` for the injected `<style>` tag; the navigation toggle icon uses the `system` library; an explicit `disable-navigation-toggle` is kept | `Nonce` parameter |
| `zoomable-frame` | `allow`, `label` (→ the iframe `title`), `name` | `Allow`, `Label`, `Name` parameters |
| `animated-image` | SSR placeholder visibility via `styleMap` | none |
| `date-picker` (+ styles) | the live region uses the shared `wa-visually-hidden` class | none (the day-slot rendering is unchanged) |
| `internal/submit-on-enter` | implicit submission counts only submittable controls (`wa-input` by type, `wa-tag-input`, `wa-number-input`, `wa-otp-input`, `wa-slider`) | none; the form showcase's Enter behaviour is re-verified by its flow spec |
| `popover.styles`, `segmented-field.styles`, `visually-hidden.styles`, `icon/library.system`, `translations/de`, `en` | styles, icons, new stepper/combobox terms | none |

### Standing re-verifications (every upgrade)

The following modules are **byte-identical** to 3.13.0 (modulo chunk names), so every allowlist entry and
cited pattern that rests on them still holds:

- `extraElementMethods`: `wa-mutation-observer`/`wa-resize-observer` `stopObserver`/`startObserver`,
  `wa-relative-time` `update`. The observer and relative-time modules and `webawesome-element` are unchanged.
- `sourceVerifiedEvents`: `wa-color-picker` `wa-show`/`wa-after-show`/`wa-hide`/`wa-after-hide`. The
  color-picker module is unchanged, so the relays stay.
- `sourceVerifiedSlots`: `wa-date-picker` `day-YYYY-MM-DD`. The date-picker change is limited to the live
  region's class name and its stylesheet list; the day-slot rendering is unchanged, and the CEM still omits
  the slot (the staleness test confirms).
- `cemOnlyEvents`: `wa-data-grid` `request` (data-grid unchanged). Two new CEM-invented events of the same
  kind: `wa-combobox` `request` and `wa-stepper` `detail` (the analyzer names them after the variable passed
  to `dispatchEvent`; neither is in the `@event` JSDoc or `dist\events`).
- `WaWirePatterns` / `WaWireFormat`: `date-picker/internal/iso.ts`, `matchers.ts` and
  `time-input/internal/segments.ts` are unchanged, and so are the list separators of the existing list
  attributes.
- `wa-date-input` `updateForwardedDaySlots`: the date-input module is unchanged, so the day-slot forwarding
  nudge stays (WAB-46 follow-up).
- Event dispatch re-check (`bubbles: false`, `new CustomEvent(`, `stopPropagation()`): the new event classes
  `WaBeforeStepChangeEvent` (cancelable), `WaStepChangeEvent`, `WaOptionsRequestEvent` and
  `WaOptionsErrorEvent` are all `bubbles: true, composed: true`. `wa-stepper` handles clicks on its host and
  stops nothing; the combobox changes add no `stopPropagation()` and no non-bubbling dispatch. **No relay is
  needed.**
- `tools\upgrade\external-type-aliases.json`: the new unions are all literal; no new external alias appears.
- DOM ownership (`this.remove()`, removals or moves of light-DOM children): the combobox removes the showing
  options and appends the response only when applying a `dataSource` response, which Blazor cannot set. In
  event mode the consumer's (Blazor's) options stay untouched; the element only appends and removes hidden
  `data-retained-option` stand-ins it created itself for selections whose option left, which is harmless.
  `wa-stepper`/`wa-step` move no light-DOM children (the stepper only toggles `data-wa-step-vertical` on them).

### Documentation ingest

See the documentation changeset (`Sync-WaDocs.ps1 -Version 3.14.0`); counts are recorded under
Implementation notes.

### Next-release check items

The 3.13.0 CHANGELOG entry has no open "Next-release check" notes. The WAB-48 follow-ups rest on
`tag-input`, which is byte-identical in 3.14.0: `OnCreate` stays notification-only, and a native form reset
still restores `defaultValue` without a `change` (WA-side, carried forward). The WAB-46 follow-ups are
owner-deferred and carry forward unchanged.

## Phase 1 — Breaking changes

None. The report has no `breakingChanges`, and no wrapper member is removed or renamed.

## Phase 2 — New components

### `WaStepper` (`wa-stepper`)

Free, experimental, Navigation category. A display of the steps of a process; not a form control.

| Attribute | Parameter | Notes |
|---|---|---|
| `active` | `string? Active` | sticky, `DefaultActive = ""`; the element changes it itself (`goTo()`, clicks, `data-stepper` invokers), like `WaTabGroup.Active` |
| `clickable` | `bool Clickable` | |
| `label` | `string? Label` | `DefaultLabel = ""` |
| `linear` | `bool Linear` | |
| `orientation` | `WaStepperOrientation? Orientation` | new enum `Horizontal`/`Vertical`/`Auto`, default `Horizontal` |
| `did-ssr`, `dir`, `lang` | none | global ignores |

Events (all bubble, composed; the detail carries the step elements, which the JS initializer projects to their
names):
- `wa-before-step-change` → `OnBeforeStepChange` (`EventCallback<WaStepChangeEventArgs>`). Cancelable
  upstream; .NET cannot cancel it (Blazor dispatches after the DOM event), so the callback is
  notification-only and says so, like the other cancelable events.
- `wa-step-change` → `OnStepChange` (`EventCallback<WaStepChangeEventArgs>`).
- `WaStepChangeEventArgs`: `Name` (`string`), `PreviousName` (`string?`).
- `detail` (CEM-invented) → `cemOnlyEvents`.
- Nested steppers (a stepper inside a step's description) stop their events at their own wrapper
  (`AddOwnEventStopPropagation`), as for nested tab groups.

Slots: default → `ChildContent` (the `WaStepperStep` children).
Methods: `GoToAsync(string name)`, `NextAsync()`, `PreviousAsync()`.

### `WaStepperStep` (`wa-step`)

| Attribute | Parameter | Notes |
|---|---|---|
| `name` | `string? Name` | `DefaultName = ""` |
| `active` | `bool Active` | set by the parent stepper; documented as needed only for server-side pre-rendering |
| `attention` | `WaAttention? Attention` | reuses `WaAttention` (`none`/`pulse`/`bounce`), default `None` |
| `completed`, `disabled`, `loading` | `bool` | |
| `variant` | `WaVariant? Variant` | default `Brand` |
| `with-description` | `bool WithDescription` | SSR hint |
| `role` | none | global ignore (the element sets `listitem`) |

Slots: default → `ChildContent` (the label), `description` → `DescriptionContent`, `icon` → `IconContent`
plus an `IconName` shortcut.

## Phase 3 — Modified components

### `WaCombobox`: server mode

- `server` → `bool Server`; `loading` → `bool Loading`; `filter-debounce` → `int? FilterDebounce` (ms,
  `DefaultFilterDebounce = 250`).
- `wa-options-request` → `OnOptionsRequest` (`EventCallback<WaOptionsRequestEventArgs>`, `Query`); the
  `AbortSignal` of the detail is not transferable and is dropped by the payload projection.
- **Loading state in event mode.** The element sets `loading` itself when it schedules a request, and
  upstream expects the consumer to clear it after swapping the options. A `Loading` parameter that stays
  `false` never re-renders, so it cannot clear an attribute the element set. The wrapper therefore clears the
  element's `loading` property after the `OnOptionsRequest` handler for the latest query has completed and the
  resulting render has reached the DOM, unless the `Loading` parameter is `true`. A consumer who wants to
  control it explicitly sets `Loading` itself.
- `reload()` → `ReloadAsync()`.
- Slots `empty`, `error`, `loading`, `no-results` → `EmptyContent`, `ErrorContent`, `LoadingContent`,
  `NoResultsContent`.
- `wa-options-error` → `OnOptionsError` (`EventCallback<WaOptionsErrorEventArgs>`: `Error` message, `Query`). Only the
  `dataSource` callback path dispatches it, a JS function property no parameter sets, so it fires only when the
  consumer's own JavaScript assigns one. Bound anyway, following the `WaDataGrid.OnDataError` precedent (same
  `dataSource` rejection, same message projection); the plan first listed it under `ignoredEvents`, which the
  parity check rejects, because that list means "bound by the value handling".
- `request` (CEM-invented) → `cemOnlyEvents`.
- Not wrapped: the JS-only `dataSource` callback property (not a CEM attribute).

### `WaDialog`, `WaDrawer`

- `with-label` → `bool WithLabel` (SSR hint), as on the form controls.

### `WaDivider`

- default slot → `ChildContent` (the label); `label-placement` → `WaDividerLabelPlacement? LabelPlacement`
  (new enum `Start`/`Center`/`End`, default `Center`); `with-label` → `bool WithLabel` (SSR hint; the element
  recomputes it from the slot).

### `WaPage`

- `nonce` → `string? Nonce`.

### `WaZoomableFrame`

- `allow` → `string? Allow` (a Permissions Policy, a grammar rather than a closed token set, so a string);
  `label` → `string? Label` (`DefaultLabel = ""`); `name` → `string? Name`.

## Phase 4 — Intentional deviations (`parity-config.json`)

Each entry with its own `ignoreReasons` key:
- `componentClassOverrides`: `wa-step` → `WaStepperStep` (name clash with the `WaStep` value type).
- `wa-combobox`: `cemOnlyEvents` `request`.
- `wa-stepper`: `cemOnlyEvents` `detail`.
- Whatever the render-based checks report beyond this is resolved in the wrapper or recorded here with its
  reason (see Implementation notes).

## Phase 5 — Tests and docs

- `wa-test-engineer`: `WaStepperIntegrationTests` (slots, child steps, method guards and recorded interop calls,
  `OnStepChange`/`OnBeforeStepChange` payload handling), `WaStepperStepIntegrationTests` (slots, icon shortcut
  precedence), and additions to the combobox, divider, dialog, drawer, page and zoomable-frame tests (new
  slots, `ReloadAsync` guard and interop call, the loading-clear logic). No EditForm coverage: neither new
  component is a form control. No breaking-change validation tests, since nothing is breaking.
- `ApiParity\WaElementDefaults.cs` refreshed from `ElementDefaultsTableTests`' received file.
- `EventCallbackManifestTests` refresh plus an e2e dispatch/payload case for every new callback
  (`OnStepChange`, `OnBeforeStepChange`, `OnOptionsRequest`, `OnOptionsError`; the last by assigning a rejecting `dataSource` from the spec).
- CHANGELOG `## [3.14.0]`: New components, Changed, Library, Public API. No `### Breaking changes` heading.
- Demo: `New-WaDemoPages.ps1 -PruneRemoved`, curated `StepperPage.razor` and `StepPage.razor` (from
  `inputs\WebAwesome\components\stepper.md`/`step.md`), `ComponentCategoryMap` entries (`wa-stepper`,
  `wa-step` → Navigation), a server-mode example on the combobox page and a label example on the divider page;
  the **registration form showcase** gains a stepper that tracks the form's sections, plus flow spec steps.
- Public API snapshot promoted (additions only expected).

## Implementation notes

- **Documentation ingest.** `v3.14.0` is tagged in the public repository, so `Sync-WaDocs.ps1 -Version 3.14.0` ran
  without `-DocsTagVersion`: 155 files, 138 from the tag and 17 Pro/reference docs from the release zip, none needing
  capture (cs:323: 2 added, `components\step.md` and `components\stepper.md`; 22 changed).
- **`OnOptionsError` instead of an `ignoredEvents` entry.** The parity check reads `ignoredEvents` as "bound by the
  value handling" and failed on `wa-options-error`. The event is bound as `OnOptionsError`
  (`WaOptionsErrorEventArgs`: `Error` message, `Query`), with a JS initializer projection like `wa-data-error`'s.
  It fires only when the consumer's own JavaScript assigns a rejecting `dataSource`; the e2e case does exactly that.
- **Loading-state clear.** `WaCombobox` binds `onwa-options-request` to its own handler (plus the own-event stop
  propagation): a request counter, the consumer's `OnOptionsRequest`, then, if the request is still the latest and
  `Loading` is false, a pending flag that `OnAfterRenderAsync` turns into `setProperty(loading, false)` once the
  swapped options are in the DOM. bUnit covers the single clear, the `Loading = true` opt-out and the superseded
  request (all with mutation proofs); the Pro e2e case checks `loading` is back to `false` after the options swap.
- **Demo generator.** `New-WaDemoPages.ps1` built class names from the tag only and emitted `<WaStep>` (the step value
  struct) for `wa-step`. It now honours the parity config's `componentClassOverrides`.
- **Region rule.** The engineer's `WaStepper`/`WaStepperStep` drafts kept the injected interop service and private
  helpers outside an `Internals` region; moved there per the CLAUDE.md hard rule.
- **Registration form showcase.** A "Registration progress" stepper (orientation `Auto`) sits above the fields, with
  the steps Attendee, Sessions, Arrival and Verify. A step is completed when its section's required fields are filled
  and valid; the active step is the first unfinished section. The flow spec asserts the progress at each stage.
- **e2e cases.** `WaStepper` before-change/change from a clicked step (free, `ContentEventsHarness`); `WaCombobox`
  server-mode `OnOptionsRequest` with the option swap and the loading clear, and `OnOptionsError` from a rejecting
  `dataSource` (both Pro, `ProFormEventsHarness`). No exemptions were added.

## Follow-ups

- **Wrapper API gap: cancelling a step change.** Upstream guards a step change with `preventDefault()` on
  `wa-before-step-change`. .NET cannot cancel it, so `OnBeforeStepChange` is notification-only; the demo
  leaves out the upstream "prevent" example. A linear stepper with `Completed` flags is the Blazor way to gate
  progress.
- **Wrapper API gap: `dataSource`.** The combobox's callback data source is a JS function property. Blazor
  uses the event mode (`Server` + `OnOptionsRequest` + swapped `WaOption` children) instead.
- The WAB-46 and WAB-48 follow-ups carry forward unchanged (see "Next-release check items").

## Validation checklist

- [x] `dotnet build src/WebAwesome.slnx -p:Configuration=Debug` and `Release`: 0 warnings, 0 errors
- [x] `dotnet test src/WebAwesome.slnx` green on net9.0 and net10.0: 1264 per TFM, Debug and Release (baseline 1239)
- [x] `ApiSurfaceParityTests` green. At arming the gaps were exactly the two new components, the six modified
      wrappers' new members and the element-defaults oracle (ten new defaults).
- [x] Render-based parity (attributes, slots, events, registrations, enum values, defaults) green for the new
      and changed wrappers
- [x] `PublicApiSnapshotTests` baseline promoted (additions only: 144 lines added, 0 removed)
- [ ] e2e free CDN and Pro (`temp\wa-src\3.14.0`), including the interaction sweep and showcase flows: **not run
      yet**. The release preflight run was stopped by the host for low system memory after its build and test gates
      (all green) and before its e2e gates. It also reported a Pro asset override (`appsettings.Local.json` in both
      demo hosts, written before this run) and a demo instance already running from another session; both were left
      untouched. Rerun `tools\release\Test-WaReleasePreflight.ps1` once memory allows and the override is cleared.
      3.14.0 is published on jsDelivr (loader and stepper module checked), so the free pass needs no local assets.
## Risks

- **Experimental upstream API.** `wa-stepper`/`wa-step` are experimental and may change in a later release.
  The parity harness catches that.
- **Sticky `Active`.** The stepper changes `active` itself. As with `WaTabGroup`, a consumer that re-renders
  the same `Active` value after the user moved on does not move the stepper back; call `GoToAsync` instead,
  or track the value from `OnStepChange`.
- **Server-mode loading clear.** Clearing `loading` after the handler depends on the handler's task
  completing after the options were updated; a handler that fires and forgets its fetch needs `Loading`
  set explicitly. Documented on the parameter.
