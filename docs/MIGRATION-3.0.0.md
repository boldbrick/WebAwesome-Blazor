# Web Awesome Blazor 3.0.0 Migration Guide

Migration from `3.0.0-beta.6` to the `3.0.0` stable release. Alongside the upstream API alignment, this release fixes event delivery library-wide (no `wa-*` event callback ever fired in earlier releases) and removes wrapper members whose JS counterparts never existed — so some removals below cannot have working call sites in your code.

## Breaking Changes

### 1. `appearance` combined token renamed (BREAKING at the CSS/output level)

#### Changes
- Web Awesome 3.0.0 renamed the combined appearance token from `outlined filled` to `filled-outlined`.
- `WaAppearance.OutlinedFilled` (used by `WaBadge`, `WaButton`, `WaCallout`, `WaDetails`, `WaTag`) now emits `appearance="filled-outlined"`.
- `WaInputAppearance` gains the new `FilledOutlined` value for `WaInput`, `WaSelect`, `WaTextArea`.

#### Migration
- No C# changes — enum values keep their names; only the emitted attribute token changed.
- If you have CSS selectors, tests, or scraping that match `appearance="outlined filled"`, update them to `appearance="filled-outlined"`.

### 2. WaInclude — event rename (BREAKING)

#### Changes
- Upstream renamed the event `wa-error` to `wa-include-error`; the wrapper parameter `OnError` is now `OnIncludeError` (payload type `IncludeErrorEventArgs` unchanged).

#### Code Search & Replace
- `OnError=` → `OnIncludeError=` (on `WaInclude` usages only).

### 3. Removed members that never functioned (BREAKING formally, inert in practice)

#### Changes
- `WaTabGroup.OnTabClose` and `WaTabCloseEventArgs` removed — `wa-tab-close` does not exist in Web Awesome 3.0.
- `WaSlideChangeEventArgs.Slide` and `WaIntersectionEventArgs.Target` removed — DOM elements cannot be marshaled into Blazor `ElementReference`s from an event payload.
- `WaDropdown.RepositionAsync`, `WaPopover.RepositionAsync`, `WaTooltip.RepositionAsync`, `WaInclude.ReloadAsync`, `WaMutationObserver.TakeRecordsAsync` removed — the underlying JS methods do not exist on those elements (`WaPopup.RepositionAsync` remains; `wa-popup` has a real `reposition()`).

#### Migration
- Delete any call sites; they could only ever have thrown or done nothing. For dropdown/popover/tooltip positioning, rely on the components' own placement handling.

### 4. Behavior change: events now actually fire

#### Changes
- Event delivery was broken library-wide before this release (wrong attribute prefix + missing custom-event registration). Every `EventCallback` you had wired to a `wa-*` event starts receiving real invocations in 3.0.0.
- `WaDialog`/`WaDrawer`/`WaDropdown` `ShowAsync`/`HideAsync` now drive the element's `open` property; `WaCopyButton.CopyAsync` triggers a real click; `WaZoomableFrame.SetZoomAsync`/`ResetAsync` set the `zoom` property.

#### Migration
- Review handlers that were effectively dead code — they will now run. Pay attention to handlers with side effects (navigation, state resets) previously masked by the bug.

## New Features
- **`WaPage`** (Pro component, new in WA 3.0.0): page scaffolding with 15 slots (`Banner`, `Header`, `Navigation`, `Aside`, `MainHeader`, …), `NavOpen`/`NavigationPlacement`/`MobileBreakpoint`/`View` parameters, and `ShowNavigationAsync`/`HideNavigationAsync`/`ToggleNavigationAsync`.
- **`WaComparison`**: before/after comparison wrapper (component existed upstream; the wrapper was missing).
- **`WaScroller`, `WaTree`, `WaTreeItem`**: scrolling container and tree view with typed `WaTreeSelection` modes and `wa-selection-change` delivery.
- Server-hosted demo app (`WebAwesome.Blazor.Demo.Server`) proving every component under interactive server rendering.

## Migration Checklist
- [ ] Update the package reference to `WebAwesome.Blazor` 3.0.0 and your Web Awesome assets to 3.0.0 (CDN URL or self-hosted).
- [ ] Rename `OnError` → `OnIncludeError` on `WaInclude` usages.
- [ ] Update anything matching the literal `outlined filled` attribute token to `filled-outlined`.
- [ ] Remove call sites of the deleted methods/members (section 3).
- [ ] Review all `wa-*` event handlers — they fire for real now (section 4).
- [ ] Run your test suite; validate dialogs/drawers/dropdowns that use `ShowAsync`/`HideAsync`.

## Automated Migration

### Find/Replace Patterns
| Find | Replace | Scope |
|---|---|---|
| `OnError=` | `OnIncludeError=` | `WaInclude` usages |
| `outlined filled` | `filled-outlined` | CSS/tests matching the emitted attribute |
| `.RepositionAsync(` | *(remove)* | `WaDropdown`, `WaPopover`, `WaTooltip` refs |
| `.ReloadAsync(` | *(remove)* | `WaInclude` refs |
| `.TakeRecordsAsync(` | *(remove)* | `WaMutationObserver` refs |

## Support
File issues at <https://github.com/boldbrick/WebAwesome-Blazor/issues> with the WA version, bindings version, and a minimal repro.

## Version Compatibility
| Bindings | Web Awesome | .NET |
|---|---|---|
| 3.0.0 | 3.0.0 | net10.0 (primary), net9.0 |
