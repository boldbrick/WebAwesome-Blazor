# Web Awesome 3.14.0 to 3.15.0 Upgrade Implementation Plan

JIRA: WAB-71 (epic WAB-70 "Web Awesome 3.15")
Branch: `/main/WA-3.15/WAB-71` (new train subtrunk `/main/WA-3.15`, created off `/main` cs:417; epic folder cs:418)
Source tag: https://github.com/shoelace-style/webawesome/tree/v3.15.0

## Overview

Upgrade the Blazor bindings from Web Awesome 3.14.0 (package 3.14.0.4) to 3.15.0. Upstream this is an additive
release: six new components (five free, one Pro), a declared `disable-sticky` attribute on `wa-page`, a declared
day slot on `wa-date-picker`, and `wa-data-grid`, `wa-step` and `wa-stepper` leave experimental status. Nothing is
removed or renamed upstream.

The bindings make one **breaking** change of their own: the 3.15.0 train removes the members that 3.14.0.1
deprecated (see "Phase 1"). So the CHANGELOG gets a `### Breaking changes` heading and
`docs\migration\MIGRATION-3.15.0.md` is written.

Release gate: `/main` head cs:417 carries `Version.props` 3.14.0.4, merged from the head of `/main/WA-3.14`
(cs:416). No unreleased work is pending on `/main/WA-3.14`; the owner keeps that train open for further 3.14.0.x
revisions.

Change report: `temp\wa-api\changes_3.14.0_to_3.15.0.json` / `.md` (6 added, 0 removed, 21 modified, 0 breaking).

## Analysis Summary

| Metric | Count |
|---|---|
| New components | 6 (`wa-code-highlighter`, `wa-counter`, `wa-lightbox`, `wa-lightbox-item`, `wa-number-ticker`: free; `wa-rich-text-editor`: Pro; all experimental) |
| Removed components | 0 |
| Modified components | 21 (16 only add `cssParts`; 3 status changes; `wa-page` attribute; `wa-date-picker` slot) |
| Breaking changes (report) | 0 |
| Components in CEM | 96 (was 90) |
| Wrapper breaking changes | 6 removed obsolete members (bindings-side, scheduled in 3.14.0.1) |

`cssParts` additions need no wrapper work (the harness checks components, attributes, events, slots and methods).

### Parity worklist at arming (Phase 3)

Armed against 3.15.0, the suite failed 7 tests per TFM, all expected:
- `AllComponents_HaveWrapperClasses` and `AllCemAttributes_AreRenderedByTheirParameter`: the six new components.
- `ElementDefaultsTable_MatchesTheCem`: the new components' defaults (refresh `WaElementDefaults.cs`).
- `AllCemEvents_AreCorroboratedByTheTypeDeclarations`: `wa-rich-text-editor` `type`.
- `RenderedAttributeAllowlists_AreNotStale` and `AllEnumParameters_AreBoundToResolvedUnion`: `wa-page`
  `disable-sticky` is CEM-declared now (typed `string`).
- `SourceVerifiedSlots_AreNotStale`: `wa-date-picker` `day-YYYY-MM-DD` is CEM-declared now.

### Upstream source changes outside the CEM

The `.d.ts` files of about 40 existing components changed. A module-by-module review found no renamed or removed
public methods, no changed method signatures or return types and no changed event detail shapes. Behaviour notes:

