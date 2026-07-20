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
- Removed `WaTabGroup.OnTabClose` and `WaTabCloseEventArgs` — `wa-tab-close` does not exist in Web Awesome 3.0.
- Removed `WaSlideChangeEventArgs.Slide` and `WaIntersectionEventArgs.Target` (`ElementReference` properties that can never be populated from an event payload — DOM elements cannot be marshaled into element references).

**Guardrails added:** `EventBindingRegistrationTests` source-scans the wrappers and fails if any event is bound without the `on` prefix or bound without a matching registration in the JS initializer's event list — both failure modes are silent at build time and runtime, which is how this shipped in the first place.

### Fixed — imperative methods that invoked nonexistent element methods (browser-verified sweep)
Every `extraElementMethods` allowlist entry was verified against the live WA 3.0.0 custom elements in a real browser (probing `typeof element[method]` after `customElements.whenDefined`), after the showcase work caught `WaDialog.HideAsync()` throwing "Method 'hide' does not exist on element wa-dialog". Findings and fixes:
- `wa-dialog`/`wa-drawer` have `show()` but **no** `hide()`; `wa-dropdown` has neither. `ShowAsync`/`HideAsync` on `WaDialog`, `WaDrawer`, and `WaDropdown` now drive the element's `open` property (symmetric, documented, attribute-backed).
- `wa-copy-button` has no `copy()` — `CopyAsync` now clicks the element, which triggers the copy exactly like a user interaction.
- `wa-zoomable-frame` has no `setZoom()`/`reset()` (only `zoomIn()`/`zoomOut()`) — `SetZoomAsync` now sets the `zoom` property; `ResetAsync` sets it to 1.0.
- Removed methods with no working equivalent (never functional, so nothing could depend on them): `WaDropdown.RepositionAsync`, `WaPopover.RepositionAsync`, `WaTooltip.RepositionAsync` (no `reposition()` on those elements — only `wa-popup` has one, and `WaPopup.RepositionAsync` stays), `WaInclude.ReloadAsync` (no `reload()`; Lit ignores same-value `src` re-assignment), `WaMutationObserver.TakeRecordsAsync` (no `takeRecords()`; the element's observer is private).
- `parity-config.json` `extraElementMethods` now contains only browser-verified entries (observer `stopObserver`/`startObserver`, `wa-relative-time.update`), each dated; the `ElementMethodInvocationTests` audit enforces the list.

### Added
- Curated showcase pages (`src\WebAwesome.Blazor.Demo\Pages\Showcases\`, nav section "Showcases", covered by the e2e sweep): registration form (all 10 form controls in one validated EditForm), dashboard, settings, overlays, media gallery, and content/observers — together they exercise all 59 wrappers and all 8 CSS layout components with realistic content, including live typed event payloads (tab change, tree selection, resize/mutation/intersection observers).
- New wrapper components: `WaScroller`, `WaTree`, `WaTreeItem` — confirmed present with full attributes/slots/events in the exact bound 3.0.0-beta.6 API surface (`wa-tree`/`wa-tree-item` since WA 2.0, `wa-scroller` since 3.0), so this was a real gap, not a not-yet-released feature. `WaTree.Selection` uses the new `WaTreeSelection` enum (Single/Multiple/Leaf). `WaTreeItem.getChildrenItems()` (CEM method) and `WaTree`'s `wa-selection-change` full item list are intentionally not exposed as strongly-typed `WaTreeItem[]`/`ElementReference[]` — there's no supported way to turn arbitrary DOM elements returned from JS into live Blazor `ElementReference`s outside of Blazor's own element-reference capture; `WaTreeSelectionChangeEventArgs.Selection` is left as raw `object[]`, consistent with the existing `MutationEventArgs`/`ResizeEventArgs` convention for the same limitation.
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
