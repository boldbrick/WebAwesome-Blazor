# Web Awesome 3.12.0 to 3.13.0 Upgrade Implementation Plan

JIRA: WAB-48 (epic WAB-47 "Web Awesome 3.13")
Branch: `/main/WA-3.13/WAB-48` (new train subtrunk `/main/WA-3.13`, created off `/main` cs:314; epic folder cs:315)
Source tag: https://github.com/shoelace-style/webawesome/tree/v3.13.0

## Overview

Upgrade the Blazor bindings from Web Awesome 3.12.0 to 3.13.0. Upstream, this is a small, additive release:
one new free form control, `wa-tag-input`, and eleven components promoted from experimental to stable.
There are no removed components, no removed or renamed attributes, events, slots or methods, and no
breaking changes in the report. No wrapper API is removed or renamed either, so **no migration guide** is
needed.

Release gate: `/main` head cs:314 carries `Version.props` 3.12.0 (the WA-3.12 promotion), so the WA-3.13 train
could start. No patch work is pending on `/main/WA-3.12`.

Change report: `temp\wa-api\changes_3.12.0_to_3.13.0.json` / `.md`
(1 added, 0 removed, 11 modified, 0 breaking).

## Analysis Summary

| Metric | Count |
|---|---|
| New components | 1 (`wa-tag-input`, free, experimental, since 3.13) |
| Removed components | 0 |
| Modified components | 11 (status only) |
| Breaking changes (report) | 0 |
| Components in CEM | 88 (was 87) |
| Wrapper breaking changes | 0 |

Modified components: the status of each of these changes from `experimental` to `stable`. The attributes,
events, slots, methods and CSS parts stay the same.

| Component | Wrapper impact |
|---|---|
| `wa-accordion`, `wa-accordion-item` | none |
| `wa-date-input`, `wa-date-picker`, `wa-known-date`, `wa-time-input` | XML summaries of `WaDateInput`, `WaDateRangeInput`, `WaDatePicker`, `WaDateRangePicker`, `WaKnownDate`, `WaTimeInput` drop "experimental" |
| `wa-otp-input`, `wa-pagination`, `wa-random-content`, `wa-video`, `wa-video-playlist` | none |

The demo's flask badge is driven by the surface JSON (`status`), so it follows automatically once
`wwwroot\data\api-surface.json` is refreshed.

### Upstream source changes outside the CEM

The compiled chunks carry their `// _bundle_/src/...` module markers, so the 3.12.0 and 3.13.0 builds were
compared module by module (chunk hashes normalized). 4 modules are new (`tag-input`, its styles, its
validator, its React wrapper), none removed, and 14 changed:

| Module | Change | Wrapper impact |
|---|---|---|
| `accordion`, `accordion-item` | roving-tabindex bookkeeping (`isTabbable`, `focusin` handler) removed, keyboard index taken from the focused item | none (no API) |
| `combobox`, `select` (+ styles) | `wa-select` passes `pill` and `size` to its tags | none |
| `data-grid` | active-cell update parked while a mouse button is down; column style/resize refactoring | none (no API); the e2e data grid specs re-verify it |
| `icon/library.system` | system icon data | none |
| `page` (+ styles) | the `mobile-navigation-footer` slot is shown only in mobile view when a footer slot has content | none (slot names unchanged) |
| `popover` | a press that starts inside the popover no longer light-dismisses it | none |
| `tooltip` | focus/blur containment checks use composed-tree ancestry (crosses slots and shadow roots) | none; this probably helps with the old "WaTooltip focus triggers" note |
| `translations/de`, `en` | new tag-input terms | none |

### Standing re-verifications (every upgrade)

The following modules are **byte-identical** to 3.12.0 (modulo chunk names), so every allowlist entry and
cited pattern that rests on them still holds:

- `extraElementMethods`: `wa-mutation-observer`/`wa-resize-observer` `stopObserver`/`startObserver`,
  `wa-relative-time` `update`. The observer and relative-time modules and `webawesome-element` are unchanged.
