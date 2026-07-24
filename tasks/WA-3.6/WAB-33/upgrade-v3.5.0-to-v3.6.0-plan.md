# Web Awesome 3.5.0 to 3.6.0 Upgrade Implementation Plan

## Overview
Upgrade the WebAwesome.Blazor bindings from Web Awesome **3.5.0** to **3.6.0**.
Source tag: https://github.com/shoelace-style/webawesome/tree/v3.6.0
JIRA: WAB-33 (epic WAB-32 "Web Awesome 3.6"). Branch: `/main/WA-3.6/WAB-33` (new WA-3.6 train, subtrunk `/main/WA-3.6` created off `/main`, previous train WA-3.5 already released at cs:170/178).

## Analysis Summary
Authoritative worklist: `temp\wa-api\changes_3.5.0_to_3.6.0.json` (CEM surface diff, 3.5.0 → 3.6.0).

| Metric | Count |
|---|---|
| New components | 0 |
| Removed components | 0 |
| Modified components | 30 |
| Breaking changes | 19 |

This is a focused upgrade with **no added or removed components**. Three real code changes, plus a large set of metadata-only status flips:

1. **Size scale overhaul** (18 components, flagged breaking): the `size` attribute's type union widened from `'small' | 'medium' | 'large'` to `'xs' | 's' | 'm' | 'l' | 'xl' | 'small' | 'medium' | 'large'`, and the upstream default renamed `'medium'` → `'m'`. The legacy `small|medium|large` values remain valid aliases, so this is **additive** for wrapper consumers; the default rename is behavior-preserving (same rendered size).
2. **`wa-file-input` slot `file-icon` removed** (breaking): the wrapper's `FileIconContent` render fragment now targets a non-existent slot.
3. **`wa-number-input` new event `beforeinput`** (additive): a cancelable custom event (`bubbles/cancelable/composed`, no detail) emitted before the value changes.
4. **13 components experimental → stable** (metadata only): bar/bubble/doughnut/line/pie/polar-area/radar/scatter-chart, `wa-chart`, `wa-sparkline`, `wa-toast`, `wa-toast-item`, `wa-combobox`, `wa-dropdown-item`, `wa-file-input`, `wa-number-input`. Demo badges are data-driven from `data\api-surface.json` via `ComponentBadges`/`ApiSurfaceService`, so these flip automatically when the target surface is copied in Phase 3 — no wrapper or page edits.

### Parity-harness impact
`ApiSurfaceParityTests` checks attribute/event/method/class **existence** only (not enum unions or defaults), so the size-union change does **not** by itself fail parity — `Size` already exists on every affected wrapper. The only parity-forcing change is the new `beforeinput` event on `wa-number-input` (requires a matching `EventCallback`). The removed `file-icon` slot is not parity-checked (slots have no parity test) but must still be removed for correctness (dead markup) and is a public-API change.

## Phase 1 — Breaking changes

### 1. `WaFileInput` — remove `file-icon` slot
**File**: `src\WebAwesome.Blazor\Components\WaFileInput.cs`
- Remove the `FileIconContent` `[Parameter]` (approx. lines 140–143).
- Remove its render block that emits `slot="file-icon"` (approx. lines 199–206).
- Update tests referencing `FileIconContent`; remove any `file-icon` example from the demo page.
- Public-API impact: one removed member (explained by report: `wa-file-input - slots removed: file-icon`).

## Phase 2 — New / additive members

### 2. `WaSize` — expose the widened size scale
**File**: `src\WebAwesome.Blazor\Components\Enums.cs`
- Add `ExtraSmall` and `ExtraLarge` enum members (append after `Large` to preserve ordinal values of existing members).
- Add `ToHtmlValue()` cases: `ExtraSmall => "xs"`, `ExtraLarge => "xl"`.
- Keep `Small/Medium/Large` mapping to `"small"/"medium"/"large"` (still valid aliases in 3.6.0) — no rename, no behavior change for existing consumers, no demo/test churn.
- Applies to every wrapper that maps `Size` through `WaSize` (WaButton, WaCallout, WaTag, WaDropdown, WaRadio, WaToastItem, WaTextarea, and all `WaInputBase<T>` derivatives) automatically.

### 3. `WaNumberInput` — bind the `beforeinput` event
**File**: `src\WebAwesome.Blazor\Components\WaNumberInput.cs`
- Add `[Parameter] public EventCallback<EventArgs> OnBeforeInput { get; set; }` (natural PascalCase per wrapper API conventions; mapped in parity-config via `eventOverrides`).
- Bind it: `builder.AddAttributeIfHasDelegate(seq, "onbeforeinput", OnBeforeInput);` in `BuildRenderTree`.

