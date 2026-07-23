# Web Awesome 3.3.1 to 3.4.0 Upgrade Implementation Plan

## Overview
Implementation plan for upgrading the WebAwesome.Blazor wrapper library from Web Awesome 3.3.1 to 3.4.0. This starts a **new release train, WA-3.4**, developed on the new subtrunk `/main/WA-3.4` (branched off `/main` head cs:154, which carries the released 3.3.1 — release gate satisfied), task branch `/main/WA-3.4/WAB-28`.

## Analysis Summary
The authoritative worklist is the CEM-derived API surface diff
`temp\wa-api\changes_3.3.1_to_3.4.0.json` / `.md`, produced from
`surface_3.3.1.json` vs `surface_3.4.0.json` (73 components in both):

| Metric | Count |
|---|---|
| New components | 0 |
| Removed components | 0 |
| Modified components | 5 |
| Breaking changes | 4 |

The parity harness (`ApiSurfaceParityTests`) checks member **presence** (attribute → `[Parameter]`, event → `EventCallback`, method → `Async` wrapper), not member **types**. Two of the four "breaking" entries are type-only widenings that leave the wrapper's parameter present and functional, so they require **no wrapper code change**; the other two are removals of attributes the wrappers only ever inherited from `WaInputBase<TValue>` (shared base parameters), so they also require no per-component change. The real work is the small additive set on `wa-combobox` and `wa-zoomable-frame`.

## Breaking Changes to Address (from `breakingChanges`)

1. **`wa-color-picker` — `swatches` type widened** `string | string[]` → `string | string[] | WaColorPickerSwatch[]`.
   - The attribute (string; semicolon-separated) is unchanged; the addition is a JS-only array-of-`{ color, label }` form settable as a property. `WaColorPicker.Swatches` (string) stays; `SetSwatchesAsync(string[])` already exists for the array form. **No wrapper change.** The `{ color, label }` object form remains a JS-only capability (not marshaled from Blazor); noted in CHANGELOG.

2. **`wa-combobox` — `autocomplete` attribute removed.**
   - The wrapper never exposed a combobox-specific `Autocomplete`; it only inherits `WaInputBase.Autocomplete` (shared by `wa-input`/`wa-select`/etc., where `autocomplete` is still valid). Removing it from the combobox CEM removes one parity obligation; the inherited parameter stays (still compiles for consumers) and renders only when explicitly set (`AddAttributeIfNotNullOrEmpty`), now an inert passthrough on combobox. **No wrapper change; not breaking for wrapper consumers.**

3. **`wa-input` — `autocorrect` type changed** `'off' | 'on'` → `boolean`.
   - Per the upstream doc, the **attribute** form is still `"off"`/`"on"` (the `boolean` applies to the JS property). `WaInput.AutoCorrect` (`string?`, rendered as the attribute) therefore stays correct and more expressive than a bool for the attribute path. **No wrapper change** (mapped via existing `attributeOverrides` `autocorrect`→`AutoCorrect`).

4. **`wa-slider` — `required` attribute removed.**
   - The wrapper never exposed a slider-specific `Required`; it only inherits `WaInputBase.Required` (shared by all form controls, still valid elsewhere). Removal drops one parity obligation; the inherited parameter stays and renders an inert `required` on the slider only. **No wrapper change; not breaking for wrapper consumers.**

**Net: no migration guide required** — no wrapper public API is removed or renamed; every affected parameter remains present and source-compatible.

## New Components to Add
None.

## Component Enhancements (additive — delegated to wa-wrapper-engineer)

### `WaCombobox` (`wa-combobox`)
New parameters (mirroring the equivalent `WaInput` text-input parameters for consistency):
- `AllowCreate` (`bool`) → `allow-create`
- `AutoCapitalize` (`string?`) → `autocapitalize`
- `AutoCorrect` (`string?`) → `autocorrect` (attribute form `"off"`/`"on"`, consistent with `WaInput.AutoCorrect`)
- `EnterKeyHint` (`string?`) → `enterkeyhint`
- `InputMode` (`string?`) → `inputmode`
- `Spellcheck` (`bool?`) → `spellcheck`

New event:
- `OnCreate` (`EventCallback<WaCreateEventArgs>`) → `wa-create`; detail `{ inputValue: string }` → `WaCreateEventArgs.InputValue` (`string`). JSON-safe string detail, so the default `detailArgs` payload applies — register `wa-create` in `WebAwesome.Blazor.lib.module.js` `eventNames` (no `specialArgs` entry needed).

### `WaZoomableFrame` (`wa-zoomable-frame`)
- `WithThemeSync` (`bool`) → `with-theme-sync` (enables light/dark + theme-selector class sync from host into the iframe).

## Intentional Deviations (parity-config.json)
- `wa-combobox` `attributeOverrides`: add `autocapitalize`→`AutoCapitalize`, `autocorrect`→`AutoCorrect`, `enterkeyhint`→`EnterKeyHint`, `inputmode`→`InputMode` (idiomatic .NET casing; identical pattern to the existing `wa-input` overrides). `allow-create`→`AllowCreate` and `spellcheck`→`Spellcheck` need no override (mechanical PascalCase already matches).
- No new `ignoredAttributes` needed for the removed `autocomplete`/`required` (removed from CEM → no longer enumerated by the parity test).
- Existing deviations carry forward unchanged.

