# Web Awesome 3.11.0 to 3.12.0 Upgrade Implementation Plan

JIRA: WAB-46 (epic WAB-45 "Web Awesome 3.12")
Branch: `/main/WA-3.12/WAB-46` (new train subtrunk `/main/WA-3.12`, created off `/main` cs:238; epic folder cs:239)
Source tag: https://github.com/shoelace-style/webawesome/tree/v3.12.0

## Overview

Upgrade the Blazor bindings from Web Awesome 3.11.0 to 3.12.0. Upstream, this release is small: one
component gains link behavior (`wa-dropdown-item`: `href`/`target`/`rel`/`download`), and six
components change only their documented CSS parts. There are no new or removed components and no
upstream breaking changes.

Most of the work in this upgrade is **mandatory extra scope**: the fixes for GitHub issue #1
(https://github.com/boldbrick/WebAwesome-Blazor/issues/1, reported by @Eonasdan). The assignment is
`temp\wa-3.12.0-issue-1-assignment.md`. Items 5 and 7 are wrapper-level breaking changes, so this
upgrade **does** ship `docs\MIGRATION-3.12.0.md`, although the upstream diff is non-breaking.

Change report: `temp\wa-api\changes_3.11.0_to_3.12.0.json` / `.md`
(0 added, 0 removed, 7 modified, 0 breaking).

## Analysis Summary

| Metric | Count |
|---|---|
| New components | 0 |
| Removed components | 0 |
| Modified components | 7 |
| Breaking changes (report) | 0 |
| Components in CEM | 87 (unchanged) |
| Wrapper breaking changes (issue #1) | 2 (`WaRelativeTime.Format`/`Numeric`, `WaTextArea.Rows`) |

Modified components:

| Component | Change | Wrapper impact |
|---|---|---|
| `wa-dropdown-item` | attributes `href`, `target`, `rel`, `download` added | new parameters on `WaDropdownItem` |
| `wa-color-picker` | cssParts `form-control`, `form-control-input`, `hint` added | none (CSS parts aren't in the parameter surface) |
| `wa-page` | cssPart `dialog-wrapper` removed | none, never referenced |
| `wa-radio-group` | cssPart `radios` removed | none, never referenced |
| `wa-slider` | cssPart `tooltip__content` renamed to `tooltip__body` | none in the library; check demo CSS for `::part(tooltip__content)` |
| `wa-textarea` | cssPart `form-control-input` removed | none, never referenced |
| `wa-video` | cssPart `progress` removed | none, never referenced |

### Documentation ingest

`v3.12.0` is tagged in the public GitHub repository, so `Sync-WaDocs.ps1 -Version 3.12.0` ran without
the `-DocsTagVersion` workaround used for 3.11.0. It wrote 154 files: 137 from the GitHub tag and 17
Pro/reference docs filled from the release zip's bundled references. None needed manual capture.
Most component docs changed provenance back from the 3.11.0 bundled references to the public docs
tree. Checked in as cs:240 (3 added, the rest modified; 26 files whose bytes were unchanged dropped
out automatically).

### Next-release check items (from the 3.11.0 CHANGELOG entry)

- Observer `startObserver()`/`stopObserver()`: still `private startObserver; private stopObserver;` in
  `mutation-observer.d.ts` (lines 34-35) and `resize-observer.d.ts` (lines 22-23) in 3.12.0. The
  allowlist stands, and the verification stamps will be updated.
- `wa-relative-time` `update()`: `relative-time.d.ts` still declares only `disconnectedCallback`,
  `willUpdate` and `render`, and `WebAwesomeElement extends LitElement` is unchanged in
  `internal\webawesome-element.d.ts`, so `update()` is still the inherited Lit lifecycle method. The
  allowlist stands, and the stamp will be updated.
- `WaDataGrid`'s eight JS accessor-only properties are still an open follow-up. They're out of scope
  here and carry forward unchanged.

## Phase 1 — Breaking changes (wrapper-level, from issue #1)

### 1a. WaRelativeTime format/numeric (issue item 5)

The CEM declares `format: 'long' | 'short' | 'narrow'` (default `long`) and
`numeric: 'always' | 'auto'` (default `auto`). The wrapper's `WaFormat { Auto, Relative, Numeric }`
never matched it:
- `Relative`/`Numeric` send invalid styles to `Intl.RelativeTimeFormat`. That throws a RangeError,
  so the element renders nothing.
- `Short`/`Narrow` can't be expressed at all.
- `Numeric=false` does nothing, because Blazor drops a `false` bool attribute, so
  `numeric="always"` is never sent.

- `src\WebAwesome.Blazor\Components\Enums.cs`: delete `WaFormat` and its `ToHtmlValue()` case (only
  `WaRelativeTime` uses it, which will be verified by grep). Add `WaRelativeTimeFormat { Long, Short, Narrow }`
  and `WaRelativeTimeNumeric { Auto, Always }`, each with `ToHtmlValue()`.
- `src\WebAwesome.Blazor\Components\WaRelativeTime.cs`: the parameters become
  `WaRelativeTimeFormat? Format` and `WaRelativeTimeNumeric? Numeric`, emitted only when set
  (`AddAttributeIfNotNull`). Delete the dead private `GetDateString()`. While editing, move the
  private helper into an `Internals` region as `CLAUDE.md` requires.
- Migration: `Format="WaFormat.Auto"` means omit the parameter. `Numeric="false"` becomes
  `Numeric="WaRelativeTimeNumeric.Always"`. `WaFormat.Relative`/`WaFormat.Numeric` never worked.
- Not needed: the issue's "August instead of 8" example belongs to `WaFormatDate`, which already
  supports it via `Month="WaDateTimeStyle.Long"`. That will be stated in the migration guide and the
  issue reply.

### 1b. WaTextArea.Rows (issue item 7)

`Rows` is a non-nullable `int` passed to `AddAttributeIfNotNull`, so `rows="0"` is always emitted and
overrides WA's default of 4 (CEM `rows` default `4`). It becomes `int? Rows`, emitted only when set.
The migration note is that `Rows` is now `int?`. That's source-compatible for assignments and
breaking only for C# code that reads the property as `int`.

## Phase 2 — New components

None.

## Phase 3 — Modified components and issue #1 non-breaking fixes

### 3a. WaDropdownItem link attributes (CEM, additive)

- `Href`, `Target` (`string?`, following the `WaButton`/`WaBreadcrumbItem` convention for the same
  attribute), `Rel`, and `Download`, all `string?` and emitted when non-empty.
- Existing defect found during analysis: `WaDropdownItem` binds `OnBlur`/`OnFocus` to the attribute
  names `"blur"`/`"focus"` instead of `"onblur"`/`"onfocus"`, so Blazor never registers them as
  event handlers. It's a one-line fix in the file being edited anyway, so it's fixed here with a
  bUnit assertion and a CHANGELOG `### Fixed` note.

### 3b. Model-to-UI value sync (issue item 2)

Root cause: in WA 3 the `value`/`checked` attribute maps to the `defaultValue`/`defaultChecked`
field. After the user has interacted, the element ignores the attribute. Blazor assigns the `value`
property only on native INPUT/SELECT/TEXTAREA; on custom elements it calls `setAttribute`. So once
the user has edited a field, C#-side changes to the bound value no longer reach the UI. That breaks
reset-after-submit, normalizing setters, and "clear"/"select all" buttons.

The affected list was re-derived from the 3.12.0 CEM (attributes whose `fieldName` starts with
`default`). It matches the assignment:

| Wrapper | Attribute → field | Live property type (3.12.0 `.d.ts`) |
|---|---|---|
| `WaInput` | `value` → `defaultValue` | `string \| null` |
| `WaTextArea` | `value` → `defaultValue` | `string \| null` |
| `WaNumberInput` | `value` → `defaultValue` | `string \| null` |
| `WaColorPicker` | `value` → `defaultValue` | `string \| null` |
| `WaDateInput` | `value` → `defaultValue` | `string` (setter also accepts Date/object) |
| `WaKnownDate` | `value` → `defaultValue` | `string` (setter also accepts Date/null) |
| `WaOtpInput` | `value` → `defaultValue` | `string` |
| `WaTimeInput` | `value` → `defaultValue` | `string` (setter also accepts Date/null) |
| `WaRadioGroup` | `value` → `defaultValue` | `string \| number \| null` |
| `WaSlider` | `value` → `defaultValue` | `number` (range mode: `minValue`/`maxValue` are plain live properties) |
| `WaCheckbox` | `checked` → `defaultChecked` | `boolean` |
| `WaSwitch` | `checked` → `defaultChecked` | `boolean` |

Not affected, because the attribute maps to the live property: `WaSelect`, `WaCombobox`,
`WaDatePicker`, `WaRating` (`rating` also has a separate `default-value` attribute). `wa-option`'s
`selected` → `defaultSelected` also shows up in the CEM query. `WaOption` isn't a bound form control:
selection is driven through the parent `WaSelect`/`WaCombobox` `value`, which is live. So it's out of
scope, and noted only for completeness.

Design (shared, in `WaInputBase<TValue>`):
- The wrapper keeps emitting the attribute, which SSR and the initial value need.
- A derived wrapper declares the live property to sync, via a protected virtual such as
  `LiveValuePropertyName` (`"value"`/`"checked"`, null means no sync), and the JS-typed value, via
  `GetLiveValue()`: string for most, number for slider, bool for checkbox/switch.
- `OnAfterRenderAsync`: skip on `firstRender` (the attribute delivered the initial value), skip when
  `Element` is null, and skip when the live value equals the **last synced** value. Otherwise call the
  interop and record the value. This avoids an interop call per render.
- Values that arrive **from** the element through the change/input binders update the last-synced
  marker before the model is set. So a UI-originated value is never pushed back, which avoids the
  typing race under Blazor Server (a stale round-trip would otherwise overwrite newer keystrokes). A
  C#-side change (reset, normalizing setter) differs from the marker and is pushed.
- New JS helper `syncProperty(element, name, value)` in `wwwroot\webawesome-interop.js` assigns only
  when `element[name] !== value`, so the cursor doesn't jump mid-typing. A matching
  `WebAwesomeJSInterop.SyncPropertyAsync` is added.
- `WaTextArea` moves from `InputBase<string?>` onto `WaInputBase<string?>`. The public surface stays
  compatible: the duplicated `Element`, `Class`, `Style`, `Size`, `Disabled`, `Readonly`, `Required`,
  `MinLength`, `MaxLength`, `Autocomplete`, `Label`/`MarkupLabel`, `Hint`/`MarkupHint`, `OnBlur`,
  `OnFocus`, `OnInput` and the IFormValidation members come from the base with identical names and
  types. It additionally gains the base's `OnKeyDown`/`OnKeyUp`/`OnKeyPress` (additive, and useful
  for the issue's Ctrl+Enter scenario). Rendering (attribute names and values) must stay identical,
  which the existing `WaTextArea` tests guard. The public API snapshot diff must show only
  member-origin moves plus the additive members.

### 3c. UI-to-model after SetRangeTextAsync (issue item 3)

WA's `setRangeText` on `wa-input`/`wa-textarea` updates `value` but dispatches no `input`/`change`
event (unchanged in 3.12.0). `WaInput.SetRangeTextAsync`/`WaTextArea.SetRangeTextAsync` therefore
leave `CurrentValue`/`EditContext` stale. The fix: after the JS call, read back `value`
(`GetPropertyAsync<string?>`), update the last-synced marker, and assign `CurrentValueAsString`, which
notifies the `EditContext`. No synthetic DOM events.

### 3d. Opt-in immediate binding (issue item 4, additive)

`[Parameter] public bool Immediate { get; set; }` on `WaInput`, `WaTextArea`, and, for consistency,
`WaNumberInput` (the same `CreateBinder<string?>` shape). When true, the value binder runs on
`oninput`. It's merged with the user's `OnInput` into **one** `oninput` handler that first updates
the value and then invokes `OnInput`, because two attributes with the same name would clash.
`AddCommonEventHandlers` must not emit its own `oninput` when the wrapper merges it. The binder
keeps running on `onchange` too, so the committed value is still exact when `Immediate` is set.

### 3e. License path (issue item 1, build)

`src\Directory.Build.props` packs `$(SolutionDir)..\LICENSE.md`. When the repo is a submodule, the
consumer's `SolutionDir` misplaces it (NU5030/NU5019, because `GeneratePackageOnBuild=True`). The fix
is `$(MSBuildThisFileDirectory)..\LICENSE.md`, anchored to the props file. The fork's
`$(MSBuildProjectDirectory)/../../` hard-codes the project depth and isn't used. Verify with
`dotnet build src\WebAwesome.Blazor\WebAwesome.Blazor.csproj -c Release -p:SolutionDir=<other dir>\`
and check that the nupkg contains `LICENSE.md`. Out of scope, noted only: `SolutionDir`-derived
`ArtifactsDir`/`BbToolsDir` make a submodule build write into the consumer's `output\`, which is
harmless.

## Phase 4 — Intentional deviations and tooling (parity-config.json)

- `targetWaVersion` → `3.12.0`, `enabled` stays true.
- Re-stamp both `extraElementMethods` reasons with the 3.12.0 verification (see above).
- **Enum-value parity test (issue item 6):** the ApiParity tests check names, not values. A new test
  covers every enum-typed parameter mapped to a CEM attribute whose type is a string-literal union:
  every `ToHtmlValue()` output must be a member of the union. This would have caught item 5.
  Deliberate exceptions go into a new `parity-config.json` list, per component, with an
  `ignoreReasons` entry. The test must be shown to fail against the pre-fix `WaFormat` mapping.
- No new attribute/event ignores are expected. `href`/`target`/`rel`/`download` map mechanically.

## Phase 5 — Tests and docs

bUnit (wa-test-engineer):
- `WaRelativeTime`: `format`/`numeric` omitted by default, and emitted as `short`/`narrow`/`long`
  and `always`/`auto` when set.
- `WaTextArea`: `rows` omitted by default, emitted when set. The existing textarea tests stay green
  after the rebase.
- `Immediate`: renders an `oninput` binder that updates the value **and** still invokes `OnInput`,
  and there's no duplicate `oninput` attribute. Without `Immediate`, rendering is unchanged.
- `SetRangeTextAsync` reads the value back and updates `CurrentValue`/`EditContext` (mocked
  interop), for both `WaInput` and `WaTextArea`.
- Value sync: after a C#-side change the interop sync is invoked once with the live property name
  and value. Nothing is invoked on first render, and nothing when the value came from the element.
- `WaDropdownItem`: link attributes, and `onblur`/`onfocus` wiring.
- 3.12.0 breaking-change validation per the `WaBreakingChangesValidationTests`
  pattern: `WaFormat` gone, the new enum types and nullability, `Rows` is `int?`.
- The enum-value parity test (Phase 4).

Browser e2e (Playwright, `tools\e2e\tests\value-sync-binding.spec.js`, in the style of
`checkbox-switch-binding.spec.js`). **Each spec must fail on the pre-fix code first**, then pass
after the fix:
- For each of the 12 item-2 wrappers: the user edits, C# sets the model through a button, and the
  element's live property and visible value match the model.
- `SetRangeTextAsync` updates the bound display.
- `Immediate` + keypress updates the model before blur.
- `WaRelativeTime` Short/Narrow/Always render the expected text, computed in-browser with
  `Intl.RelativeTimeFormat`.
- Driving controls in the demo: "Programmatic value / reset" examples on the Input, Textarea and
  Checkbox pages, plus one table-driven harness page for the remaining wrappers, routed at
  `/testing/value-sync`. That route is outside `/components/`, so it isn't derived from
  `api-surface.json`, isn't linked from the sidebar, and can't create an unmapped nav entry. It's
  added to `tools\e2e\tests\helpers\routes.js` as a harness route so the sweep still visits it.
- Red-first protocol: the harness and specs are authored against the **pre-fix** API and run red.
  `Immediate="true"` compiles pre-fix because `InputBase` captures unmatched attributes. The
  relative-time rows use the pre-fix enum (`Numeric="false"`, `WaFormat.Relative`,
  `WaFormat.Numeric`), which is the only way the old API could request a non-default style. After
  the fix, only the relative-time markup is migrated to the new enums (the consumer migration
  itself). The spec file is unchanged between the red and green runs.
- Run hygiene: `DEMO_BASE_URL=http://localhost:5100`, Pro override cleared
  (`tools\demo\Set-WaProAssets.ps1 -Clear`; neither `appsettings.Local.json` currently exists).

Docs:
- `docs\MIGRATION-3.12.0.md` covering items 5 and 7, plus the additive `Immediate`, the value-sync
  behavior change, and the `WaTextArea` base-class move.
- `docs\CHANGELOG.md` `## [3.12.0]` with `### Breaking changes` (items 5 and 7), `### Changed`,
  `### Fixed`, `### Library`, and `### Public API`. It credits @Eonasdan (issue #1).
- Demo: curate a `WaDropdownItem` link example on `DropdownItemPage.razor`. Migrate any existing
  `WaFormat`/`Numeric` usage on `RelativeTimePage.razor` and add Short/Narrow/Always examples. Grep
  the demo CSS for `tooltip__content`. No showcase changes (no added or removed components).
- Promote the public API baseline only when every diff is explained by this plan.

## Validation checklist

- [ ] `dotnet build src/WebAwesome.slnx -p:Configuration=Debug`: 0 warnings, 0 errors
- [ ] `dotnet build src/WebAwesome.slnx -p:Configuration=Release`: 0 warnings, 0 errors
- [ ] Submodule-style build (`-p:SolutionDir=<other>\`) succeeds, and the nupkg contains `LICENSE.md`
- [ ] `dotnet test src/WebAwesome.slnx` green on net9.0 and net10.0 (baseline 627 per TFM)
- [ ] `ApiSurfaceParityTests` green (expected gaps at arming: the four `wa-dropdown-item` attributes)
- [ ] Enum-value parity test green (shown red against the pre-fix `WaFormat`)
- [ ] `EventBindingRegistrationTests`, `ElementMethodInvocationTests` green
- [ ] `PublicApiSnapshotTests` baseline promoted, every diff explained
- [ ] e2e: new value-sync specs red on pre-fix code (captured), green after the fix; full sweep green

## Risks

- **Typing race with value sync.** Pushing the live property after every render could overwrite
  in-flight keystrokes under Blazor Server. It's mitigated by the last-synced marker (UI-originated
  values are never pushed back) plus the `!==` guard in JS. The e2e `Immediate` spec exercises it.
- **WaTextArea rebase.** Moving the base class must not change rendered attribute names or
  sequencing semantics. The existing tests plus the snapshot diff guard it.
- **`Immediate` + `OnInput` merge.** A second `oninput` attribute would silently replace the first
  in Blazor's diff. The bUnit test asserts a single handler that does both.
- **Slider range mode.** `WaSlider` with `Range=true` binds `min-value`/`max-value`, not `value`.
  Those are live properties, so only single-value mode needs syncing.
- **Red-first for relative time.** The pre-fix API can't express Short/Narrow, so the red run shows
  the pre-fix enum's failure (RangeError or wrong text) on the same spec rows. This is stated
  explicitly in the report.
