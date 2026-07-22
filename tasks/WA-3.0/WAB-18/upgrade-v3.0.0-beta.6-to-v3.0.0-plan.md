# Web Awesome 3.0.0-beta.6 to 3.0.0 Upgrade Implementation Plan

## Overview
This document is the implementation plan for upgrading the WebAwesome.Blazor wrapper library from `3.0.0-beta.6` to the `3.0.0` stable release. It is derived from the authoritative API change report `temp\wa-api\changes_3.0.0-beta.6_to_3.0.0.json` (baseline: the checked-in `expected-api-surface.json`, whose `version` field equals the current `3.0.0-beta.6`).

## Analysis Summary

| Metric | Count |
|---|---|
| New components | 1 (`wa-page`) |
| Removed components | 0 |
| Modified components | 12 |
| Breaking changes | 10 |

The change report flags 10 "breaking" items, but for the Blazor wrappers most reduce to a single shared token fix plus one event rename — the API-surface parity tests key on member *presence* (parameter / event-callback / method names), not on enum token strings, so several report entries are functional correctness fixes rather than parity gaps.

### Tooling-noise entries (no action)
`wa-dialog` and `wa-drawer` show `cssParts` "changes" named `Count`, `Length`, `LongLength`, `Rank`, `SyncRoot`, `IsReadOnly`, `IsFixedSize`, `IsSynchronized`. These are .NET `System.Array` member names leaking from the surface export/compare when a `cssParts` collection serializes as an array; they are not real Web Awesome CSS parts and are **not** implemented. `cssParts` are not covered by the parity tests. No wrapper or config change is warranted; recorded here so the diff is explained.

## Breaking Changes to Address (plan Phase 1)

### 1. `appearance` token rename: "outlined filled" → "filled-outlined" (BREAKING)
**Affected components**: `wa-badge`, `wa-button`, `wa-callout`, `wa-details`, `wa-input`, `wa-select`, `wa-tag`, `wa-textarea`.
**Root cause**: Web Awesome 3.0.0 renamed the combined appearance token from the space-separated `outlined filled` to the hyphenated `filled-outlined`, and added it to the `appearance` union of several components that previously lacked it (`badge`, `button`, `input`, `details`, `select`, `textarea`); for `callout` and `tag` it is a rename of the existing `outlined filled` value.

**File**: `src/WebAwesome.Blazor/Components/Enums.cs`
- `WaAppearanceExt.ToStringForHtml` — change the `WaAppearance.OutlinedFilled` mapping from `"outlined filled"` to `"filled-outlined"`.
- `WaEnumExtensions.ToHtmlValue(this WaAppearance)` — change the `WaAppearance.OutlinedFilled` case from `"outlined filled"` to `"filled-outlined"`.
- `WaInputAppearance` (used by `wa-input`, `wa-select`, `wa-textarea`) — add a new value `FilledOutlined`, and add its `ToHtmlValue` case → `"filled-outlined"`. This is additive.

`WaAppearance` (used by badge/button/callout/details/tag) already declares `OutlinedFilled`; only its emitted token changes. The default-skip logic in `WaCallout` and `WaTag` (`if (Appearance != WaAppearance.OutlinedFilled)`) remains correct — only the emitted string changes, not the default.

### 2. `wa-include` event rename: `wa-error` → `wa-include-error` (BREAKING)
**File**: `src/WebAwesome.Blazor/Components/WaInclude.cs`
- Rename the `OnError` `EventCallback<IncludeErrorEventArgs>` parameter to `OnIncludeError` (parity expects the callback `OnIncludeError` for event `wa-include-error`).
- Update `BuildRenderTree` to emit `wa-include-error` instead of `wa-error`.
- Keep the `IncludeErrorEventArgs` payload type (upstream payload is `{ status: number }`).

### 3. `wa-icon` `auto-width` type widened `false` → `boolean` (BREAKING in report, no-op for wrapper)
**File**: `src/WebAwesome.Blazor/Components/WaIcon.cs` — the wrapper already models `auto-width` as `bool AutoWidth` and emits it correctly. No change required; verified during implementation.

## New Components to Add (plan Phase 2)

### 4. `wa-page` (WaPage) — Pro component, new in 3.0.0
**File**: `src/WebAwesome.Blazor/Components/WaPage.cs` (new)
Delegated to a `wa-wrapper-engineer` agent with the `addedComponents.wa-page` excerpt.

- **Attributes → parameters**:
  - `disable-navigation-toggle` → `DisableNavigationToggle` (bool, default false)
  - `mobile-breakpoint` → `MobileBreakpoint` (string, default "768px")
  - `navigation-placement` → `NavigationPlacement` (enum `'start' | 'end'`)
  - `nav-open` → `NavOpen` (bool, default false)
  - `view` → `View` (enum `'mobile' | 'desktop'`)
