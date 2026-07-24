# Changelog

All notable changes to the Web Awesome Blazor Bindings. Versions mirror the bound [Web Awesome](https://github.com/shoelace-style/webawesome) release; the format follows [Keep a Changelog](https://keepachangelog.com/).

## [3.7.0] — 2026-07-24

Alignment with the Web Awesome 3.7.0 release, opening the **WA-3.7 train**. An additive upgrade with **no breaking changes**: two new Pro media components (`WaVideo`, `WaVideoPlaylist`) and a new tooltip attribute on `WaCopyButton`. No migration guide is required.

### New components
- `WaVideo` (`wa-video`, Pro/experimental) — embeds and plays video content with custom controls and captions. Exposes the `Controls` (`WaVideoControls`) and `Preload` (`WaVideoPreload`) presets, `Src`/`Poster`/`Thumbnails`/`Title`/`IconLibrary`, the `Playing`/`Muted`/`Autoplay`/`Loop`/`AutoplayMuted`/`AutoplayOnVisible` flags, and `Volume`/`Duration`/`CurrentTime`; the icon slots (`poster-icon`, `play-icon`, `pause-icon`, `volume-icon`, `mute-icon`, `fullscreen-icon`, `exit-fullscreen-icon`) plus `controls-start`/`controls-after-play`; the native media events `OnPlay`/`OnPause`/`OnEnded`/`OnError`/`OnTimeUpdate`/`OnVolumeChange`/`OnLoadedMetadata`; and the playback methods `PlayAsync`/`PauseAsync`/`TogglePlayAsync`/`ToggleMuteAsync`/`SeekAsync`/`SetVolumeAsync`/`SetPlaybackRateAsync`/`RequestFullscreenAsync`/`ExitFullscreenAsync`/`GetStateAsync` (returning a `WaVideoState` record).
- `WaVideoPlaylist` (`wa-video-playlist`, Pro/experimental) — wraps multiple `WaVideo` elements into a playlist with navigation. Exposes `Controls`/`IconLibrary`, the `OnVideoChange` callback (`WaVideoChangeEventArgs`, carrying `PreviousIndex`/`CurrentIndex`/`VideoTitle`), and the `NextAsync`/`PreviousAsync`/`GoToAsync` navigation methods.

### Changed
- `WaCopyButton` gained the `Tooltip` parameter (`WaCopyButtonTooltip` — `Full`/`Copy`/`None`, default `Full`), binding the WA 3.7.0 `tooltip` attribute that controls the built-in tooltip behavior. `wa-copy-button` was also promoted **experimental → stable** upstream (no wrapper API change; the demo badge is data-driven and clears automatically).

### Library
- Versioned reference docs refreshed to the `v3.7.0` tag: 132 public-docs files updated; 16 Pro/reference docs (chart family, combobox, file-input, toast/toast-item, sparkline, and the new `video`/`video-playlist`) filled from the release zip's bundled references; 0 needed manual capture.
- New train WA-3.7: subtrunk `/main/WA-3.7` branched off `/main` (released 3.6.0, cs:185); developed on `/main/WA-3.7/WAB-35`.
- Event delivery: `WaVideo`'s seven media events are re-dispatched by the element as non-bubbling, non-composed events on the host and delivered through Blazor's built-in non-bubbling event registration (no `registerCustomEventType` needed); `wa-video-change` is a real custom event, registered in the JS initializer with a `specialArgs` mapping that projects the marshalable payload (indices + incoming video title) and drops the live DOM node.

### Public API
- Baseline promoted: additions for `WaVideo`, `WaVideoPlaylist`, `WaVideoState`, `WaVideoChangeEventArgs`, the `WaVideoControls`/`WaVideoPreload`/`WaCopyButtonTooltip` enums, and `WaCopyButton.Tooltip`. All additive, no removals; every diff explained by the WA 3.7.0 change report.

### Deviations recorded (parity-config.json)
- `wa-video` `eventOverrides`: `timeupdate` → `OnTimeUpdate`, `volumechange` → `OnVolumeChange`, `loadedmetadata` → `OnLoadedMetadata` (natural .NET PascalCase; mechanical conversion would lowercase the second word), each with a rationale in `ignoreReasons`.
- `wa-video` `ignoredMethods`: `getVideoElement` — returns the live native `HTMLVideoElement`, a DOM node that is not marshalable across Blazor JS interop.

### Next-release check outcomes
- Observer `stopObserver()`/`startObserver()` and `wa-relative-time` `update()`: re-verified against the 3.7.0 sources (`startObserver`/`stopObserver` still private in `mutation-observer.d.ts`/`resize-observer.d.ts`; `update()` still the inherited Lit `ReactiveElement` lifecycle, only `updateTimeout` in `relative-time.d.ts`) — the `extraElementMethods` allowlist stands, verification stamps updated.

## [3.6.0] — 2026-07-24

Alignment with the Web Awesome 3.6.0 release, opening the **WA-3.6 train**. A focused upgrade with **no new or removed components**: an expanded size scale, one new event on `WaNumberInput`, one removed `WaFileInput` slot, and the promotion of the chart family and several Pro components from experimental to stable. See [MIGRATION-3.6.0.md](MIGRATION-3.6.0.md).

### Breaking changes
- `wa-file-input` — slot `file-icon` removed upstream; `WaFileInput.FileIconContent` has been removed. Use `DropzoneContent` for custom dropzone visuals. (The 18 upstream `size` "breaking" entries are non-destructive for the wrappers — see below.)

### New components
- None.

### Changed
- `WaSize` enum gained `ExtraSmall` (`xs`) and `ExtraLarge` (`xl`), exposing the size scale Web Awesome 3.6.0 widened to `xs | s | m | l | xl` across the sized components (button, callout, tag, dropdown, radio/radio-group, toast-item, textarea, and every `WaInputBase<T>` form control). The existing `Small`/`Medium`/`Large` members are unchanged and still render `small`/`medium`/`large` (valid aliases upstream).
- `WaNumberInput` gained the cancelable `OnBeforeInput` callback (`EventCallback<EventArgs>`), bound to the re-dispatched `beforeinput` custom event and registered in the JS event initializer.
- 13 components promoted **experimental → stable** upstream (no wrapper API change): the chart family (`wa-chart`, `wa-bar-chart`, `wa-bubble-chart`, `wa-doughnut-chart`, `wa-line-chart`, `wa-pie-chart`, `wa-polar-area-chart`, `wa-radar-chart`, `wa-scatter-chart`), `wa-sparkline`, `wa-toast`, `wa-toast-item`, `wa-combobox`, `wa-dropdown-item`, `wa-file-input`, `wa-number-input`. The demo's experimental badge is data-driven from the API surface, so it clears automatically.

### Non-breaking upstream "breaking" changes (no wrapper action)
- `size` attribute on 18 components (button, callout, color-picker, combobox, dropdown, file-input, checkbox, input, number-input, radio, radio-group, rating, select, slider, switch, tag, textarea, toast-item): the type union widened to add `xs | s | m | l | xl` and the upstream default renamed `medium` → `m`. The legacy `small | medium | large` values remain valid, and the wrapper omits the attribute when `Size` is unset, so existing usage is unaffected; the new sizes are opt-in via `WaSize.ExtraSmall`/`ExtraLarge`.

### Library
- Versioned reference docs refreshed to the `v3.6.0` tag: 122 public-docs files updated; 14 Pro/reference docs (chart family, combobox, file-input, toast, toast-item, sparkline) filled from the release zip's bundled references; 0 needed manual capture.
- New train WA-3.6: subtrunk `/main/WA-3.6` branched off `/main` (released 3.5.0, cs:170/178); developed on `/main/WA-3.6/WAB-33`.

### Public API
- Baseline promoted: `+WaSize.ExtraSmall`, `+WaSize.ExtraLarge`, `+WaNumberInput.OnBeforeInput`, `−WaFileInput.FileIconContent`. Every diff is explained by the WA 3.6.0 change report.

### Deviations recorded (parity-config.json)
- `wa-number-input` `eventOverrides`: `beforeinput` → `OnBeforeInput` (natural .NET PascalCase; mechanical conversion would yield `OnBeforeinput`), with a rationale in `ignoreReasons`.

### Next-release check outcomes
- Observer `stopObserver()`/`startObserver()` and `wa-relative-time` `update()`: re-verified against the 3.6.0 sources (`startObserver`/`stopObserver` still private in `mutation-observer.d.ts`/`resize-observer.d.ts`; `update()` still the inherited Lit `ReactiveElement` lifecycle, only `updateTimeout` in `relative-time.d.ts`) — the `extraElementMethods` allowlist stands, verification stamps updated.

## [3.5.0] — 2026-07-23

Alignment with the Web Awesome 3.5.0 release, opening the **WA-3.5 train**. An additive upgrade: a new `WaMarkdown` component, a cluster of SSR hydration-hint attributes across several components, a color-picker popup placement, a copy-button trigger slot, and the promotion of `WaRating` into a full form-associated control. **No breaking changes** — the four upstream "breaking" entries in the change report are all non-destructive for the wrappers. No migration guide is required.

### New components
- `WaMarkdown` (`wa-markdown`, free/experimental) — renders markdown as HTML via the upstream Marked integration. The source is supplied through a `Content` string (or `ChildContent`) that the wrapper emits into the required child `<script type="text/markdown">` element; `TabSize` maps to `tab-size`. Exposes `RenderMarkdownAsync()` for re-rendering after the source changes.

### Changed
- `WaButton` gained the SSR hydration-hint attributes `WithStart` (`with-start`) and `WithEnd` (`with-end`).
- `WaColorPicker` gained `Placement` (`placement`, reusing the existing `WaPlacement` enum) — the preferred placement of the popup.
- `WaCopyButton` gained a default-slot `ChildContent` for a custom trigger element (previously only the icon slots were exposed).
- `WaDialog` and `WaDrawer` gained the SSR footer hint `WithFooter` (`with-footer`).
- `WaSlider` gained the SSR hints `WithHint` (`with-hint`) and `WithLabel` (`with-label`).
- `WaTextArea` gained `WithCount` (`with-count`) — shows a character count below the textarea (remaining characters when `MaxLength` is set).
- `WaToastItem` gained the SSR icon hint `WithIcon` (`with-icon`).
- `WaRating` became a full form-associated control upstream: gained `DefaultValue` (`default-value`, the form-reset value) and the `OnInvalid` callback (`wa-invalid`). The `name`/`required` attributes and custom-validity (`setCustomValidity`/`resetValidity`) are already provided by the shared `WaInputBase<decimal>` base; `wa-invalid` was already registered in the JS event initializer.

### Non-breaking upstream "breaking" changes (no wrapper action)
- `wa-rating` `change` event: the CEM `type` metadata changed (now null); the event is unchanged and remains folded into the two-way `@bind-Value` binding.
- `wa-rating` `blur`/`focus` methods removed from the CEM: the underlying native `HTMLElement.blur()`/`focus()` remain, so `WaRating.BlurAsync()`/`FocusAsync()` stand (covered by the global `nativeElementMethods` allowlist).
- `wa-textarea` `autocorrect`: the JS property type widened from `string` to `boolean`; the **attribute** form is still `"off"`/`"on"`, so `WaTextArea.AutoCorrect` (`string?`, rendered as the attribute) is unchanged — the same widening handled for `wa-input`/`wa-combobox` in 3.4.0.

### Demo
- All component pages added since 3.0.0 are now fully curated (previously TODO skeletons): the chart family (9 pages), `toast`/`toast-item`, `markdown`, `sparkline`, `number-input`, `file-input` — examples translated from the upstream docs. Showcases extended with the new components: dashboard (issue-burndown line chart, doughnut workstream chart, MRR sparkline), overlays (toast notifications), content (markdown block), registration form (file input).
- Pro badges corrected: the 3.2.0/3.3.0 Pro components (chart family, `sparkline`, `file-input`, `toast`, `toast-item`) now carry the Pro flag, and `wa-page` — free/stable upstream as of this release — no longer does. Pro-ness is now derived automatically by `Export-WaApiSurface.ps1` from the reference docs bundled in the release zip (`[Pro]` heading marker, available since 3.3.0); `tools\upgrade\pro-components.json` remains only as the pre-3.3.0 fallback. Wrapper XML docs updated to match (Pro remark added to the 14 Pro wrappers, removed from `WaPage`).
- Every component page's API Reference now links the canonical upstream documentation (`webawesome.com/docs/components/<name>`), rendered by the shared `ApiTable` component; skeleton pages generated by `tools\demo\New-WaDemoPages.ps1` now include `ComponentBadges` in the heading, so future pages get both automatically.
- Sidebar reorganized to the current webawesome.com docs taxonomy: Actions, Forms, Layout, Navigation, Feedback, Media, **Data Viz** (the chart family and sparkline), Helpers — replacing the pre-3.0 category names (Form Controls, Imagery, Organization, Utilities, Feedback & Status) under which none of the post-3.0 components had been categorized. The CSS layout section is now titled "Layout Utilities" to distinguish it from the Layout component category.

### Library
- Versioned reference docs refreshed to the `v3.5.0` tag: the public GitHub docs tree replaces `inputs\WebAwesome` (including the new `markdown.md`); the 14 Pro/non-GitHub docs (charts family, combobox, file-input, toast, toast-item) were carried forward and re-stamped, with `toast-item.md` updated for the new `with-icon` attribute.
- New train WA-3.5: subtrunk `/main/WA-3.5` branched off `/main` (released 3.4.0, cs:160); developed on `/main/WA-3.5/WAB-30`.

### Public API
- Baseline promoted: additions for `WaMarkdown` (+ its members), `WaButton.WithStart`/`WithEnd`, `WaColorPicker.Placement`, `WaCopyButton.ChildContent`, `WaDialog.WithFooter`, `WaDrawer.WithFooter`, `WaRating.DefaultValue`/`OnInvalid`, `WaSlider.WithHint`/`WithLabel`, `WaTextArea.WithCount`, `WaToastItem.WithIcon`. All additive, no removals; every diff explained by the WA 3.5.0 change report.

### Deviations recorded (parity-config.json)
- `wa-rating`: `name`/`custom-error` added to `ignoredAttributes`, `formStateRestoreCallback` to `ignoredMethods` (same rationale as the other form controls, now that rating is form-associated).
- `wa-markdown`: `getMarked`/`updateAll` added to `ignoredMethods` — both are static class-level methods (verified in `markdown.d.ts`), not per-element instance methods, and `getMarked` returns a non-marshalable JS `Marked` object.

### Next-release check outcomes (carried from 3.4.0)
- Observer `stopObserver()`/`startObserver()` method names and `wa-relative-time` `update()`: re-verified against the 3.5.0 sources (`stopObserver`/`startObserver` still private members in `mutation-observer.d.ts`/`resize-observer.d.ts`; `update()` still the inherited Lit `ReactiveElement` lifecycle method, only `updateTimeout` appears in `relative-time.d.ts`) — the allowlist stands. `wa-page` `visiblePixelsInViewport` still present.
- `WaButton.Form` (CEM-invisible since 3.1.0) and the `WaCheckbox`/`WaSwitch` `.checked` binding workaround: unaffected by WA 3.5.0, carried forward unchanged.

## [3.4.0] — 2026-07-23

Alignment with the Web Awesome 3.4.0 release, opening the **WA-3.4 train**. A small additive upgrade: new text-input attributes and a create event on `WaCombobox`, and a theme-sync attribute on `WaZoomableFrame`. **No breaking changes** — the four upstream "breaking" entries in the change report are all non-destructive for the wrappers (two are type widenings that leave the existing parameter functional, two are removals of attributes the wrappers only inherited from the shared `WaInputBase<TValue>` base). No migration guide is required.

### Changed
- `WaCombobox` gained the text-input attributes WA 3.4.0 added to `wa-combobox`: `AllowCreate` (`allow-create`), `AutoCapitalize`, `AutoCorrect`, `EnterKeyHint`, `InputMode` (idiomatic .NET casing, mapped via `attributeOverrides`) and `Spellcheck`. `AutoCorrect` is exposed as `string?` (attribute form `"off"`/`"on"`), matching `WaInput.AutoCorrect`, even though the CEM now types the JS property as `boolean`.
- `WaCombobox` gained the cancelable `wa-create` event as `OnCreate` (`EventCallback<WaCreateEventArgs>`); the new `WaCreateEventArgs` carries the typed `InputValue`. Registered in the JS event initializer (JSON-safe string detail, default payload).
- `WaZoomableFrame` gained `WithThemeSync` (`with-theme-sync`), enabling automatic light/dark and theme-selector-class syncing from the host document into the iframe.

### Non-breaking upstream "breaking" changes (no wrapper action)
- `wa-color-picker` `swatches`: the type was widened to also accept an array of `{ color, label }` objects. The semicolon-separated string attribute is unchanged; `WaColorPicker.Swatches` (string) and `SetSwatchesAsync(string[])` stand. The labeled-object form remains a JS-only capability, not marshaled from Blazor.
- `wa-input` `autocorrect`: the JS property type changed from `'off' | 'on'` to `boolean`; the **attribute** form is still `"off"`/`"on"`, so `WaInput.AutoCorrect` (`string?`, rendered as the attribute) is unchanged.
- `wa-combobox` `autocomplete` removed and `wa-slider` `required` removed: neither was a component-specific wrapper parameter; both are inherited from `WaInputBase<TValue>` (still valid on other form controls) and now render only as inert passthroughs on those two elements when explicitly set. No wrapper public API was removed.

### Library
- Versioned reference docs refreshed to the `v3.4.0` tag: 35 public-docs files changed upstream (re-downloaded); the Pro `combobox.md` already documented the 3.4.0 additions (webawesome.com tracks latest) — its header was re-stamped for 3.4.0.
- New train WA-3.4: subtrunk `/main/WA-3.4` branched off `/main` (released 3.3.1); developed on `/main/WA-3.4/WAB-28`.

### Public API
- Baseline promoted: additions for the six new `WaCombobox` parameters, `WaCombobox.OnCreate`, the new `WaCreateEventArgs` class, and `WaZoomableFrame.WithThemeSync`. All additions, no removals; every diff is explained by the WA 3.4.0 change report.

### Deviations recorded (parity-config.json)
- `wa-combobox` `attributeOverrides` for `autocapitalize`/`autocorrect`/`enterkeyhint`/`inputmode` (idiomatic .NET casing, mirroring the existing `wa-input` overrides).

### Next-release check outcomes (carried from 3.3.1)
- Observer `stopObserver()`/`startObserver()` method names and `wa-relative-time` `update()`: re-verified against the 3.4.0 sources (`stopObserver`/`startObserver` still private members in `mutation-observer.d.ts`/`resize-observer.d.ts`; `update()` still the inherited Lit `ReactiveElement` lifecycle method, only `updateTimeout` appears in `relative-time.d.ts`) — the allowlist stands.
- `WaButton.Form` (CEM-invisible since 3.1.0) and the `WaCheckbox`/`WaSwitch` `.checked` binding workaround: unaffected by WA 3.4.0, carried forward unchanged.

## [3.3.1] — 2026-07-23

Alignment with the Web Awesome 3.3.1 release. A pure version-alignment patch: no wrapper code changes, no new/removed/modified components, no breaking changes. The CEM API surface is byte-identical to 3.3.0 apart from the version string. Developed on the WA-3.3 train subtrunk (patch release — `/main` is not involved).

### Changed
- Bound Web Awesome version bumped to 3.3.1 (library version, README alignment/CDN references, demo asset version tracks the library version structurally). Upstream 3.3.1 is a packaging fix that removes a `preinstall` script in `webawesome-pro` which was causing issues in some package managers — no effect on the public component API.

### Library
- Versioned reference docs refreshed to the `v3.3.1` tag: only `resources/changelog.md` changed upstream (the new 3.3.1 release note); no component doc, attribute, event, slot, method, or enum changes.

### Public API
- No change. The public API snapshot (`approved-public-api.txt`) is unchanged — there are no wrapper source changes to promote.

### Next-release check outcomes (carried from 3.3.0)
- Observer `stopObserver()`/`startObserver()` method names and `wa-relative-time` `update()`: re-verified against the 3.3.1 sources (`stopObserver`/`startObserver` still private members in `mutation-observer.d.ts`/`resize-observer.d.ts`; `update()` still the inherited Lit `ReactiveElement` lifecycle method, only `updateTimeout` appears in `relative-time.d.ts`) — the allowlist and deviations stand.
- `WaButton.Form` (CEM-invisible since 3.1.0) and the `WaCheckbox`/`WaSwitch` `.checked` binding workaround: unaffected by WA 3.3.1, carried forward unchanged.

## [3.3.0] — 2026-07-23

Alignment with the Web Awesome 3.3.0 release. A substantial but additive upgrade: eleven new components (a nine-member Chart.js chart family plus `WaToast`/`WaToastItem`), a new form-control validity-reset method, and new start/end slots on `WaBadge`. **No breaking changes** — the eight upstream "breaking" entries in the change report are all non-destructive (a `string` → `string | null` nullability annotation on the `name` attribute of five form controls, already covered by the wrappers, and three empty/`null` "removed" array entries that are CEM-export artifacts, not real removals). No migration guide is required.

### New components
- **Chart family** (all new in WA 3.3.0, experimental upstream): `WaChart` (the flexible Chart.js wrapper) plus eight presets — `WaBarChart`, `WaBubbleChart`, `WaDoughnutChart`, `WaLineChart`, `WaPieChart`, `WaPolarAreaChart`, `WaRadarChart`, `WaScatterChart`. Implemented over a shared abstract `WaChartBase` (each preset supplies its own tag; `WaBarChart` adds `Orientation`). Common surface: `Label`, `Description`, `Type` (`WaChartType`), `Grid` (`WaChartGrid`), `IndexAxis` (`WaChartAxis`), `LegendPosition` (`WaChartLegendPosition`), `Min`/`Max` (`double?`, invariant-culture emission), `Stacked`, `WithoutAnimation`/`WithoutLegend`/`WithoutTooltip`, `XLabel`/`YLabel`, and the default slot for the `<script type="application/json">` Chart.js config. The Chart.js `plugins` array is a JS-only property (not an authorable attribute) and is recorded as a parity deviation.
- `WaToast` (new in WA 3.3.0, experimental upstream): a toast stack (`ComponentBase`). `Placement` (`WaToastPlacement`), default slot for `WaToastItem` children, and `CreateAsync(message)` for programmatic notifications.
- `WaToastItem` (new in WA 3.3.0, experimental upstream): an individual notification (`ComponentBase`). `Duration` (ms, `int?`), `Size` (`WaSize`), `Variant` (`WaVariant`); default + `icon` slots (icon-slot convenience via `IconName`); `OnShow`/`OnAfterShow`/`OnHide`/`OnAfterHide` events; `HideAsync()`. All four events were already registered in the JS initializer — no interop change was needed.

### Changed
- Form controls gained `ResetValidityAsync()` (WA 3.3.0 added `resetValidity` to the form-associated custom-element validation surface). Added to `IFormValidation` and implemented once on `WaInputBase<TValue>` (covering `WaCheckbox`, `WaColorPicker`, `WaCombobox`, `WaInput`, `WaNumberInput`, `WaRadioGroup`, `WaSelect`, `WaSlider`, `WaSwitch`) plus the four non-`WaInputBase` controls (`WaButton`, `WaFileInput`, `WaRadio`, `WaTextArea`).
- `setCustomValidity` became CEM-documented for the form controls in WA 3.3.0; the existing `SetCustomValidityAsync` wrappers now satisfy method parity directly (no code change).
- `WaBadge` gained `start` and `end` slots (both documented as accepting a `wa-icon`): new `StartContent`/`EndContent` render fragments with `StartIconName`/`EndIconName` icon-slot convenience parameters.
- `WaFileInput` gained a `Disabled` parameter (`wa-file-input` added the `disabled` attribute upstream).

### Library
- Versioned reference docs refreshed to the `v3.3.0` tag (the public GitHub docs tree; the 15 CEM components with no public doc page — the nine charts, `toast`, `toast-item`, and the Pro `combobox`/`page`/`file-input`/`sparkline` — were captured from the public web docs at `webawesome.com`, source URL noted at the top of each file).
- Demo: skeleton pages generated for the eleven new components (TODO-marked; showcase curation for the chart/toast components is deliberate follow-up). No components were removed.

### Public API
- Baseline promoted: additions for the eleven new wrappers, `WaChartBase`, the new enums (`WaChartType`, `WaChartGrid`, `WaChartAxis`, `WaChartLegendPosition`, `WaToastPlacement`) and their `ToHtmlValue` extensions, `ResetValidityAsync` on `IFormValidation` and every form control, `WaBadge` start/end slot parameters, and `WaFileInput.Disabled`. All additions, no removals; every diff is explained by the WA 3.3.0 change report.

### Deviations recorded (parity-config.json)
- Global `dir`/`lang`/`did-ssr` (added to every component's base element in WA 3.3.0) are ignored: `dir`/`lang` are native HTML globals passed through via `AdditionalAttributes`; `did-ssr` is an internal SSR hydration marker.
- Per form control: `custom-error` (managed via `SetCustomValidityAsync`/`ResetValidityAsync`, not an authorable attribute) and `formStateRestoreCallback` (a FACE lifecycle callback, not consumer-facing) are ignored; `name` remains form-integration-managed.
- Per chart: `plugins` (JS-only `never[]` property) is ignored.

### Next-release check outcomes (carried from 3.2.1)
- Observer `stopObserver()`/`startObserver()` method names and `wa-relative-time` `update()`: re-verified against the 3.3.0 sources (`stopObserver`/`startObserver` still private members in `mutation-observer.d.ts`/`resize-observer.d.ts`; `update()` still the inherited Lit `ReactiveElement` lifecycle method, only `updateTimeout` appears in `relative-time.d.ts`) — the allowlist stands.
- `WaButton.Form` (CEM-invisible since 3.1.0) and the `WaCheckbox`/`WaSwitch` `.checked` binding workaround: unaffected by WA 3.3.0, carried forward unchanged.

## [3.2.1] — 2026-07-23

Alignment with the Web Awesome 3.2.1 release. A pure version-alignment patch: no wrapper code changes, no new/removed components, no breaking changes. The CEM API surface is byte-identical to 3.2.0 apart from the version string.

### Changed
- Bound Web Awesome version bumped to 3.2.1 (library version, README alignment/CDN references, demo asset version tracks the library version structurally). Upstream 3.2.1 is a build-script fix so `llms.txt` and `dist/skills` are no longer omitted from the Web Awesome **Pro** packages ([pr:2022]) — no effect on the public component API.

### Library
- Versioned reference docs refreshed to the `v3.2.1` tag: only `components/animation.md` and `components/popup.md` changed upstream, and both are documentation-example edits (the interactive demos now drive their sandboxes with `wa-combobox` instead of `wa-select`) — no attribute/event/slot/method/enum changes.

### Public API
- No change. The public API snapshot (`approved-public-api.txt`) is unchanged — there are no wrapper source changes to promote.

### Next-release check outcomes (carried from 3.2.0)
- Observer `stopObserver()`/`startObserver()` method names, `wa-relative-time` `update()`, and `wa-page` `visiblePixelsInViewport(element)`: re-verified against the 3.2.1 sources (`stopObserver`/`startObserver` still private members in `mutation-observer.d.ts`/`resize-observer.d.ts`; `update()` still the inherited Lit `ReactiveElement` lifecycle method; `visiblePixelsInViewport` still present in `page.d.ts`) — the allowlist and deviations stand.

## [3.2.0] — 2026-07-23

Alignment with the Web Awesome 3.2.0 release. See [MIGRATION-3.2.0.md](MIGRATION-3.2.0.md) for the migration guide. A small, mostly additive upgrade: three new components, three new `WaIcon` attributes, and one behavioral breaking change to `WaQrCode` colors.

### Breaking changes
- `WaQrCode.Fill` and `WaQrCode.Background` changed from non-nullable `string` (defaulting to `"black"`/`"white"`, always emitted) to nullable `string?` (defaulting to `null`, emitted only when set). WA 3.2.0 changed both `wa-qr-code` attribute defaults from `'black'`/`'white'` to `''`, so an unset QR code now inherits theme colors instead of being forced to black-on-white. Set `Fill`/`Background` explicitly to keep the old look. (Upstream flags this as a breaking attribute change; it is the only one of the four upstream "breaking" default changes that alters the wrapper's public surface.)
- Not breaking for the bindings: WA 3.2.0 also changed the `size` default of `wa-radio` and `wa-radio-group` from `'medium'` to unset. The wrappers already exposed `Size` as nullable `WaSize?` with no forced default, so they already matched the new behavior — no signature or emission change.

### New components
- `WaSparkline` (new in WA 3.2.0, experimental upstream): compact inline trend chart (`ComponentBase`). `Appearance` (`WaSparklineAppearance`), `Curve` (`WaSparklineCurve`), `Data` (space-separated numbers), `Label`, `Trend` (`WaSparklineTrend`). No events, slots, or methods.
- `WaNumberInput` (new in WA 3.2.0, experimental upstream): numeric input form control, `WaInputBase<decimal?>` with full `@bind-Value`/`EditForm` integration (invariant-culture parse). `Appearance`, `Min`/`Max`/`Step`, `Pill`, `Placeholder`, `AutoFocus`, `InputMode`, `EnterKeyHint`, `WithoutSteppers`, `WithHint`/`WithLabel`; start/end/label/hint/increment-icon/decrement-icon slots; `OnInvalid`; `FocusAsync`/`BlurAsync`/`SelectAsync`/`StepUpAsync`/`StepDownAsync`.
- `WaFileInput` (new in WA 3.2.0, experimental upstream): file-selection form control with a click/drag-and-drop dropzone. Modeled as `ComponentBase, IFormValidation` rather than `WaInputBase` because the CEM exposes no bindable scalar `value` (selected files are runtime `File` objects). `Accept`, `Multiple`, `Required`, `Size`, `Label`/`Hint`; dropzone/file-icon/label/hint slots; `OnChange`/`OnInput`/`OnFocus`/`OnBlur`/`OnInvalid`; `FocusAsync`/`BlurAsync`; `SetCustomValidityAsync`. Read files via the change/input events plus JS interop. The bound `wa-invalid` event was already registered in the JS initializer; no interop changes were needed.

### Changed
- `WaIcon` gains three WA 3.2.0 attributes: `Animation` (new `WaIconAnimation` enum — Beat, Fade, BeatFade, Bounce, Flip, Shake, Spin, SpinPulse, SpinReverse), `Flip` (new `WaFlip` enum — X, Y, Both), and `Rotate` (degrees, `int`).
- `wa-progress-ring` added the `indicator` and `track` CSS parts upstream. CSS parts are not part of the C# wrapper surface (the parity suite tracks attributes/events/methods), so this is styling-only with no wrapper change. (The change report's Markdown rendering showed spurious `.NET` array-property entries for this component — a report-generator display artifact; the JSON change report is authoritative.)

### Fixed
- Demo (server host): `WebAwesome.Blazor.Demo.Server\App.razor` rendered `<WebAwesomeAssets />` statically in its head *and* received the shared demo `App.razor`'s copy through `HeadContent`/`HeadOutlet`, duplicating the `webawesome.css` link and loader script tags (caught by the e2e `component-badges` asset-tag test, which only ran against the WASM host until now). The static instance was removed — both hosts now rely solely on the documented `HeadContent` pattern. Because the loader now arrives with the interactive render, the click-driven e2e tests gained a `waitForWaReady` helper (`tools\e2e\tests\helpers\wa-ready.js`) that awaits custom-element definition + first render before interacting; a pre-upgrade click is silently ignored (a human just clicks again, a test must wait).

### Public API
- Baseline promoted: additions for `WaFileInput`, `WaNumberInput`, `WaSparkline` (+ their enums and `ToHtmlValue` extensions), `WaIcon.Animation`/`Flip`/`Rotate`, and the `WaQrCode.Fill`/`Background` nullability change. Every diff is explained by the WA 3.2.0 change report.

### Next-release check outcomes (carried from 3.1.0)
- Observer `stopObserver()`/`startObserver()` method names and `wa-relative-time` `update()`: re-verified against the 3.2.0 sources (`stopObserver`/`startObserver` still private members in `mutation-observer.d.ts`/`resize-observer.d.ts`; `update()` still the inherited Lit `ReactiveElement` lifecycle method, only `updateTimeout` appears in `relative-time.d.ts`) — the allowlist stands.
- `WaButton.Form` (CEM-invisible since 3.1.0) and the `WaCheckbox`/`WaSwitch` `.checked` binding workaround: unaffected by WA 3.2.0, carried forward unchanged.

## [3.1.0] — 2026-07-22

Alignment with the Web Awesome 3.1.0 release. See [MIGRATION-3.1.0.md](MIGRATION-3.1.0.md) for the migration guide.

### Breaking changes
- Removed `WaButtonGroup.Variant` — the `variant` attribute left the `wa-button-group` CEM in WA 3.1.0. Verified upstream (issue #1677, maintainer response): the group-level variant functionality had already been dropped earlier and the property lingered by mistake; the supported way is setting the variant on each button, which also allows mixing variants within a group.

### New components
- `WaCombobox` (Pro, experimental upstream, new in WA 3.1.0): filterable single/multi-select form control modeled on `WaSelect` — `WaInputBase<string?>` with `WaOption` children, `Multiple` + `SelectedValues`/`SelectedValuesChanged`, `AllowCustomValue`, `MaxOptionsVisible`, `WithClear`, `Pill`, `Appearance`, `Placement`, `Open`, start/end/clear-icon/expand-icon slots, `OnClear`/`OnShow`/`OnHide`/`OnAfterShow`/`OnAfterHide`/`OnInvalid`, and `ShowAsync`/`HideAsync`/`FocusAsync`/`BlurAsync`. All six `wa-*` events were already registered in the JS initializer; no interop changes were needed.

### Changed
- `WaButton.Form` is **kept** although the `form` attribute left the 3.1.0 CEM: upstream PR #1815 moved form association to the native platform mechanism (`ElementInternals`; `el.form` now returns the real `HTMLFormElement`), so the `form="id"` content attribute remains fully functional — it just stopped being a declared Lit property. The wrapper parameter renders the attribute as before and composes with `EditForm`: an external submit button triggers the form's `submit` event, which `EditForm` intercepts as usual. The other form controls still intentionally expose no `Form` parameter (their form story goes through `InputBase`/`EditContext`). **Next-release check:** the attribute is now CEM-invisible, so the parity suite cannot flag upstream changes to it — re-verify `WebAwesomeFormAssociatedElement` still supports `form="id"` on each upgrade; also re-assess against WA's evolving SSR support (beta/experimental in newer versions), where Blazor static-SSR form handling differs from the interactive `onsubmit` interception path.
- `WaCard.Appearance` now accepts `WaAppearance.OutlinedFilled` upstream — WA 3.1.0 added `filled-outlined` to the card appearance union; the enum already emitted the token, so this is upstream catching up, not a wrapper change.
- `wa-page`'s new `visiblePixelsInViewport(element)` method is intentionally not wrapped — it is an internal scroll-gap layout helper taking a live DOM element, not marshalable from Blazor (recorded as a parity deviation).

### Fixed
- `WebAwesomeAssets` now absolutizes app-base-relative asset URLs against the application base URI. Web Awesome's autoloader derives its component base path from the loader script's raw `src` attribute; a relative value (typical for self-hosted setups, e.g. `BasePath: "webawesome"`) produced a bare dynamic-import specifier that fails silently — styles loaded, but **no component ever upgraded**. Found live while verifying the Pro self-hosted asset path; guarded by the e2e spec `tools\e2e\tests\pro-assets.spec.js`. Note the self-hosted folder must be the `dist-cdn` build — the npm-style `dist` build's loader only re-exports and never starts the autoloader.

### Library extended
- Demo: Pro and experimental components are marked like in the official docs — an orange `Pro` pill and a flask icon, in the navigation and on page headers. Data-driven: `tools\upgrade\pro-components.json` (curated per upgrade; the CEM carries no Pro marker) is stamped into the API surface by `Export-WaApiSurface.ps1` as `"pro": true`, and the upstream `status` field drives the flask.
- Demo: Web Awesome asset tags are no longer hard-coded in the hosts — `WebAwesomeAssets` emits them from configuration in both the WebAssembly and server host (this also removed a stale 3.0.0 CDN pin the version-sync had missed in `Demo.Server\App.razor`; the version now defaults to the library version structurally). A Web Awesome **Pro** license can be exercised locally or in CI without committing anything: `tools\demo\Set-WaProAssets.ps1` generates an ignored `appsettings.Local.json` from `WA_PRO_DIST` (self-hosted `dist-cdn`, offline), `WA_PRO_CDN_BASE` (kit base URL with a `{version}` placeholder — the library substitutes its bound WA version, so the value is version-independent), or explicit `WA_PRO_STYLESHEET_URL`/`WA_PRO_LOADER_URL`; `demo.yml` injects `WA_PRO_CDN_BASE` from the repository variable of the same name for the published Pages demo. The release preflight gained a leak gate (override artifacts must not be versioned, no kit-like URLs in sources, ignore rules present in both `ignore.conf` and `.gitignore`).

### Next-release check outcomes (carried from 3.0.0)
- `WaCheckbox`/`WaSwitch` `.checked` two-way-binding workaround: carried forward unchanged — Blazor-runtime behavior, unaffected by WA 3.1.0.
- Removed `initialize()` calls: still correct — no WA 3.1.0 element reintroduces an explicit init step.
- Observer `stopObserver()`/`startObserver()` method names and `wa-relative-time` `update()`: re-verified against the 3.1.0 sources — still present; the allowlist stands.

## [3.0.0] — 2026-07-22

Alignment with the Web Awesome 3.0.0 stable release, plus the browser-verified correctness sweep that made every `wa-*` event and imperative method actually work. See [MIGRATION-3.0.0.md](MIGRATION-3.0.0.md) for the migration guide.

### Breaking changes
- `appearance` combined token renamed by WA 3.0.0: `WaAppearance.OutlinedFilled` now emits `filled-outlined` (was `outlined filled`) across `WaBadge`, `WaButton`, `WaCallout`, `WaDetails`, `WaTag`; `WaInputAppearance` gains the new `FilledOutlined` value for `WaInput`, `WaSelect`, `WaTextArea`. No C# signature change — only the emitted attribute token.
- `WaInclude`: `OnError` renamed to `OnIncludeError`, bound to the renamed upstream event `wa-include-error` (was `wa-error`).
- Removed `WaTabGroup.OnTabClose` and `WaTabCloseEventArgs` — `wa-tab-close` does not exist in Web Awesome 3.0 (and no event ever fired before this release, see Fixed below).
- Removed `WaSlideChangeEventArgs.Slide` and `WaIntersectionEventArgs.Target` — `ElementReference` properties that can never be populated from an event payload.
- Removed never-functional imperative methods (their JS counterparts do not exist on the elements): `WaDropdown.RepositionAsync`, `WaPopover.RepositionAsync`, `WaTooltip.RepositionAsync`, `WaInclude.ReloadAsync`, `WaMutationObserver.TakeRecordsAsync`.

### New components
- `WaPage` (Pro, new in WA 3.0.0): full slot surface (15 slots), navigation state parameters, `ShowNavigationAsync`/`HideNavigationAsync`/`ToggleNavigationAsync`.
- `WaComparison`: pre-existing WA component whose wrapper was missing — gap surfaced by the armed parity suite and closed.
- `WaScroller`, `WaTree`, `WaTreeItem`: implemented against the bound API surface (`wa-tree`/`wa-tree-item` since WA 2.0, `wa-scroller` since 3.0). `WaTree.Selection` uses the new `WaTreeSelection` enum (Single/Multiple/Leaf). `WaTreeItem.getChildrenItems()` and the full item list of `wa-selection-change` are intentionally not exposed as typed element references — arbitrary DOM elements returned from JS cannot become live Blazor `ElementReference`s; `WaTreeSelectionChangeEventArgs.Selection` stays `object[]`, consistent with `MutationEventArgs`/`ResizeEventArgs`.

### Fixed
- `WaCheckbox`/`WaSwitch`: two-way `@bind-Value` silently never updated on user interaction. Root cause: Blazor's built-in change-event value extraction only reads the DOM `.checked` property when the target's `tagName === "INPUT"`; for the custom elements `<wa-checkbox>`/`<wa-switch>` it falls back to reading `.value`, which the wrapper hard-codes to the constant string `"True"` for form-submission purposes (mirroring native checkbox `value` semantics) — so the bound value never reflected the real checked state. **Workaround applied:** replaced the `EventCallback.Factory.CreateBinder<bool>` `onchange` wiring with a manual handler that reads the true state via `WebAwesomeJSInterop.GetPropertyAsync<bool>(Element, "checked")`. **Next-release check:** verify whether the upstream Blazor runtime (or a future WA release exposing these as form-associated custom elements recognized by `ElementInternals`) changes this; if Blazor ever special-cases arbitrary form-associated custom elements for `.checked` extraction, this workaround can likely be removed.
- Ten wrapper components (`WaDialog`, `WaDrawer`, `WaDropdown`, `WaInclude`, `WaMutationObserver`, `WaPopover`, `WaRelativeTime`, `WaResizeObserver`, `WaZoomableFrame`, `WaTooltip`) called a JS method named `"initialize"` in `OnAfterRenderAsync` that does not exist on any of the corresponding Web Awesome custom elements (confirmed against the actual 3.0.0-beta.6 CDN sources) — this threw `InvalidOperationException`/`JSException` on first render of every one of these components. **Workaround applied:** removed the dead `initialize()` calls entirely; these custom elements self-initialize via their own `connectedCallback`, no explicit interop call is needed. **Next-release check:** re-run the demo app's automated Playwright sweep (see below) against the new version before assuming this stays fixed — if a future WA release genuinely adds a required explicit init step for any of these elements, this area regresses silently otherwise.
- `WaResizeObserver.DisconnectAsync`/`ReconnectAsync` and `WaMutationObserver.DisconnectAsync`/`ReconnectAsync` invoked nonexistent JS methods `"disconnect"`/`"reconnect"`. **Workaround applied:** corrected to the real methods `stopObserver()`/`startObserver()` (confirmed from source). **Next-release check:** re-verify these method names against the target CEM/source; they are not part of the public CEM-documented `methods` list (both observers report empty `methods` in the API surface), so `Compare-WaApiSurface.ps1` cannot flag a rename here automatically — a manual source check is required each upgrade.
- Demo: theme switcher (`MainLayout.razor`) swapped the `<link>` stylesheet for the "Awesome"/"Shoelace" themes but never applied the required `wa-theme-{name}` class to `<html>` — Web Awesome's theme CSS scopes every rule under `.wa-theme-{name}` (confirmed against the actual theme CSS from the CDN), so the stylesheet swap alone had no visible effect. Fixed in `wwwroot\index.html` (`demoTheme.setTheme` now also toggles the class) and `MainLayout.razor` (passes the theme name through).
- Demo: `WaSelect`'s "Multiple Selection" example used only `SelectedValues`/`SelectedValuesChanged` without `@bind-Value`, which `InputBase<T>` still requires (throws `ValueExpression` is required even when the base `Value` isn't otherwise used) — added a dummy `@bind-Value`.
- Demo: several `Pages\Components\*.razor` skeleton/curated pages used bare component usages without `@bind-Value` (throws for every `WaInputBase<T>`-derived control), rendered nothing (container components with no slotted content), or used invalid CSS custom properties (e.g. `--wa-color-blue-fill-loud`, which doesn't exist — only the five semantic groups brand/success/neutral/warning/danger have `-fill-loud` roles). See per-page fixes across this session.

### Fixed — wa-* custom events never reached .NET at all (investigated and resolved)
The previously flagged "suspected gap" around custom event binding was live-browser-verified (Playwright, synthetic `CustomEvent` dispatch at bound elements) and turned out to be worse than suspected: **no `wa-*` event callback in the entire library ever fired.** Two independent root causes, both silent:
1. Every event was bound under a bare `wa-...` attribute name (e.g. `AddAttributeIfHasDelegate(7, "wa-tab-change", OnTabChange)`); Blazor only wires event listeners for attribute names carrying the `on` prefix, and silently ignores delegate-valued attributes without it.
2. Even with the prefix, Blazor delivers custom (non-standard) DOM events only after `Blazor.registerCustomEventType(name, { createEventArgs })` registers them; nothing did.

**Fix applied:**
- All event bindings renamed to `onwa-...` across every wrapper.
- New Blazor JS initializer `wwwroot\WebAwesome.Blazor.lib.module.js` (auto-loaded by the runtime from `_content/WebAwesome.Blazor/`) registers all 38 bound `wa-*` event types with `createEventArgs` mappings that sanitize `event.detail` into JSON-safe payloads (DOM nodes dropped). Special mappings: `wa-show`/`wa-hide` derive `IsOpen` from the event type (serves `WaDetails.OnToggle`); `wa-intersect` flattens the `IntersectionObserverEntry` into `IsIntersecting`/`IntersectionRatio`; `wa-mutation`/`wa-resize` project observer records to JSON-safe subsets; `wa-selection-change` projects selected tree items to `{ id, textContent }`; `wa-reposition` reads `position`/`positionInPixels` off the element (the event has no detail).
- Event args classes now all inherit `System.EventArgs` — Blazor's dispatcher rejects handler parameter types that don't (`EventArgsTypeCache` throws "must inherit from System.EventArgs"; found live, invisible at compile time).
- `WaIntersectionObserver` no longer fabricates its event args (it hard-coded `IsIntersecting = true`, `IntersectionRatio = 0.0`); real values now come from the event detail. `WaSplitPanel.OnReposition` gets real positions from the element instead of the extra JS-interop round-trips it used to make.
- Verified end-to-end with Playwright: real tab clicks deliver the typed payload (`WaTabChangeEventArgs.Name`) into the Blazor handler; new regression spec `tools\e2e\tests\custom-event-payload.spec.js`.

**Behavioral/API changes in the same area (justified as the events never fired before, so nothing could have depended on them):**
- `WaTabGroup.OnTabChange` is now raised by the real `wa-tab-show` browser event (Web Awesome 3.0 has no `wa-tab-change` event); `OnTabShow` and `OnTabChange` share one browser event.
- The `OnTabClose`/event-args and `ElementReference`-property removals are listed under Breaking changes above.

**Guardrails added:** `EventBindingRegistrationTests` source-scans the wrappers and fails if any event is bound without the `on` prefix or bound without a matching registration in the JS initializer's event list — both failure modes are silent at build time and runtime, which is how this shipped in the first place.

### Fixed — imperative methods that invoked nonexistent element methods (browser-verified sweep)
Every `extraElementMethods` allowlist entry was verified against the live WA 3.0.0 custom elements in a real browser (probing `typeof element[method]` after `customElements.whenDefined`), after the showcase work caught `WaDialog.HideAsync()` throwing "Method 'hide' does not exist on element wa-dialog". Findings and fixes:
- `wa-dialog`/`wa-drawer` have `show()` but **no** `hide()`; `wa-dropdown` has neither. `ShowAsync`/`HideAsync` on `WaDialog`, `WaDrawer`, and `WaDropdown` now drive the element's `open` property (symmetric, documented, attribute-backed).
- `wa-copy-button` has no `copy()` — `CopyAsync` now clicks the element, which triggers the copy exactly like a user interaction.
- `wa-zoomable-frame` has no `setZoom()`/`reset()` (only `zoomIn()`/`zoomOut()`) — `SetZoomAsync` now sets the `zoom` property; `ResetAsync` sets it to 1.0.
- Methods with no working JS equivalent were removed (never functional, so nothing could depend on them) — listed under Breaking changes above; only `wa-popup` has a real `reposition()`, so `WaPopup.RepositionAsync` stays.
- `parity-config.json` `extraElementMethods` now contains only browser-verified entries (observer `stopObserver`/`startObserver`, `wa-relative-time.update`), each dated; the `ElementMethodInvocationTests` audit enforces the list.

### Added
- Server-hosted demo variant (`src\WebAwesome.Blazor.Demo.Server`, Blazor Web App with interactive server render mode, prerendering off): reuses the entire demo UI from `WebAwesome.Blazor.Demo` via `AddAdditionalAssemblies`, so every page runs under both Blazor hosting models. Static assets (CSS, theme helper, `data/api-surface.json`) flow through as static web assets from the referenced demo project; the theming helper moved from inline `index.html` script to `wwwroot\js\demo-theme.js` so both hosts share one copy. Runs on `http://localhost:5100`; the Playwright suite runs against it via `DEMO_BASE_URL=http://localhost:5100` (verified: all 82 tests pass on both hosts, including the custom-event payload regression over the SignalR circuit).
- Curated showcase pages (`src\WebAwesome.Blazor.Demo\Pages\Showcases\`, nav section "Showcases", covered by the e2e sweep): registration form (all 10 form controls in one validated EditForm), dashboard, settings, overlays, media gallery, and content/observers — together they exercise all 59 wrappers and all 8 CSS layout components with realistic content, including live typed event payloads (tab change, tree selection, resize/mutation/intersection observers).
- Persistent browser-based test automation (Playwright) verifying the demo app end-to-end — see `tools\e2e\README.md`. Grew out of manually diagnosing the bugs above; ad hoc Playwright scripts caught issues neither the build nor bUnit tests could (real DOM event semantics, JS interop failures, actual rendered CSS).
- Demo nav now groups components into the same documentation categories Web Awesome uses in its own docs (Actions, Feedback & Status, Form Controls, Imagery, Navigation, Organization, Utilities), via `WebAwesome.Blazor.Demo\Services\ComponentCategoryMap.cs`.

### Library
- Comprehensive EditForm integration coverage (bUnit) for all 10 form controls (`src\WebAwesome.Blazor.Tests\Forms\`): binding, change propagation, DataAnnotations validation lifecycle, validation CSS classes, custom validity interop, common parameter rendering. Notable findings recorded during this work: `WaRange` and `WaSlider` both render the `wa-slider` tag (duplication to resolve at the next upgrade); `WaRange`/`WaRating` `TryParseValueFromString` overrides are unreachable through the actual `onchange` wiring; `WaColorPicker` is `WaInputBase<string>` while sibling string controls use `string?`.
- Upgrade process extended with explicit coverage gates for newly added components and features (event-delivery contract, element-method audit, form-control test coverage, public API baseline promotion, showcase curation, browser verification) — see `docs\UPGRADE-PROCESS.md` "Coverage gates" and the updated `/wa-upgrade` skill phases.
- Public API snapshot test (`PublicApiSnapshotTests` + `approved-public-api.txt` baseline, via PublicApiGenerator): catches accidental breaking changes between our releases; intentional changes are promoted into the baseline and mentioned here.
- EditForm end-to-end integration tests (bUnit): two-way binding, DataAnnotations validation lifecycle, validation CSS class merging, `SetCustomValidityAsync` interop round-trip, and the WaCheckbox checked-state interop workaround.
- Element-method invocation audit (`ElementMethodInvocationTests`): every JS element method a wrapper invokes must be CEM-documented, a native DOM method, or explicitly allowlisted with a reason in `parity-config.json` (`extraElementMethods`) — the class of bug behind the observer `disconnect`/`reconnect` → `stopObserver`/`startObserver` rename can no longer slip through silently; allowlisted entries must be re-verified against the target source on every upgrade.
- Multi-targeting: the package now ships `net9.0` and `net10.0` assemblies; .NET 10 (LTS) is the primary supported target.
- NuGet packaging: full package metadata, icon, package readme, symbol packages (snupkg), per-framework XML documentation for IntelliSense.
- Automated upgrade pipeline: CEM-driven API diff tooling (`tools\upgrade\`), API-surface parity tests, and the `/wa-upgrade` orchestration skill (see `docs\UPGRADE-PROCESS.md`).
- Repository hygiene: corrected README setup snippet (official `@awesome.me/webawesome` CDN), verified 3.0.0-beta.6 documentation inputs, corrected API parity baseline.
- Test stack migrated to the current generations: xunit → `xunit.v3` 3.2.2, bUnit 1.40.0 → 2.7.2 (official .NET 10 support); transitive AngleSharp lifted to 1.5.2 (mXSS advisory CVE-2026-54570).
- NuGet dependency policy (see `docs\technical.md`): the package's framework dependencies are floored at the base target majors (`9.0.0`/`10.0.0`) instead of pinning latest patches — consumers are no longer forced onto specific patch levels; test/tooling dependencies track latest stable.
- Release engineering: CI split into branch verification (`build.yml`) and a tag-triggered release workflow (`release.yml`, `wa-blazor-*` tags) that rebuilds from the tag and publishes to nuget.org via Trusted Publishing behind a go-live gate; release/branching model codified (patch releases tagged on train subtrunks, new trains branch from the main line behind a release gate) — see `README.md` and `CONTRIBUTING.md`.
- Repository restructuring: task-scoped working documents moved from `docs\prompts\` to `tasks\<epic>\<task>\`; durable wrapper technical standards extracted into `docs\technical.md`.

## [3.0.0-beta.6] — 2026-07-18

### Breaking changes
- `WaIcon`: removed `FixedWidth` (fixed width is the Font Awesome 7 default); added `AutoWidth` and `SwapOpacity`.
- `WaDetails`: renamed `IconPosition` to `IconPlacement`; enum `WaIconPosition` replaced by `WaIconPlacement`.
- `WaButtonGroup`: removed `Size`.

### New components
- `WaIntersectionObserver` with typed `WaIntersectionEventArgs`.

### Changed
- `WaCard`: added `Orientation` and header/footer action slots.
- `WaDropdownItem`: added variant support.
- Implemented element methods requiring JS interop across components (show/hide, focus/blur, `setCustomValidity`), backed by a single interop module.

### Library
- Integration tests and breaking-change validation tests added.

See [MIGRATION-3.0.0-beta.6.md](MIGRATION-3.0.0-beta.6.md) for the full migration guide.

## [3.0.0-beta.4] — 2025-09-27

Initial release: 57 component wrappers and 8 CSS layout components (`WaCluster`, `WaFlank`, `WaFrame`, `WaGrid`, `WaSplit`, `WaStack`, `WaText`, `WaVisuallyHidden`), strongly-typed enum parameters, `InputBase<TValue>`-based form controls with EditForm integration.
