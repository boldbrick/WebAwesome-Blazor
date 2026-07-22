# Web Awesome Blazor 3.1.0 Migration Guide

Migration from `3.0.0` to `3.1.0`, mirroring the Web Awesome 3.1.0 release. A small upgrade: two attribute removals upstream translate into two removed wrapper parameters, and the new `wa-combobox` Pro component gains a wrapper.

## Breaking Changes

### 1. `WaButton.Form` removed (BREAKING)

#### Changes
- Web Awesome 3.1.0 removed the `form` attribute override from `wa-button` (and from all form controls, where the wrappers never exposed it — form association in Blazor is handled by `EditForm`/`EditContext`).
- The `WaButton.Form` parameter is removed. The other form-submission overrides (`FormAction`, `FormMethod`, `FormEnctype`, `FormNoValidate`, `FormTarget`) remain.

#### Migration
- Delete `Form="..."` from `WaButton` usages. Place the button inside the `EditForm` it should submit; associating a button with a form by id is no longer supported upstream.

### 2. `WaButtonGroup.Variant` removed (BREAKING)

#### Changes
- Web Awesome 3.1.0 removed the `variant` attribute from `wa-button-group`; the `WaButtonGroup.Variant` parameter is removed with it.

#### Migration
- Delete `Variant="..."` from `WaButtonGroup` usages and set the variant on the individual `WaButton` children instead.

## New Features
- **`WaCombobox`** (Pro component, experimental upstream, new in WA 3.1.0): a filterable single/multi-select form control, API near-twin of `WaSelect` — `@bind-Value`, `WaOption` children, `Multiple` + `SelectedValues`/`SelectedValuesChanged`, `AllowCustomValue`, `MaxOptionsVisible`, `WithClear`, `Pill`, `Appearance`, `Placement`, start/end/clear-icon/expand-icon slots, `ShowAsync`/`HideAsync`/`FocusAsync`/`BlurAsync`, and full `EditForm` validation integration. Note the experimental upstream status — the API may change in later releases.
- `WaCard` now accepts `WaAppearance.OutlinedFilled` (`filled-outlined`), which WA 3.1.0 added to the card's appearance union.

## Migration Checklist
- [ ] Update the package reference to `WebAwesome.Blazor` 3.1.0 and your Web Awesome assets to 3.1.0 (CDN URL or self-hosted).
- [ ] Remove `Form="..."` from `WaButton` usages (section 1).
- [ ] Remove `Variant="..."` from `WaButtonGroup` usages; set variants per button (section 2).
- [ ] Run your test suite.

## Automated Migration

### Find/Replace Patterns
| Find | Replace | Scope |
|---|---|---|
| `Form="` | *(remove attribute)* | `WaButton` usages |
| `Variant="` | *(move to child buttons)* | `WaButtonGroup` usages |

## Support
File issues at <https://github.com/boldbrick/WebAwesome-Blazor/issues> with the WA version, bindings version, and a minimal repro.

## Version Compatibility
| Bindings | Web Awesome | .NET |
|---|---|---|
| 3.1.0 | 3.1.0 | net10.0 (primary), net9.0 |