| Area | Change | Wrapper impact |
|---|---|---|
| Icons | `setDefaultIconFamily(family, variant?)`; new `getDefaultIconVariant()`; family/variant defaults read from `[data-fa-family]`/`[data-fa-variant]`; the `system` library follows the default family/variant (Pro icon sets) | none; the interop still matches. Additive candidates recorded as follow-ups |
| `wa-button` (submit/reset) | form activation deferred until the click finishes; skipped when the click was `preventDefault`ed | none; worth a CHANGELOG note (`@onclick:preventDefault` on a submit button now cancels the submission) |
| `wa-slider` | values snap to `min + k*step`; precision from the decimals of step and min | none; CHANGELOG note |
| `wa-popover` | `anchor` also accepts a virtual element; ARIA synced on the trigger | none |
| `wa-tooltip` | pointerdown (touch), keyboard-click light dismiss, no open while disabled | none |
| `wa-tree` | handles `wa-lazy-change` for lazy items; re-applies expand/collapse icons | none |
| `wa-page` | `disable-sticky` declared; breakpoint from a width measurer | parity entry change (Phase 3) |
| `wa-tab-group`, `wa-dropdown`, `wa-markdown` | initial selection without scroll; focus return; shared dedent | none |
| Shadow parts | new `color-picker`, `textarea-wrapper`, `date-picker`, `button-group`, `tab-panel` parts (old ones kept); undeclared `tag__base` exportpart dropped on select/combobox (unused here) | none |

### Standing re-verifications (every upgrade)

- **Watch list (function arities).** `dataGridColumnFunctions`, `elementFunctions` and `iconLibraryFunctions` are
  unchanged against the 3.15.0 `.d.ts` files. New function-valued members exist only on new components:
  `wa-counter.valueFormatter(value, unit)` (arity 2), `wa-number-ticker.valueFormatter(value)` (arity 1),
  `wa-rich-text-editor.imageProvider(file?)` and `embedProvider(input)` (arity 1, Promise), and
  `wa-rich-text-editor.suggestions` (an array of source objects with their own functions, like grid columns).
- **`NOTE checked against WA 3.14.0` facts** (WaCopyButton, WaInclude, WaMutationObserver, WaPopover,
  WaRelativeTime, WaTooltip, and the four in `webawesome-interop.js`): all still true; bump them to 3.15.0.
- **`extraElementMethods`**: observers' `stopObserver`/`startObserver` (chunks byte-identical) and
  `wa-relative-time` `requestUpdate`: valid.
- **`sourceVerifiedEvents`**: `wa-color-picker` show/hide family is still dispatched as plain non-bubbling
  `CustomEvent`s and still missing from its JSDoc: the relays stay.
- **`cemOnlyEvents`**: `wa-combobox`/`wa-data-grid` `request` and `wa-stepper` `detail` are still CEM-invented.
  New: `wa-rich-text-editor` `type` (from a synthetic `new FocusEvent(type)`).
- **Event dispatch re-check** (`bubbles: false`, `new CustomEvent(`, `stopPropagation()`): no new non-bubbling or
  stopped events on existing components; `relayedEvents` stays correct. New components: all `wa-*` events bubble
  and are composed. `wa-rich-text-editor` dispatches `selectionchange` as a plain non-bubbling `Event`, synthetic
  non-bubbling `focus`/`blur` on the host, and stops Escape keydown while its menus are open (keydown relay host).
  `wa-lightbox` stops Escape in a document listener, which does not block Blazor.
- **`wa-date-input` day-slot forwarding**: still only in `firstUpdated` and on default-slot changes, so the
  `WaDayContent` nudge stays (owner exception, re-check next upgrade).

### Documentation ingest