- `sourceVerifiedEvents`: `wa-color-picker` `wa-show`/`wa-after-show`/`wa-hide`/`wa-after-hide`. The
  color-picker module is unchanged: it still dispatches them as plain, non-bubbling events, so the relays
  stay.
- `sourceVerifiedSlots`: `wa-date-picker` `day-YYYY-MM-DD`. The CEM still omits it (the staleness test
  confirms), and the date-picker module is unchanged.
- `cemOnlyEvents`: `wa-data-grid` `request`. It is still only in the CEM; the staleness test confirms.
- `WaWirePatterns` / `WaWireFormat`: `date-picker/internal/iso.ts`, `matchers.ts` and
  `time-input/internal/segments.ts` are unchanged, and so are the list separators of the existing list
  attributes.
- `wa-date-input` `updateForwardedDaySlots`: unchanged, so the day-slot forwarding nudge stays (WAB-46
  follow-up).
- Event dispatch re-check (`bubbles: false`, `new CustomEvent(`, `stopPropagation()`): the only changed
  modules that dispatch events are data-grid (unchanged dispatches) and the new tag-input (below).
- `tools\upgrade\external-type-aliases.json`: the new `wa-tag-input` unions are all literal, and no new
  external alias appears. `EnumValueParityTests` fails if that is wrong.

### Documentation ingest

`v3.13.0` is tagged in the public repository, so `Sync-WaDocs.ps1 -Version 3.13.0` ran without
`-DocsTagVersion`. It wrote 153 files: 136 from the tag and 17 Pro/reference docs from the release zip.
None needed capture. Checked in as cs:316 (1 added, `components\tag-input.md`; 2 deleted; 152 modified).

### Next-release check items

The 3.12.0 CHANGELOG entry has no open "Next-release check" notes. The WAB-46 follow-ups are
owner-deferred (focus moves inside a control, net11 union types, the `[WebAwesomePro]` idea, `WaDataGrid`
accessor-only properties) or re-verified above (the day-slot nudge). All of them carry forward unchanged.

## Phase 1 — Breaking changes

None. The report has no `breakingChanges`, and no wrapper member is removed or renamed.

## Phase 2 — New component: `WaTagInput` (`wa-tag-input`)

Free and experimental (`status: experimental`, `since: 3.13`). A form-associated control that collects a
list of short strings as removable tags. Its live value is a JS `string[]` property. The `value` attribute
only sets `defaultValue`, as a delimiter-separated string, for the initial value and form reset.

**Hierarchy:** `WaTagInput : WaLabeledInputBase<IReadOnlyList<string>?>, IWaClearableControl, IWaAffixedControl`.
The element declares the whole label cluster (`label`/`hint` attributes and slots, `with-label`,
`with-hint`), the clear cluster (`with-clear`, `clear-icon`, `wa-clear`) and the affix slots
(`start`/`end`). It has no popup, so it does not derive from `WaPopupInputBase`.

**Value binding (decision):** `@bind-Value` of `IReadOnlyList<string>?`. This follows the `WaSelect`/`WaCombobox`
`Multiple` precedent:
- The list is pushed into the live `value` property as a string array (`LiveValuePropertyName` = `value`,
  `GetLiveValue()` = a stable `string[]` that is replaced only when the content changes). It is pushed on
  the first render when the list is non-empty, and after every C#-side change.
- **No `value` attribute is rendered.** The attribute is delimiter-separated and trimmed, so it can't carry
  a tag that contains a delimiter character or has leading or trailing spaces, and it only sets
  `defaultValue`. This is recorded as an `unrenderedAttributes` deviation (`wa-tag-input:value`). A consumer
  who wants an HTML reset default can still pass `value="a,b"` through `AdditionalAttributes`.
- `change` (a bubbling native `Event`) is handled by the value binder, which reads the element's `value`
  array (`GetStringArrayValue`), records it as synced and assigns `CurrentValue`. So `change` joins
  `ignoredEvents`, as on the other form controls.
- `OnInput` (common handler) is bound to `input`. Its `ChangeEventArgs.Value` carries the element's value,
  which is the tag array, not the typed text. Document this on the wrapper.
