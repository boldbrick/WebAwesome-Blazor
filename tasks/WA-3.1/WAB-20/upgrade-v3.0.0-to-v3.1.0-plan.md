# Web Awesome 3.0.0 to 3.1.0 Upgrade Implementation Plan

Task: WAB-20 (`WA bindings for 3.1.0`), epic WAB-19 (`Web Awesome 3.1`).
Source tag: https://github.com/shoelace-style/webawesome/tree/v3.1.0
Branch: `/main/WA-3.1/WAB-20` (new train subtrunk `/main/WA-3.1` created off `/main`; release gate satisfied — 3.0.0 released to `/main` at cs:107).

## Analysis Summary

Driven by the authoritative CEM change report `temp\wa-api\changes_3.0.0_to_3.1.0.json`:

| Metric | Count |
|---|---|
| New components | 1 (`wa-combobox`) |
| Removed components | 0 |
| Modified components | 12 |
| Breaking changes | 11 |

The upgrade is comparatively small: one new Pro form control, one enum-union widening we already cover, one genuinely new element method, and a broad removal of the `form` attribute across the form controls plus two targeted attribute removals.

## Phase 1 — Breaking changes

### 1. `form` attribute removed from form controls (9 components)
Upstream removed the `form` attribute from `wa-button`, `wa-checkbox`, `wa-color-picker`, `wa-input`, `wa-radio`, `wa-select`, `wa-slider`, `wa-switch`, `wa-textarea`.

- `wa-button` (`WaButton.cs`) is the **only** wrapper that actually exposed a typed `Form` parameter (line 119) and rendered the `form` attribute (line 240). **Action:** remove the `Form` [Parameter] property and its render line. This is a public-API removal (reflected in the snapshot baseline and CHANGELOG). The other `formaction`/`formmethod`/`formenctype`/`formnovalidate`/`formtarget`/`name`/`value` button attributes are unchanged and stay.
- The 8 form controls never exposed a `Form` parameter — `form` association is handled by Blazor `EditForm`/`EditContext`, and `form` was silenced via `parity-config.json` `ignoredAttributes: ["form"]` for each. **Action:** no wrapper code change. Clean up the now-obsolete config: remove `"form"` from each component's `ignoredAttributes` and delete the `"form"` entry from `ignoreReasons` (the attribute no longer exists in the CEM, so the ignore is dead config). This is config hygiene, not a functional deviation.

### 2. `wa-button-group` — `variant` attribute removed (BREAKING)
`WaButtonGroup.cs` exposes `Variant` (line 60) and renders it (line 86). **Action:** remove the `Variant` [Parameter] property and its render line. Public-API removal.

### 3. `wa-card` — `appearance` union widened (flagged breaking, no-op for us)
Upstream widened `appearance` from `'accent' | 'filled' | 'outlined' | 'plain'` to add `'filled-outlined'`. Our `WaAppearance` enum **already** defines `OutlinedFilled` mapping to the string `"filled-outlined"` (Enums.cs lines 59, 79, 757). **Action:** none — `WaCard.Appearance` already accepts the new value. Note in CHANGELOG as newly-supported card appearance.

## Phase 2 — New components

### 4. `wa-combobox` → `WaCombobox` (new, Pro, experimental, since 3.1)
A single/multi-select combobox form control — semantically a near-twin of `wa-select`. **Model it on `WaSelect.cs`**: derive from `WaInputBase<string?>`, reuse `AddCommonAttributes`/`AddCommonEventHandlers`/`AddLabelAndHintSlots`, mirror the `Multiple` + `SelectedValues`/`SelectedValuesChanged` pattern, `value`/`onchange` binding via `CurrentValueAsString`, and `SetUpdatesAttributeName("value")`.