`v3.15.0` is tagged in the public repository, so `Sync-WaDocs.ps1 -Version 3.15.0` ran without `-DocsTagVersion`:
162 files, 18 filled from the release zip's bundled references (charts, combobox, data-grid, date-input,
date-picker, file-input, rich-text-editor, sparkline, video, video-playlist), none carried forward, none needing
capture. cs:419: 6 added component docs, 55 changed files. `inputs\WebAwesome\ai\` and `LICENSE.md` are unchanged
(content-identical), so `THIRD-PARTY-NOTICES.md` needs no update and `ai\` does not trigger a package-skill refresh.

### Next-release check items

- 3.14.0.1 "Extensions: Enter on a `Link` in a grid cell raises `OnCellClick`": **carried forward**. The grid's
  keyboard navigation module is byte-identical; Enter still activates the cell, and `<a>` is not in its pass-through
  tag sets.
- 3.14.0.1 "the 3.15.0 train removes the deprecated members": **closed** by Phase 1.
- `wa-zoomable-frame` zoom icons still use the default library (not `library="system"`); `WebAwesomeOptions.IconPath`
  applies. Recorded only.

### Version notes in code

- `TODO WA 3.15.0 train` (6 members + the interop global mutator path): due now, Phase 1.
- `NOTE checked against WA 3.14.0` (10 notes): still true, bumped to 3.15.0 in Phase 4.

## Phase 1 — Breaking changes (bindings-side)

Removal of the members deprecated in 3.14.0.1, with their `TODO` comments, tests and demo/e2e uses:

| Removed | Replacement |
|---|---|
| `WaSelect.GetTag` | `GetTagFunction` |
| `WaSelect.SetGetTagFunctionAsync(string)` | `GetTagFunction` |
| `WaRating.SetSymbolFunctionAsync(string)` | `GetSymbolFunction` |
| `WaSliderBase<TValue>.SetValueFormatterAsync(string)` (`WaSlider`, `WaRange`) | `ValueFormatterFunction` |
| `WaSlider.ValueFormatter` | `ValueFormatterFunction` |
| `IconLibraryOptions.Mutator` (global function path) | `MutatorFunction = WaJsFunction.Global(path)` |

The interop's global-path mutator handling goes too. The demo's icon-library harness keeps a global mutator case via
`WaJsFunction.Global`, so the e2e coverage of global mutators stays.

## Phase 2 — New components

| Component | Notes |
|---|---|
| `WaCodeHighlighter` (`wa-code-highlighter`) | `Code`, `Language`, `Label`, `Wrap`, `WithoutCopy`, `WithoutDedent`; default slot; `OnLoad`, error callback (`wa-error` when no grammar exists). Not wrapped: the global `registerCodeGrammar` (follow-up). |
| `WaCounter` (`wa-counter`) | `Date` (strongly typed instant), `Direction`, `Format`, `LargestUnit`/`SmallestUnit` (new unit enum), `Paused`, `WithoutLabels`, `Delimiter`, `Duration`, `Label`; seven unit slots + `finished`; `OnFinish`; `ValueFormatterFunction` (arity 2). |
| `WaNumberTicker` (`wa-number-ticker`) | Intl number options as on `WaFormatNumber`, `Value`, `StartValue`, `Duration`/`Delay` (ms), `Easing`, `Eager`; `OnFinish`; `RestartAsync`; `ValueFormatterFunction` (arity 1). |
| `WaLightbox` (`wa-lightbox`) | `Open`, `Index` (element-driven), `Loop`, `Label`, `WithThumbnails`, `WithoutCounter`/`Navigation`/`Zoom`; show/hide family (`wa-hide` cancelable upstream, notification-only in .NET); `OnItemChange` with a projected payload; `NextAsync`, `PreviousAsync`, `ZoomInAsync`, `ZoomOutAsync`; icon slots with `IconName` shortcuts, `actions` slot. |
| `WaLightboxItem` (`wa-lightbox-item`) | `Src`, `Srcset`, `Sizes`, `Alt`, `Caption`, `Thumbnail`, `Type`, `Width`/`Height`; `play-icon` slot; `FocusAsync`. |
| `WaRichTextEditor` (`wa-rich-text-editor`, Pro) | Form control (`WaLabeledInputBase<string?>`, like `WaTextArea`): HTML or Markdown value per `Format`; toolbar, heading levels, colors, link and feature options; input/change binding, focus/blur, `wa-invalid`, keydown relay; `OnSuggestionsError` (projected); editing methods (`RunCommandAsync`, `IsActiveAsync`, `InsertHtmlAsync`, `InsertTextAsync`, `UndoAsync`, `RedoAsync`, `GetSelectionAsync`, ...). Provider functions bound where the `elementFunctions` mechanism fits; `suggestions` is a wrapper API gap. |

Engineer groups (parallel): rich text editor alone; ticker + counter + highlighter; lightbox + lightbox item.

## Phase 3 — Modified components

- `WaPage.DisableSticky` already exists (`WaPageSections` flags, rendered since 3.12.0 under an
  `extraRenderedAttributes` entry). Now CEM-declared as `string`: the entry becomes a `tokenListAttributes` entry
  (banner, header, subheader, aside, menu — the JSDoc names three, the styles match all five).
- `WaDatePicker`/`WaDateRangePicker` day slots: already served by `WaDayContent`; the `sourceVerifiedSlots` entry
  is removed now that the CEM declares the slot.
- Status changes: `wa-data-grid`, `wa-step`, `wa-stepper` become stable; wrapper docs that call them experimental
  are corrected (and `WaCombobox`, stable since 3.14 but still described as experimental).

## Phase 4 — Intentional deviations (`parity-config.json`)

- `wa-page`: `tokenListAttributes` `disable-sticky` (replaces `extraRenderedAttributes`), with its reason.
- `wa-date-picker`: `sourceVerifiedSlots` entry and reason removed.
- `wa-rich-text-editor`: `cemOnlyEvents` `type`; `custom-error` and `formStateRestoreCallback` as on the other
  form controls; whatever else the engineers report, each with its own reason.

## Phase 5 — Tests and docs

- `wa-test-engineer`: integration tests for the six new wrappers (slots, icon shortcuts, method guards and recorded
  interop calls, payload handling); bUnit EditForm coverage for `WaRichTextEditor` (binding, change propagation,
  validation lifecycle, custom validity); breaking-change validation for 3.15.0.
- `WaElementDefaults.cs` refreshed; `EventCallbackManifestTests` refresh and e2e dispatch/payload cases for every new
  callback.
- `docs\migration\MIGRATION-3.15.0.md` and the CHANGELOG `## [3.15.0]` section (Breaking changes, New components,
  Changed, Library, Public API).