**File**: `src\WebAwesome.Blazor\wwwroot\WebAwesome.Blazor.lib.module.js`
- Register the `beforeinput` custom event type so Blazor delivers it (it is a re-dispatched custom event, not a Blazor built-in). Add via a clearly labeled non-`wa-` registration (empty-detail → `detailArgs` default yields `{}`, deserialized into `EventArgs`). The `EventBindingRegistrationTests` regexes only inspect `wa-*` bindings/registrations, so a native-named event neither triggers nor conflicts with them.

## Phase 3 — Intentional deviations & allowlists (parity-config.json)
**File**: `src\WebAwesome.Blazor.Tests\ApiParity\parity-config.json`
- `targetWaVersion` → `3.6.0` (Phase 3 of pipeline).
- `wa-number-input`: add `eventOverrides` `{ "beforeinput": "OnBeforeInput" }`; add matching `ignoreReasons` entry documenting the natural-PascalCase rename.
- **Re-verify `extraElementMethods` against 3.6.0 source** (the CEM diff cannot see these):
  - `wa-mutation-observer` / `wa-resize-observer`: `stopObserver` / `startObserver` — confirm still present in `mutation-observer.d.ts` / `resize-observer.d.ts`.
  - `wa-relative-time`: `update` (inherited Lit `ReactiveElement` lifecycle) — confirm.
  - Append `3.6.0 (2026-07-24)` to the two `extraElementMethods:*` `ignoreReasons` verification stamps.

## Phase 4 — Version, surface, demo sync
- `src\Version.props`: `Version`/`AssemblyVersion`/`FileVersion` → `3.6.0`.
- `README.md`: version references → `3.6.0`.
- `src\WebAwesome.Blazor.Tests\ApiParity\expected-api-surface.json` ← `temp\wa-api\surface_3.6.0.json`.
- `src\WebAwesome.Blazor.Demo\wwwroot\index.html`: WA CDN version → `3.6.0`.
- `src\WebAwesome.Blazor.Demo\wwwroot\data\api-surface.json` ← `temp\wa-api\surface_3.6.0.json` (drives nav, API tables, and experimental→stable badges).

## Phase 5 — Tests & docs
- **Tests** (wa-test-engineer):
  - `WaFileInput`: remove `FileIconContent` assertions.
  - `WaNumberInput`: add coverage for `OnBeforeInput` binding (bUnit `onbeforeinput` trigger); existing EditForm/number-input coverage stays (not a new form control).
  - `WaSize`: add `ToHtmlValue` cases for `ExtraSmall`→`xs`, `ExtraLarge`→`xl`.
- **MIGRATION-3.6.0.md**: breaking = `WaFileInput.FileIconContent` removed (`file-icon` slot gone upstream); note the size scale expansion (`ExtraSmall`/`ExtraLarge`) and the behavior-preserving upstream default rename (`medium`→`m`).
- **CHANGELOG.md**: `## [3.6.0]` entry — Breaking (file-icon slot), New (WaSize xs/xl, WaNumberInput.OnBeforeInput), Changed (13 components stabilized), Library.
- **Demo**: run `New-WaDemoPages.ps1 -PruneRemoved` (no adds/removes, effectively a no-op sweep); remove any `file-icon` example from `FileInputPage.razor`; no new pages to curate; showcases unaffected (no added/removed components).
- **PublicApiSnapshot**: promote `received-public-api.txt` after confirming the diff = `+WaSize.ExtraSmall`, `+WaSize.ExtraLarge`, `+WaNumberInput.OnBeforeInput`, `-WaFileInput.FileIconContent`.

## Validation Checklist
- [ ] `WaFileInput.FileIconContent` and its render block removed
- [ ] `WaSize.ExtraSmall`/`ExtraLarge` added with `xs`/`xl` mappings
- [ ] `WaNumberInput.OnBeforeInput` bound and `beforeinput` registered in the JS initializer
- [ ] parity-config: target 3.6.0, beforeinput override + reason, extraElementMethods re-verified & re-stamped
- [ ] Version.props / README / expected-api-surface / demo index.html / demo api-surface.json updated
- [ ] `dotnet build` Debug + Release green; `dotnet test` green (parity + all suites)
- [ ] MIGRATION-3.6.0.md + CHANGELOG entry written
- [ ] Public API baseline promoted (diff matches report)
- [ ] Playwright e2e sweep green

## Risks
- **Low overall.** No new/removed components.
- `beforeinput` delivery depends on the JS `registerCustomEventType` registration (silent failure mode if missed) — covered by e2e and bUnit.
- Size enum extension is purely additive; keeping legacy HTML values avoids snapshot/demo churn.

## Notes for Implementation
- Prefer CEM + `.d.ts` over markdown for semantics. `beforeinput` confirmed re-dispatched as a custom event in `temp\wa-src\3.6.0\dist\chunks\chunk.2J6RA3WZ.js` (`bubbles/cancelable/composed`, no detail).
- Pro-source stays under `temp\` — only the API surface JSON and bundled reference docs are checked in.