CEM surface (19 attributes, 10 events, 7 slots, 4 methods):
- Attributes covered by `WaInputBase`: `disabled`, `name` (NameAttributeValue), `required`, `label`, `hint`, `size`. Combobox-specific parameters to add: `allow-custom-value` (bool `AllowCustomValue`), `appearance` (`WaInputAppearance?` — `'filled' | 'outlined' | 'filled-outlined'`), `autocomplete` (`'list' | 'none'` — new small enum or string; note this collides conceptually with `WaInputBase.Autocomplete` which is the browser autofill string, so expose combobox autocomplete under a distinct name/type — see risks), `max-options-visible` (int `MaxOptionsVisible`), `multiple` (bool), `open` (bool `Open`), `pill` (bool), `placeholder` (string), `placement` (`'top' | 'bottom'` — reuse `WaPlacement?` or a narrow enum), `with-clear` (bool `WithClear`), `with-hint` (bool `WithHint`), `with-label` (bool `WithLabel`), `value`.
- Events: `blur`/`focus`/`input` (via `WaInputBase`), `change` → folded into two-way binding (add `ignoredEvents: ["change"]` + reuse the existing `change` ignore reason), `wa-after-hide`/`wa-after-show`/`wa-clear`/`wa-hide`/`wa-invalid`/`wa-show` → `OnAfterHide`/`OnAfterShow`/`OnClear`/`OnHide`/`OnInvalid`/`OnShow` with the `onwa-` prefix. All six wa-* events are ALREADY registered in `WebAwesome.Blazor.lib.module.js` (no JS change needed).
- Slots: default (options), `clear-icon`, `end`, `expand-icon`, `hint`, `label`, `start` → RenderFragments (`ChildContent`, `ClearIconContent`, `EndContent`, `ExpandIconContent`, plus `MarkupHint`/`MarkupLabel` from base, `StartContent`).
- Methods: `blur`/`focus` (native-ish; add `BlurAsync`/`FocusAsync` like WaSelect), `hide`/`show` → `HideAsync`/`ShowAsync`.

Delegated to a `wa-wrapper-engineer` agent with the `addedComponents.wa-combobox` excerpt and the instruction to mirror `WaSelect`.

## Phase 3 — Modified components (additive)

### 5. `wa-page` — new method `visiblePixelsInViewport(element)` 
`visiblePixelsInViewport(element: HTMLElement | null) => number | null` is an internal scroll-gap layout helper (its own doc comment cites a StackOverflow scroll-gap fix) that takes a live DOM element argument — not a consumer-facing API and not marshalable from Blazor. **Action:** do NOT wrap it; add `wa-page` `ignoredMethods: ["visiblePixelsInViewport"]` with a rationale in `ignoreReasons`. Intentional deviation.

## Element-method allowlist re-verification (Phase 4.6)

Re-verified every `extraElementMethods` entry against the 3.1.0 extracted source (`temp\wa-src\3.1.0\dist\components\*`):
- `wa-mutation-observer` / `wa-resize-observer` `startObserver`/`stopObserver`: still present (private) in `mutation-observer.d.ts` / `resize-observer.d.ts` — allowlist valid, wrappers unchanged.
- `wa-relative-time` `update`: a Lit `ReactiveElement` lifecycle method (inherited via `WebAwesomeElement`, hence absent from the component `.d.ts` — the CEM-invisible category); the element still extends the Lit base in 3.1.0, so `update()` still exists at runtime. Allowlist valid. Runtime confirmed by the Phase 5 Playwright sweep.

## Carried-forward "Next-release check" workarounds (from CHANGELOG `[Unreleased]`)

1. `WaCheckbox`/`WaSwitch` `.checked` two-way-binding workaround (manual `GetPropertyAsync<bool>(Element,"checked")`): Blazor-runtime behavior, not changed by a WA release; the `form` attribute removal is unrelated to `.checked` extraction. **Carry forward unchanged**; the Phase 5 checkbox/switch binding e2e confirms.
2. Removed dead `initialize()` calls on ten wrappers: **carry forward**; Phase 5 Playwright sweep must stay green. No `initialize()` method reintroduced in 3.1.0 sources.
3. Observer `disconnect`/`reconnect` → `stopObserver`/`startObserver` rename: **re-verified against 3.1.0 source (still correct)**; note closure state in the CHANGELOG entry.