- Demo: `New-WaDemoPages.ps1 -PruneRemoved`, curated pages for the six components, `ComponentCategoryMap` entries,
  showcase integration (number ticker / counter → dashboard; rich text editor → form showcase; lightbox → content
  showcase; code highlighter → content showcase), flow spec steps.
- Public API snapshots promoted (6 removals + additions); package skills refreshed (public API changed).

## Validation checklist

- [ ] Debug and Release builds, 0 warnings, 0 errors
- [ ] `dotnet test` green on net9.0 and net10.0 (baseline 1457 + 213 per TFM)
- [ ] `ApiSurfaceParityTests` and the render-based parity checks green
- [ ] `PublicApiSnapshotTests` (both packages) promoted; every difference explained
- [ ] Both Release `.nupkg` files contain `docs/`
- [ ] e2e free CDN and Pro (`temp\wa-src\3.15.0`), sweep, interaction sweep, showcase flows, event coverage

## Risks

- **Experimental upstream API.** All six new components are experimental; the parity harness catches changes.
- **Rich text editor value normalization.** The element serializes its value; a bound value can come back
  normalized, which re-renders the wrapper. Same pattern as other live-value controls.
- **Pro-only component.** `wa-rich-text-editor` 404s on the free CDN; it renders only with Pro assets (as
  `WaCombobox`/`WaDataGrid`).

## Follow-ups

- Icons: `SetDefaultIconFamilyAsync` variant argument and `GetDefaultIconVariantAsync` (additive, new upstream API).
- `registerCodeGrammar` interop for `WaCodeHighlighter` custom grammars.
- `wa-rich-text-editor` `suggestions` (array of JS data sources) — wrapper API gap.
- Carried forward: Enter on a link in a grid cell; `wa-date-input` day-slot nudge.