- `TryParseValueFromString` mirrors the element's `parseDelimited`: it splits on any delimiter character
  (a comma when `Delimiter` is empty), trims, and drops empty items. `FormatValueAsString` joins with the
  first delimiter character. Both are used only by `CurrentValueAsString`, never rendered.

**Attributes** (`Default<Name>` constants for every CEM literal default; the sticky overloads apply):

| Attribute | Parameter | Notes |
|---|---|---|
| `allow-duplicates` | `bool AllowDuplicates` | |
| `appearance` | `WaInputAppearance? Appearance` | default `Outlined` |
| `autocapitalize` | `WaAutoCapitalize? AutoCapitalize` | override entry as on `wa-combobox` |
| `autocomplete` | `string? Autocomplete` | form-control slot 11 |
| `autocorrect` | `bool? AutoCorrect` | `AddOnOffAttribute`; `onOffAttributes` entry |
| `custom-error` | none | `ignoredAttributes` (custom validity is imperative, as on the other 18 form controls) |
| `delimiter` | `string? Delimiter` | default `","`; an empty string must render `delimiter=""` (Enter only), so not `AddAttributeIfNotNullOrEmpty` |
| `did-ssr`, `dir`, `lang` | none | global ignores |
| `disabled`, `size`, `name` | inherited | `name` in `ignoredAttributes` (from the bound field) |
| `enterkeyhint` | `WaEnterKeyHint? EnterKeyHint` | |
| `hint`, `label`, `with-hint`, `with-label` | inherited from `WaLabeledInputBase` | |
| `inputmode` | `WaInputMode? InputMode` | |
| `max-tags`, `min-tags` | `int? MaxTags`, `int? MinTags` | no CEM default |
| `pill` | `bool Pill` | |
| `placeholder` | `string? Placeholder` | default `""` |
| `readonly`, `required` | `bool Readonly`, `bool Required` | form-control slots 7/8 |
| `spellcheck` | `bool? Spellcheck` | `AddTrueFalseAttribute` (the converter reads `"false"`); `trueFalseAttributes` entry |
| `value` | none (live property) | `unrenderedAttributes` entry, see above |
| `with-clear` | `bool WithClear` | `IWaClearableControl` |

