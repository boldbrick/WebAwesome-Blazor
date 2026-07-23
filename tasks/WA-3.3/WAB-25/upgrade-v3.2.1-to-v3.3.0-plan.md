# Web Awesome 3.2.1 → 3.3.0 Upgrade Implementation Plan

## Overview

Upgrade the WebAwesome.Blazor bindings from Web Awesome **3.2.1** to **3.3.0**.
Ticket: **WAB-25** (epic **WAB-24** "Web Awesome 3.3"). Train: **WA-3.3** (new subtrunk `/main/WA-3.3`, branched off `/main` at cs 140; release gate satisfied — `/main` head Version.props = 3.2.1, matching the fully-promoted WA-3.2 subtrunk head, so no unreleased version work blocks the new train).

Source tag: https://github.com/shoelace-style/webawesome/tree/v3.3.0
Authoritative worklist: `temp\wa-api\changes_3.2.1_to_3.3.0.json` (CEM diff). The upgrade is code-complete when `ApiSurfaceParityTests` pass against `expected-api-surface.json` armed to 3.3.0.

## Change report summary

| Metric | Count |
|---|---|
| New components | 11 |
| Removed components | 0 |
| Modified components | 62 |
| Breaking changes (reported) | 8 |

### Reading of the 8 "breaking changes" — none are destructive

- **`name` attribute "changed" on wa-button, wa-checkbox, wa-combobox, wa-select, wa-slider** — the only delta is the TypeScript annotation `string` → `string | null` (defaults unchanged or `'' `→ `null`). No semantic break. `name` is already exposed (`WaButton.Name`) or ignored via form-control InputBase integration for the others. **No wrapper change required.**
- **wa-carousel-item / wa-spinner "attributes removed: "** and **wa-radio "methods removed: "** — the removed arrays contain a single `null` entry (a CEM-export artifact), not a real removal. Verified against the 3.3.0 surface: no attribute/method actually disappeared. **No action; tooling noise.**

Conclusion: this upgrade has **no destructive changes**. Phase 4 step 1 (breaking changes) is a no-op beyond recording the nullable-`name` annotations.

## Ubiquitous additive changes (all 73 components)

Every component gained three attributes from an upstream base-class change:

- **`dir`, `lang`** — native HTML global attributes → pass through via `AdditionalAttributes`. Add to `globalIgnoredAttributes` (same treatment as the existing `title`).
- **`did-ssr`** — internal SSR hydration marker (type `null`, not consumer-authorable) → add to `globalIgnoredAttributes`.

These are handled once in `parity-config.json`; no per-component code.

## Form-control additive changes (13 controls)

Controls: wa-button, wa-checkbox, wa-color-picker, wa-combobox, wa-file-input, wa-input, wa-number-input, wa-radio, wa-radio-group, wa-select, wa-slider, wa-switch, wa-textarea. Each gained a form-associated custom-element (FACE) validation surface:

- **`custom-error` attribute** (`string | null`) — the manually-set custom validity message, reflected. Managed imperatively through `IFormValidation.SetCustomValidityAsync` / new `ResetValidityAsync`, not an authorable parameter → **ignore per control** in `parity-config.json` with a shared reason.
- **`setCustomValidity` method** — already exposed everywhere via `IFormValidation.SetCustomValidityAsync`. Newly CEM-documented (previously invisible to the diff); **no code change**, parity now owns it.
- **`resetValidity` method** — new. Expose as `ResetValidityAsync` (element method via interop). Add to `IFormValidation` + implement once in `WaInputBase<TValue>` (covers 9 controls) and individually in the 4 non-`WaInputBase` controls: `WaButton`, `WaFileInput`, `WaRadio`, `WaTextArea`.
- **`formStateRestoreCallback` method** — a FACE lifecycle callback the browser invokes; not consumer-facing and not marshalable → **ignore per control** with a reason.
- **`name` / `disabled` newly listed on wa-file-input, wa-input, wa-number-input, wa-radio** — `disabled` is already inherited (`WaInputBase.Disabled`, and present on the standalone controls); `name` is form-integration-managed → **ignore per control** (reason already in `ignoreReasons`).

## Other modified-component code changes

Exactly one, confirmed by full scan of all 62 modified entries:

- **wa-badge** gained **`start`** and **`end`** slots, both documented "An element, such as `<wa-icon>`" → add `StartContent`/`EndContent` `RenderFragment` + `StartIconName`/`EndIconName` per the icon-slot convention (pattern: `WaButton`). (The `cssParts` deltas reported for wa-badge — `Length`, `Rank`, `SyncRoot`, `Count`, … — are `System.Array` reflection artifacts in the export, not real CSS parts; ignore.)

## New components (11) — `pro: false`, `status: experimental`, `since 3.3`