## Element Method Audit (hard rule — CEM diff cannot see these)
Re-verify every `extraElementMethods` allowlist entry against the extracted 3.4.0 source (`temp\wa-src\3.4.0\dist\components\*`):
- `wa-mutation-observer` → `startObserver`, `stopObserver`
- `wa-resize-observer` → `startObserver`, `stopObserver`
- `wa-relative-time` → `update`

Action: confirm presence in the 3.4.0 `.d.ts`/source and append the 3.4.0 verification date to the two covering `ignoreReasons` entries.

## Per-file Actions
- `inputs\WebAwesome\**` — refreshed to the `v3.4.0` tag (35 changed public-docs files re-downloaded; Pro `combobox.md` header re-stamped, content already current). Checked in as its own changeset.
- `src\WebAwesome.Blazor.Tests\ApiParity\expected-api-surface.json` — overwrite with `surface_3.4.0.json`.
- `src\WebAwesome.Blazor.Tests\ApiParity\parity-config.json` — `targetWaVersion` → `3.4.0`; add the four `wa-combobox` `attributeOverrides`; append the 3.4.0 element-method re-verification note to the two `extraElementMethods` `ignoreReasons`.
- `src\WebAwesome.Blazor\Components\WaCombobox.cs` — add the six parameters + `OnCreate`.
- `src\WebAwesome.Blazor\Components\WaZoomableFrame.cs` — add `WithThemeSync`.
- `src\WebAwesome.Blazor\Components\EventArgs.cs` — add `WaCreateEventArgs`.
- `src\WebAwesome.Blazor\wwwroot\WebAwesome.Blazor.lib.module.js` — register `wa-create`.
- `src\Version.props` — `Version` → `3.4.0`; `AssemblyVersion`/`FileVersion` → `3.4.0.0` (minor changed).
- `README.md` — active train → WA 3.4, alignment → 3.4.0, CDN references, "per WA 3.4.0" wording.
- `src\WebAwesome.Blazor.Demo\wwwroot\index.html` — no change (WA asset version tracks the library version structurally; no hard-coded version present).
- `src\WebAwesome.Blazor.Demo\wwwroot\data\api-surface.json` — overwrite with `surface_3.4.0.json`.
- `docs\CHANGELOG.md` — add `## [3.4.0]` section; fold any `## [Unreleased]` items in.
- `src\WebAwesome.Blazor.Tests\PublicApi\approved-public-api.txt` — promote the additive diff (new combobox/zoomable-frame parameters, `OnCreate`, `WaCreateEventArgs`); confirm every line is explained by the change report.
- No migration doc (no destructive/renaming breaking changes).

## Tests (delegated to wa-test-engineer)
- `WaComboboxIntegrationTests` — cover the new parameters and `OnCreate`/`WaCreateEventArgs` rendering + event delivery.
- `WaZoomableFrameIntegrationTests` — cover `WithThemeSync` rendering.
- Both `WaCombobox` and `WaSlider` derive from `WaInputBase<T>`; existing EditForm coverage stands (no new form control added, so no new EditForm suite required, but confirm combobox EditForm coverage still passes).

## Validation Checklist
- [ ] Element-method allowlist re-verified against 3.4.0 source
- [ ] Parity harness re-armed to 3.4.0; `ApiSurfaceParityTests` green
- [ ] `WaCombobox` + `WaZoomableFrame` additive parameters/events implemented
- [ ] `wa-create` registered in the JS initializer; `EventBindingRegistrationTests` green
- [ ] parity deviations recorded with reasons
- [ ] Version bumped (Version.props, README)
- [ ] Demo api-surface.json refreshed; demo builds
- [ ] `dotnet build` Debug and Release green (incl. demo)
- [ ] `dotnet test` green on net9.0 + net10.0
- [ ] Public API snapshot diff explained + promoted
- [ ] CHANGELOG entry added
- [ ] Browser e2e sweep green against WA 3.4.0 (jsdelivr 200 confirmed earlier today → default free CDN)

## Risk Assessment
- **Low.** Additive surface on two components plus one new event; no destructive breaking change. The largest risk is the new-train VCS choreography (subtrunk off `/main`, epic-folder-first, task branch), which is handled per the train rule. Browser e2e is the meaningful runtime check; WA 3.4.0 was probed at HTTP 200 on jsdelivr earlier today, so the sweep runs against the default free CDN (no local-asset override needed).

## Notes for Implementation
- All shell commands use PowerShell (hard rule); this rule is passed to every delegated agent.
- Pro sources remain under `temp\`; only the API surface JSON is checked in.
- `wa-combobox` `AutoCorrect` is kept as `string?` (attribute form `"off"`/`"on"`) for symmetry with `WaInput.AutoCorrect`, even though the CEM types the JS property as `boolean`.