**Events:** `OnFocus`/`OnBlur` bind `focusin`/`focusout` (the focus lands on the internal `<input>` or a
tag's remove button). `OnInput` is bound to `input`, `change` is covered by the value binder, and
`OnClear` goes through `FormControlRendering.AddClearEventHandler`. There is also
`OnCreate` (`EventCallback<WaCreateEventArgs>`, `onwa-create`) and `OnInvalid` (`onwa-invalid`).
`wa-create` and `wa-invalid` are already registered in the JS initializer (`wa-create` is also used by
`wa-combobox`), and `WaCreateEventArgs` (`InputValue`) is reused; its summary becomes generic.
`wa-create` is cancelable upstream (`preventDefault()` rejects the tag). As with the other cancelable
events, .NET can't cancel it, so the callback is notification-only and says so.

Dispatch check: `WaCreateEvent` is `bubbles: true, cancelable: true, composed: true`, `WaClearEvent`
bubbles, and `input`/`change` are dispatched with `bubbles: true, composed: true`. The only
`stopPropagation()` calls are on the inner `wa-tag`'s `wa-remove` and on clear-button mouse
events. None of the element's own events and no keydown is stopped, so **no relay is needed**.

**Slots:** `label`/`hint` (`MarkupLabel`/`MarkupHint`), `start`/`end` (`StartContent`/`EndContent` plus
`StartIconName`/`EndIconName`), and `clear-icon` (`ClearIconContent`).

**Methods:** `FocusAsync(...)`/`BlurAsync()` for `focus`/`blur`. `SetCustomValidityAsync`/`ResetValidityAsync`
are inherited. `formStateRestoreCallback` goes to `ignoredMethods`.

**Not wrapped:** the JS-only `inputValue` property (the typed text that isn't a tag yet). It isn't a CEM
attribute or method.

## Phase 3 — Modified components

Only status changes, so the work is the six XML summaries listed above.

## Phase 4 — Intentional deviations (`parity-config.json`)

`wa-tag-input` component entry, each list item with its own `ignoreReasons` key (as implemented):
- `ignoredAttributes`: `name` (from the bound field), `custom-error` (custom validity is imperative), `value`
  (the live array property instead of the delimiter-separated default). The plan first used
  `unrenderedAttributes` for `value`. Because of the naming convention, that still paired the `Value`
  parameter with the `value` attribute in the list-separator check. The attribute is exposed through no
  parameter, which is what `ignoredAttributes` means.
- `ignoredEvents`: `change` (the value binder)
- `ignoredMethods`: `formStateRestoreCallback` (browser callback)
- `onOffAttributes`: `autocorrect`. There is no `trueFalseAttributes` entry for `spellcheck`: its default is
  `true`, so the CEM-default rule covers it and the entry would be stale.
- `attributeOverrides`: `autocapitalize` → `AutoCapitalize`, `autocorrect` → `AutoCorrect`, `enterkeyhint` →
  `EnterKeyHint`, `inputmode` → `InputMode`. `max-tags`/`min-tags` follow the convention.
- The shared `unreachableEnumUnionValues` `WaSize` entry (deprecated `small`/`medium`/`large`) lists
  `wa-tag-input:size` too.

The empty-list convention follows the `WaSelect`/`WaCombobox` precedent. `Value` is null until the model or
the user sets tags, and it becomes an empty list, not null, once the user removes the last tag. A parsed
empty string binds null.

## Phase 5 — Tests and docs

- `wa-test-engineer`: `WaTagInputIntegrationTests` (slots and icon shortcuts, the value binder reading the
  array, live-property sync on first render and on change, `OnCreate` payload, method guards and recorded
  interop calls) and bUnit EditForm coverage under `Base\` (binding, change propagation, DataAnnotations
  `[MinLength]`/`[Required]` validation lifecycle, custom validity). No breaking-change validation tests, since
  nothing is breaking.
- `ApiParity\WaElementDefaults.cs` refreshed from `ElementDefaultsTableTests`' received file.
- `EventCallbackManifestTests` refresh plus an e2e dispatch/payload case for every new callback
  (`OnCreate`, `OnClear`, `OnInvalid`, value binding).
- CHANGELOG `## [3.13.0]`: New components (`WaTagInput`), Changed (the status promotions), Library (docs
  refresh, demo). There is no `### Breaking changes` heading, because the release is non-breaking.
- Demo: `New-WaDemoPages.ps1 -PruneRemoved`, a curated `TagInputPage.razor` (from
  `inputs\WebAwesome\components\tag-input.md`), a `ComponentCategoryMap` entry (`wa-tag-input` → Forms, per
  the doc's front matter), and the tag input added to the **form showcase** plus its flow spec.
- Public API snapshot promoted (only additions expected).

## Validation checklist

- [ ] `dotnet build src/WebAwesome.slnx -p:Configuration=Debug` and `Release`: 0 warnings, 0 errors
- [ ] `dotnet test src/WebAwesome.slnx` green on net9.0 and net10.0 (baseline 1211 per TFM)
- [ ] `ApiSurfaceParityTests` green (at arming, the only gap must be `wa-tag-input`)
- [ ] Render-based parity (attributes, slots, events, registrations, enum values, defaults) green for `WaTagInput`
- [ ] `PublicApiSnapshotTests` baseline promoted, every difference explained by this plan
- [ ] e2e green on the free CDN (sweep, interaction sweep, showcase flows, event coverage), Pro pass against
      `temp\wa-src\3.13.0`, and the Pro override cleared afterwards

## Risks

- **Array-valued `change`/`input`.** Blazor's built-in change reader forwards `target.value`, which is an
  array here. This works for `wa-select`/`wa-combobox` in `Multiple` mode. The e2e value-binding case
  proves it for `wa-tag-input`.
- **Live-property echo.** The element's `value` getter returns a fresh copy, and the setter ignores an
  identical array. The wrapper keeps a stable array and marks UI-originated values as synced, so a
  round trip doesn't re-push.
- **Experimental upstream API.** `wa-tag-input` is experimental and may change in a later release. The
  parity harness catches that.