### Chart family (9) — delegate to wa-wrapper-engineer group A

`wa-chart` (WaChart), `wa-bar-chart` (WaBarChart), `wa-bubble-chart` (WaBubbleChart), `wa-doughnut-chart` (WaDoughnutChart), `wa-line-chart` (WaLineChart), `wa-pie-chart` (WaPieChart), `wa-polar-area-chart` (WaPolarAreaChart), `wa-radar-chart` (WaRadarChart), `wa-scatter-chart` (WaScatterChart).

All nine share an identical attribute set (differing only in the `type` default). Implement an **abstract `WaChartBase`** holding the shared `[Parameter]`s + a shared `BuildRenderTree` over a `protected abstract string TagName`; nine thin concrete classes each emit their own tag. `WaBarChart` additionally has `orientation` (`'vertical' | 'horizontal'`).

Shared attributes → parameters (default slot = `ChildContent`, the JSON `<script>` config block):

- `description` → `Description` (string?), `label` → `Label` (string?), `xLabel` → `XLabel` (string?), `yLabel` → `YLabel` (string?)
- `type` → `Type` (enum **WaChartType**: bar, line, pie, doughnut, polarArea, radar, scatter, bubble; `ToHtmlValue` must emit `polarArea` camelCase)
- `grid` → `Grid` (enum **WaChartGrid**: x, y, both, none)
- `indexAxis` → `IndexAxis` (enum **WaChartAxis**: x, y)
- `legendPosition` → `LegendPosition` (enum **WaChartLegendPosition**: top, left, bottom, right, start, end)
- `min` → `Min` (double?), `max` → `Max` (double?) — value-axis bounds, fractional meaningful → `double?`
- `stacked`, `withoutAnimation`, `withoutLegend`, `withoutTooltip` → bool parameters `Stacked`, `WithoutAnimation`, `WithoutLegend`, `WithoutTooltip`
- `orientation` (bar only) → `Orientation` (reuse `WaOrientation` enum if present with matching values, else new enum)
- **`plugins`** — `never[]` JS-only property (confirmed in `chart.d.ts`), not an authorable HTML attribute → **ignore per chart** in `parity-config.json` with a reason. `config` is not in the CEM attribute set (JS property set via the JSON slot) → no parameter.

**Semantics-to-verify (recorded per failure-handling rule):** `xLabel`/`yLabel` appear in the CEM with camelCase attribute keys (no kebab split). Render them with the attribute name exactly as the CEM lists them; the parameter names are `XLabel`/`YLabel` (`ToPascalCase` of a dashless token). Chart rendering is browser-verified only on skeleton demo pages, so any attribute-casing nuance surfaces there, not in a consumer-blocking path.

No events, no methods, no wrapper-relevant slots beyond the default config slot.

### Toast family (2) — delegate to wa-wrapper-engineer group B (with wa-badge)

- **`wa-toast` (WaToast)** — attribute `placement` → `Placement` (enum: top-start, top-center, top-end, bottom-start, bottom-center, bottom-end; new enum **WaToastPlacement**). Default slot = `ChildContent` (holds `<wa-toast-item>`). Method `create(message, options?)` → `CreateAsync(string message, …)` via interop (element method; returns the created item — wrapper may return `Task`; **semantics-to-verify**: marshaling the returned element is not required for the wrapper, return `Task`).
- **`wa-toast-item` (WaToastItem)** — attributes `duration` (ms → `int?` `Duration`), `size` (reuse `WaSize`), `variant` (values brand/success/warning/danger/neutral — reuse `WaVariant` if it matches, else new enum). Slots: default = `ChildContent`, `icon` = `IconContent` + `IconName` (icon-slot convention). Events `wa-show`/`wa-after-show`/`wa-hide`/`wa-after-hide` → `OnShow`/`OnAfterShow`/`OnHide`/`OnAfterHide` (`EventCallback`, empty-detail → `System.EventArgs` or reuse existing show/hide args). Method `hide()` → `HideAsync`.

**Event delivery:** all four toast-item events (`wa-show`, `wa-after-show`, `wa-hide`, `wa-after-hide`) are **already registered** in `WebAwesome.Blazor.lib.module.js` — no JS initializer change needed. Bind under the `onwa-` prefix.

## parity-config.json changes (deviations — recorded with reasons)

