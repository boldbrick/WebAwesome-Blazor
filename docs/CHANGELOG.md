# Changelog

All notable changes to the Web Awesome Blazor Bindings. Versions mirror the bound [Web Awesome](https://github.com/shoelace-style/webawesome) release; the format follows [Keep a Changelog](https://keepachangelog.com/).

## [Unreleased]

### Fixed
- `WaCheckbox`/`WaSwitch`: two-way `@bind-Value` silently never updated on user interaction. Root cause: Blazor's built-in change-event value extraction only reads the DOM `.checked` property when the target's `tagName === "INPUT"`; for the custom elements `<wa-checkbox>`/`<wa-switch>` it falls back to reading `.value`, which the wrapper hard-codes to the constant string `"True"` for form-submission purposes (mirroring native checkbox `value` semantics) — so the bound value never reflected the real checked state. **Workaround applied:** replaced the `EventCallback.Factory.CreateBinder<bool>` `onchange` wiring with a manual handler that reads the true state via `WebAwesomeJSInterop.GetPropertyAsync<bool>(Element, "checked")`. **Next-release check:** verify whether the upstream Blazor runtime (or a future WA release exposing these as form-associated custom elements recognized by `ElementInternals`) changes this; if Blazor ever special-cases arbitrary form-associated custom elements for `.checked` extraction, this workaround can likely be removed.
- Ten wrapper components (`WaDialog`, `WaDrawer`, `WaDropdown`, `WaInclude`, `WaMutationObserver`, `WaPopover`, `WaRelativeTime`, `WaResizeObserver`, `WaZoomableFrame`, `WaTooltip`) called a JS method named `"initialize"` in `OnAfterRenderAsync` that does not exist on any of the corresponding Web Awesome custom elements (confirmed against the actual 3.0.0-beta.6 CDN sources) — this threw `InvalidOperationException`/`JSException` on first render of every one of these components. **Workaround applied:** removed the dead `initialize()` calls entirely; these custom elements self-initialize via their own `connectedCallback`, no explicit interop call is needed. **Next-release check:** re-run the demo app's automated Playwright sweep (see below) against the new version before assuming this stays fixed — if a future WA release genuinely adds a required explicit init step for any of these elements, this area regresses silently otherwise.
- `WaResizeObserver.DisconnectAsync`/`ReconnectAsync` and `WaMutationObserver.DisconnectAsync`/`ReconnectAsync` invoked nonexistent JS methods `"disconnect"`/`"reconnect"`. **Workaround applied:** corrected to the real methods `stopObserver()`/`startObserver()` (confirmed from source). **Next-release check:** re-verify these method names against the target CEM/source; they are not part of the public CEM-documented `methods` list (both observers report empty `methods` in the API surface), so `Compare-WaApiSurface.ps1` cannot flag a rename here automatically — a manual source check is required each upgrade.
- Demo: theme switcher (`MainLayout.razor`) swapped the `<link>` stylesheet for the "Awesome"/"Shoelace" themes but never applied the required `wa-theme-{name}` class to `<html>` — Web Awesome's theme CSS scopes every rule under `.wa-theme-{name}` (confirmed against the actual theme CSS from the CDN), so the stylesheet swap alone had no visible effect. Fixed in `wwwroot\index.html` (`demoTheme.setTheme` now also toggles the class) and `MainLayout.razor` (passes the theme name through).
- Demo: `WaSelect`'s "Multiple Selection" example used only `SelectedValues`/`SelectedValuesChanged` without `@bind-Value`, which `InputBase<T>` still requires (throws `ValueExpression` is required even when the base `Value` isn't otherwise used) — added a dummy `@bind-Value`.
- Demo: several `Pages\Components\*.razor` skeleton/curated pages used bare component usages without `@bind-Value` (throws for every `WaInputBase<T>`-derived control), rendered nothing (container components with no slotted content), or used invalid CSS custom properties (e.g. `--wa-color-blue-fill-loud`, which doesn't exist — only the five semantic groups brand/success/neutral/warning/danger have `-fill-loud` roles). See per-page fixes across this session.

### Suspected gap, not yet investigated
- `WaIntersectionObserver.HandleIntersectEvent` (private) fabricates its event args (`IsIntersecting = true`, `IntersectionRatio = 0.0` hard-coded, with a comment admitting the real values "would come from the JS event") instead of reading the real `wa-intersect` event detail — `OnIntersect` never reflects actual intersection state. Separately, several other wrappers (e.g. `WaCarousel.OnSlideChange`, `WaTabGroup.OnTabChange`) bind a custom `EventCallback<T>` with a structured payload type directly to a non-"on"-prefixed custom DOM event name (`wa-slide-change`, `wa-tab-change`, ...) with no `[EventHandler]` registration anywhere in the assembly; whether Blazor's client runtime actually knows how to deserialize such an event's `detail` into the declared C# type (as opposed to silently no-oping or producing a default-valued instance) was not verified this session and should be checked with a live-browser test before relying on it. Not fixed here — flagging so it isn't mistaken for "known good" just because it compiles and nothing crashes.

### Added
- New wrapper components: `WaScroller`, `WaTree`, `WaTreeItem` — confirmed present with full attributes/slots/events in the exact bound 3.0.0-beta.6 API surface (`wa-tree`/`wa-tree-item` since WA 2.0, `wa-scroller` since 3.0), so this was a real gap, not a not-yet-released feature. `WaTree.Selection` uses the new `WaTreeSelection` enum (Single/Multiple/Leaf). `WaTreeItem.getChildrenItems()` (CEM method) and `WaTree`'s `wa-selection-change` full item list are intentionally not exposed as strongly-typed `WaTreeItem[]`/`ElementReference[]` — there's no supported way to turn arbitrary DOM elements returned from JS into live Blazor `ElementReference`s outside of Blazor's own element-reference capture; `WaTreeSelectionChangeEventArgs.Selection` is left as raw `object[]`, consistent with the existing `MutationEventArgs`/`ResizeEventArgs` convention for the same limitation.
- Persistent browser-based test automation (Playwright) verifying the demo app end-to-end — see `tools\e2e\README.md`. Grew out of manually diagnosing the bugs above; ad hoc Playwright scripts caught issues neither the build nor bUnit tests could (real DOM event semantics, JS interop failures, actual rendered CSS).
- Demo nav now groups components into the same documentation categories Web Awesome uses in its own docs (Actions, Feedback & Status, Form Controls, Imagery, Navigation, Organization, Utilities), via `WebAwesome.Blazor.Demo\Services\ComponentCategoryMap.cs`.

### Library
- Multi-targeting: the package now ships `net9.0` and `net10.0` assemblies; .NET 10 (LTS) is the primary supported target.
- NuGet packaging: full package metadata, icon, package readme, symbol packages (snupkg), per-framework XML documentation for IntelliSense.
- Automated upgrade pipeline: CEM-driven API diff tooling (`tools\upgrade\`), API-surface parity tests, and the `/wa-upgrade` orchestration skill (see `docs\UPGRADE-PROCESS.md`).
- Repository hygiene: corrected README setup snippet (official `@awesome.me/webawesome` CDN), verified 3.0.0-beta.6 documentation inputs, corrected API parity baseline.

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
