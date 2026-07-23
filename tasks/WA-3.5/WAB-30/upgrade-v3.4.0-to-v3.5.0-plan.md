# Web Awesome 3.4.0 to 3.5.0 Upgrade Implementation Plan

## Overview
Implementation plan for upgrading the WebAwesome.Blazor wrapper library from the bound Web Awesome **3.4.0** to **3.5.0**. This opens the **WA-3.5 train** (subtrunk `/main/WA-3.5` branched off `/main` at the released 3.4.0 head, cs:160; developed on `/main/WA-3.5/WAB-30`). JIRA task **WAB-30** under epic **WAB-29** ("Web Awesome 3.5").

## Analysis Summary
Authoritative worklist: `temp\wa-api\changes_3.4.0_to_3.5.0.json` (1 added, 0 removed, 9 modified, 4 breaking).

A small, almost entirely **additive** upgrade: one new free component (`wa-markdown`), a cluster of new SSR hydration-hint boolean attributes (`with-*`) across seven components, a `placement` attribute on the color picker, a default trigger slot on the copy button, and the promotion of `wa-rating` into a full form-associated control. All four upstream "breaking" changes are non-destructive for the wrappers (mirroring the 3.4.0 pattern) — **no migration guide is required**.

> Note on the Markdown change report: `changes_3.4.0_to_3.5.0.md` lists spurious `cssParts` entries for `wa-textarea` (`Length`, `LongLength`, `Rank`, `SyncRoot`, ...) — a rendering artifact in `Compare-WaApiSurface.ps1` (it enumerated a .NET array object's members). The JSON report is authoritative and shows only `cssParts.added: ["count"]` for `wa-textarea`. CSS parts do not map to wrapper API in any case (parity checks attributes/events/methods only).

## Breaking Changes to Address (all non-destructive for the wrappers)

### 1. `wa-rating` — `change` event metadata change (NO wrapper action)
The `change` event's CEM `type` went from `"Event"` to `null`; the event still exists (`ChangeEvent`, same name). The wrapper folds `change` into the two-way `@bind-Value` binding, already recorded as an `ignoredEvents: ["change"]` deviation. No wrapper change.

### 2 & 3. `wa-rating` — `blur` / `focus` methods removed from CEM (NO wrapper action)
Upstream dropped the explicitly-documented `blur()`/`focus()` methods; the underlying element still exposes the **native** `HTMLElement.blur()`/`focus()`. `WaRating.BlurAsync()`/`FocusAsync()` invoke the native methods, which are allowlisted globally in `parity-config.json` `nativeElementMethods`. `ApiSurfaceParityTests` matches CEM→wrapper (extra wrapper methods are not flagged), so both stand. No wrapper change; documented in CHANGELOG as a non-destructive upstream removal.

### 4. `wa-textarea` — `autocorrect` type widened `string` → `boolean` (NO wrapper action)
Identical to the `wa-input`/`wa-combobox` widening handled in 3.4.0: the JS **property** type changed, but the **attribute** form remains `"off"`/`"on"`. `WaTextArea.AutoCorrect` stays `string?` (rendered as the attribute) and is already mapped via `attributeOverrides`. No wrapper change.

## New Component

### 5. `wa-markdown` → `WaMarkdown` (new, free, experimental, since 3.4)
**File**: `src\WebAwesome.Blazor\Components\WaMarkdown.cs` (new)
Renders markdown content as HTML. Source markdown is read from a child `<script type="text/markdown">` element (CEM declares 0 slots; the script child is the content channel).

- **Attributes** (CEM): `tab-size` (number, default 4) → `[Parameter] public int TabSize { get; set; } = 4;` rendered as `tab-size`. `did-ssr`, `dir`, `lang` are globally ignored (SSR marker + native global attributes, passed through `AdditionalAttributes`).
- **Content**: expose the markdown text via a string parameter (e.g. `Content`) rendered inside a `<script type="text/markdown">` child; also accept `ChildContent` for raw markup passthrough. Support `AdditionalAttributes`, `Class`, `Style`, `Element` reference capture consistent with other non-input wrappers (e.g. `WaCopyButton`).
- **Methods** (CEM): three declared.
  - `renderMarkdown()` (instance) → `RenderMarkdownAsync()` invoking the `renderMarkdown` element method.
  - `getMarked()` and `updateAll()` are **static** class methods (verified in `dist\components\markdown\markdown.d.ts`: `static getMarked()`, `static updateAll()`). They operate on the shared `Marked` parser instance / all instances, not a single element, and `getMarked` returns a non-marshalable JS `Marked` object. Both are recorded as intentional deviations (`ignoredMethods`), not wrapped.
- **Events / slots**: none.

**parity-config.json** entry:
```json
"wa-markdown": { "ignoredMethods": [ "getMarked", "updateAll" ] }
```
with `ignoreReasons` for `getMarked` and `updateAll` (static class-level, operate on the shared Marked instance / all instances, not per-element and not marshalable from Blazor).

## Modified Components (additive)

### 6. `wa-button` → `WaButton` — SSR hint attributes
**File**: `WaButton.cs`. Add `[Parameter] public bool WithStart` (`with-start`) and `[Parameter] public bool WithEnd` (`with-end`), rendered as booleans. Parity: `with-start`→`WithStart`, `with-end`→`WithEnd` (mechanical). Use free render-tree sequence numbers (e.g. 64, 65).

### 7. `wa-color-picker` → `WaColorPicker` — popup placement
**File**: `WaColorPicker.cs`. Add `[Parameter] public WaPlacement? Placement` rendered as `placement` (CEM default `bottom-start`; leave null → element default). `WaPlacement` already exists and is used by `TooltipPlacement`. Parity: `placement`→`Placement`.

### 8. `wa-copy-button` → `WaCopyButton` — default trigger slot
**File**: `WaCopyButton.cs`. Add `[Parameter] public RenderFragment? ChildContent` rendered as the default slot (custom trigger element). Slots are not parity-enforced; this is a real feature addition. No config.

### 9. `wa-dialog` → `WaDialog` — SSR footer hint
**File**: `WaDialog.cs`. Add `[Parameter] public bool WithFooter` (`with-footer`). `FooterContent` slot already exists. Sequence e.g. 14.

### 10. `wa-drawer` → `WaDrawer` — SSR footer hint
**File**: `WaDrawer.cs`. Add `[Parameter] public bool WithFooter` (`with-footer`). Free sequence (e.g. 15).

### 11. `wa-rating` → `WaRating` — promoted to full form control
**File**: `WaRating.cs`. `WaRating` already derives from `WaInputBase<decimal>`.
- New CEM attributes:
  - `required` → already covered by base `Required` (rendered via `AddCommonAttributes`). No change.
  - `name` → covered by `InputBase` form integration (`NameAttributeValue`, rendered via `AddCommonAttributes`). Ignored deviation (same as all other form controls).
  - `custom-error` → managed imperatively via `IFormValidation.SetCustomValidityAsync`/`ResetValidityAsync` on the base. Ignored deviation.
  - `default-value` (number, default 0) → **new** to the form-control family; expose `[Parameter] public decimal DefaultValue { get; set; }` rendered as `default-value`. Parity: `default-value`→`DefaultValue` (mechanical). The form-reset value.
- New CEM event `wa-invalid` → add `[Parameter] public EventCallback<EventArgs> OnInvalid` + `builder.AddAttributeIfHasDelegate(..., "onwa-invalid", OnInvalid)`, matching every other form control. `wa-invalid` is already registered globally in `WebAwesome.Blazor.lib.module.js` — no JS change.
- New CEM methods:
  - `setCustomValidity` → base `SetCustomValidityAsync` (auto-matched).
  - `resetValidity` → base `ResetValidityAsync` (auto-matched).
  - `formStateRestoreCallback` → FACE lifecycle callback, ignored deviation (same as all other form controls).

**parity-config.json** entry (extend the existing one):
```json
"wa-rating": {
  "ignoredAttributes": [ "getSymbol", "name", "custom-error" ],
  "ignoredEvents": [ "change" ],
  "ignoredMethods": [ "formStateRestoreCallback" ]
}
```
(`getSymbol` and `change` are pre-existing; `name`, `custom-error`, `formStateRestoreCallback` added with the shared reasons already present in `ignoreReasons`.)

### 12. `wa-slider` → `WaSlider` — SSR label/hint hints
**File**: `WaSlider.cs`. Add `[Parameter] public bool WithHint` (`with-hint`) and `[Parameter] public bool WithLabel` (`with-label`). Free sequences (e.g. 34, 35).

### 13. `wa-textarea` → `WaTextArea` — character-count display
**File**: `WaTextarea.cs`. Add `[Parameter] public bool WithCount` (`with-count`, default false) rendered as boolean (sequence e.g. 37). Real feature (shows a character count). (`autocorrect` type widening: no change — see Breaking #4.)

### 14. `wa-toast-item` → `WaToastItem` — SSR icon hint
**File**: `WaToastItem.cs`. Add `[Parameter] public bool WithIcon` (`with-icon`). Free sequence (e.g. 7).

## Event Delivery Contract
`wa-invalid` (the only newly-bound event, on `WaRating`) is already registered in `src\WebAwesome.Blazor\wwwroot\WebAwesome.Blazor.lib.module.js`. No JS module change is required. `EventBindingRegistrationTests` should remain green.

## Element Method Audit (re-verify against 3.5.0 source)
- `wa-mutation-observer` / `wa-resize-observer` `extraElementMethods: stopObserver, startObserver`: re-verify present in `temp\wa-src\3.5.0\dist\components\{mutation,resize}-observer\*.d.ts`.
- `wa-relative-time` `extraElementMethods: update`: re-verify (inherited Lit `ReactiveElement.update()`; only `updateTimeout` in `relative-time.d.ts`).
- `wa-page` `visiblePixelsInViewport` ignored method: re-verify still present.
- `WaRating.BlurAsync`/`FocusAsync` now rely purely on `nativeElementMethods` (blur/focus) since the CEM dropped them — confirm `ElementMethodInvocationTests` stays green.
- Update the `extraElementMethods:*` `ignoreReasons` entries to append the 3.5.0 re-verification date.

## Version / harness bump (Phase 3)
- Copy `temp\wa-api\surface_3.5.0.json` → `src\WebAwesome.Blazor.Tests\ApiParity\expected-api-surface.json`.
- `parity-config.json`: `targetWaVersion` → `3.5.0`, `enabled` → true, add the deviations above.
- `src\Version.props`: `Version`/`AssemblyVersion`/`FileVersion` → 3.5.0 (minor bump). `README.md` version references.
- Demo: CDN version in `src\WebAwesome.Blazor.Demo\wwwroot\index.html` → 3.5.0; copy surface → `src\WebAwesome.Blazor.Demo\wwwroot\data\api-surface.json`.
- jsdelivr availability of WA 3.5.0 to be checked before the browser sweep; if 404, serve local assets via `tools\demo\Set-WaProAssets.ps1` (WA_PRO_DIST → extracted 3.5.0 dist), cleared with `-Clear` afterwards; never check the override in.

## Testing (Phase 5)
- `wa-test-engineer`: integration tests for `WaMarkdown`; additive-attribute assertions for the modified wrappers; `WaRating` form-control additions (`OnInvalid`, `DefaultValue`).
- **Coverage rule**: `WaRating` (derives from `WaInputBase<decimal>`) already a form control — ensure its EditForm bUnit coverage under `src\WebAwesome.Blazor.Tests\Base\` covers the new `DefaultValue`/`OnInvalid`/custom-validity surface. `WaMarkdown` is not a form control (no EditForm coverage needed).
- Breaking-change validation tests (version-scoped) asserting the non-destructive shape (autocorrect still `string?`; blur/focus still present; rating name/required/default-value/wa-invalid present).
- `docs\CHANGELOG.md`: `## [3.5.0]` entry (Breaking upstream/no-action, New components, Changed, Library, Public API, Deviations, Next-release check outcomes). No `MIGRATION-3.5.0.md` (no wrapper-breaking changes).
- Demo pages: `tools\demo\New-WaDemoPages.ps1 -PruneRemoved` (adds a skeleton `WaMarkdown` page). No showcase removals (no components removed). `WaMarkdown` is not a form control → not added to the form showcase; queued as showcase-curation follow-up.
- `PublicApiSnapshotTests`: promote the baseline (additions: `WaMarkdown` + its members; `WithStart`/`WithEnd`, `Placement`, `WithFooter`×2, `DefaultValue`+`OnInvalid` on rating, `WithHint`/`WithLabel` on slider, `WithCount`, `WithIcon`, `ChildContent` on copy button). All additive; every diff explained by the change report.
- Debug + Release build (incl. demo), full `dotnet test`, then Playwright sweep from `tools\e2e`.

## Implementation Priority
1. **Phase 3** — arm parity harness + version bump (red parity tests become the worklist).
2. **Phase 4** — breaking changes (verify no-op + record deviations), then new/modified wrappers via `wa-wrapper-engineer` (single group: `WaMarkdown` + the 8 modified components ≤10), then parity-config deviations + element-method re-verification; iterate to green.
3. **Phase 5** — tests, CHANGELOG, demo pages, public-API snapshot, Release build, browser sweep.
4. **Phase 6** — phased check-ins, JIRA In Review, `--publish` merge to `/main/WA-3.5`.

## Validation Checklist
- [ ] `WaMarkdown` created; renders `<wa-markdown>` with `tab-size` and script-child source; `RenderMarkdownAsync` present.
- [ ] SSR `with-*` attributes added to button/dialog/drawer/slider/textarea/toast-item.
- [ ] `WaColorPicker.Placement`, `WaCopyButton.ChildContent`.
- [ ] `WaRating`: `DefaultValue`, `OnInvalid`; parity deviations recorded.
- [ ] `parity-config.json` retargeted (3.5.0, enabled) with all deviations + reasons; `extraElementMethods` re-verified against 3.5.0.
- [ ] `ApiSurfaceParityTests`, `EventBindingRegistrationTests`, `ElementMethodInvocationTests`, `PublicApiSnapshotTests` green.
- [ ] Version bumped; README + demo synced.
- [ ] Debug + Release build clean; full test suite green; Playwright sweep green.

## Risk Assessment
- **Low risk overall.** Additive upgrade; no removed components, no wrapper-breaking changes.
- **Low**: `wa-markdown` content model (script-child) is the only non-mechanical piece; verified against `markdown.d.ts`.
- **Low**: SSR `with-*` attributes are inert booleans (default false) — no behavior change unless set.
- **Nil**: the four upstream "breaking" entries are non-destructive for the wrappers.