1. `globalIgnoredAttributes`: add `did-ssr`, `dir`, `lang` (+ `ignoreReasons`).
2. Per form control (13): add `custom-error` to `ignoredAttributes`, `formStateRestoreCallback` to `ignoredMethods`; add `name` to `ignoredAttributes` for wa-file-input, wa-input, wa-number-input, wa-radio (reason exists).
3. Per chart (9): add `plugins` to `ignoredAttributes` (+ reason).
4. Update the two `extraElementMethods` reasons (observers `stopObserver`/`startObserver`, relative-time `update`) to note **re-verified against WA 3.3.0 (2026-07-23)** — confirmed still private in `mutation-observer.d.ts` / `resize-observer.d.ts`, and `update` still absent from `relative-time.d.ts` (only `updateTimeout`), none CEM-documented.
5. `targetWaVersion` → `3.3.0`, `enabled` stays `true` (Phase 3).

## Element method audit (Phase 4 step 6)

- `resetValidity` — CEM-documented in 3.3.0 for all form controls; invocations from `WaButton`/`WaFileInput`/`WaRadio`/`WaTextArea` (Components dir, source-scanned) resolve as known. (`WaInputBase` lives in `Base\`, not scanned.)
- Existing `extraElementMethods` re-verified against 3.3.0 source (see parity-config change 4). No new allowlist entries needed.

## Per-file actions

- `src\WebAwesome.Blazor\Base\IFormValidation.cs` — add `ResetValidityAsync`.
- `src\WebAwesome.Blazor\Base\WaInputBase.cs` — implement `ResetValidityAsync`.
- `src\WebAwesome.Blazor\Components\WaButton.cs`, `WaFileInput.cs`, `WaRadio.cs`, `WaTextarea.cs` — implement `ResetValidityAsync`.
- `src\WebAwesome.Blazor\Components\WaBadge.cs` — add start/end slots (+ icon convenience).
- New: `WaChartBase` + 9 chart wrappers, `WaToast.cs`, `WaToastItem.cs`.
- `src\WebAwesome.Blazor\Components\Enums.cs` — WaChartType, WaChartGrid, WaChartAxis, WaChartLegendPosition, WaToastPlacement (+ orientation/variant reuse or add).
- `src\WebAwesome.Blazor\Components\EventArgs.cs` — toast-item event args if not reusing existing.
- `src\WebAwesome.Blazor.Tests\ApiParity\expected-api-surface.json` ← `surface_3.3.0.json`; `parity-config.json` deviations; `src\Version.props` → 3.3.0 (Assembly/File 3.3.0.0); `README.md` version refs; demo `wwwroot\index.html` CDN → 3.3.0; demo `wwwroot\data\api-surface.json` ← target surface.
- No `WebAwesome.Blazor.lib.module.js` change (all bound events already registered).

## Tests & docs (Phase 5)

- wa-test-engineer: integration tests for the 9 charts + wa-toast + wa-toast-item (pattern `Wa*IntegrationTests.cs`); validity-reset tests; wa-badge slot tests. **No new form controls** in 3.3.0 (charts/toast are not form-associated), so no new `WaInputBase` EditForm coverage is required; add `ResetValidityAsync` coverage to existing form-control tests.
- No breaking changes → **no `MIGRATION-3.3.0.md`** required (note the nullable-`name` annotations and the new validity methods in the CHANGELOG instead).
- `docs\CHANGELOG.md`: `## [3.3.0]` with New components (charts, toast, toast-item), Changed (form-control validity: `ResetValidityAsync`, custom validity now CEM-owned; wa-badge start/end slots; global dir/lang/did-ssr pass-through), Library (version bump). Fold any `## [Unreleased]`.
- Demo: `tools\demo\New-WaDemoPages.ps1 -PruneRemoved` (adds 11 skeleton pages; none removed). Showcase: nothing removed; 11 additions noted as curation follow-up; no new form control to add to the form showcase.
- Promote `PublicApiSnapshotTests` baseline after confirming every diff is explained (new wrappers/enums/event args, `ResetValidityAsync`).
- Full Debug + Release build; `dotnet test`; Playwright e2e sweep.

## Validation checklist

- [ ] parity harness armed to 3.3.0, `ApiSurfaceParityTests` green
- [ ] `ElementMethodInvocationTests` / `EventBindingRegistrationTests` green
- [ ] 11 new wrappers build & documented (CS1591 clean)
- [ ] wa-badge start/end slots; `ResetValidityAsync` on all 13 controls
- [ ] deviations recorded in parity-config with reasons
- [ ] public API baseline promoted; CHANGELOG updated
- [ ] Debug + Release build (incl. demo) green; full test suite green; e2e green

## Risks

- **Low overall** — no destructive changes; the bulk is one base-class attribute triple (global-ignored) and a regular family of experimental chart wrappers.
- **Chart attribute casing** (`xLabel`/`yLabel`) — verify on the skeleton demo page; non-blocking.
- **wa-toast `create` return value** — wrapper returns `Task`; programmatic toast creation from Blazor is a thin passthrough, full parity of the returned element is out of scope.
