# Web Awesome Blazor 3.1.0 Migration Guide

Migration from `3.0.0` to `3.1.0`, mirroring the Web Awesome 3.1.0 release. A small upgrade: one removed wrapper parameter, and the new `wa-combobox` Pro component gains a wrapper.

## Breaking Changes

### 1. `WaButtonGroup.Variant` removed (BREAKING)

#### Changes
- Web Awesome 3.1.0 removed the `variant` attribute from `wa-button-group`; the `WaButtonGroup.Variant` parameter is removed with it. Per the upstream maintainers (issue #1677), the group-level variant had already stopped functioning in earlier WA releases and the property lingered by mistake.

#### Migration
- Delete `Variant="..."` from `WaButtonGroup` usages and set the variant on the individual `WaButton` children instead (this also allows mixing variants within a group).

## Not a Breaking Change: `WaButton.Form` is kept

Web Awesome 3.1.0 removed `form` from `wa-button`'s declared properties, but only because form association moved to the native platform mechanism (`ElementInternals`, upstream PR #1815) — the `form="id"` attribute remains fully functional. `WaButton.Form` therefore stays and keeps working, including with `EditForm`: give the `EditForm` an `id` and an external `<WaButton Type="WaButtonType.Submit" Form="that-id">` triggers its submit/validation pipeline as if it were inside the form.

## New Features
- **`WaCombobox`** (Pro component, experimental upstream, new in WA 3.1.0): a filterable single/multi-select form control, API near-twin of `WaSelect` — `@bind-Value`, `WaOption` children, `Multiple` + `SelectedValues`/`SelectedValuesChanged`, `AllowCustomValue`, `MaxOptionsVisible`, `WithClear`, `Pill`, `Appearance`, `Placement`, start/end/clear-icon/expand-icon slots, `ShowAsync`/`HideAsync`/`FocusAsync`/`BlurAsync`, and full `EditForm` validation integration. Note the experimental upstream status — the API may change in later releases.
- `WaCard` now accepts `WaAppearance.OutlinedFilled` (`filled-outlined`), which WA 3.1.0 added to the card's appearance union.

## Migration Checklist
- [ ] Update the package reference to `WebAwesome.Blazor` 3.1.0 and your Web Awesome assets to 3.1.0 (CDN URL or self-hosted).
- [ ] Remove `Variant="..."` from `WaButtonGroup` usages; set variants per button (section 1).
- [ ] Run your test suite.

## Automated Migration

### Find/Replace Patterns
| Find | Replace | Scope |
|---|---|---|
| `Variant="` | *(move to child buttons)* | `WaButtonGroup` usages |

## Support
File issues at <https://github.com/boldbrick/WebAwesome-Blazor/issues> with the WA version, bindings version, and a minimal repro.

## Version Compatibility
| Bindings | Web Awesome | .NET |
|---|---|---|
| 3.1.0 | 3.1.0 | net10.0 (primary), net9.0 |