- **Methods** (JS-interop, `Async` suffix): `hideNavigation` → `HideNavigationAsync`, `showNavigation` → `ShowNavigationAsync`, `toggleNavigation` → `ToggleNavigationAsync`.
- **Events**: none.
- **Slots**: 15 slots (default + `aside`, `banner`, `footer`, `header`, `main-footer`, `main-header`, `menu`, `navigation`, `navigation-footer`, `navigation-header`, `navigation-toggle`, `navigation-toggle-icon`, `skip-to-content`, `subheader`) → `ChildContent` plus one `RenderFragment?` per named slot, each rendered into a wrapper with the appropriate `slot="..."` attribute. Slots are not parity-checked but are part of a faithful wrapper.
- New enums as needed for `navigation-placement` and `view` (follow the existing enum + `ToHtmlValue` extension pattern). `NavigationPlacement` may reuse `start`/`end` semantics; add a dedicated `WaPageNavigationPlacement` and `WaPageView` if no existing enum matches exactly.
- Reference doc: `inputs\WebAwesome\components\page.md` (ingested from the public web docs; Pro component).

## Modified Components (plan Phase 2, additive) 
No additive attribute/event/method gaps beyond the breaking items above. The `appearance` union widenings are handled by the enum work in Phase 1. No `modifiedComponents` entry adds a new parameter/event/method that the parity tests would flag (beyond `wa-include-error`, handled as a rename in item 2).

## Intentional Deviations (parity-config.json)
None anticipated. If, after implementation, a residual parity gap remains that reflects a deliberate wrapper choice, record it in `parity-config.json` under the component's `ignoredAttributes`/`ignoredEvents`/`ignoredMethods` (or an override map) **with** a matching `ignoreReasons` entry. The existing `wa-details` event overrides and the global `title` ignore remain unchanged.

## Testing Updates (plan Phase 3)
Delegated to `wa-test-engineer`:
- New `WaPageIntegrationTests.cs` following the existing `Wa*IntegrationTests.cs` pattern (attributes render, methods invoke via interop, slots project).
- Breaking-change validation tests for 3.0.0: `appearance` now emits `filled-outlined` (both `WaAppearance.OutlinedFilled` and `WaInputAppearance.FilledOutlined`); `wa-include` emits `wa-include-error`.
- Repair any existing tests that asserted the old `outlined filled` token or the `wa-error`/`OnError` include event.

## Implementation Priority

### Phase 1: Breaking changes (Critical)
1. `Enums.cs`: appearance token → `filled-outlined`; add `WaInputAppearance.FilledOutlined`.
2. `WaInclude.cs`: rename `OnError` → `OnIncludeError`, emit `wa-include-error`.
3. Verify `WaIcon.AutoWidth` (no change expected).

### Phase 2: New features
4. `WaPage.cs` new wrapper (+ enums) via wa-wrapper-engineer.

### Phase 3: Testing and validation
5. Author/repair tests, iterate `dotnet build` + `dotnet test` until the armed `ApiSurfaceParityTests` and the whole suite are green.

## Validation Checklist
- [ ] `expected-api-surface.json` replaced with `surface_3.0.0.json`; `parity-config.json` `targetWaVersion=3.0.0`, `enabled=true`.
- [ ] `Version.props` bumped to `3.0.0` (Version, AssemblyVersion/FileVersion 3.0.0.0); `README.md` version references updated.
- [ ] Demo `index.html` CDN version = 3.0.0; demo `wwwroot/data/api-surface.json` = target surface.
- [ ] appearance token fix applied; `WaInputAppearance.FilledOutlined` added.
- [ ] `wa-include` event renamed.
- [ ] `WaPage` wrapper created with all attributes, 3 methods, 15 slots.
- [ ] `ApiSurfaceParityTests` green; full `dotnet test` green on net9.0 and net10.0.
- [ ] `dotnet build` Debug and Release (incl. demo) green.
- [ ] Migration doc `docs\MIGRATION-3.0.0.md`, `CHANGELOG.md` entry, demo pages regenerated.

## Risk Assessment
- **Medium**: `appearance` token rename — any consumer relying on the emitted `outlined filled` string or CSS keyed on it must migrate to `filled-outlined`. Covered by the migration guide.
- **Medium**: `wa-include` `OnError` → `OnIncludeError` rename — a source-breaking parameter rename for consumers.
- **Low**: `wa-page` new component (purely additive); `wa-icon` auto-width (no wrapper change).

## Notes for Implementation
- Follow `CLAUDE.md` code style (file-scoped namespace, regions, no underscore fields, doc comments on public members, no magic constants).
- Reuse existing enum + `ToHtmlValue` extension patterns for new `wa-page` enums.
- Pro-source rule: all extracted Pro sources stay under `temp\`; only the API surface JSON and API-describing docs are checked in.