## Per-file action list

- `src\WebAwesome.Blazor\Components\WaButton.cs` — remove `Form` parameter + render line.
- `src\WebAwesome.Blazor\Components\WaButtonGroup.cs` — remove `Variant` parameter + render line.
- `src\WebAwesome.Blazor\Components\WaCombobox.cs` — NEW wrapper (via wa-wrapper-engineer).
- `src\WebAwesome.Blazor\Components\WaCard.cs` — no change (already supports `filled-outlined`).
- `src\WebAwesome.Blazor\Components\Enums.cs` — no change (WaAppearance already complete). Possibly a narrow combobox autocomplete/placement enum if the engineer chooses enums over strings.
- `src\WebAwesome.Blazor.Tests\ApiParity\parity-config.json` — remove obsolete `form` ignores + reason; add `wa-combobox` (`ignoredEvents:["change"]`); add `wa-page` `ignoredMethods:["visiblePixelsInViewport"]` + reasons; refresh observer/relative-time reason notes for 3.1.0.
- `src\WebAwesome.Blazor.Tests\ApiParity\expected-api-surface.json` — replaced with the 3.1.0 surface (Phase 3 of pipeline).
- `src\Version.props`, `README.md` — bump 3.0.0 → 3.1.0.
- `src\WebAwesome.Blazor.Demo\wwwroot\index.html` — CDN version 3.1.0; `wwwroot\data\api-surface.json` — 3.1.0 surface.
- Tests: `WaComboboxIntegrationTests.cs` (new), combobox EditForm coverage under `Base\`, breaking-change validation tests for 3.1.0 (WaButton.Form / WaButtonGroup.Variant removed), updates to WaButton/WaButtonGroup tests.
- Docs: `docs\MIGRATION-3.1.0.md` (breaking changes), `docs\CHANGELOG.md` `[3.1.0]` entry, `inputs\WebAwesome` refreshed to 3.1.0 (separate changeset), demo skeleton page for combobox, public API snapshot promotion.

## Validation checklist

- [ ] WaButton `Form` removed; WaButtonGroup `Variant` removed; tests updated.
- [ ] WaCombobox wrapper created; parity `AllComponents/Attributes/Events/Methods` green.
- [ ] parity-config obsolete `form` ignores removed; combobox `change` + page `visiblePixelsInViewport` deviations recorded with reasons.
- [ ] EventBindingRegistrationTests green (no new JS registration needed — verify).
- [ ] ElementMethodInvocationTests green (allowlist re-verified).
- [ ] PublicApiSnapshotTests: diff matches report (Form/Variant removed, WaCombobox added); baseline promoted.
- [ ] New form-control combobox added to EditForm bUnit suite and form showcase.
- [ ] Debug + Release build (incl. demo) and full `dotnet test` green.
- [ ] Playwright sweep green (carried-forward workarounds confirmed).
- [ ] MIGRATION-3.1.0.md + CHANGELOG [3.1.0] written; inputs\WebAwesome refresh checked in.

## Risks

- **Medium:** `wa-button` `Form` removal and `wa-button-group` `Variant` removal are consumer-facing breaking changes — must be in the migration guide and snapshot diff.
- **Medium:** `wa-combobox` `autocomplete` attribute (`'list' | 'none'`) is a combobox behavior, distinct from `WaInputBase.Autocomplete` (the browser autofill string). Exposing both under `Autocomplete` would collide; the wrapper must use a distinct parameter name/type for the combobox autocomplete mode (recorded via `attributeOverrides` if the CEM `autocomplete` maps to a non-`Autocomplete` property). The wrapper engineer must resolve this explicitly.
- **Low:** combobox is `experimental` upstream (since 3.1) — API may churn in later trains; wrapper is best-effort against the 3.1.0 CEM.
- **Low:** `wa-card` `filled-outlined` already supported; no risk, but confirm the demo/api-surface reflects the new union.
